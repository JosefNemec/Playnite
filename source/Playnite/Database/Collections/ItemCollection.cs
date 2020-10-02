using Newtonsoft.Json;
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
    public class ItemCollection<TItem> : IItemCollection<TItem> where TItem : DatabaseObject
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly object collectionLock = new object();
        private string storagePath;
        private readonly Action<TItem> initMethod;
        private bool isEventBufferEnabled = false;
        private int bufferDepth = 0;
        private List<TItem> AddedItemsEventBuffer = new List<TItem>();
        private List<TItem> RemovedItemsEventBuffer = new List<TItem>();
        private Dictionary<Guid, ItemUpdateEvent<TItem>> ItemUpdatesEventBuffer = new Dictionary<Guid, ItemUpdateEvent<TItem>>();
        private readonly bool isPersistent = true;

        public ConcurrentDictionary<Guid, TItem> Items { get; }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public GameDatabaseCollection CollectionType { get; } = GameDatabaseCollection.Uknown;

        public TItem this[Guid id]
        {
            get => Get(id);
            set
            {
                new NotImplementedException();
            }
        }

        public event EventHandler<ItemCollectionChangedEventArgs<TItem>> ItemCollectionChanged;

        public event EventHandler<ItemUpdatedEventArgs<TItem>> ItemUpdated;

        internal bool IsEventsEnabled { get; set; } = true;

        public ItemCollection(bool isPersistent = true, GameDatabaseCollection type = GameDatabaseCollection.Uknown)
        {
            this.isPersistent = isPersistent;
            Items = new ConcurrentDictionary<Guid, TItem>();
            CollectionType = type;
        }

        public ItemCollection(Action<TItem> initMethod, bool isPersistent = true, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : this(isPersistent, type)
        {
            this.initMethod = initMethod;
        }

        public ItemCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown)
        {
            this.isPersistent = true;
            Items = new ConcurrentDictionary<Guid, TItem>();
            InitializeCollection(path);
            CollectionType = type;
        }

        public ItemCollection(string path, Action<TItem> initMethod, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : this(path, type)
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
            // This fixes an issue where people mess up their library with custom scripts
            // which create collection files instead of directories :|
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }

            if (Directory.Exists(storagePath))
            {
                Parallel.ForEach(
                    Directory.EnumerateFiles(storagePath, "*.json"),
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    (objectFile) =>
                    {
                        if (Guid.TryParse(Path.GetFileNameWithoutExtension(objectFile), out var _))
                        {
                            try
                            {
                                var obj = Serialization.FromJsonFile<TItem>(objectFile);
                                if (obj != null)
                                {
                                    initMethod?.Invoke(obj);
                                    Items.TryAdd(obj.Id, obj);
                                }
                                else
                                {
                                    logger.Warn($"Failed to deserialize collection item {objectFile}");
                                }
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                logger.Error(e, $"Failed to load item from {objectFile}");
                            }
                        }
                        else
                        {
                            logger.Warn($"Skipping non-id collection item {objectFile}");
                        }
                    });
            }
            else
            {
                FileSystem.CreateDirectory(storagePath);
            }
        }

        internal string GetItemFilePath(Guid id)
        {
            return Path.Combine(storagePath, $"{id.ToString()}.json");
        }

        internal void SaveItemData(TItem item)
        {
            using (var fs = FileSystem.CreateWriteFileStreamSafe(GetItemFilePath(item.Id)))
            using (var sw = new StreamWriter(fs))
            using (var writer = new JsonTextWriter(sw))
            {
                var ser = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });

                ser.Serialize(writer, item);
            }
        }

        internal TItem GetItemData(Guid id)
        {
            using (var fs = FileSystem.OpenReadFileStreamSafe(GetItemFilePath(id)))
            {
                return Serialization.FromJsonStream<TItem>(fs);
            }
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

        public bool ContainsItem(Guid id)
        {
            return Items?.ContainsKey(id) == true;
        }

        public List<TItem> Get(IList<Guid> ids)
        {
            var items = new List<TItem>(ids.Count);
            foreach (var id in ids)
            {
                var item = Get(id);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        public virtual TItem Add(string itemName, Func<TItem, string, bool> existingComparer)
        {
            if (string.IsNullOrEmpty(itemName)) throw new ArgumentNullException(nameof(itemName));
            var existingItem = this.FirstOrDefault(a => existingComparer(a, itemName));
            if (existingItem != null)
            {
                return existingItem;
            }
            else
            {
                var newItem = typeof(TItem).CrateInstance<TItem>(itemName);
                Add(newItem);
                return newItem;
            }
        }

        public virtual TItem Add(string itemName)
        {
            return Add(itemName, (existingItem, newName) => existingItem.Name.Equals(newName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual IEnumerable<TItem> Add(List<string> itemsToAdd, Func<TItem, string, bool> existingComparer)
        {
            var toAdd = new List<TItem>();
            foreach (var itemName in itemsToAdd)
            {
                var existingItem = this.FirstOrDefault(a => existingComparer(a, itemName));
                if (existingItem != null)
                {
                    yield return existingItem;
                }
                else
                {
                    var newItem = typeof(TItem).CrateInstance<TItem>(itemName);
                    toAdd.Add(newItem);
                    yield return newItem;
                }
            }

            if (toAdd?.Any() == true)
            {
                Add(toAdd);
            }
        }

        public virtual IEnumerable<TItem> Add(List<string> itemsToAdd)
        {
            return Add(itemsToAdd, (existingItem, newName) => existingItem.Name.Equals(newName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void Add(TItem itemToAdd)
        {
            if (Items.ContainsKey(itemToAdd.Id))
            {
                throw new Exception($"Item {itemToAdd.Id} already exists.");
            }

            lock (collectionLock)
            {
                if (isPersistent)
                {
                    SaveItemData(itemToAdd);
                }

                Items.TryAdd(itemToAdd.Id, itemToAdd);
            }

            OnCollectionChanged(new List<TItem>() { itemToAdd }, new List<TItem>());
        }

        public virtual void Add(IEnumerable<TItem> itemsToAdd)
        {
            if (itemsToAdd?.Any() != true)
            {
                return;
            }

            lock (collectionLock)
            {
                foreach (var item in itemsToAdd)
                {
                    if (Items.ContainsKey(item.Id))
                    {
                        throw new Exception($"Item {item.Id} already exists.");
                    }

                    if (isPersistent)
                    {
                        SaveItemData(item);
                    }

                    Items.TryAdd(item.Id, item);
                }
            }

            OnCollectionChanged(itemsToAdd.ToList(), new List<TItem>());
        }

        public virtual bool Remove(Guid id)
        {
            var item = Get(id);
            if (item == null)
            {
                throw new Exception($"Item {item.Id} doesn't exists.");
            }

            lock (collectionLock)
            {
                if (isPersistent)
                {
                    FileSystem.DeleteFile(GetItemFilePath(id));
                }

                Items.TryRemove(id, out var removed);
            }

            OnCollectionChanged(new List<TItem>(), new List<TItem>() { item });
            return true;
        }

        public virtual bool Remove(TItem itemToRemove)
        {
            return Remove(itemToRemove.Id);
        }

        public virtual bool Remove(IEnumerable<TItem> itemsToRemove)
        {
            if (itemsToRemove?.Any() != true)
            {
                return false;
            }

            lock (collectionLock)
            {
                foreach (var item in itemsToRemove)
                {
                    var existing = Get(item.Id);
                    if (existing == null)
                    {
                        throw new Exception($"Item {item.Id} doesn't exists.");
                    }

                    if (isPersistent)
                    {
                        FileSystem.DeleteFile(GetItemFilePath(item.Id));
                    }

                    Items.TryRemove(item.Id, out var removed);
                }
            }

            OnCollectionChanged(new List<TItem>(), itemsToRemove.ToList());
            return true;
        }

        public virtual void Update(TItem itemToUpdate)
        {
            TItem oldData = null;
            TItem loadedItem;
            lock (collectionLock)
            {
                if (isPersistent)
                {
                    try
                    {
                        oldData = GetItemData(itemToUpdate.Id);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to read stored item data.");
                    }

                    // This should never ever happen, but there are automatic crash reports of Playnite db files being corrupted.
                    // This happens because of trash launchers from games like Zula,
                    // which mess with Playnite process and dump their log entries to our files.
                    // This will most likely cause some other issues, but at least it won't crash the whole app.
                    if (oldData == null)
                    {
                        logger.Error("Failed to read stored item data.");
                        oldData = this[itemToUpdate.Id].GetClone();
                    }
                }
                else
                {
                    oldData = Get(itemToUpdate.Id);
                }

                if (oldData == null)
                {
                    throw new Exception($"Item {oldData.Id} doesn't exists.");
                }

                if (isPersistent)
                {
                    SaveItemData(itemToUpdate);
                }

                loadedItem = Get(itemToUpdate.Id);
                if (!ReferenceEquals(loadedItem, itemToUpdate))
                {
                    itemToUpdate.CopyDiffTo(loadedItem);
                }
            }

            OnItemUpdated(new List<ItemUpdateEvent<TItem>>() { new ItemUpdateEvent<TItem>(oldData, loadedItem) });
        }

        public virtual void Update(IEnumerable<TItem> itemsToUpdate)
        {
            var updates = new List<ItemUpdateEvent<TItem>>();
            lock (collectionLock)
            {
                foreach (var item in itemsToUpdate)
                {
                    TItem oldData;
                    if (isPersistent)
                    {
                        try
                        {
                            oldData = GetItemData(item.Id);
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            // This should never ever happen, but there are automatic crash reports of Playnite db files being corrupted.
                            // This happens because of trash launchers from games like Zula,
                            // which mess with Playnite process and dump their log entries to our files.
                            // This will most likely cause some other issues, but at least it won't crash the whole app.
                            logger.Error(e, "Failed to read stored item data.");
                            oldData = this[item.Id].GetClone();
                        }
                    }
                    else
                    {
                        oldData = Get(item.Id);
                    }

                    if (oldData == null)
                    {
                        throw new Exception($"Item {oldData.Id} doesn't exists.");
                    }

                    if (isPersistent)
                    {
                        SaveItemData(item);
                    }

                    var loadedItem = Get(item.Id);
                    if (!ReferenceEquals(loadedItem, item))
                    {
                        item.CopyDiffTo(loadedItem);
                    }

                    updates.Add(new ItemUpdateEvent<TItem>(oldData, loadedItem));
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

        internal void OnCollectionChanged(List<TItem> addedItems, List<TItem> removedItems)
        {
            if (!IsEventsEnabled)
            {
                return;
            }

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

        internal void OnItemUpdated(IEnumerable<ItemUpdateEvent<TItem>> updates)
        {
            if (!IsEventsEnabled)
            {
                return;
            }

            if (!isEventBufferEnabled)
            {
                ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs<TItem>(updates));
            }
            else
            {
                foreach (var update in updates)
                {
                    if (ItemUpdatesEventBuffer.TryGetValue(update.NewData.Id, out var existing))
                    {
                        existing.NewData = update.NewData;
                    }
                    else
                    {
                        ItemUpdatesEventBuffer.Add(update.NewData.Id, update);
                    }
                }
            }
        }

        public void BeginBufferUpdate()
        {
            isEventBufferEnabled = true;
            bufferDepth++;
        }

        public void EndBufferUpdate()
        {
            // In case nested buffers are used then we end only when top level one clear.
            if (bufferDepth >= 1)
            {
                bufferDepth--;
            }

            if (bufferDepth == 0)
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
                    OnItemUpdated(ItemUpdatesEventBuffer.Values);
                    ItemUpdatesEventBuffer.Clear();
                }
            }
        }

        public IEnumerable<TItem> GetClone()
        {
            return this.Select(a => a.GetClone());
        }
    }
}
