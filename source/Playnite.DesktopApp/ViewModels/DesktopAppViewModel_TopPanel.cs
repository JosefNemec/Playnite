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
        private static readonly ILogger logger = LogManager.GetLogger();

        private DesktopAppViewModel model;
        public TopPanelItem PanelItem { get; }
        public RelayCommandBase Command { get; set; }

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
            Command = new RelayCommand(() =>
            {
                try
                {
                    PanelItem.Activated?.Invoke();
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to run top panel extension button action.");
                }
            });
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
