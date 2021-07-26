using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    ///
    /// </summary>
    public class TopPanelItem : ObservableObject
    {
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

        /// <summary>
        /// Called when item is activated.
        /// </summary>
        public Action Activated { get; set; }
    }
}
