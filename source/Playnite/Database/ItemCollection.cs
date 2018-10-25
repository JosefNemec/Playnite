using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections;
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
        private static ILogger logger = LogManager.GetLogger();

        private readonly object collectionLock = new object();
        private string storagePath;
        private readonly Action<TItem> initMethod;
        private bool isEventBufferEnabled = false;
        private List<TItem> AddedItemsEventBuffer = new List<TItem>();
        private List<TItem> RemovedItemsEventBuffer = new List<TItem>();
        private List<ItemUpdateEvent<TItem>> ItemUpdatesEventBuffer = new List<ItemUpdateEvent<TItem>>();

        public List<TItem> Items { get; }

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

        public ItemCollection() : this(null)
        {
        }

        public ItemCollection(Action<TItem> initMethod)
        {
            Items = new List<TItem>();
            this.initMethod = initMethod;
        }

        public void InitializeCollection(string storagePath)
        {
            if (!string.IsNullOrEmpty(this.storagePath))
            {
                throw new Exception("Collection alredy initialized.");
            }

            this.storagePath = storagePath;
            if (Directory.Exists(storagePath))
            {
                foreach (var objectFile in Directory.GetFiles(storagePath))
                {
                    try
                    {
                        var obj = Serialization.FromJson<TItem>(FileSystem.FileReadAsString(objectFile));
                        initMethod?.Invoke(obj);
                        Items.Add(obj);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to load item from {objectFile}");
                    }
                }
            }
        }

        private string GetItemFilePath(Guid id)
        {
            return Path.Combine(storagePath, $"{id.ToString()}.json");
        }

        private void SaveItemData(TItem item)
        {
            FileSystem.FileWriteString(GetItemFilePath(item.Id), Serialization.ToJson(item, true));
        }

        private TItem GetItemData(Guid id)
        {
            return Serialization.FromJson<TItem>(FileSystem.FileReadAsString(GetItemFilePath(id)));
        }

        public TItem Get(Guid id)
        {
            return Items.FirstOrDefault(a => a.Id == id);
        }

        public virtual void Add(TItem item)
        {
            lock (collectionLock)
            {
                SaveItemData(item);
                Items.Add(item);
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
                    Items.Add(item);
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
                Items.Remove(item);
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
                    Items.Remove(Get(item.Id));
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
            return Items.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
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
    }
}