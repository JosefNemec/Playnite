using Playnite.Common;
using Playnite.Converters;
using Playnite.Extensions.Markup;
using Playnite.SDK;
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
        }

        private void SetEnumBinding(
            MenuItem target,
            string bindingPath,
            object bindingSource,
            object bindingEnum)
        {
            BindingOperations.SetBinding(target, MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = bindingSource,
                    Path = new PropertyPath(bindingPath),
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = bindingEnum
                });
        }

        private void PopulateEnumOptions<T>(
            ItemCollection parent,
            string bindingPath,
            object bindingSource,
            bool sorted = false,
            List<T> ignoreValues = null) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            if (sorted)
            {
                values = values.OrderBy(a => a.GetDescription());
            }

            foreach (T type in values)
            {
                if (ignoreValues?.Contains(type) == true)
                {
                    continue;
                }

                var item = new MenuItem
                {
                    Header = type.GetDescription(),
                    IsCheckable = true
                };

                SetEnumBinding(item, bindingPath, bindingSource, type);
                parent.Add(item);
            }
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

            PopulateEnumOptions<SortOrderDirection>(sortItem.Items, nameof(settings.ViewSettings.SortingOrderDirection), settings.ViewSettings);
            sortItem.Items.Add(new Separator());
            PopulateEnumOptions<SortOrder>(sortItem.Items, nameof(settings.ViewSettings.SortingOrder), settings.ViewSettings, true);

            // Group By
            var groupItem = new MenuItem
            {
                Header = ResourceProvider.GetString("LOCMenuGroupByTitle")
            };

            var dontGroupItem = MainMenu.AddMenuChild(groupItem.Items, GroupableField.None.GetDescription(), null);
            dontGroupItem.IsCheckable = true;
            SetEnumBinding(dontGroupItem, nameof(settings.ViewSettings.GroupingOrder), settings.ViewSettings, GroupableField.None);
            groupItem.Items.Add(new Separator());
            PopulateEnumOptions<GroupableField>(groupItem.Items, nameof(settings.ViewSettings.GroupingOrder), settings.ViewSettings, true,
                new List<GroupableField> { GroupableField.None });

            Items.Add(sortItem);
            Items.Add(groupItem);
            Items.Add(new Separator());

            // View Type
            PopulateEnumOptions<ViewType>(Items, nameof(settings.ViewSettings.GamesViewType), settings.ViewSettings);
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
