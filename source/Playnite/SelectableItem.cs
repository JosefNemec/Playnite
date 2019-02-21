using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class SelectableItem<TItem> : ObservableObject
    {
        private bool? selected = false;
        public bool? Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        private TItem item;
        public TItem Item
        {
            get => item;
            set
            {
                item = value;
                OnPropertyChanged();
            }
        }

        public SelectableItem(TItem item)
        {
            this.item = item;
        }

        public override string ToString()
        {
            return Item?.ToString() ?? base.ToString();
        }
    }

    public abstract class SelectableItemList : ObservableObject
    {
        public event EventHandler SelectionChanged;
        public abstract string AsString { get; }
        public abstract List<Guid> GetSelectedIds();
        public abstract void SetSelection(IEnumerable<Guid> toSelect);

        internal void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(AsString));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SelectableItemList<TItem> : SelectableItemList, ICollection<SelectableItem<TItem>>
    {
        private readonly Func<TItem, Guid> idSelector;
        internal readonly List<SelectableItem<TItem>> Items;

        public override string AsString
        {
            get => ToString();
        }

        public SelectableItemList(IEnumerable<TItem> collection, Func<TItem, Guid> idSelector, IEnumerable<Guid> selected = null)
        {
            this.idSelector = idSelector;
            Items = new List<SelectableItem<TItem>>(collection.Select(a =>
            {
                var newItem = new SelectableItem<TItem>(a)
                {
                    Selected = selected?.Contains(idSelector(a)) == true
                };

                newItem.PropertyChanged += NewItem_PropertyChanged;
                return newItem;
            }));
        }

        internal void NewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableItem<DatabaseObject>.Selected))
            {
                if (!SuppressNotifications)
                {
                    OnSelectionChanged();
                }
            }
        }

        public override void SetSelection(IEnumerable<Guid> toSelect)
        {
            SuppressNotifications = true;
            if (toSelect?.Any() == true)
            {
                foreach (var item in Items)
                {
                    item.Selected = toSelect?.Contains(idSelector(item.Item)) == true;
                }
            }
            else
            {
                Items.ForEach(a => a.Selected = false);
            }

            SuppressNotifications = false;
            OnSelectionChanged();
        }

        public override List<Guid> GetSelectedIds()
        {
            return Items.Where(a => a.Selected == true).Select(a => idSelector(a.Item)).ToList();
        }

        public int Count => Items.Count;

        public bool IsReadOnly => true;

        public void Add(SelectableItem<TItem> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(SelectableItem<TItem> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(SelectableItem<TItem>[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(SelectableItem<TItem> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SelectableItem<TItem>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", this.Where(a => a.Selected == true).Select(a => a.Item.ToString()));
        }
    }

    public class SelectableDbItemList : SelectableItemList<DatabaseObject>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SelectableDbItemList(IEnumerable<DatabaseObject> collection, IEnumerable<Guid> selected = null)
            : base(collection.OrderBy(a => a.Name), (a) => a.Id, selected)
        {
        }

        // TODO keep ordering when item is added or removed
        public void Add(DatabaseObject item, bool selected = false)
        {
            if (Items.FirstOrDefault(a => a.Item.Id == item.Id) != null)
            {
                throw new Exception("Item is already part of the collection.");
            }

            var newItem = new SelectableItem<DatabaseObject>(item)
            {
                Selected = selected
            };
            newItem.PropertyChanged += NewItem_PropertyChanged;
            Items.Add(newItem);            
            OnCollectionChanged();
            if (selected)
            {
                OnSelectionChanged();
            }
        }

        public bool Remove(DatabaseObject item)
        {
            var listItem = Items.FirstOrDefault(a => a.Item.Id == item.Id);
            if (listItem == null)
            {
                return false;
            }
            else
            {
                listItem.PropertyChanged -= NewItem_PropertyChanged;
                Items.Remove(listItem);
                if (listItem.Selected == true)
                {
                    OnSelectionChanged();
                }

                OnCollectionChanged();
                return true;
            }
        }

        private void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public override string ToString()
        {
            return string.Join(", ", this.Where(a => a.Selected == true).Select(a => a.Item.Name));
        }
    }

    public class SelectableLibraryPluginList : SelectableItemList<ILibraryPlugin>
    {
        public SelectableLibraryPluginList(IEnumerable<ILibraryPlugin> collection, IEnumerable<Guid> selected = null)
            : base(collection, (a) => a.Id, selected)
        {
        }

        public override string ToString()
        {
            return string.Join(", ", this.Where(a => a.Selected == true).Select(a => a.Item.Name));
        }
    }
}
