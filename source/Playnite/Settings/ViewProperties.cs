using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite
{
    public enum SortOrder
    {
        Name,
        LastActivity,
        Provider,
        Categories,
        Genres,
        ReleaseDate,
        Developers,
        Publishers,
        IsInstalled,
        Hidden,
        Favorite,
        InstallDirectory,
        Icon,
        Platform,
        Tags,
        Playtime,
        Added,
        Modified,
        PlayCount,
        Series,
        Version,
        AgeRating,
        Region,
        Source,
        CompletionStatus,
        UserScore,
        CriticScore,
        CommunityScore
    }

    public enum SortOrderDirection
    {
        Ascending,
        Descending
    }

    public enum GroupOrder
    {
        None,
        Provider,
        Category,
        Platform
    }

    public enum ViewType : int
    {
        List = 0,
        Images = 1,
        Grid = 2
    }

    public class ViewSettings : ObservableObject
    {
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

        private GroupOrder groupingOrder = GroupOrder.None;
        public GroupOrder GroupingOrder
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

        private ViewType gamesViewType;
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

        public const double MinCoversZoom = 90;
        public const double DefaultCoversZoom = 180;
        public const double MaxCoversZoom = 270;

        private double coversZoom = DefaultCoversZoom;
        public double CoversZoom
        {
            get
            {
                return coversZoom;
            }

            set
            {
                coversZoom = value;
                OnPropertyChanged();
            }
        }
    }        
}
