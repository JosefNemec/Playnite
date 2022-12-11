using Playnite.Common;
using Playnite.Converters;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ButtonClear", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonClose", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_PanelItemsHost", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_ButtonDeleteFilter", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonRenameFilter", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonSaveFilter", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ComboFilterPresets", Type = typeof(ComboBox))]
    public class FilterPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private ButtonBase ButtonClear;
        private ButtonBase ButtonClose;
        private Panel PanelItemsHost;
        private ButtonBase ButtonDeleteFilter;
        private ButtonBase ButtonRenameFilter;
        private ButtonBase ButtonSaveFilter;
        private ComboBox ComboFilterPresets;

        static FilterPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterPanel), new FrameworkPropertyMetadata(typeof(FilterPanel)));
        }

        public FilterPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public FilterPanel(DesktopAppViewModel mainModel)
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

            PanelItemsHost = Template.FindName("PART_PanelItemsHost", this) as Panel;
            ButtonClear = Template.FindName("PART_ButtonClear", this) as ButtonBase;
            if (ButtonClear != null)
            {
                ButtonClear.Command = mainModel.ClearFiltersCommand;
            }

            ButtonClose = Template.FindName("PART_ButtonClose", this) as ButtonBase;
            if (ButtonClose != null)
            {
                ButtonClose.Command = mainModel.CloseFilterPanelCommand;
            }

            ButtonDeleteFilter = Template.FindName("PART_ButtonDeleteFilter", this) as ButtonBase;
            if (ButtonDeleteFilter != null)
            {
                BindingTools.SetBinding(ButtonDeleteFilter,
                    ButtonBase.CommandProperty,
                    mainModel,
                    nameof(mainModel.RemoveFilterPresetCommand));
                BindingTools.SetBinding(ButtonDeleteFilter,
                    ButtonBase.CommandParameterProperty,
                    mainModel,
                    nameof(mainModel.ActiveFilterPreset));
            }

            ButtonRenameFilter = Template.FindName("PART_ButtonRenameFilter", this) as ButtonBase;
            if (ButtonRenameFilter != null)
            {
                BindingTools.SetBinding(ButtonRenameFilter,
                    ButtonBase.CommandProperty,
                    mainModel,
                    nameof(mainModel.RenameFilterPresetCommand));
                BindingTools.SetBinding(ButtonRenameFilter,
                    ButtonBase.CommandParameterProperty,
                    mainModel,
                    nameof(mainModel.ActiveFilterPreset));
            }

            ButtonSaveFilter = Template.FindName("PART_ButtonSaveFilter", this) as ButtonBase;
            if (ButtonSaveFilter != null)
            {
                BindingTools.SetBinding(ButtonSaveFilter,
                    ButtonBase.CommandProperty,
                    mainModel,
                    nameof(mainModel.AddFilterPresetCommand));
            }

            ComboFilterPresets = Template.FindName("PART_ComboFilterPresets", this) as ComboBox;
            if (ComboFilterPresets != null)
            {
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
            }

            SetToggleFilter(nameof(FilterSettings.IsInstalled), nameof(DatabaseStats.Installed), LOC.GameIsInstalledTitle);
            SetToggleFilter(nameof(FilterSettings.IsUnInstalled), nameof(DatabaseStats.UnInstalled), LOC.GameIsUnInstalledTitle);
            SetToggleFilter(nameof(FilterSettings.Hidden), nameof(DatabaseStats.Hidden), LOC.GameHiddenTitle);
            SetToggleFilter(nameof(FilterSettings.Favorite), nameof(DatabaseStats.Favorite), LOC.GameFavoriteTitle);
            SetToggleFilterWithTooltip(nameof(FilterSettings.UseAndFilteringStyle), LOC.UseFilterStyleAndTitle, LOC.UseFilterStyleAndTooltip);

            SetLabelTag(nameof(FilterSettings.Platform), LOC.PlatformTitle);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Platforms), nameof(FilterSettings.Platform));

            SetLabelTag(nameof(FilterSettings.Library), LOC.Library);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Libraries), nameof(FilterSettings.Library), false);

            SetLabelTag(nameof(FilterSettings.Name), LOC.NameLabel, new StringNullOrEmptyToBoolConverter(), nameof(FilterSettings.Name));
            SetFilterSearchBoxFilter(nameof(FilterSettings.Name));

            SetLabelTag(nameof(FilterSettings.Genre), LOC.GenreLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Genres), nameof(FilterSettings.Genre));

            SetLabelTag(nameof(FilterSettings.ReleaseYear), LOC.GameReleaseYearTitle);
            SetFilterStringSelectionBoxFilter(nameof(DatabaseFilter.ReleaseYears), nameof(FilterSettings.ReleaseYear));

            SetLabelTag(nameof(FilterSettings.Developer), LOC.DeveloperLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Developers), nameof(FilterSettings.Developer));

            SetLabelTag(nameof(FilterSettings.Publisher), LOC.PublisherLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Publishers), nameof(FilterSettings.Publisher));

            SetLabelTag(nameof(FilterSettings.Category), LOC.CategoryLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Categories), nameof(FilterSettings.Category));

            SetLabelTag(nameof(FilterSettings.Tag), LOC.TagLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Tags), nameof(FilterSettings.Tag));

            SetLabelTag(nameof(FilterSettings.Feature), LOC.FeatureLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Features), nameof(FilterSettings.Feature));

            SetLabelTag(nameof(FilterSettings.PlayTime), LOC.TimePlayed);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.PlayTime), typeof(PlaytimeCategory));

            SetLabelTag(nameof(FilterSettings.InstallSize), LOC.InstallSizeLabel);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.InstallSize), typeof(InstallSizeGroup));

            SetLabelTag(nameof(FilterSettings.CompletionStatuses), LOC.CompletionStatus);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.CompletionStatuses), nameof(FilterSettings.CompletionStatuses));

            SetLabelTag(nameof(FilterSettings.Series), LOC.SeriesLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Series), nameof(FilterSettings.Series));

            SetLabelTag(nameof(FilterSettings.Region), LOC.RegionLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Regions), nameof(FilterSettings.Region));

            SetLabelTag(nameof(FilterSettings.Source), LOC.SourceLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.Sources), nameof(FilterSettings.Source));

            SetLabelTag(nameof(FilterSettings.AgeRating), LOC.AgeRatingLabel);
            SetFilterSelectionBoxFilter(nameof(DatabaseFilter.AgeRatings), nameof(FilterSettings.AgeRating));

            SetLabelTag(nameof(FilterSettings.Version), LOC.VersionLabel, new StringNullOrEmptyToBoolConverter(), nameof(FilterSettings.Version));
            SetFilterSearchBoxFilter(nameof(FilterSettings.Version));

            SetLabelTag(nameof(FilterSettings.UserScore), LOC.UserScore);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.UserScore), typeof(ScoreGroup));

            SetLabelTag(nameof(FilterSettings.CommunityScore), LOC.CommunityScore);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.CommunityScore), typeof(ScoreGroup));

            SetLabelTag(nameof(FilterSettings.CriticScore), LOC.CriticScore);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.CriticScore), typeof(ScoreGroup));

            SetLabelTag(nameof(FilterSettings.LastActivity), LOC.GameLastActivityTitle);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.LastActivity), typeof(PastTimeSegment));

            SetLabelTag(nameof(FilterSettings.RecentActivity), LOC.RecentActivityLabel);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.RecentActivity), typeof(PastTimeSegment));

            SetLabelTag(nameof(FilterSettings.Added), LOC.DateAddedLabel);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.Added), typeof(PastTimeSegment));

            SetLabelTag(nameof(FilterSettings.Modified), LOC.DateModifiedLabel);
            SetFilterEnumSelectionBoxFilter(nameof(FilterSettings.Modified), typeof(PastTimeSegment));
        }

        private void SetToggleFilterWithTooltip(string binding, string text, string tooltip)
        {
            var elem = new CheckBox();
            elem.SetResourceReference(CheckBox.StyleProperty, "FilterPanelCheckBox");
            BindingTools.SetBinding(elem,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.FilterSettings,
                binding,
                BindingMode.TwoWay);
            elem.Content = ResourceProvider.GetString(text);
            elem.ToolTip = ResourceProvider.GetString(tooltip);
            PanelItemsHost.Children.Add(elem);
        }

        private void SetToggleFilter(string binding, string countBinding, string text)
        {
            var elem = new CheckBox();
            elem.SetResourceReference(CheckBox.StyleProperty, "FilterPanelCheckBox");
            BindingTools.SetBinding(elem,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.FilterSettings,
                binding,
                BindingMode.TwoWay);
            BindingTools.SetBinding(elem,
                ContentControl.ContentProperty,
                mainModel.GamesStats,
                countBinding);
            elem.ContentStringFormat = ResourceProvider.GetString(text) + " ({0})";
            PanelItemsHost.Children.Add(elem);
        }

        private void SetLabelTag(string binding, string text, IValueConverter converter = null, string bindingName = null)
        {
            if (PanelItemsHost == null)
            {
                return;
            }

            var elem = new Label();
            elem.SetResourceReference(Label.StyleProperty, "FilterPanelLabel");
            BindingTools.SetBinding(elem,
                FrameworkElement.TagProperty,
                mainModel.AppSettings.FilterSettings,
                bindingName ?? $"{binding}.{nameof(IdItemFilterItemProperties.IsSet)}",
                fallBackValue: false,
                converter: converter);
            elem.Content = ResourceProvider.GetString(text);
            PanelItemsHost.Children.Add(elem);
        }

        private void SetFilterSearchBoxFilter(string filterBinding)
        {
            if (PanelItemsHost == null)
            {
                return;
            }

            var elem = new SearchBox();
            elem.SetResourceReference(SearchBox.StyleProperty, "FilterPanelFilterSearchBox");
            BindingTools.SetBinding(elem,
                SearchBox.TextProperty,
                mainModel.AppSettings.FilterSettings,
                filterBinding,
                BindingMode.TwoWay,
                delay: 100);
            PanelItemsHost.Children.Add(elem);
        }

        private void SetFilterSelectionBoxFilter(string listBinding, string filterBinding, bool isFullext = true)
        {
            if (PanelItemsHost == null)
            {
                return;
            }

            var elem = new FilterSelectionBox();
            elem.SetResourceReference(FilterSelectionBox.StyleProperty, "FilterPanelFilterSelectionBox");
            BindingTools.SetBinding(elem,
                FilterSelectionBox.ItemsListProperty,
                mainModel.DatabaseFilters,
                listBinding);
            BindingTools.SetBinding(elem,
                FilterSelectionBox.FilterPropertiesProperty,
                mainModel.AppSettings.FilterSettings,
                filterBinding,
                BindingMode.TwoWay);
            elem.IsFullTextEnabled = isFullext;
            PanelItemsHost.Children.Add(elem);
        }

        private void SetFilterEnumSelectionBoxFilter(string filterBinding, Type enumType)
        {
            if (PanelItemsHost == null)
            {
                return;
            }

            var elem = new FilterEnumSelectionBox();
            elem.SetResourceReference(FilterEnumSelectionBox.StyleProperty, "FilterPanelFilterEnumSelectionBox");
            elem.EnumType = enumType;
            BindingTools.SetBinding(elem,
                FilterEnumSelectionBox.FilterPropertiesProperty,
                mainModel.AppSettings.FilterSettings,
                filterBinding,
                BindingMode.TwoWay);
            PanelItemsHost.Children.Add(elem);
        }

        private void SetFilterStringSelectionBoxFilter(string listBinding, string filterBinding)
        {
            if (PanelItemsHost == null)
            {
                return;
            }

            var elem = new FilterStringSelectionBox();
            elem.SetResourceReference(FilterStringSelectionBox.StyleProperty, "FilterPanelFilterStringSelectionBox");
            BindingTools.SetBinding(elem,
                FilterStringSelectionBox.ItemsListProperty,
                mainModel.DatabaseFilters,
                listBinding);
            BindingTools.SetBinding(elem,
                FilterStringSelectionBox.FilterPropertiesProperty,
                mainModel.AppSettings.FilterSettings,
                filterBinding,
                BindingMode.TwoWay);
            PanelItemsHost.Children.Add(elem);
        }
    }
}
