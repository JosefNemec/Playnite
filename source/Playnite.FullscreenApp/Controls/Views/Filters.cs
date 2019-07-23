using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
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
    [TemplatePart(Name = "PART_ButtonClear", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ToggleInstalled", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleUninstalled", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleFavorite", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleHidden", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ButtonLibraries", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonPlatforms", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonCategories", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonAdditional", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_SelectSortBy", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_SelectSortDirection", Type = typeof(Selector))]
    public class Filters : Control
    {
        private FullscreenAppViewModel mainModel;
        private ButtonBase ButtonClear;
        private ToggleButton ToggleInstalled;
        private ToggleButton ToggleUninstalled;
        private ToggleButton ToggleFavorite;
        private ToggleButton ToggleHidden;
        private ButtonBase ButtonLibraries;
        private ButtonBase ButtonPlatforms;
        private ButtonBase ButtonCategories;
        private ButtonBase ButtonAdditional;
        private Selector SelectSortBy;
        private Selector SelectSortDirection;

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

            if (Template != null)
            {
                ButtonClear = Template.FindName("PART_ButtonClear", this) as ButtonBase;
                if (ButtonClear != null)
                {
                    ButtonClear.Command = mainModel.ClearFiltersCommand;
                    BindingTools.SetBinding(ButtonClear,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.FilterPanelVisible));
                }

                AssignBoolFilter(ref ToggleInstalled, "PART_ToggleInstalled", nameof(FilterSettings.IsInstalled));
                AssignBoolFilter(ref ToggleUninstalled, "PART_ToggleUninstalled", nameof(FilterSettings.IsUnInstalled));
                AssignBoolFilter(ref ToggleFavorite, "PART_ToggleFavorite", nameof(FilterSettings.Favorite));
                AssignBoolFilter(ref ToggleHidden, "PART_ToggleHidden", nameof(FilterSettings.Hidden));
                AssignFilter(ref ButtonLibraries, "PART_ButtonLibraries", GameField.PluginId, nameof(FilterSettings.Library));
                AssignFilter(ref ButtonPlatforms, "PART_ButtonPlatforms", GameField.Platform, nameof(FilterSettings.Platform));
                AssignFilter(ref ButtonCategories, "PART_ButtonCategories", GameField.Categories, nameof(FilterSettings.Category));

                ButtonAdditional = Template.FindName("PART_ButtonAdditional", this) as ButtonBase;
                if (ButtonAdditional != null)
                {
                    ButtonAdditional.Command = mainModel.OpenAdditionalFiltersCommand;
                    BindingTools.SetBinding(
                        ButtonAdditional,
                        ButtonBase.TagProperty,
                        mainModel.AppSettings.Fullscreen.FilterSettings,
                        nameof(FullscreenFilterSettings.IsSubAdditionalFilterActive));
                }

                SelectSortBy = Template.FindName("PART_SelectSortBy", this) as Selector;
                if (SelectSortBy != null)
                {
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
                }

                SelectSortDirection = Template.FindName("PART_SelectSortDirection", this) as Selector;
                if (SelectSortDirection != null)
                {
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
                }
            }
        }

        private void AssignBoolFilter(ref ToggleButton button, string partId, string bindBased)
        {
            button = Template.FindName(partId, this) as ToggleButton;
            if (button != null)
            {
                BindingTools.SetBinding(
                    button,
                    ToggleButton.IsCheckedProperty,
                    mainModel.AppSettings.Fullscreen.FilterSettings,
                    bindBased,
                    BindingMode.TwoWay,
                    UpdateSourceTrigger.PropertyChanged);
            }

        }

        private void AssignFilter(ref ButtonBase button, string partId, GameField field, string bindBased)
        {
            button = Template.FindName(partId, this) as ButtonBase;
            if (button != null)
            {
                button.Command = mainModel.LoadSubFilterCommand;
                button.CommandParameter = field;
                BindingTools.SetBinding(
                    button,
                    ButtonBase.TagProperty,
                    mainModel.AppSettings.Fullscreen.FilterSettings,
                    $"{bindBased}.{nameof(FilterItemProperites.IsSet)}");
            }
        }
    }
}
