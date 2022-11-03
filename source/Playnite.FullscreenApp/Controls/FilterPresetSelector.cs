using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.SDK;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

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
                mainModel.PropertyChanged += MainModel_PropertyChanged;
            }
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ItemsFilterPresets == null)
            {
                return;
            }

            if (e.PropertyName == nameof(MainViewModelBase.SortedFilterFullscreenPresets))
            {
                ItemsFilterPresets.Items.Clear();
                foreach (var preset in mainModel.SortedFilterFullscreenPresets)
                {
                    var item = new CheckBoxEx
                    {
                        Style = ResourceProvider.GetResource<Style>("ItemFilterQuickPreset"),
                        Command = mainModel.ApplyFilterPresetCommand,
                        CommandParameter = preset,
                        DataContext = preset
                    };

                    BindingTools.SetBinding(item,
                         AutomationProperties.NameProperty,
                         preset,
                         nameof(preset.Name));
                     BindingTools.SetBinding(item,
                        CheckBox.IsCheckedProperty,
                        mainModel,
                        nameof(mainModel.ActiveFilterPreset),
                        converter: new Converters.ObjectEqualityToBoolConverter(),
                        converterParameter: preset,
                        mode: BindingMode.OneWay);
                    ItemsFilterPresets.Items.Add(item);
                }
            }

            if (e.PropertyName == nameof(MainViewModelBase.ActiveFilterPreset) && mainModel.ActiveFilterPreset != null)
            {
                foreach (CheckBoxEx item in ItemsFilterPresets.Items)
                {
                    if (item.DataContext == mainModel.ActiveFilterPreset)
                    {
                        ItemsFilterPresets.ScrollIntoView(item);
                        break;
                    }
                }
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
        }
    }
}
