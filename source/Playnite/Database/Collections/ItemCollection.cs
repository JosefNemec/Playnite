using LiteDB;
using Playnite.SDK;
using Playnite.SDK.Models;
using SqlNado;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    // We currently use LiteDB for permanent storage.
    // We don't use latest LiteDB 5, but instead latest LiteDB 4, because V5 has some issues:
    //  - doesn't allow disabling of memory cache (which in our case just wastes memory)
    //  - write speeds are slower
    public class ItemCollection<TItem> : IItemCollection<TItem> where TItem : DatabaseObject
    {
        class EventBufferHandler<T> : IDisposable where T : DatabaseObject
        {
            private IItemCollection<T> collection;

            public EventBufferHandler(IItemCollection<T> collection)
            {
                this.collection = collection;
                collection.BeginBufferUpdate();
            }

            public void Dispose()
            {
                collection.EndBufferUpdate();
            }
        }

        private ILogger logger = LogManager.GetLogger(typeof(TItem).Name + "_coll");
        private readonly object collectionLock = new object();
        private string storagePath;
        private readonly Action<TItem> initMethod;
        private bool isEventBufferEnabled = false;
        private int bufferDepth = 0;
        private List<TItem> AddedItemsEventBuffer = new List<TItem>();
        private List<TItem> RemovedItemsEventBuffer = new List<TItem>();
        private Dictionary<Guid, ItemUpdateEvent<TItem>> ItemUpdatesEventBuffer = new Dictionary<Guid, ItemUpdateEvent<TItem>>();
        private readonly bool isPersistent = true;
        internal LiteDatabase liteDb { get; private set; }
        private LiteCollection<TItem> liteCollection;
        private BsonMapper mapper;

        public ConcurrentDictionary<Guid, TItem> Items { get; }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public GameDatabaseCollection CollectionType { get; } = GameDatabaseCollection.Uknown;

        public TItem this[Guid id]
        {
            get => Get(id);
            set
            {
                new NotSupportedException();
            }
        }

        public event EventHandler<ItemCollectionChangedEventArgs<TItem>> ItemCollectionChanged;

        public event EventHandler<ItemUpdatedEventArgs<TItem>> ItemUpdated;

        internal bool IsEventsEnabled { get; set; } = true;

        public ItemCollection(BsonMapper mapper, bool isPersistent = true, GameDatabaseCollection type = GameDatabaseCollection.Uknown)
        {
            this.isPersistent = isPersistent;
            this.mapper = mapper;
            Items = new ConcurrentDictionary<Guid, TItem>();
            CollectionType = type;
        }

        public ItemCollection(Action<TItem> initMethod, BsonMapper mapper, bool isPersistent = true, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : this(mapper, isPersistent, type)
        {
            this.initMethod = initMethod;
        }

        public ItemCollection(string path, BsonMapper mapper, GameDatabaseCollection type = GameDatabaseCollection.Uknown)
        {
            this.isPersistent = true;
            this.mapper = mapper;
            Items = new ConcurrentDictionary<Guid, TItem>();
            InitializeCollection(path);
            CollectionType = type;
        }

        public void Dispose()
        {
            liteDb?.Dispose();
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

            var dbPath = path + ".db";
            void openDb()
            {
                liteDb = new LiteDatabase($"Filename={dbPath};Mode=Exclusive;Cache Size=0", mapper);
                liteCollection = liteDb.GetCollection<TItem>();
                liteCollection.EnsureIndex(a => a.Id, true);
            }

            void loadCollections()
            {
                Parallel.ForEach(
                    liteCollection.FindAll(),
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    (objectFile) =>
                    {
                        if (objectFile != null)
                        {
                            initMethod?.Invoke(objectFile);
                            Items.TryAdd(objectFile.Id, objectFile);
                        }
                    });

                // Also try to load other collection to see if db is corrupted
                foreach (var collName in liteDb.GetCollectionNames().Where(a => a != liteCollection.Name))
                {
                    var coll = liteDb.GetCollection(collName);
                    // One these would fail for known corruptions
                    coll.Count();
                    coll.FindAll().ToList();
                }
            }

            openDb();

            try
            {
                loadCollections();
            }
            catch (Exception liteEx) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(liteEx, $"DB file {dbPath} is most likely damaged, trying to repair.");
                Items.Clear();
                liteDb.Dispose();

                var backupPath = dbPath + ".backup";
                File.Copy(dbPath, backupPath, true);

                try
                {
                    var oldData = new Dictionary<string, List<BsonDocument>>();
                    using (var dbStream = File.OpenRead(dbPath))
                    {
                        var reader = new LiteDBConversion.FileReaderV7(dbStream, null);
                        foreach (var coll in reader.GetCollections())
                        {
                            oldData.Add(coll, reader.GetDocuments(coll).ToList());
                        }
                    }

                    File.Delete(dbPath);
                    using (var db = new LiteDatabase($"Filename={dbPath};Mode=Exclusive;Cache Size=0"))
                    {
                        foreach (var collName in oldData.Keys)
                        {
                            db.GetCollection(collName).InsertBulk(oldData[collName]);
                        }
                    }

                    openDb();
                    loadCollections();
                    logger.Debug($"{dbPath} restored successfully.");
                }
                catch (Exception resExc)
                {
                    logger.Error(resExc, "Failed to restore data from damaged db file.");
                    File.Delete(dbPath);
                    File.Move(backupPath, dbPath);
                }
            }
        }

        internal string GetItemFilePath(Guid id)
        {
            return Path.Combine(storagePath, $"{id.ToString()}.json");
        }

        internal void SaveItemData(TItem item)
        {
            liteCollection.Upsert(item);
        }

        internal void SaveItemData(IEnumerable<TItem> items)
        {
            liteCollection.Upsert(items);
        }

        internal TItem GetItemData(Guid id)
        {
            return liteCollection.FindById(id);
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

        public virtual TItem GetOrGenerate(MetadataProperty property)
        {
            if (property is MetadataNameProperty nameProp)
            {
                var existingItem = this.FirstOrDefault(a => GameFieldComparer.StringEquals(a.Name, nameProp.Name));
                if (existingItem != null)
                {
                    return existingItem;
                }
                else
                {
                    return typeof(TItem).CrateInstance<TItem>(nameProp.Name);
                }
            }
            else if (property is MetadataIdProperty idProp)
            {
                return this[idProp.Id];
            }

            throw new NotSupportedException($"{property.GetType()} property type is not supported in this collection.");
        }

        public virtual IEnumerable<TItem> GetOrGenerate(IEnumerable<MetadataProperty> properties)
        {
            var res = new List<TItem>();
            foreach (var property in properties)
            {
                res.Add(GetOrGenerate(property));
            }

            return res;
        }

        public virtual TItem Add(MetadataProperty property)
        {
            if (property is MetadataNameProperty nameProp)
            {
                return Add(nameProp.Name, GameFieldComparer.FieldEquals);
            }
            else if (property is MetadataIdProperty idProp)
            {
                return this[idProp.Id];
            }

            throw new NotSupportedException($"{property.GetType()} property type is not supported in this collection.");
        }

        public virtual IEnumerable<TItem> Add(IEnumerable<MetadataProperty> properties)
        {
            var res = new List<TItem>();
            foreach (var property in properties)
            {
                res.Add(Add(property));
            }

            return res;
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
            return Add(itemName, (existingItem, newName) => existingItem.Name?.Equals(newName, StringComparison.InvariantCultureIgnoreCase) == true);
        }

        public virtual IEnumerable<TItem> Add(List<string> itemsToAdd, Func<TItem, string, bool> existingComparer)
        {
            var res = new List<TItem>();
            var toAdd = new List<TItem>();
            foreach (var itemName in itemsToAdd)
            {
                var existingItem = this.FirstOrDefault(a => existingComparer(a, itemName));
                if (existingItem != null)
                {
                    res.Add(existingItem);
                }
                else
                {
                    var newItem = typeof(TItem).CrateInstance<TItem>(itemName);
                    toAdd.Add(newItem);
                    res.Add(newItem);
                }
            }

            if (toAdd?.Any() == true)
            {
                Add(toAdd);
            }

            return res;
        }

        public virtual IEnumerable<TItem> Add(List<string> itemsToAdd)
        {
            return Add(itemsToAdd, (existingItem, newName) => existingItem.Name?.Equals(newName, StringComparison.InvariantCultureIgnoreCase) == true);
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
                    liteCollection.Delete(id);
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
                        liteCollection.Delete(item.Id);
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
            throw new NotSupportedException();
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

        public IDisposable BufferedUpdate()
        {
            return new EventBufferHandler<TItem>(this);
        }
    }
}
