using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite
{
    public enum SortOrder
    {
        [Description("LOCGameNameTitle")]
        Name,
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("LOCGameProviderTitle")]
        Library,
        [Description("LOCCategoryLabel")]
        Categories,
        [Description("LOCGameLastActivityTitle")]
        LastActivity,
        [Description("LOCGenreLabel")]
        Genres,
        [Description("LOCGameReleaseDateTitle")]
        ReleaseDate,
        [Description("LOCDeveloperLabel")]
        Developers,
        [Description("LOCPublisherLabel")]
        Publishers,
        [Description("LOCTagLabel")]
        Tags,
        [Description("LOCSeriesLabel")]
        Series,
        [Description("LOCAgeRatingLabel")]
        AgeRating,
        [Description("LOCVersionLabel")]
        Version,
        [Description("LOCRegionLabel")]
        Region,
        [Description("LOCSourceLabel")]
        Source,
        [Description("LOCPlayCountLabel")]
        PlayCount,
        [Description("LOCTimePlayed")]
        Playtime,
        [Description("LOCCompletionStatus")]
        CompletionStatus,
        [Description("LOCUserScore")]
        UserScore,
        [Description("LOCCriticScore")]
        CriticScore,
        [Description("LOCCommunityScore")]
        CommunityScore,
        [Description("LOCDateAddedLabel")]
        Added,
        [Description("LOCDateModifiedLabel")]
        Modified,
        [Description("LOCGameInstallationStatus")]
        IsInstalled,
        [Description("LOCGameHiddenTitle")]
        Hidden,
        [Description("LOCGameFavoriteTitle")]
        Favorite,
        [Description("LOCGameInstallDirTitle")]
        InstallDirectory,
        [Description("LOCFeatureLabel")]
        Features
    }

    public enum SortOrderDirection
    {
        [Description("LOCMenuSortAscending")]
        Ascending,
        [Description("LOCMenuSortDescending")]
        Descending
    }

    public enum GroupableField
    {
        [Description("LOCMenuGroupDont")]
        None,
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("LOCGameProviderTitle")]
        Library,
        [Description("LOCCategoryLabel")]
        Category,
        [Description("LOCGameLastActivityTitle")]
        LastActivity,
        [Description("LOCGenreLabel")]
        Genre,
        [Description("LOCGameReleaseYearTitle")]
        ReleaseYear,
        [Description("LOCDeveloperLabel")]
        Developer,
        [Description("LOCPublisherLabel")]
        Publisher,
        [Description("LOCTagLabel")]
        Tag,
        [Description("LOCSeriesLabel")]
        Series,
        [Description("LOCAgeRatingLabel")]
        AgeRating,
        [Description("LOCRegionLabel")]
        Region,
        [Description("LOCSourceLabel")]
        Source,
        [Description("LOCTimePlayed")]
        PlayTime,
        [Description("LOCCompletionStatus")]
        CompletionStatus,
        [Description("LOCUserScore")]
        UserScore,
        [Description("LOCCriticScore")]
        CriticScore,
        [Description("LOCCommunityScore")]
        CommunityScore,
        [Description("LOCDateAddedLabel")]
        Added,
        [Description("LOCDateModifiedLabel")]
        Modified,
        [Description("LOCFeatureLabel")]
        Feature,
        [Description("LOCGameInstallationStatus")]
        InstallationStatus
    }

    public enum ViewType : int
    {
        [Description("LOCDetailsViewLabel")]
        Details = 0,
        [Description("LOCGridViewLabel")]
        Grid = 1,
        [Description("LOCListViewLabel")]
        List = 2
    }

    public class ViewSettings : ObservableObject
    {
        public const double MinGridItemWidth = 120;
        public const double DefaultGridItemWidth = 200;
        public const double MaxGridItemWidth = 560;

        private SortOrder sortingOrder = SortOrder.Name;
        public SortOrder SortingOrder
        {
            get
            {
                return sortingOrder;
            }

            set
            {
                sortingOrder = value;
                OnPropertyChanged();
            }
        }

        private SortOrderDirection sortingOrderDirection = SortOrderDirection.Ascending;
        public SortOrderDirection SortingOrderDirection
        {
            get
            {
                return sortingOrderDirection;
            }

            set
            {
                sortingOrderDirection = value;
                OnPropertyChanged();
            }
        }

        private GroupableField groupingOrder = GroupableField.None;
        public GroupableField GroupingOrder
        {
            get
            {
                return groupingOrder;
            }

            set
            {
                groupingOrder = value;
                OnPropertyChanged();
            }
        }

        private ViewType gamesViewType = ViewType.Details;
        public ViewType GamesViewType
        {
            get
            {
                return gamesViewType;
            }

            set
            {
                gamesViewType = value;
                OnPropertyChanged();
            }
        }

        private GroupableField selectedExplorerField = GroupableField.Library;
        public GroupableField SelectedExplorerField
        {
            get
            {
                return selectedExplorerField;
            }

            set
            {
                selectedExplorerField = value;
                OnPropertyChanged();
            }
        }

        private ObservableConcurrentDictionary<string, bool> gridViewHeaders = new ObservableConcurrentDictionary<string, bool>()
        {
            { "Icon", true },
            { "Name", true },
            { "Platform", false },
            { "Developers", false },
            { "Publishers", false },
            { "ReleaseDate", true },
            { "Genres", true },
            { "LastActivity", true },
            { "IsInstalled", false },
            { "InstallDirectory", false },
            { "Categories", false },
            { "Playtime", true },
            { "Added", false },
            { "Modified", false },
            { "PlayCount", false },
            { "Series", false },
            { "Version", false },
            { "AgeRating", false },
            { "Region", false },
            { "Source", false },
            { "CompletionStatus", false },
            { "UserScore", false },
            { "CriticScore", false },
            { "CommunityScore", false },
            { "Tags", false },
            { "Library", true },
            { "Features", false }
        };

        public ObservableConcurrentDictionary<string, bool> GridViewHeaders
        {
            get
            {
                return gridViewHeaders;
            }

            set
            {
                if (gridViewHeaders != null)
                {
                    gridViewHeaders.PropertyChanged -= GridViewHeaders_PropertyChanged;
                }

                gridViewHeaders = value;
                gridViewHeaders.PropertyChanged += GridViewHeaders_PropertyChanged;
                OnPropertyChanged();
            }
        }

        private List<string> collapsedCategories = new List<string>();
        public List<string> CollapsedCategories
        {
            get
            {
                return collapsedCategories;
            }

            set
            {
                collapsedCategories = value;
                OnPropertyChanged();
            }
        }

        public ViewSettings()
        {
            GridViewHeaders.PropertyChanged += GridViewHeaders_PropertyChanged;
        }

        private void GridViewHeaders_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GridViewHeaders.Values))
            {
                OnPropertyChanged(nameof(GridViewHeaders));
            }
        }

        #region Serialization Conditions

        public bool ShouldSerializeCollapsedCategories()
        {
            return CollapsedCategories.HasItems();
        }

        #endregion Serialization Conditions
    }
}
