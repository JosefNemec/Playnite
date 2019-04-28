using System;
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
        [Description("LOCGameLastActivityTitle")]
        LastActivity,
        [Description("LOCGameProviderTitle")]
        Library,
        [Description("LOCGameCategoriesTitle")]
        Categories,
        [Description("LOCGameGenresTitle")]
        Genres,
        [Description("LOCGameReleaseDateTitle")]
        ReleaseDate,
        [Description("LOCGameDevelopersTitle")]
        Developers,
        [Description("LOCGamePublishersTitle")]
        Publishers,
        [Description("LOCGameIsInstalledTitle")]
        IsInstalled,
        [Description("LOCGameHiddenTitle")]
        Hidden,
        [Description("LOCGameFavoriteTitle")]
        Favorite,
        [Description("LOCGameInstallDirTitle")]
        InstallDirectory,
        [Description("LOCGamePlatformTitle")]
        Platform,
        [Description("LOCGameTagsTitle")]
        Tags,
        [Description("LOCTimePlayed")]
        Playtime,
        [Description("LOCAddedLabel")]
        Added,
        [Description("LOCModifiedLabel")]
        Modified,
        [Description("LOCPlayCountLabel")]
        PlayCount,
        [Description("LOCSeriesLabel")]
        Series,
        [Description("LOCVersionLabel")]
        Version,
        [Description("LOCAgeRatingLabel")]
        AgeRating,
        [Description("LOCRegionLabel")]
        Region,
        [Description("LOCSourceLabel")]
        Source,
        [Description("LOCCompletionStatus")]
        CompletionStatus,
        [Description("LOCUserScore")]
        UserScore,
        [Description("LOCCriticScore")]
        CriticScore,
        [Description("LOCCommunityScore")]
        CommunityScore
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
        [Description("LOCLibraries")]
        Library,
        [Description("LOCCategoryLabel")]
        Category,
        [Description("LOCGenreLabel")]
        Genre,
        [Description("LOCDeveloperLabel")]
        Developer,
        [Description("LOCPublisherLabel")]
        Publisher,
        [Description("LOCTagLabel")]
        Tag,
        [Description("LOCPlatformTitle")]
        Platform,
        [Description("LOCSeriesLabel")]
        Series,
        [Description("LOCAgeRatingLabel")]
        AgeRating,
        [Description("LOCRegionLabel")]
        Region,
        [Description("LOCSourceLabel")]
        Source,
        [Description("LOCGameReleaseYearTitle")]
        ReleaseYear
    }

    public enum ViewType : int
    {
        [Description("LOCDetailsLabel")]
        Details = 0,
        [Description("LOCGridLabel")]
        Grid = 1,
        [Description("LOCListLabel")]
        List = 2
    }

    public class ViewSettings : ObservableObject
    {
        // TODO change to something reasonable
        public const double MinCoversZoom = 90;
        public const double DefaultCoversZoom = 230;
        public const double MaxCoversZoom = 460;

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

        private SortOrderDirection sortingOrderDirection = SortOrderDirection.Descending;
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


    }        
}
