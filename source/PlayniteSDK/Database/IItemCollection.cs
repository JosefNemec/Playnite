using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
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

    public interface IItemCollection<TItem> : ICollection<TItem> where TItem : DatabaseObject
    {
        TItem this[Guid id] { get; set; }

        TItem Get(Guid id);

        TItem Add(string itemName);

        IEnumerable<TItem> Add(List<string> items);

        void Add(TItem item);

        void Add(IEnumerable<TItem> items);

        bool Remove(Guid id);

        bool Remove(TItem item);

        bool Remove(IEnumerable<TItem> items);

        void Update(TItem item);

        void Update(IEnumerable<TItem> items);

        void BeginBufferUpdate();

        void EndBufferUpdate();

        IEnumerable<TItem> GetClone();

        event EventHandler<ItemCollectionChangedEventArgs<TItem>> ItemCollectionChanged;

        event EventHandler<ItemUpdatedEventArgs<TItem>> ItemUpdated;        
    }
}
