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
        Button = 0,
        /// <summary>
        /// View item style.
        /// </summary>
        View = 1
    }

    /// <summary>
    /// Represents sidebar API object.
    /// </summary>
    public class SidebarItem : ObservableObject
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

        private bool visible = true;
        /// <summary>
        /// Gets or sets item visibility.
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
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
        /// Gets or sets maximum progress value.
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

        private Thickness iconPadding = new Thickness(8);
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
        /// Called when item is activated.
        /// </summary>
        public Action Activated { get; set; }

        /// <summary>
        /// Called when view is to be opened.
        /// </summary>
        /// <returns>View control to be shown.</returns>
        public Func<Control> Opened { get; set; }

        /// <summary>
        /// Called when view is closed.
        /// </summary>
        public Action Closed { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="SidebarItem"/>.
        /// </summary>
        public SidebarItem()
        {
            if (ResourceProvider.GetResource("SidebarItemPadding") is Thickness thick)
            {
                iconPadding = thick;
            }
        }
    }
}
