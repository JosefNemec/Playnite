using Playnite;
using Playnite.SDK;
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

    public class SelectableObjectList<TItem> : ObservableObject, ICollection<SelectableItem<TItem>>, INotifyCollectionChanged
    {
        internal readonly List<SelectableItem<TItem>> Items;

        public int Count => Items.Count;

        public bool IsReadOnly => true;

        public string AsString => ToString();

        public event EventHandler SelectionChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SelectableObjectList(
            IEnumerable<TItem> collection,
            IEnumerable<TItem> selected = null)
        {
            Items = new List<SelectableItem<TItem>>(collection.Select(a =>
            {
                var newItem = new SelectableItem<TItem>(a);         
                newItem.Selected = selected?.Contains(a) == true;
                newItem.PropertyChanged += NewItem_PropertyChanged;
                return newItem;
            }));
        }

        internal virtual void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(AsString));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void NewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableItem<TItem>.Selected))
            {
                if (!SuppressNotifications)
                {
                    OnSelectionChanged();
                }
            }
        }

        public void SetSelection(IEnumerable<TItem> toSelect)
        {
            SuppressNotifications = true;
            if (toSelect?.Any() == true)
            {
                foreach (var item in Items)
                {
                    item.Selected = toSelect?.Contains(item.Item) == true;
                }
            }
            else
            {
                Items.ForEach(a => a.Selected = false);
            }

            SuppressNotifications = false;
            OnSelectionChanged();
        }

        public List<TItem> GetSelectedItems()
        {
            return Items.Where(a => a.Selected == true).Select(a => a.Item).ToList();
        }

        public void Add(TItem item, bool selected = false)
        {
            if (Items.Any(a => a.Item.Equals(item)))
            {
                return;
            }

            var newItem = new SelectableItem<TItem>(item)
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

        public void Add(SelectableItem<TItem> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(SelectableItem<TItem> item)
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

    public class SelectableStringList : SelectableObjectList<string>
    {
        public SelectableStringList(
            IEnumerable<string> collection,
            IEnumerable<string> selected = null,
            bool includeNoneItem = false) : base(collection, selected)
        {
            if (includeNoneItem)
            {
                var newItem = new SelectableItem<string>(FilterSettings.MissingFieldString);
                newItem.PropertyChanged += NewItem_PropertyChanged;
                Items.Insert(0, newItem);
            }
        }
    }

    public abstract class SelectableIdItemList : ObservableObject
    {
        public event EventHandler SelectionChanged;
        public abstract string AsString { get; }
        public abstract List<Guid> GetSelectedIds();
        public abstract void SetSelection(IEnumerable<Guid> toSelect);

        internal virtual void OnSelectionChanged()
        {
            OnPropertyChanged(nameof(AsString));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SelectableIdItemList<TItem> : SelectableIdItemList, ICollection<SelectableItem<TItem>>
    {
        private readonly Func<TItem, Guid> idSelector;
        internal readonly List<SelectableItem<TItem>> Items;

        public override string AsString
        {
            get => ToString();
        }

        public SelectableIdItemList(
            IEnumerable<TItem> collection,
            Func<TItem, Guid> idSelector,
            IEnumerable<Guid> selected = null,
            IEnumerable<Guid> undetermined = null)
        {
            this.idSelector = idSelector;
            Items = new List<SelectableItem<TItem>>(collection.Select(a =>
            {
                var newItem = new SelectableItem<TItem>(a);
                if (selected?.Contains(idSelector(a)) == true)
                {
                    newItem.Selected = true;
                }
                else if (undetermined?.Contains(idSelector(a)) == true)
                {
                    newItem.Selected = null;
                }
                else
                {
                    newItem.Selected = false;
                }

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

    public class SelectableDbItemList : SelectableIdItemList<DatabaseObject>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SelectableDbItemList(
            IEnumerable<DatabaseObject> collection,
            IEnumerable<Guid> selected = null,
            IEnumerable<Guid> undetermined = null,
            bool includeNoneItem = false)
            : base(collection.OrderBy(a => a.Name), (a) => a.Id, selected, undetermined)
        {
            if (includeNoneItem)
            {
                var newItem = new SelectableItem<DatabaseObject>(new DatabaseObject()
                {
                    Id = Guid.Empty,
                    Name = ResourceProvider.GetString("LOCNone")
                })
                {
                    Selected = selected?.Contains(Guid.Empty) == true
                };
                newItem.PropertyChanged += NewItem_PropertyChanged;
                Items.Insert(0, newItem);
            }
        }

        // TODO keep ordering when item is added or removed
        public void Add(DatabaseObject item, bool selected = false)
        {
            var existing = Items.FirstOrDefault(a => a.Item.Id == item.Id);
            if (existing != null)
            {
                if (selected)
                {
                    existing.Selected = true;
                }
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

    public class SelectableLibraryPluginList : SelectableIdItemList<LibraryPlugin>
    {
        public SelectableLibraryPluginList(IEnumerable<LibraryPlugin> collection, IEnumerable<Guid> selected = null)
            : base(collection, (a) => a.Id, selected)
        {
        }

        public override string ToString()
        {
            return string.Join(", ", this.Where(a => a.Selected == true).Select(a => a.Item.Name));
        }
    }
}
