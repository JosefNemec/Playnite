using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents event occuring when database are permanetly updated in database.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ItemUpdateEvent<TItem> where TItem : DatabaseObject
    {
        /// <summary>
        /// Gets or sets old item state.
        /// </summary>
        public TItem OldData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets new item state.
        /// </summary>
        public TItem NewData
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of ItemUpdateEvent.
        /// </summary>
        /// <param name="oldData">Old state.</param>
        /// <param name="newData">New state.</param>
        public ItemUpdateEvent(TItem oldData, TItem newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    /// <summary>
    /// Represents arguments for collection update events.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ItemUpdatedEventArgs<TItem> : EventArgs where TItem : DatabaseObject
    {
        /// <summary>
        /// Gets or sets list of update events.
        /// </summary>
        public List<ItemUpdateEvent<TItem>> UpdatedItems
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of ItemUpdatedEventArgs.
        /// </summary>
        /// <param name="oldData">Old item state.</param>
        /// <param name="newData">New item state.</param>
        public ItemUpdatedEventArgs(TItem oldData, TItem newData)
        {
            UpdatedItems = new List<ItemUpdateEvent<TItem>>() { new ItemUpdateEvent<TItem>(oldData, newData) };
        }

        /// <summary>
        /// Creates new instance of ItemUpdatedEventArgs.
        /// </summary>
        /// <param name="updatedItems">Update events list.</param>
        public ItemUpdatedEventArgs(IEnumerable<ItemUpdateEvent<TItem>> updatedItems)
        {
            UpdatedItems = updatedItems.ToList();
        }
    }

    /// <summary>
    /// Represents arguments for collection change events.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ItemCollectionChangedEventArgs<TItem> : EventArgs where TItem : DatabaseObject
    {
        /// <summary>
        /// Gets or sets list of added items.
        /// </summary>
        public List<TItem> AddedItems
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets list of removed items.
        /// </summary>
        public List<TItem> RemovedItems
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of ItemCollectionChangedEventArgs.
        /// </summary>
        /// <param name="addedItems">List of added items.</param>
        /// <param name="removedItems">List of removed items.</param>
        public ItemCollectionChangedEventArgs(List<TItem> addedItems, List<TItem> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }

    /// <summary>
    /// Describes collection of items for game database.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface IItemCollection<TItem> : ICollection<TItem> where TItem : DatabaseObject
    {
        /// <summary>
        /// Gets or sets item from collection.
        /// </summary>
        /// <param name="id">Id of an item.</param>
        /// <returns><c>null</c> if no item is found otherwise item represents by specified id.</returns>
        TItem this[Guid id] { get; set; }

        /// <summary>
        /// Gets item from collection.
        /// </summary>
        /// <param name="id">Id of an item.</param>
        /// <returns><c>null</c> if no item is found otherwise item represents by specified id.</returns>
        TItem Get(Guid id);

        /// <summary>
        /// Adds new item into collection.
        /// </summary>
        /// <param name="itemName">Name of new item.</param>
        /// <returns>Newly added item or existing item if one is present with the same name.</returns>
        TItem Add(string itemName);

        /// <summary>
        /// Adds new item into collection.
        /// </summary>
        /// <param name="itemName">Name of new item.</param>
        /// <param name="existingComparer">Method to detect existing item from database compared to new item.</param>
        /// <returns>Newly added item or existing item if one is present with the same name.</returns>
        TItem Add(string itemName, Func<TItem, string, bool> existingComparer);

        /// <summary>
        /// Adds new items into collection.
        /// </summary>
        /// <param name="items">Names of items to be added.</param>
        /// <returns>Newly added items or existing items if there are some present with the same names.</returns>
        IEnumerable<TItem> Add(List<string> items);

        /// <summary>
        /// Adds new items into collection.
        /// </summary>
        /// <param name="items">Names of items to be added.</param>
        /// <param name="existingComparer">Method to detect existing item from database compared to new item.</param>
        /// <returns></returns>
        IEnumerable<TItem> Add(List<string> items, Func<TItem, string, bool> existingComparer);

        /// <summary>
        /// Adds itemss to into collection.
        /// </summary>
        /// <param name="items">Item to be added.</param>
        void Add(IEnumerable<TItem> items);

        /// <summary>
        /// Removes item from collection.
        /// </summary>
        /// <param name="id">Id of an item to be removed.</param>
        /// <returns></returns>
        bool Remove(Guid id);

        /// <summary>
        /// Removes items from collection.
        /// </summary>
        /// <param name="items">List of items to be removed.</param>
        /// <returns></returns>
        bool Remove(IEnumerable<TItem> items);

        /// <summary>
        /// Updates state of item in collection.
        /// </summary>
        /// <param name="item">New state of an object.</param>
        void Update(TItem item);

        /// <summary>
        /// Updates states of items in collection.
        /// </summary>
        /// <param name="items">New states of items.</param>
        void Update(IEnumerable<TItem> items);

        /// <summary>
        /// Sets collection into buffered update state.
        /// </summary>
        void BeginBufferUpdate();

        /// <summary>
        /// Sets collection from buffered update state.
        /// </summary>
        void EndBufferUpdate();

        /// <summary>
        /// Gets clone of an collection.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TItem> GetClone();

        /// <summary>
        /// Occurs when items are added or removed.
        /// </summary>
        event EventHandler<ItemCollectionChangedEventArgs<TItem>> ItemCollectionChanged;

        /// <summary>
        /// Occurs when items are updated.
        /// </summary>
        event EventHandler<ItemUpdatedEventArgs<TItem>> ItemUpdated;
    }
}
