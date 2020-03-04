using Newtonsoft.Json;
using Playnite.SDK.Models;
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

    public class ListViewColumnProperty : ObservableObject
    {
        public GameField Field { get; set; }

        private bool visible = false;
        public bool Visible
        {
            get
            {
                return visible;
            }

            set
            {
                visible = value;
                OnPropertyChanged();
            }
        }

        private double width = double.NaN;
        public double Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
                OnPropertyChanged();
            }
        }

        public ListViewColumnProperty()
        {
        }

        public ListViewColumnProperty(GameField field)
        {
            Field = field;
        }
    }

    public class ListViewColumnsProperties : ObservableObject
    {
        private ListViewColumnProperty icon = new ListViewColumnProperty(GameField.Icon);
        public ListViewColumnProperty Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty name = new ListViewColumnProperty(GameField.Name);
        public ListViewColumnProperty Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty platform = new ListViewColumnProperty(GameField.Platform);
        public ListViewColumnProperty Platform
        {
            get
            {
                return platform;
            }

            set
            {
                platform = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty developers = new ListViewColumnProperty(GameField.Developers);
        public ListViewColumnProperty Developers
        {
            get
            {
                return developers;
            }

            set
            {
                developers = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty publishers = new ListViewColumnProperty(GameField.Publishers);
        public ListViewColumnProperty Publishers
        {
            get
            {
                return publishers;
            }

            set
            {
                publishers = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty releaseDate = new ListViewColumnProperty(GameField.ReleaseDate);
        public ListViewColumnProperty ReleaseDate
        {
            get
            {
                return releaseDate;
            }

            set
            {
                releaseDate = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty genres = new ListViewColumnProperty(GameField.Genres);
        public ListViewColumnProperty Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty lastActivity = new ListViewColumnProperty(GameField.LastActivity);
        public ListViewColumnProperty LastActivity
        {
            get
            {
                return lastActivity;
            }

            set
            {
                lastActivity = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty isInstalled = new ListViewColumnProperty(GameField.IsInstalled);
        public ListViewColumnProperty IsInstalled
        {
            get
            {
                return isInstalled;
            }

            set
            {
                isInstalled = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty installDirectory = new ListViewColumnProperty(GameField.InstallDirectory);
        public ListViewColumnProperty InstallDirectory
        {
            get
            {
                return installDirectory;
            }

            set
            {
                installDirectory = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty categories = new ListViewColumnProperty(GameField.Categories);
        public ListViewColumnProperty Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty playtime = new ListViewColumnProperty(GameField.Playtime);
        public ListViewColumnProperty Playtime
        {
            get
            {
                return playtime;
            }

            set
            {
                playtime = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty added = new ListViewColumnProperty(GameField.Added);
        public ListViewColumnProperty Added
        {
            get
            {
                return added;
            }

            set
            {
                added = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty modified = new ListViewColumnProperty(GameField.Modified);
        public ListViewColumnProperty Modified
        {
            get
            {
                return modified;
            }

            set
            {
                modified = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty playCount = new ListViewColumnProperty(GameField.PlayCount);
        public ListViewColumnProperty PlayCount
        {
            get
            {
                return playCount;
            }

            set
            {
                playCount = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty series = new ListViewColumnProperty(GameField.Series);
        public ListViewColumnProperty Series
        {
            get
            {
                return series;
            }

            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty version = new ListViewColumnProperty(GameField.Version);
        public ListViewColumnProperty Version
        {
            get
            {
                return version;
            }

            set
            {
                version = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty ageRating = new ListViewColumnProperty(GameField.AgeRating);
        public ListViewColumnProperty AgeRating
        {
            get
            {
                return ageRating;
            }

            set
            {
                ageRating = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty region = new ListViewColumnProperty(GameField.Region);
        public ListViewColumnProperty Region
        {
            get
            {
                return region;
            }

            set
            {
                region = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty source = new ListViewColumnProperty(GameField.Source);
        public ListViewColumnProperty Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty completionStatus = new ListViewColumnProperty(GameField.CompletionStatus);
        public ListViewColumnProperty CompletionStatus
        {
            get
            {
                return completionStatus;
            }

            set
            {
                completionStatus = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty userScore = new ListViewColumnProperty(GameField.UserScore);
        public ListViewColumnProperty UserScore
        {
            get
            {
                return userScore;
            }

            set
            {
                userScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty criticScore = new ListViewColumnProperty(GameField.CriticScore);
        public ListViewColumnProperty CriticScore
        {
            get
            {
                return criticScore;
            }

            set
            {
                criticScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty communityScore = new ListViewColumnProperty(GameField.CommunityScore);
        public ListViewColumnProperty CommunityScore
        {
            get
            {
                return communityScore;
            }

            set
            {
                communityScore = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty tags = new ListViewColumnProperty(GameField.Tags);
        public ListViewColumnProperty Tags
        {
            get
            {
                return tags;
            }

            set
            {
                tags = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty pluginId = new ListViewColumnProperty(GameField.PluginId);
        public ListViewColumnProperty PluginId
        {
            get
            {
                return pluginId;
            }

            set
            {
                pluginId = value;
                OnPropertyChanged();
            }
        }

        private ListViewColumnProperty features = new ListViewColumnProperty(GameField.Features);
        public ListViewColumnProperty Features
        {
            get
            {
                return features;
            }

            set
            {
                features = value;
                OnPropertyChanged();
            }
        }
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

        private ListViewColumnsProperties listViewColumns;
        public ListViewColumnsProperties ListViewColumns
        {
            get
            {
                return listViewColumns;
            }

            set
            {
                listViewColumns = value;
                OnPropertyChanged();
            }
        }

        private List<GameField> listViewColumsOrder;
        public List<GameField> ListViewColumsOrder
        {
            get
            {
                return listViewColumsOrder;
            }

            set
            {
                listViewColumsOrder = value;
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
        }

        #region Serialization Conditions

        public bool ShouldSerializeCollapsedCategories()
        {
            return CollapsedCategories.HasItems();
        }

        #endregion Serialization Conditions
    }
}
