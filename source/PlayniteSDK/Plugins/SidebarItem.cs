using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    public enum SiderbarItemType
    {
        Button,
        View
    }

    public abstract class SidebarItem : ObservableObject
    {
        public SiderbarItemType Type { get; set; }

        private object icon;
        public object Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private double progressValue = 0;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private double progressMaximum = 100;
        public double ProgressMaximum
        {
            get => progressMaximum;
            set
            {
                progressMaximum = value;
                OnPropertyChanged();
            }
        }

        private Thickness iconPadding = (Thickness)ResourceProvider.GetResource("SidebarItemPadding");
        public Thickness IconPadding
        {
            get => iconPadding;
            set
            {
                iconPadding = value;
                OnPropertyChanged();
            }
        }

        public virtual void  Activated()
        {
        }

        public virtual Control Opened()
        {
            return null;
        }

        public virtual void Closed()
        {
        }
    }
}
