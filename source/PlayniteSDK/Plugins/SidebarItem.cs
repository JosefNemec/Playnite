using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Sidebar item type.
    /// </summary>
    public enum SiderbarItemType
    {
        /// <summary>
        /// Button item style.
        /// </summary>
        Button,
        /// <summary>
        /// View item style.
        /// </summary>
        View
    }

    /// <summary>
    /// Represents sidebar API object.
    /// </summary>
    public abstract class SidebarItem : ObservableObject
    {
        /// <summary>
        /// Gets or sets item type.
        /// </summary>
        public SiderbarItemType Type { get; set; }

        private object icon;
        /// <summary>
        /// Gets or sets item icon.
        /// </summary>
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
        /// <summary>
        /// Gets or sets item title.
        /// </summary>
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
        /// <summary>
        /// Gets or sets current progress value.
        /// </summary>
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
        /// <summary>
        /// Gets or sets maximum progress value;
        /// </summary>
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
        /// <summary>
        /// Gets or sets visual item padding.
        /// </summary>
        public Thickness IconPadding
        {
            get => iconPadding;
            set
            {
                iconPadding = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Called when Button item type is activated.
        /// </summary>
        public virtual void  Activated()
        {
        }

        /// <summary>
        /// Called when View item type is activated.
        /// </summary>
        /// <returns>View control to be shown.</returns>
        public virtual Control Opened()
        {
            return null;
        }

        /// <summary>
        /// Called when View item type is closed.
        /// </summary>
        public virtual void Closed()
        {
        }
    }
}
