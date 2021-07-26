using Playnite.Common;
using Playnite.DesktopApp.Markup;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.ViewModels
{
    public class TopPanelWrapperItem : ObservableObject
    {
        private DesktopAppViewModel model;
        public TopPanelItem PanelItem { get; }
        public RelayCommand<object> Command { get; set; }

        public string Title
        {
            get => PanelItem.Title;
        }

        public bool Visible
        {
            get => PanelItem.Visible;
            set { }
        }

        public object IconObject => SdkHelpers.ResolveUiItemIcon(PanelItem.Icon);

        public TopPanelWrapperItem(TopPanelItem item, DesktopAppViewModel model)
        {
            this.model = model;
            PanelItem = item;
            Command = new RelayCommand<object>((_) => PanelItem.Activated?.Invoke());
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }

    public partial class DesktopAppViewModel
    {
        public List<TopPanelWrapperItem> GetTopPanelPluginItems()
        {
            var newItems = new List<TopPanelWrapperItem>();
            foreach (var item in Extensions.GetTopPanelPluginItems())
            {
                newItems.Add(new TopPanelWrapperItem(item, this));
            }

            return newItems;
        }
    }
}
