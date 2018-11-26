using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
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


    // TODO move this to Playnite project
    public class SelectableDbItemList<TItem> : ObservableObject, ICollection<SelectableItem<TItem>> where TItem : DatabaseObject
    {
        public string AsString
        {
            get => ToString();
        }
        
        private List<SelectableItem<TItem>> items;

        public event EventHandler SelectionChanged;

        public SelectableDbItemList(IEnumerable<TItem> collection, IEnumerable<Guid> selected = null)
        {            
            items = new List<SelectableItem<TItem>>(collection.Select(a =>
            {
                var newItem = new SelectableItem<TItem>(a)
                {
                    Selected = selected?.Contains(a.Id) == true
                };

                newItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableItem<TItem>.Selected))
                    {
                        if (!SuppressNotifications)
                        {
                            SelectionChanged?.Invoke(this, EventArgs.Empty);
                            OnPropertyChanged(nameof(AsString));
                        }
                    }
                };

                return newItem;
            }));
        }

        public int Count => items.Count;

        public bool IsReadOnly => true;

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
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<SelectableItem<TItem>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public IEnumerable<Guid> GetSelectedIds()
        {
            return items.Where(a => a.Selected == true).Select(a => a.Item.Id);
        }

        public void SetSelection(IEnumerable<Guid> toSelect)
        {
            SuppressNotifications = true;
            if (toSelect?.Any() == true)
            {                
                //TODO Suppress notifications for every item
                foreach (var item in items)
                {
                    item.Selected = toSelect?.Contains(item.Item.Id) == true;
                }
            }
            else
            {
                items.ForEach(a => a.Selected = false);
            }

            SuppressNotifications = false;
            OnPropertyChanged(nameof(AsString));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return string.Join(", ", this.Where(a => a.Selected == true).Select(a => a.Item.ToString()));
        }
    }
}
