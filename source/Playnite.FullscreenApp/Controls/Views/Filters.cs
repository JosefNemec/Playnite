using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
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

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_PanelItemsHost", Type = typeof(Panel))]
    public class Filters : Control
    {
        private FullscreenAppViewModel mainModel;
        private Panel PanelItemsHost;

        static Filters()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Filters), new FrameworkPropertyMetadata(typeof(Filters)));
        }

        public Filters() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public Filters(FullscreenAppViewModel mainModel) : base()
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

            PanelItemsHost = Template.FindName("PART_PanelItemsHost", this) as Panel;
            PanelItemsHost.Focusable = false;

            var ButtonClear = new ButtonEx();
            ButtonClear.Command = mainModel.ClearFiltersCommand;
            ButtonClear.Content = ResourceProvider.GetString(LOC.ClearLabel);
            ButtonClear.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelButtonEx");
            PanelItemsHost.Children.Add(ButtonClear);
            BindingTools.SetBinding(ButtonClear,
                    FocusBahaviors.FocusBindingProperty,
                    mainModel,
                    nameof(mainModel.FilterPanelVisible));

            AssignBoolFilter(nameof(FilterSettings.IsInstalled), LOC.GameIsInstalledTitle);
            AssignBoolFilter(nameof(FilterSettings.IsUnInstalled), LOC.GameIsUnInstalledTitle);
            AssignBoolFilter(nameof(FilterSettings.Favorite), LOC.GameHiddenTitle);
            AssignBoolFilter(nameof(FilterSettings.Hidden), LOC.GameFavoriteTitle);
            AssignFilter(GameField.PluginId, nameof(FilterSettings.Library), LOC.Library);
            AssignFilter(GameField.Platform, nameof(FilterSettings.Platform), LOC.PlatformTitle);
            AssignFilter(GameField.Categories, nameof(FilterSettings.Category), LOC.CategoryLabel);

            var ButtonAdditional = new ButtonEx();
            ButtonAdditional.Command = mainModel.OpenAdditionalFiltersCommand;
            ButtonAdditional.Content = ResourceProvider.GetString(LOC.AditionalFilters);
            ButtonAdditional.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelNagivationButton");
            BindingTools.SetBinding(
                ButtonAdditional,
                ButtonBase.TagProperty,
                mainModel.AppSettings.Fullscreen.FilterSettings,
                nameof(FullscreenFilterSettings.IsSubAdditionalFilterActive));
            PanelItemsHost.Children.Add(ButtonAdditional);

            var desc = new TextBlock();
            desc.Text = ResourceProvider.GetString(LOC.MenuSortByTitle);
            desc.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelText");
            PanelItemsHost.Children.Add(desc);

            var SelectSortBy = new ComboBoxEx();
            SelectSortBy.SetResourceReference(ComboBoxEx.StyleProperty, "FilterPanelComboBoxEx");
            SelectSortBy.ItemsSource = ItemsSource.GetEnumSources(typeof(SortOrder));
            SelectSortBy.DisplayMemberPath = nameof(ItemsSource.EnumItem.Name);
            SelectSortBy.SelectedValuePath = nameof(ItemsSource.EnumItem.Value);
            BindingTools.SetBinding(
                SelectSortBy,
                Selector.SelectedValueProperty,
                mainModel.AppSettings.Fullscreen.ViewSettings,
                nameof(FullscreenViewSettings.SortingOrder),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
            PanelItemsHost.Children.Add(SelectSortBy);

            var SelectSortDirection = new ComboBoxEx();
            SelectSortDirection.SetResourceReference(ComboBoxEx.StyleProperty, "FilterPanelComboBoxEx");
            SelectSortDirection.ItemsSource = ItemsSource.GetEnumSources(typeof(SortOrderDirection));
            SelectSortDirection.DisplayMemberPath = nameof(ItemsSource.EnumItem.Name);
            SelectSortDirection.SelectedValuePath = nameof(ItemsSource.EnumItem.Value);
            BindingTools.SetBinding(
                SelectSortDirection,
                Selector.SelectedValueProperty,
                mainModel.AppSettings.Fullscreen.ViewSettings,
                nameof(FullscreenViewSettings.SortingOrderDirection),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
            PanelItemsHost.Children.Add(SelectSortDirection);

            desc = new TextBlock();
            desc.Text = ResourceProvider.GetString(LOC.TopPanelFilterPresets);
            desc.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelText");
            PanelItemsHost.Children.Add(desc);

            var ComboFilterPresets = new ComboBoxEx();
            ComboFilterPresets.SetResourceReference(ComboBoxEx.StyleProperty, "FilterPanelComboBoxEx");
            BindingTools.SetBinding(ComboFilterPresets,
                ComboBox.ItemsSourceProperty,
                mainModel,
                nameof(mainModel.SortedFilterPresets));
            BindingTools.SetBinding(ComboFilterPresets,
                ComboBox.SelectedItemProperty,
                mainModel,
                nameof(mainModel.ActiveFilterPreset),
                mode: BindingMode.TwoWay);
            ComboFilterPresets.DisplayMemberPath = nameof(FilterPreset.Name);
            PanelItemsHost.Children.Add(ComboFilterPresets);

            var ButtonSaveFilter = new ButtonEx();
            BindingTools.SetBinding(ButtonSaveFilter,
                ButtonBase.CommandProperty,
                mainModel,
                nameof(mainModel.AddFilterPresetCommand));
            ButtonSaveFilter.SetResourceReference(ButtonEx.ContentTemplateProperty, "FilterPanelAddPresetTemplate");
            ButtonSaveFilter.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelFilterPresetActionButton");

            var ButtonRenameFilter = new ButtonEx();
            BindingTools.SetBinding(ButtonRenameFilter,
                ButtonBase.CommandProperty,
                mainModel,
                nameof(mainModel.RenameFilterPresetCommand));
            BindingTools.SetBinding(ButtonRenameFilter,
                ButtonBase.CommandParameterProperty,
                mainModel,
                nameof(mainModel.ActiveFilterPreset));
            ButtonRenameFilter.SetResourceReference(ButtonEx.ContentTemplateProperty, "FilterPanelRenamePresetTemplate");
            ButtonRenameFilter.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelFilterPresetActionButton");

            var ButtonDeleteFilter = new ButtonEx();
            BindingTools.SetBinding(ButtonDeleteFilter,
                ButtonBase.CommandProperty,
                mainModel,
                nameof(mainModel.RemoveFilterPresetCommand));
            BindingTools.SetBinding(ButtonDeleteFilter,
                ButtonBase.CommandParameterProperty,
                mainModel,
                nameof(mainModel.ActiveFilterPreset));
            ButtonDeleteFilter.SetResourceReference(ButtonEx.ContentTemplateProperty, "FilterPanelRemovePresetTemplate");
            ButtonDeleteFilter.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelFilterPresetActionButton");

            var filterButtonGrid = new UniformGrid { Columns = 3 };
            filterButtonGrid.Children.Add(ButtonSaveFilter);
            filterButtonGrid.Children.Add(ButtonRenameFilter);
            filterButtonGrid.Children.Add(ButtonDeleteFilter);
            PanelItemsHost.Children.Add(filterButtonGrid);
        }

        private void AssignBoolFilter(string bindBased, string text)
        {
            var check = new CheckBoxEx();
            check.Content = ResourceProvider.GetString(text);
            check.SetResourceReference(CheckBoxEx.StyleProperty, "FilterPanelCheckBoxEx");
            BindingTools.SetBinding(
                check,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen.FilterSettings,
                bindBased,
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
            PanelItemsHost.Children.Add(check);
        }

        private void AssignFilter(GameField field, string bindBased, string text)
        {
            var button = new ButtonEx();
            button.Content = ResourceProvider.GetString(text);
            button.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelNagivationButton");
            button.Command = mainModel.LoadSubFilterCommand;
            button.CommandParameter = field;
            BindingTools.SetBinding(
                button,
                ButtonBase.TagProperty,
                mainModel.AppSettings.Fullscreen.FilterSettings,
                $"{bindBased}.{nameof(FilterItemProperites.IsSet)}");
            PanelItemsHost.Children.Add(button);
        }
    }
}
