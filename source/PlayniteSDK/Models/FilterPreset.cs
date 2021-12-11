using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum ViewSortOrder
    {
        Name = 0,
        Platforms = 1,
        Library = 2,
        Categories = 3,
        LastActivity = 4,
        Genres = 5,
        ReleaseDate = 6,
        Developers = 7,
        Publishers = 8,
        Tags = 9,
        Series = 10,
        AgeRatings = 11,
        Version = 12,
        Regions = 13,
        Source = 14,
        PlayCount = 15,
        Playtime = 16,
        CompletionStatus = 17,
        UserScore = 18,
        CriticScore = 19,
        CommunityScore = 20,
        Added = 21,
        Modified = 22,
        IsInstalled = 23,
        Hidden = 24,
        Favorite = 25,
        InstallDirectory = 26,
        Features = 27
    }

    public enum ViewSortOrderDirection
    {
        Ascending = 0,
        Descending = 1
    }

    public enum ViewGroupField
    {
        None = 0,
        Platform = 1,
        Library = 2,
        Category = 3,
        LastActivity = 4,
        Genre = 5,
        ReleaseYear = 6,
        Developer = 7,
        Publisher = 8,
        Tag = 9,
        Series = 10,
        AgeRating = 11,
        Region = 12,
        Source = 13,
        PlayTime = 14,
        CompletionStatus = 15,
        UserScore = 16,
        CriticScore = 17,
        CommunityScore = 18,
        Added = 19,
        Modified = 20,
        Feature = 21,
        InstallationStatus = 22,
        Name = 23
    }

    public class FilterPresetSettings
    {
        public bool UseAndFilteringStyle { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsUnInstalled { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public StringFilterItemProperites ReleaseYear { get; set; }
        public IdItemFilterItemProperites Genre { get; set; }
        public IdItemFilterItemProperites Platform { get; set; }
        public IdItemFilterItemProperites Publisher { get; set; }
        public IdItemFilterItemProperites Developer { get; set; }
        public IdItemFilterItemProperites Category { get; set; }
        public IdItemFilterItemProperites Tag { get; set; }
        public IdItemFilterItemProperites Series { get; set; }
        public IdItemFilterItemProperites Region { get; set; }
        public IdItemFilterItemProperites Source { get; set; }
        public IdItemFilterItemProperites AgeRating { get; set; }
        public IdItemFilterItemProperites Library { get; set; }
        public IdItemFilterItemProperites CompletionStatuses { get; set; }
        public IdItemFilterItemProperites Feature { get; set; }
        public EnumFilterItemProperites UserScore { get; set; }
        public EnumFilterItemProperites CriticScore { get; set; }
        public EnumFilterItemProperites CommunityScore { get; set; }
        public EnumFilterItemProperites LastActivity { get; set; }
        public EnumFilterItemProperites Added { get; set; }
        public EnumFilterItemProperites Modified { get; set; }
        public EnumFilterItemProperites PlayTime { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    public class IdItemFilterItemProperites
    {
        public List<Guid> Ids { get; set; }
        public string Text { get; set; }

        public IdItemFilterItemProperites()
        {
        }

        public IdItemFilterItemProperites(List<Guid> ids)
        {
            Ids = ids;
        }

        public IdItemFilterItemProperites(Guid id)
        {
            Ids = new List<Guid>() { id };
        }

        public IdItemFilterItemProperites(string text)
        {
            Text = text;
        }
    }

    public class StringFilterItemProperites
    {
        public List<string> Values { get; set; }

        public StringFilterItemProperites()
        {
        }

        public StringFilterItemProperites(List<string> values)
        {
            Values = values;
        }

        public StringFilterItemProperites(string value)
        {
            Values = new List<string>() { value };
        }
    }

    public class EnumFilterItemProperites : ObservableObject
    {
        public List<int> Values { get; set; }

        public EnumFilterItemProperites()
        {
        }

        public EnumFilterItemProperites(List<int> values)
        {
            Values = values;
        }

        public EnumFilterItemProperites(int value)
        {
            Values = new List<int>() { value };
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class FilterPreset : DatabaseObject
    {
        private FilterPresetSettings settings;
        /// <summary>
        ///
        /// </summary>
        public FilterPresetSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private ViewSortOrder? sortingOrder;
        /// <summary>
        ///
        /// </summary>
        public ViewSortOrder? SortingOrder
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

        private ViewSortOrderDirection? sortingOrderDirection;
        /// <summary>
        ///
        /// </summary>
        public ViewSortOrderDirection? SortingOrderDirection
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

        private ViewGroupField? groupingOrder;
        /// <summary>
        ///
        /// </summary>
        public ViewGroupField? GroupingOrder
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

        private bool showInFullscreeQuickSelection = true;
        /// <summary>
        ///
        /// </summary>
        public bool ShowInFullscreeQuickSelection
        {
            get => showInFullscreeQuickSelection;
            set
            {
                showInFullscreeQuickSelection = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);
            if (target is FilterPreset tro)
            {
                if (!Data.Serialization.AreObjectsEqual(Settings, tro.Settings))
                {
                    tro.Settings = Settings;
                }

                if (SortingOrder != tro.SortingOrder)
                {
                    tro.SortingOrder = SortingOrder;
                }

                if (SortingOrderDirection != tro.SortingOrderDirection)
                {
                    tro.SortingOrderDirection = SortingOrderDirection;
                }

                if (GroupingOrder != tro.GroupingOrder)
                {
                    tro.GroupingOrder = GroupingOrder;
                }

                if (ShowInFullscreeQuickSelection != tro.ShowInFullscreeQuickSelection)
                {
                    tro.ShowInFullscreeQuickSelection = ShowInFullscreeQuickSelection;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
