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
    public class SelectableDbItemList : ObservableObject, ICollection<SelectableItem<DatabaseObject>>
    {
        public string AsString
        {
            get => ToString();
        }
        
        private List<SelectableItem<DatabaseObject>> items;

        public event EventHandler SelectionChanged;

        public SelectableDbItemList(IEnumerable<DatabaseObject> collection, IEnumerable<Guid> selected = null)
        {            
            items = new List<SelectableItem<DatabaseObject>>(collection.Select(a =>
            {
                var newItem = new SelectableItem<DatabaseObject>(a)
                {
                    Selected = selected?.Contains(a.Id) == true
                };

                newItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableItem<DatabaseObject>.Selected))
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

        public void Add(SelectableItem<DatabaseObject> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(SelectableItem<DatabaseObject> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(SelectableItem<DatabaseObject> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(SelectableItem<DatabaseObject>[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<SelectableItem<DatabaseObject>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public List<Guid> GetSelectedIds()
        {
            return items.Where(a => a.Selected == true).Select(a => a.Item.Id).ToList();
        }

        public void SetSelection(IEnumerable<Guid> toSelect)
        {
            SuppressNotifications = true;
            if (toSelect?.Any() == true)
            {
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
