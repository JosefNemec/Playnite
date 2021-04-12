using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml.Linq;

namespace Playnite.FullscreenApp.Controls
{
    [TemplatePart(Name = "PART_ItemsFilterPresets", Type = typeof(ItemsControl))]
    public class FilterPresetSelector : Control
    {
        private readonly FullscreenAppViewModel mainModel;
        private ItemsControl ItemsFilterPresets;

        static FilterPresetSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterPresetSelector), new FrameworkPropertyMetadata(typeof(FilterPresetSelector)));
        }

        public FilterPresetSelector() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public FilterPresetSelector(FullscreenAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Template == null)
            {
                return;
            }

            ItemsFilterPresets = Template.FindName("PART_ItemsFilterPresets", this) as ItemsControl;
            if (ItemsFilterPresets != null)
            {
                ScrollToSelectedBehavior.SetEnabled(ItemsFilterPresets, true);
                ItemsFilterPresets.SetResourceReference(ListBox.ItemContainerStyleProperty, "ItemFilterQuickPreset");
                BindingTools.SetBinding(ItemsFilterPresets,
                    ListBox.ItemsSourceProperty,
                    mainModel,
                    nameof(mainModel.SortedFilterFullscreenPresets));
                BindingTools.SetBinding(ItemsFilterPresets,
                    ListBox.SelectedItemProperty,
                    mainModel,
                    nameof(mainModel.ActiveFilterPreset),
                    mode: BindingMode.TwoWay);

                ItemsFilterPresets.PreviewMouseLeftButtonUp += ItemsFilterPresets_PreviewMouseLeftButtonUp;
            }
        }

        private void ItemsFilterPresets_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender is ListBox))
            {
                return;
            }

            if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                if (item.DataContext is FilterPreset preset)
                {
                    mainModel.ActiveFilterPreset = preset;
                }
            }
        }
    }
}
