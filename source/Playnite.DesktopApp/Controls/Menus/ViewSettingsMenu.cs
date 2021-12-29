using Playnite.Common;
using Playnite.Converters;
using Playnite.Extensions.Markup;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class ViewSettingsMenu : ContextMenu
    {
        private readonly PlayniteSettings settings;

        static ViewSettingsMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ViewSettingsMenu), new FrameworkPropertyMetadata(typeof(ViewSettingsMenu)));
        }

        public ViewSettingsMenu() : this(PlayniteApplication.Current?.AppSettings)
        {
        }

        public ViewSettingsMenu(PlayniteSettings settings)
        {
            this.settings = settings;
            InitializeItems();
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            StaysOpen = false;
        }

        public static void GenerateSortMenu(ItemCollection itemsRoot, PlayniteSettings settings)
        {
            MenuHelpers.PopulateEnumOptions<SortOrderDirection>(itemsRoot, nameof(settings.ViewSettings.SortingOrderDirection), settings.ViewSettings);
            itemsRoot.Add(new Separator());
            MenuHelpers.PopulateEnumOptions<SortOrder>(itemsRoot, nameof(settings.ViewSettings.SortingOrder), settings.ViewSettings, true);
        }

        public static void GenerateGroupMenu(ItemCollection itemsRoot, PlayniteSettings settings)
        {
            var dontGroupItem = MainMenu.AddMenuChild(itemsRoot, GroupableField.None.GetDescription(), null);
            dontGroupItem.IsCheckable = true;
            MenuHelpers.SetEnumBinding(dontGroupItem, nameof(settings.ViewSettings.GroupingOrder), settings.ViewSettings, GroupableField.None);
            itemsRoot.Add(new Separator());
            MenuHelpers.PopulateEnumOptions<GroupableField>(itemsRoot, nameof(settings.ViewSettings.GroupingOrder), settings.ViewSettings, true,
                new List<GroupableField> { GroupableField.None });
        }

        public void InitializeItems()
        {
            if (settings == null)
            {
                return;
            }

            Items.Clear();

            // Sort By
            var sortItem = new MenuItem
            {
                Header = ResourceProvider.GetString("LOCMenuSortByTitle")
            };
            GenerateSortMenu(sortItem.Items, settings);

            // Group By
            var groupItem = new MenuItem
            {
                Header = ResourceProvider.GetString("LOCMenuGroupByTitle")
            };
            GenerateGroupMenu(groupItem.Items, settings);
            Items.Add(sortItem);
            Items.Add(groupItem);
            Items.Add(new Separator());

            // View Type
            MenuHelpers.PopulateEnumOptions<DesktopView>(Items, nameof(settings.ViewSettings.GamesViewType), settings.ViewSettings);
            Items.Add(new Separator());

            // View
            var filterItem = MainMenu.AddMenuChild(Items, "LOCMenuViewFilterPanel", null);
            filterItem.IsCheckable = true;
            BindingOperations.SetBinding(filterItem, MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = settings,
                    Path = new PropertyPath(nameof(PlayniteSettings.FilterPanelVisible))
                });

            var explorerItem = MainMenu.AddMenuChild(Items, "LOCMenuViewExplorerPanel", null);
            explorerItem.IsCheckable = true;
            BindingOperations.SetBinding(explorerItem, MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = settings,
                    Path = new PropertyPath(nameof(PlayniteSettings.ExplorerPanelVisible))
                });
        }
    }
}
