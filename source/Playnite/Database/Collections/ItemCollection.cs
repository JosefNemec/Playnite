using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class ItemUpdateEvent<TItem> where TItem : DatabaseObject
    {
        public TItem OldData
        {
            get; set;
        }

        public TItem NewData
        {
            get; set;
        }

        public ItemUpdateEvent(TItem oldData, TItem newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void ItemUpdatedEventHandler<TItem>(object sender, ItemUpdatedEventArgs<TItem> args) where TItem : DatabaseObject;

    public class ItemUpdatedEventArgs<TItem> : EventArgs where TItem : DatabaseObject
    {
        public List<ItemUpdateEvent<TItem>> UpdatedItems
        {
            get; set;
        }

        public ItemUpdatedEventArgs(TItem oldData, TItem newData)
        {
            UpdatedItems = new List<ItemUpdateEvent<TItem>>() { new ItemUpdateEvent<TItem>(oldData, newData) };
        }

        public ItemUpdatedEventArgs(List<ItemUpdateEvent<TItem>> updatedItems)
        {
            UpdatedItems = updatedItems;
        }
    }

    public delegate void ItemCollectionChangedEventHandler<TItem>(object sender, ItemCollectionChangedEventArgs<TItem> args) where TItem : DatabaseObject;

    public class ItemCollectionChangedEventArgs<TItem> : EventArgs where TItem : DatabaseObject
    {
        public List<TItem> AddedItems
        {
            get; set;
        }

        public List<TItem> RemovedItems
        {
            get; set;
        }

        public ItemCollectionChangedEventArgs(List<TItem> addedItems, List<TItem> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }

    public class ItemCollection<TItem> : ICollection<TItem> where TItem : DatabaseObject
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly object collectionLock = new object();
        private string storagePath;
        private readonly Action<TItem> initMethod;
        private bool isEventBufferEnabled = false;
        private List<TItem> AddedItemsEventBuffer = new List<TItem>();
        private List<TItem> RemovedItemsEventBuffer = new List<TItem>();
        private List<ItemUpdateEvent<TItem>> ItemUpdatesEventBuffer = new List<ItemUpdateEvent<TItem>>();

        public ConcurrentDictionary<Guid, TItem> Items { get; }

        public int Count => Items.Count;

        public bool IsReadOnly => false;
        
        public TItem this[Guid id]
        {
            get => Get(id);
            set
            {
                new NotImplementedException();
            }
        }

        public event ItemCollectionChangedEventHandler<TItem> ItemCollectionChanged;

        public event ItemUpdatedEventHandler<TItem> ItemUpdated;

        public ItemCollection()
        {
            Items = new ConcurrentDictionary<Guid, TItem>();
        }

        public ItemCollection(Action<TItem> initMethod) : this()
        {
            this.initMethod = initMethod;
        }

        public ItemCollection(string path)
        {
            Items = new ConcurrentDictionary<Guid, TItem>();
            InitializeCollection(path);
        }

        public ItemCollection(string path, Action<TItem> initMethod) : this(path)
        {
            this.initMethod = initMethod;
        }

        public void InitializeCollection(string path)
        {
            if (!string.IsNullOrEmpty(storagePath))
            {
                throw new Exception("Collection already initialized.");
            }

            storagePath = path;
            if (Directory.Exists(storagePath))
            {
                Parallel.ForEach(Directory.EnumerateFiles(storagePath, "*.json"), (objectFile) =>
                {
                    try
                    {
                        var obj = Serialization.FromJson<TItem>(FileSystem.ReadFileAsStringSafe(objectFile));
                        initMethod?.Invoke(obj);
                        Items.TryAdd(obj.Id, obj);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to load item from {objectFile}");
                    }
                });
            }
        }

        private string GetItemFilePath(Guid id)
        {
            return Path.Combine(storagePath, $"{id.ToString()}.json");
        }

        private void SaveItemData(TItem item)
        {
            FileSystem.WriteStringToFileSafe(GetItemFilePath(item.Id), Serialization.ToJson(item, false));
        }

        private TItem GetItemData(Guid id)
        {
            return Serialization.FromJson<TItem>(FileSystem.ReadFileAsStringSafe(GetItemFilePath(id)));
        }

        public TItem Get(Guid id)
        {
            if (Items.TryGetValue(id, out var item))
            {
                return item;
            }
            else
            {
                return null;
            }
        }

        public virtual void Add(TItem item)
        {
            lock (collectionLock)
            {
                SaveItemData(item);
                Items.TryAdd(item.Id, item);
            }

            OnCollectionChanged(new List<TItem>() { item }, new List<TItem>());
        }

        public virtual void Add(IEnumerable<TItem> items)
        {
            if (items?.Any() != true)
            {
                return;
            }

            lock (collectionLock)
            {
                foreach (var item in items)
                {
                    SaveItemData(item);
                    Items.TryAdd(item.Id, item);
                }
            }

            OnCollectionChanged(items.ToList(), new List<TItem>());
        }

        public virtual bool Remove(Guid id)
        {
            var item = Get(id);
            lock (collectionLock)
            {
                FileSystem.DeleteFile(GetItemFilePath(item.Id));
                Items.TryRemove(id, out var removed);
            }

            OnCollectionChanged(new List<TItem>(), new List<TItem>() { item });
            return true;
        }

        public virtual bool Remove(TItem item)
        {
            return Remove(item.Id);
        }

        public virtual bool Remove(IEnumerable<TItem> items)
        {
            if (items?.Any() != true)
            {
                return false;
            }

            lock (collectionLock)
            {
                foreach (var item in items)
                {
                    FileSystem.DeleteFile(GetItemFilePath(item.Id));
                    Items.TryRemove(item.Id, out var removed);
                }
            }

            OnCollectionChanged(new List<TItem>(), items.ToList());
            return true;
        }        

        public virtual void Update(TItem item)
        {            
            TItem oldData;
            lock (collectionLock)
            {
                oldData = GetItemData(item.Id);
                SaveItemData(item);
                var loadedItem = Get(item.Id);
                if (!ReferenceEquals(loadedItem, item))
                {
                    item.CopyProperties(loadedItem, true, null, true);
                }
            }

            OnItemUpdated(new List<ItemUpdateEvent<TItem>>() { new ItemUpdateEvent<TItem>(oldData, item) });
        }

        public virtual void Update(IEnumerable<TItem> items)
        {
            var updates = new List<ItemUpdateEvent<TItem>>();
            lock (collectionLock)
            {
                foreach (var item in items)
                {
                    var oldData = GetItemData(item.Id);
                    SaveItemData(item);
                    var loadedItem = Get(item.Id);
                    if (!ReferenceEquals(loadedItem, item))
                    {
                        item.CopyProperties(loadedItem, true, null, true);
                    }
                }
            }

            OnItemUpdated(updates);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TItem item)
        {
            return Items.ContainsKey(item.Id);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Items.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        private void OnCollectionChanged(List<TItem> addedItems, List<TItem> removedItems)
        {
            if (!isEventBufferEnabled)
            {
                ItemCollectionChanged?.Invoke(this, new ItemCollectionChangedEventArgs<TItem>(addedItems, removedItems));
            }
            else
            {
                AddedItemsEventBuffer.AddRange(addedItems);
                RemovedItemsEventBuffer.AddRange(removedItems);
            }
        }

        private void OnItemUpdated(List<ItemUpdateEvent<TItem>> updates)
        {
            if (!isEventBufferEnabled)
            {
                ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs<TItem>(updates));
            }
            else
            {
                ItemUpdatesEventBuffer.AddRange(updates);
            }
        }

        public void BeginBufferUpdate()
        {
            isEventBufferEnabled = true;
        }

        public void EndBufferUpdate()
        {
            isEventBufferEnabled = false;
            if (AddedItemsEventBuffer.Count > 0 || RemovedItemsEventBuffer.Count > 0)
            {
                OnCollectionChanged(AddedItemsEventBuffer.ToList(), RemovedItemsEventBuffer.ToList());
                AddedItemsEventBuffer.Clear();
                RemovedItemsEventBuffer.Clear();
            }

            if (ItemUpdatesEventBuffer.Count > 0)
            {
                OnItemUpdated(ItemUpdatesEventBuffer.ToList());
                ItemUpdatesEventBuffer.Clear();
            }
        }

        public IEnumerable<TItem> GetClone()
        {
            return this.Select(a => a.CloneJson());
        }
    }
}
