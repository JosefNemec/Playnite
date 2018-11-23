using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
