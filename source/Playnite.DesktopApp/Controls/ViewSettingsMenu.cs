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

        private void SetEnumBinding(MenuItem target, string bindingPath, object bindingSource, object bindingEnum)
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

        private void PopulateEnumOptions(ItemCollection parent, Type enumType, string bindingPath, object bindingSource)
        {
            foreach (Enum type in Enum.GetValues(enumType))
            {
                var item = new MenuItem
                {
                    Header = ResourceProvider.GetString(type.GetDescription()),
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

            PopulateEnumOptions(sortItem.Items, typeof(SortOrderDirection), nameof(settings.ViewSettings.SortingOrderDirection), settings.ViewSettings);
            sortItem.Items.Add(new Separator());
            PopulateEnumOptions(sortItem.Items, typeof(SortOrder), nameof(settings.ViewSettings.SortingOrder), settings.ViewSettings);

            // Group By
            var groupItem = new MenuItem
            {
                Header = ResourceProvider.GetString("LOCMenuGroupByTitle")
            };

            PopulateEnumOptions(groupItem.Items, typeof(GroupableField), nameof(settings.ViewSettings.GroupingOrder), settings.ViewSettings);

            Items.Add(sortItem);
            Items.Add(groupItem);
            Items.Add(new Separator());                

            // View Type
            PopulateEnumOptions(Items, typeof(ViewType), nameof(settings.ViewSettings.GamesViewType), settings.ViewSettings);
        }
    }
}
