using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_ItemsHost", Type = typeof(ItemsControl))]
    public class Filters : Control
    {
        private FullscreenAppViewModel mainModel;
        private ItemsControl PanelItemsHost;

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

            PanelItemsHost = Template.FindName("PART_ItemsHost", this) as ItemsControl;
            if (PanelItemsHost == null)
            {
                return;
            }

            PanelItemsHost.InputBindings.Add(new KeyBinding(mainModel.ToggleFiltersCommand, new KeyGesture(Key.Back)));
            PanelItemsHost.InputBindings.Add(new KeyBinding(mainModel.ToggleFiltersCommand, new KeyGesture(Key.Escape)));
            var backInput = new GameControllerInputBinding { Command = mainModel.ToggleFiltersCommand };
            BindingTools.SetBinding(backInput,
                GameControllerInputBinding.ButtonProperty,
                null,
                typeof(GameControllerGesture).GetProperty(nameof(GameControllerGesture.CancellationBinding)));
            PanelItemsHost.InputBindings.Add(backInput);
            BindingTools.SetBinding(PanelItemsHost,
                FocusBahaviors.FocusBindingProperty,
                mainModel,
                nameof(mainModel.FilterPanelVisible));

            var ButtonClear = new ButtonEx();
            ButtonClear.Command = mainModel.ClearFiltersCommand;
            ButtonClear.Content = ResourceProvider.GetString(LOC.ClearLabel);
            ButtonClear.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelButtonEx");
            PanelItemsHost.Items.Add(ButtonClear);

            AssignBoolFilter(nameof(FilterSettings.IsInstalled), LOC.GameIsInstalledTitle);
            AssignBoolFilter(nameof(FilterSettings.IsUnInstalled), LOC.GameIsUnInstalledTitle);
            AssignBoolFilter(nameof(FilterSettings.Favorite), LOC.GameFavoriteTitle);
            AssignBoolFilter(nameof(FilterSettings.Hidden), LOC.GameHiddenTitle);
            AssignBoolFilter(nameof(FilterSettings.UseAndFilteringStyle), LOC.UseFilterStyleAndTitle);
            AssignFilter(GameField.PluginId, nameof(FilterSettings.Library), LOC.Library);
            AssignFilter(GameField.Platforms, nameof(FilterSettings.Platform), LOC.PlatformTitle);
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
            PanelItemsHost.Items.Add(ButtonAdditional);

            var desc = new TextBlock();
            desc.Text = ResourceProvider.GetString(LOC.MenuSortByTitle);
            desc.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelText");
            PanelItemsHost.Items.Add(desc);

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
            PanelItemsHost.Items.Add(SelectSortBy);
            AutomationProperties.SetName(SelectSortBy, LOC.MenuSortByTitle.GetLocalized());

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
            PanelItemsHost.Items.Add(SelectSortDirection);
            AutomationProperties.SetName(SelectSortDirection, LOC.MenuSortByDirection.GetLocalized());

            desc = new TextBlock();
            desc.Text = ResourceProvider.GetString(LOC.SettingsTopPanelFilterPresetsItem);
            desc.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelText");
            PanelItemsHost.Items.Add(desc);

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
            PanelItemsHost.Items.Add(ComboFilterPresets);
            AutomationProperties.SetName(ComboFilterPresets, LOC.SettingsTopPanelFilterPresetsItem.GetLocalized());

            var ButtonSaveFilter = new ButtonEx();
            BindingTools.SetBinding(ButtonSaveFilter,
                ButtonBase.CommandProperty,
                mainModel,
                nameof(mainModel.AddFilterPresetCommand));
            ButtonSaveFilter.SetResourceReference(ButtonEx.ContentTemplateProperty, "FilterPanelAddPresetTemplate");
            ButtonSaveFilter.SetResourceReference(ButtonEx.StyleProperty, "FilterPanelFilterPresetActionButton");
            AutomationProperties.SetName(ButtonSaveFilter, LOC.FilterPresetSave.GetLocalized());

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
            AutomationProperties.SetName(ButtonRenameFilter, LOC.RenameTitle.GetLocalized());

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
            AutomationProperties.SetName(ButtonDeleteFilter, LOC.DeleteAction.GetLocalized());

            var filterButtonGrid = new UniformGrid { Columns = 3 };
            filterButtonGrid.Children.Add(ButtonSaveFilter);
            filterButtonGrid.Children.Add(ButtonRenameFilter);
            filterButtonGrid.Children.Add(ButtonDeleteFilter);
            PanelItemsHost.Items.Add(filterButtonGrid);
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
            PanelItemsHost.Items.Add(check);
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
                $"{bindBased}.{nameof(IdItemFilterItemProperties.IsSet)}");
            PanelItemsHost.Items.Add(button);
        }
    }
}
