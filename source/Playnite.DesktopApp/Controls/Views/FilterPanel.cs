using Playnite.Common;
using Playnite.Converters;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
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

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ButtonClear", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonClose", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ToggleInstalled", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleUnInstalled", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleHidden", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleFavorite", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ContentInstalledCount", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ContentUnInstalledCount", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ContentHiddenCount", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ContentFavoriteCount", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ElemPlatformLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLibraryLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemNameLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemVersionLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemGenreLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemReleaseYearLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemDeveloperLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPublisherLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCategoryLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemTagLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemFeatureLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPlayTimeLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCompletionStatusLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSeriesLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemRegionLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSourceLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemAgeRatingLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemUserScoreLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCommunityScoreLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCriticScoreLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLastActivityLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemAddedLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemModifiedLabel", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_FilterPlatform", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterLibrary", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterName", Type = typeof(SearchBox))]
    [TemplatePart(Name = "PART_FilterVersion", Type = typeof(SearchBox))]
    [TemplatePart(Name = "PART_FilterGenre", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterReleaseYear", Type = typeof(FilterStringSelectionBox))]
    [TemplatePart(Name = "PART_FilterDeveloper", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterPublisher", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterCategory", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterTag", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterFeature", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterPlayTime", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterCompletionStatus", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterSeries", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterRegion", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterSource", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterAgeRating", Type = typeof(FilterSelectionBox))]
    [TemplatePart(Name = "PART_FilterUserScore", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterCommunityScore", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterCriticScore", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterLastActivity", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterAdded", Type = typeof(FilterEnumSelectionBox))]
    [TemplatePart(Name = "PART_FilterModified", Type = typeof(FilterEnumSelectionBox))]
    public class FilterPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private ButtonBase ButtonClear;
        private ButtonBase ButtonClose;
        private ToggleButton ToggleInstalled;
        private ToggleButton ToggleUnInstalled;
        private ToggleButton ToggleHidden;
        private ToggleButton ToggleFavorite;
        private ContentControl ContentInstalledCount;
        private ContentControl ContentUnInstalledCount;
        private ContentControl ContentHiddenCount;
        private ContentControl ContentFavoriteCount;

        private FrameworkElement ElemPlatformLabel;
        private FrameworkElement ElemLibraryLabel;
        private FrameworkElement ElemNameLabel;
        private FrameworkElement ElemVersionLabel;
        private FrameworkElement ElemGenreLabel;
        private FrameworkElement ElemReleaseYearLabel;
        private FrameworkElement ElemDeveloperLabel;
        private FrameworkElement ElemPublisherLabel;
        private FrameworkElement ElemCategoryLabel;
        private FrameworkElement ElemTagLabel;
        private FrameworkElement ElemFeatureLabel;
        private FrameworkElement ElemPlayTimeLabel;
        private FrameworkElement ElemCompletionStatusLabel;
        private FrameworkElement ElemSeriesLabel;
        private FrameworkElement ElemRegionLabel;
        private FrameworkElement ElemSourceLabel;
        private FrameworkElement ElemAgeRatingLabel;
        private FrameworkElement ElemUserScoreLabel;
        private FrameworkElement ElemCommunityScoreLabel;
        private FrameworkElement ElemCriticScoreLabel;
        private FrameworkElement ElemLastActivityLabel;
        private FrameworkElement ElemAddedLabel;
        private FrameworkElement ElemModifiedLabel;

        private FilterSelectionBox FilterPlatform;
        private FilterSelectionBox FilterLibrary;
        private SearchBox FilterName;
        private SearchBox FilterVersion;
        private FilterSelectionBox FilterGenre;
        private FilterStringSelectionBox FilterReleaseYear;
        private FilterSelectionBox FilterDeveloper;
        private FilterSelectionBox FilterPublisher;
        private FilterSelectionBox FilterCategory;
        private FilterSelectionBox FilterTag;
        private FilterSelectionBox FilterFeature;
        private FilterEnumSelectionBox FilterPlayTime;
        private FilterEnumSelectionBox FilterCompletionStatus;
        private FilterSelectionBox FilterSeries;
        private FilterSelectionBox FilterRegion;
        private FilterSelectionBox FilterSource;
        private FilterSelectionBox FilterAgeRating;
        private FilterEnumSelectionBox FilterUserScore;
        private FilterEnumSelectionBox FilterCommunityScore;
        private FilterEnumSelectionBox FilterCriticScore;
        private FilterEnumSelectionBox FilterLastActivity;
        private FilterEnumSelectionBox FilterAdded;
        private FilterEnumSelectionBox FilterModified;

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

            SetToggleFilter(ref ToggleInstalled, "PART_ToggleInstalled", nameof(FilterSettings.IsInstalled));
            SetToggleFilter(ref ToggleUnInstalled, "PART_ToggleUnInstalled", nameof(FilterSettings.IsUnInstalled));
            SetToggleFilter(ref ToggleHidden, "PART_ToggleHidden", nameof(FilterSettings.Hidden));
            SetToggleFilter(ref ToggleFavorite, "PART_ToggleFavorite", nameof(FilterSettings.Favorite));

            SetToggleCount(ref ContentInstalledCount, "PART_ContentInstalledCount", nameof(DatabaseStats.Installed));
            SetToggleCount(ref ContentUnInstalledCount, "PART_ContentUnInstalledCount", nameof(DatabaseStats.UnInstalled));
            SetToggleCount(ref ContentHiddenCount, "PART_ContentHiddenCount", nameof(DatabaseStats.Hidden));
            SetToggleCount(ref ContentFavoriteCount, "PART_ContentFavoriteCount", nameof(DatabaseStats.Favorite));

            SetLabelTag(ref ElemPlatformLabel, "PART_ElemPlatformLabel", nameof(FilterSettings.Platform));
            SetLabelTag(ref ElemLibraryLabel, "PART_ElemLibraryLabel", nameof(FilterSettings.Library));
            SetLabelTag(ref ElemGenreLabel, "PART_ElemGenreLabel", nameof(FilterSettings.Genre));
            SetLabelTag(ref ElemReleaseYearLabel, "PART_ElemReleaseYearLabel", nameof(FilterSettings.ReleaseYear));
            SetLabelTag(ref ElemDeveloperLabel, "PART_ElemDeveloperLabel", nameof(FilterSettings.Developer));
            SetLabelTag(ref ElemPublisherLabel, "PART_ElemPublisherLabel", nameof(FilterSettings.Publisher));
            SetLabelTag(ref ElemCategoryLabel, "PART_ElemCategoryLabel", nameof(FilterSettings.Category));
            SetLabelTag(ref ElemTagLabel, "PART_ElemTagLabel", nameof(FilterSettings.Tag));
            SetLabelTag(ref ElemFeatureLabel, "PART_ElemFeatureLabel", nameof(FilterSettings.Feature));
            SetLabelTag(ref ElemPlayTimeLabel, "PART_ElemPlayTimeLabel", nameof(FilterSettings.PlayTime));
            SetLabelTag(ref ElemCompletionStatusLabel, "PART_ElemCompletionStatusLabel", nameof(FilterSettings.CompletionStatus));
            SetLabelTag(ref ElemSeriesLabel, "PART_ElemSeriesLabel", nameof(FilterSettings.Series));
            SetLabelTag(ref ElemRegionLabel, "PART_ElemRegionLabel", nameof(FilterSettings.Region));
            SetLabelTag(ref ElemSourceLabel, "PART_ElemSourceLabel", nameof(FilterSettings.Source));
            SetLabelTag(ref ElemAgeRatingLabel, "PART_ElemAgeRatingLabel", nameof(FilterSettings.AgeRating));
            SetLabelTag(ref ElemUserScoreLabel, "PART_ElemUserScoreLabel", nameof(FilterSettings.UserScore));
            SetLabelTag(ref ElemCommunityScoreLabel, "PART_ElemCommunityScoreLabel", nameof(FilterSettings.CommunityScore));
            SetLabelTag(ref ElemCriticScoreLabel, "PART_ElemCriticScoreLabel", nameof(FilterSettings.CriticScore));
            SetLabelTag(ref ElemLastActivityLabel, "PART_ElemLastActivityLabel", nameof(FilterSettings.LastActivity));
            SetLabelTag(ref ElemAddedLabel, "PART_ElemAddedLabel", nameof(FilterSettings.Added));
            SetLabelTag(ref ElemModifiedLabel, "PART_ElemModifiedLabel", nameof(FilterSettings.Modified));

            SetFilterSelectionBoxFilter(ref FilterPlatform, "PART_FilterPlatform", nameof(DatabaseFilter.Platforms), nameof(FilterSettings.Platform));
            SetFilterSelectionBoxFilter(ref FilterLibrary, "PART_FilterLibrary", nameof(DatabaseFilter.Libraries), nameof(FilterSettings.Library));
            SetFilterSelectionBoxFilter(ref FilterGenre, "PART_FilterGenre", nameof(DatabaseFilter.Genres), nameof(FilterSettings.Genre));
            SetFilterStringSelectionBoxFilter(ref FilterReleaseYear, "PART_FilterReleaseYear", nameof(DatabaseFilter.ReleaseYears), nameof(FilterSettings.ReleaseYear));
            SetFilterSelectionBoxFilter(ref FilterDeveloper, "PART_FilterDeveloper", nameof(DatabaseFilter.Developers), nameof(FilterSettings.Developer));
            SetFilterSelectionBoxFilter(ref FilterPublisher, "PART_FilterPublisher", nameof(DatabaseFilter.Publishers), nameof(FilterSettings.Publisher));
            SetFilterSelectionBoxFilter(ref FilterCategory, "PART_FilterCategory", nameof(DatabaseFilter.Categories), nameof(FilterSettings.Category));
            SetFilterSelectionBoxFilter(ref FilterTag, "PART_FilterTag", nameof(DatabaseFilter.Tags), nameof(FilterSettings.Tag));
            SetFilterSelectionBoxFilter(ref FilterFeature, "PART_FilterFeature", nameof(DatabaseFilter.Features), nameof(FilterSettings.Feature));
            SetFilterEnumSelectionBoxFilter(ref FilterPlayTime, "PART_FilterPlayTime", nameof(FilterSettings.PlayTime), typeof(PlaytimeCategory));
            SetFilterEnumSelectionBoxFilter(ref FilterCompletionStatus, "PART_FilterCompletionStatus", nameof(FilterSettings.CompletionStatus), typeof(CompletionStatus));
            SetFilterSelectionBoxFilter(ref FilterSeries, "PART_FilterSeries", nameof(DatabaseFilter.Series), nameof(FilterSettings.Series));
            SetFilterSelectionBoxFilter(ref FilterRegion, "PART_FilterRegion", nameof(DatabaseFilter.Regions), nameof(FilterSettings.Region));
            SetFilterSelectionBoxFilter(ref FilterSource, "PART_FilterSource", nameof(DatabaseFilter.Sources), nameof(FilterSettings.Source));
            SetFilterSelectionBoxFilter(ref FilterAgeRating, "PART_FilterAgeRating", nameof(DatabaseFilter.AgeRatings), nameof(FilterSettings.AgeRating));
            SetFilterEnumSelectionBoxFilter(ref FilterUserScore, "PART_FilterUserScore", nameof(FilterSettings.UserScore), typeof(ScoreGroup));
            SetFilterEnumSelectionBoxFilter(ref FilterCommunityScore, "PART_FilterCommunityScore", nameof(FilterSettings.CommunityScore), typeof(ScoreGroup));
            SetFilterEnumSelectionBoxFilter(ref FilterCriticScore, "PART_FilterCriticScore", nameof(FilterSettings.CriticScore), typeof(ScoreGroup));
            SetFilterEnumSelectionBoxFilter(ref FilterLastActivity, "PART_FilterLastActivity", nameof(FilterSettings.LastActivity), typeof(PastTimeSegment));
            SetFilterEnumSelectionBoxFilter(ref FilterAdded, "PART_FilterAdded", nameof(FilterSettings.Added), typeof(PastTimeSegment));
            SetFilterEnumSelectionBoxFilter(ref FilterModified, "PART_FilterModified", nameof(FilterSettings.Modified), typeof(PastTimeSegment));

            ElemNameLabel = Template.FindName("PART_ElemNameLabel", this) as FrameworkElement;
            if (ElemNameLabel != null)
            {
                BindingTools.SetBinding(ElemNameLabel,
                    FrameworkElement.TagProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.Name),
                    converter: new StringNullOrEmptyToBoolConverter(),
                    fallBackValue: false);
            }

            ElemVersionLabel = Template.FindName("PART_ElemVersionLabel", this) as FrameworkElement;
            if (ElemVersionLabel != null)
            {
                BindingTools.SetBinding(ElemVersionLabel,
                    FrameworkElement.TagProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.Version),
                    converter: new StringNullOrEmptyToBoolConverter(),
                    fallBackValue: false);
            }

            FilterName = Template.FindName("PART_FilterName", this) as SearchBox;
            if (FilterName != null)
            {
                BindingTools.SetBinding(FilterName,
                    SearchBox.TextProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.Name),
                    BindingMode.TwoWay,
                    delay: 100);
            }

            FilterVersion = Template.FindName("PART_FilterVersion", this) as SearchBox;
            if (FilterVersion != null)
            {
                BindingTools.SetBinding(FilterVersion,
                    SearchBox.TextProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.Version),
                    BindingMode.TwoWay,
                    delay: 100);
            }

            FilterLibrary.IsFullTextEnabled = false;
        }

        private void SetToggleFilter(ref ToggleButton button, string partId, string binding)
        {
            button = Template.FindName(partId, this) as ToggleButton;
            if (button != null)
            {
                BindingTools.SetBinding(button,
                    ToggleButton.IsCheckedProperty,
                    mainModel.AppSettings.FilterSettings,
                    binding,
                    BindingMode.TwoWay);
            }
        }

        private void SetToggleCount(ref ContentControl content, string partId, string binding)
        {
            content = Template.FindName(partId, this) as ContentControl;
            if (content != null)
            {
                BindingTools.SetBinding(content,
                    ContentControl.ContentProperty,
                    mainModel.GamesStats,
                    binding);
            }
        }

        private void SetLabelTag(ref FrameworkElement elem, string partId, string binding)
        {
            elem = Template.FindName(partId, this) as FrameworkElement;
            if (elem != null)
            {
                BindingTools.SetBinding(elem,
                    FrameworkElement.TagProperty,
                    mainModel.AppSettings.FilterSettings,
                    $"{binding}.{nameof(FilterItemProperites.IsSet)}",
                    fallBackValue: false);
            }
        }

        private void SetFilterSelectionBoxFilter(ref FilterSelectionBox elem, string partId, string listBinding, string filterBinding)
        {
            elem = Template.FindName(partId, this) as FilterSelectionBox;
            if (elem != null)
            {
                BindingTools.SetBinding(elem,
                    FilterSelectionBox.ItemsListProperty,
                    mainModel.DatabaseFilters,
                    listBinding);
                BindingTools.SetBinding(elem,
                    FilterSelectionBox.FilterPropertiesProperty,
                    mainModel.AppSettings.FilterSettings,
                    filterBinding,
                    BindingMode.TwoWay);
            }
        }

        private void SetFilterEnumSelectionBoxFilter(ref FilterEnumSelectionBox elem, string partId, string filterBinding, Type enumType)
        {
            elem = Template.FindName(partId, this) as FilterEnumSelectionBox;
            if (elem != null)
            {
                elem.EnumType = enumType;
                BindingTools.SetBinding(elem,
                    FilterEnumSelectionBox.FilterPropertiesProperty,
                    mainModel.AppSettings.FilterSettings,
                    filterBinding,
                    BindingMode.TwoWay);
            }
        }

        private void SetFilterStringSelectionBoxFilter(ref FilterStringSelectionBox elem, string partId, string listBinding, string filterBinding)
        {
            elem = Template.FindName(partId, this) as FilterStringSelectionBox;
            if (elem != null)
            {
                BindingTools.SetBinding(elem,
                    FilterStringSelectionBox.ItemsListProperty,
                    mainModel.DatabaseFilters,
                    listBinding);
                BindingTools.SetBinding(elem,
                    FilterStringSelectionBox.FilterPropertiesProperty,
                    mainModel.AppSettings.FilterSettings,
                    filterBinding,
                    BindingMode.TwoWay);
            }
        }
    }
}
