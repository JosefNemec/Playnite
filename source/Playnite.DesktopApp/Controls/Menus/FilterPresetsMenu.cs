using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls
{
    public class FilterPresetsMenu : ContextMenu
    {
        private readonly DesktopAppViewModel mainModel;

        static FilterPresetsMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterPresetsMenu), new FrameworkPropertyMetadata(typeof(FilterPresetsMenu)));
        }

        public FilterPresetsMenu() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public FilterPresetsMenu(DesktopAppViewModel model)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (model != null)
            {
                mainModel = model;
                Opened += FilterPresetsMenu_Opened;
            }

            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            StaysOpen = false;
        }

        private void FilterPresetsMenu_Opened(object sender, RoutedEventArgs e)
        {
            Items.Clear();
            foreach (var preset in mainModel.SortedFilterPresets)
            {
                var item = new MenuItem
                {
                    Header = preset.Name,
                    Command = mainModel.ApplyFilterPresetCommand,
                    CommandParameter = preset
                };

                BindingTools.SetBinding(item,
                    MenuItem.IsCheckedProperty,
                    mainModel,
                    nameof(mainModel.ActiveFilterPreset),
                    converter: new ObjectEqualityToBoolConverter(),
                    converterParameter: preset,
                    mode: System.Windows.Data.BindingMode.OneWay);
                Items.Add(item);
            }
        }
    }
}
