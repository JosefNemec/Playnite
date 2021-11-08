﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK.Models;

namespace Playnite
{
    public class FilterPreset : DatabaseObject
    {
        private FilterSettings settings;
        public FilterSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private SortOrder? sortingOrder;
        public SortOrder? SortingOrder
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

        private SortOrderDirection? sortingOrderDirection;
        public SortOrderDirection? SortingOrderDirection
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

        private GroupableField? groupingOrder;
        public GroupableField? GroupingOrder
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
        public bool ShowInFullscreeQuickSelection
        {
            get => showInFullscreeQuickSelection;
            set
            {
                showInFullscreeQuickSelection = value;
                OnPropertyChanged();
            }
        }

        public bool ShouldSerializeGroupingOrder()
        {
            return GroupingOrder != null;
        }

        public bool ShouldSerializeSortingOrderDirection()
        {
            return SortingOrderDirection != null;
        }

        public bool ShouldSerializeSortingOrder()
        {
            return SortingOrder != null;
        }

        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);
            if (target is FilterPreset tro)
            {
                if (!Settings.IsEqualJson(tro.Settings))
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

    public class FilterChangedEventArgs : EventArgs
    {
        public List<string> Fields
        {
            get; set;
        }

        public FilterChangedEventArgs()
        {
        }

        public FilterChangedEventArgs(List<string> fields)
        {
            Fields = fields;
        }
    }

    public class StringFilterItemProperites : ObservableObject
    {
        [JsonIgnore]
        public bool IsSet => Values.HasNonEmptyItems();

        private List<string> values;
        public List<string> Values
        {
            get => values;
            set
            {
                values = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSet));
            }
        }

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

        public bool Equals(StringFilterItemProperites obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }
    }

    public class EnumFilterItemProperites : ObservableObject
    {
        [JsonIgnore]
        public bool IsSet => Values.HasItems();

        private List<int> values;
        public List<int> Values
        {
            get => values;
            set
            {
                values = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSet));
            }
        }

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

        public bool Equals(EnumFilterItemProperites obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }
    }

    public class FilterItemProperites : ObservableObject
    {
        [JsonIgnore]
        public List<string> Texts { get; private set; }

        [JsonIgnore]
        public bool IsSet => !Text.IsNullOrEmpty() || Ids?.Any() == true;

        private List<Guid> ids;
        public List<Guid> Ids
        {
            get => ids;
            set
            {
                ids = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSet));
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                if (text?.Contains(Constants.ListSeparator) == true)
                {
                    Texts = text.Split(Constants.ListSeparators, StringSplitOptions.RemoveEmptyEntries).
                        Select(a => a.Trim()).Where(a => !a.IsNullOrEmpty()).ToList();
                }
                else
                {
                    Texts = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSet));
                OnPropertyChanged(nameof(Texts));
            }
        }

        public FilterItemProperites()
        {
        }

        public FilterItemProperites(List<Guid> ids)
        {
            Ids = ids;
        }

        public FilterItemProperites(Guid id)
        {
            Ids = new List<Guid>() { id };
        }

        public FilterItemProperites(string text)
        {
            Text = text;
        }

        public bool ShouldSerializeText()
        {
            return !Text.IsNullOrEmpty();
        }

        public bool ShouldSerializeIds()
        {
            return Ids.HasItems();
        }

        public bool Equals(FilterItemProperites obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Ids.IsListEqual(obj?.Ids) && Text == obj.Text;
        }
    }

    public class FilterSettings : ObservableObject
    {
        public const string MissingFieldString = "{}";

        [JsonIgnore]
        public bool SearchActive
        {
            get => !string.IsNullOrEmpty(Name);
        }

        [JsonIgnore]
        public bool IsActive
        {
            get
            {
                return
                    IsInstalled ||
                    IsUnInstalled ||
                    Hidden ||
                    Favorite ||
                    !string.IsNullOrEmpty(Name) ||
                    !string.IsNullOrEmpty(Version) ||
                    Series?.IsSet == true ||
                    Source?.IsSet == true ||
                    AgeRating?.IsSet == true ||
                    Region?.IsSet == true ||
                    Genre?.IsSet == true ||
                    Publisher?.IsSet == true ||
                    Developer?.IsSet == true ||
                    Category?.IsSet == true ||
                    Tag?.IsSet == true ||
                    Platform?.IsSet == true ||
                    Library?.IsSet == true ||
                    CompletionStatuses?.IsSet == true ||
                    UserScore?.IsSet == true ||
                    CriticScore?.IsSet == true ||
                    CommunityScore?.IsSet == true ||
                    LastActivity?.IsSet == true ||
                    Added?.IsSet == true ||
                    Modified?.IsSet == true ||
                    ReleaseYear?.IsSet == true ||
                    PlayTime?.IsSet == true ||
                    Feature?.IsSet == true;
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Name));
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(SearchActive));
                }
            }
        }

        private FilterItemProperites genre;
        public FilterItemProperites Genre
        {
            get
            {
                return genre;
            }

            set
            {
                if (genre != value)
                {
                    genre = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Genre));
                }
            }
        }

        private FilterItemProperites platforms;
        public FilterItemProperites Platform
        {
            get
            {
                return platforms;
            }

            set
            {
                if (platforms != value)
                {
                    platforms = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Platform));
                }
            }
        }

        private StringFilterItemProperites releaseDate;
        public StringFilterItemProperites ReleaseYear
        {
            get
            {
                return releaseDate;
            }

            set
            {
                if (releaseDate != value)
                {
                    releaseDate = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(ReleaseYear));
                }
            }
        }

        private string version;
        public string Version
        {
            get
            {
                return version;
            }

            set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Version));
                }
            }
        }

        private FilterItemProperites publishers;
        public FilterItemProperites Publisher
        {
            get
            {
                return publishers;
            }

            set
            {
                if (publishers != value)
                {
                    publishers = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Publisher));
                }
            }
        }

        private FilterItemProperites developers;
        public FilterItemProperites Developer
        {
            get
            {
                return developers;
            }

            set
            {
                if (developers != value)
                {
                    developers = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Developer));
                }
            }
        }

        private FilterItemProperites categories;
        public FilterItemProperites Category
        {
            get
            {
                return categories;
            }

            set
            {
                if (categories != value)
                {
                    categories = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Category));
                }
            }
        }

        private FilterItemProperites tags;
        public FilterItemProperites Tag
        {
            get
            {
                return tags;
            }

            set
            {
                if (tags != value)
                {
                    tags = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Tag));
                }
            }
        }

        private FilterItemProperites series;
        public FilterItemProperites Series
        {
            get
            {
                return series;
            }

            set
            {
                if (series != value)
                {
                    series = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Series));
                }
            }
        }

        private FilterItemProperites region;
        public FilterItemProperites Region
        {
            get
            {
                return region;
            }

            set
            {
                if (region != value)
                {
                    region = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Region));
                }
            }
        }

        private FilterItemProperites source;
        public FilterItemProperites Source
        {
            get
            {
                return source;
            }

            set
            {
                if (source != value)
                {
                    source = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Source));
                }
            }
        }

        private FilterItemProperites ageRating;
        public FilterItemProperites AgeRating
        {
            get
            {
                return ageRating;
            }

            set
            {
                if (ageRating != value)
                {
                    ageRating = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(AgeRating));
                }
            }
        }

        private bool isInstalled;
        public bool IsInstalled
        {
            get
            {
                return isInstalled;
            }

            set
            {
                if (isInstalled != value)
                {
                    isInstalled = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(IsInstalled));
                }
            }
        }

        private bool isUnInstalled;
        public bool IsUnInstalled
        {
            get
            {
                return isUnInstalled;
            }

            set
            {
                if (isUnInstalled != value)
                {
                    isUnInstalled = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(IsUnInstalled));
                }
            }
        }

        private bool hidden;
        public bool Hidden
        {
            get
            {
                return hidden;
            }

            set
            {
                if (hidden != value)
                {
                    hidden = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Hidden));
                }
            }
        }

        private bool favorite;
        public bool Favorite
        {
            get
            {
                return favorite;
            }

            set
            {
                if (favorite != value)
                {
                    favorite = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Favorite));
                }
            }
        }

        private FilterItemProperites library;
        public FilterItemProperites Library
        {
            get
            {
                return library;
            }

            set
            {
                if (library != value)
                {
                    library = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Library));
                }
            }
        }

        private FilterItemProperites completionStatus;
        public FilterItemProperites CompletionStatuses
        {
            get
            {
                return completionStatus;
            }

            set
            {
                if (completionStatus != value)
                {
                    completionStatus = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(CompletionStatuses));
                }
            }
        }

        private EnumFilterItemProperites userScore;
        public EnumFilterItemProperites UserScore
        {
            get
            {
                return userScore;
            }

            set
            {
                if (userScore != value)
                {
                    userScore = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(UserScore));
                }
            }
        }

        private EnumFilterItemProperites criticScore;
        public EnumFilterItemProperites CriticScore
        {
            get
            {
                return criticScore;
            }

            set
            {
                if (criticScore != value)
                {
                    criticScore = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(CriticScore));
                }
            }
        }

        private EnumFilterItemProperites communityScore;
        public EnumFilterItemProperites CommunityScore
        {
            get
            {
                return communityScore;
            }

            set
            {
                if (communityScore != value)
                {
                    communityScore = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(CommunityScore));
                }
            }
        }

        private EnumFilterItemProperites lastActivity;
        public EnumFilterItemProperites LastActivity
        {
            get
            {
                return lastActivity;
            }

            set
            {
                if (lastActivity != value)
                {
                    lastActivity = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(LastActivity));
                }
            }
        }

        private EnumFilterItemProperites added;
        public EnumFilterItemProperites Added
        {
            get
            {
                return added;
            }

            set
            {
                if (added != value)
                {
                    added = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Added));
                }
            }
        }

        private EnumFilterItemProperites modified;
        public EnumFilterItemProperites Modified
        {
            get
            {
                return modified;
            }

            set
            {
                if (modified != value)
                {
                    modified = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Modified));
                }
            }
        }

        private EnumFilterItemProperites playTime;
        public EnumFilterItemProperites PlayTime
        {
            get
            {
                return playTime;
            }

            set
            {
                if (playTime != value)
                {
                    playTime = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(PlayTime));
                }
            }
        }

        private FilterItemProperites feature;
        public FilterItemProperites Feature
        {
            get
            {
                return feature;
            }

            set
            {
                if (feature != value)
                {
                    feature = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(Feature));
                }
            }
        }

        [JsonIgnore]
        public bool SuppressFilterChanges { get; set; } = false;
        public event EventHandler<FilterChangedEventArgs> FilterChanged;

        public void OnFilterChanged(string field)
        {
            if (!SuppressFilterChanges)
            {
                FilterChanged?.Invoke(this, new FilterChangedEventArgs(new List<string>() { field }));
            }

            OnPropertyChanged(nameof(IsActive));
        }

        public void OnFilterChanged(List<string> fields)
        {
            if (!SuppressFilterChanges)
            {
                FilterChanged?.Invoke(this, new FilterChangedEventArgs(fields));
            }

            OnPropertyChanged(nameof(IsActive));
        }

        public void ClearFilters(bool notify = true)
        {
            SuppressFilterChanges = true;
            var filterChanges = new List<string>();

            if (Name != null)
            {
                Name = null;
                filterChanges.Add(nameof(Name));
            }

            if (Genre?.IsSet == true)
            {
                Genre = null;
                filterChanges.Add(nameof(Genre));
            }

            if (Platform?.IsSet == true)
            {
                Platform = null;
                filterChanges.Add(nameof(Platform));
            }

            if (ReleaseYear != null)
            {
                ReleaseYear = null;
                filterChanges.Add(nameof(ReleaseYear));
            }

            if (Version != null)
            {
                Version = null;
                filterChanges.Add(nameof(Version));
            }

            if (Publisher?.IsSet == true)
            {
                Publisher = null;
                filterChanges.Add(nameof(Publisher));
            }

            if (Developer?.IsSet == true)
            {
                Developer = null;
                filterChanges.Add(nameof(Developer));
            }

            if (Category?.IsSet == true)
            {
                Category = null;
                filterChanges.Add(nameof(Category));
            }

            if (Tag?.IsSet == true)
            {
                Tag = null;
                filterChanges.Add(nameof(Tag));
            }

            if (IsInstalled != false)
            {
                IsInstalled = false;
                filterChanges.Add(nameof(IsInstalled));
            }

            if (IsUnInstalled != false)
            {
                IsUnInstalled = false;
                filterChanges.Add(nameof(IsUnInstalled));
            }

            if (Hidden != false)
            {
                Hidden = false;
                filterChanges.Add(nameof(Hidden));
            }

            if (Favorite != false)
            {
                Favorite = false;
                filterChanges.Add(nameof(Favorite));
            }

            if (Series?.IsSet == true)
            {
                Series = null;
                filterChanges.Add(nameof(Series));
            }

            if (Region?.IsSet == true)
            {
                Region = null;
                filterChanges.Add(nameof(Region));
            }

            if (Source?.IsSet == true)
            {
                Source = null;
                filterChanges.Add(nameof(Source));
            }

            if (AgeRating?.IsSet == true)
            {
                AgeRating = null;
                filterChanges.Add(nameof(AgeRating));
            }

            if (Library != null)
            {
                Library = null;
                filterChanges.Add(nameof(Library));
            }

            if (CompletionStatuses != null)
            {
                CompletionStatuses = null;
                filterChanges.Add(nameof(CompletionStatuses));
            }

            if (UserScore != null)
            {
                UserScore = null;
                filterChanges.Add(nameof(UserScore));
            }

            if (CriticScore != null)
            {
                CriticScore = null;
                filterChanges.Add(nameof(CriticScore));
            }

            if (CommunityScore != null)
            {
                CommunityScore = null;
                filterChanges.Add(nameof(CommunityScore));
            }

            if (LastActivity != null)
            {
                LastActivity = null;
                filterChanges.Add(nameof(LastActivity));
            }

            if (Added != null)
            {
                Added = null;
                filterChanges.Add(nameof(Added));
            }

            if (Modified != null)
            {
                Modified = null;
                filterChanges.Add(nameof(Modified));
            }

            if (PlayTime != null)
            {
                PlayTime = null;
                filterChanges.Add(nameof(PlayTime));
            }

            if (Feature?.IsSet == true)
            {
                Feature = null;
                filterChanges.Add(nameof(Feature));
            }

            SuppressFilterChanges = false;
            if (notify)
            {
                OnFilterChanged(filterChanges);
            }
        }

        public void ApplyFilter(FilterSettings settings)
        {
            var filterChanges = new List<string>();
            SuppressFilterChanges = true;

            if (Name != settings.Name)
            {
                Name = settings.Name;
                filterChanges.Add(nameof(Name));
            }

            if (Genre?.Equals(settings.Genre) != true)
            {
                Genre = settings.Genre;
                filterChanges.Add(nameof(Genre));
            }

            if (Platform?.Equals(settings.Platform) != true)
            {
                Platform = settings.Platform;
                filterChanges.Add(nameof(Platform));
            }

            if (ReleaseYear?.Equals(settings.ReleaseYear) != true)
            {
                ReleaseYear = settings.ReleaseYear;
                filterChanges.Add(nameof(ReleaseYear));
            }

            if (Version != settings.Version)
            {
                Version = settings.Version;
                filterChanges.Add(nameof(Version));
            }

            if (Publisher?.Equals(settings.Publisher) != true)
            {
                Publisher = settings.Publisher;
                filterChanges.Add(nameof(Publisher));
            }

            if (Developer?.Equals(settings.Developer) != true)
            {
                Developer = settings.Developer;
                filterChanges.Add(nameof(Developer));
            }

            if (Category?.Equals(settings.Category) != true)
            {
                Category = settings.Category;
                filterChanges.Add(nameof(Category));
            }

            if (Tag?.Equals(settings.Tag) != true)
            {
                Tag = settings.Tag;
                filterChanges.Add(nameof(Tag));
            }

            if (IsInstalled != settings.IsInstalled)
            {
                IsInstalled = settings.IsInstalled;
                filterChanges.Add(nameof(IsInstalled));
            }

            if (IsUnInstalled != settings.IsUnInstalled)
            {
                IsUnInstalled = settings.IsUnInstalled;
                filterChanges.Add(nameof(IsUnInstalled));
            }

            if (Hidden != settings.Hidden)
            {
                Hidden = settings.Hidden;
                filterChanges.Add(nameof(Hidden));
            }

            if (Favorite != settings.Favorite)
            {
                Favorite = settings.Favorite;
                filterChanges.Add(nameof(Favorite));
            }

            if (Series?.Equals(settings.Series) != true)
            {
                Series = settings.Series;
                filterChanges.Add(nameof(Series));
            }

            if (Region?.Equals(settings.Region) != true)
            {
                Region = settings.Region;
                filterChanges.Add(nameof(Region));
            }

            if (Source?.Equals(settings.Source) != true)
            {
                Source = settings.Source;
                filterChanges.Add(nameof(Source));
            }

            if (AgeRating?.Equals(settings.AgeRating) != true)
            {
                AgeRating = settings.AgeRating;
                filterChanges.Add(nameof(AgeRating));
            }

            if (Library?.Equals(settings.Library) != true)
            {
                Library = settings.Library;
                filterChanges.Add(nameof(Library));
            }

            if (CompletionStatuses?.Equals(settings.CompletionStatuses) != true)
            {
                CompletionStatuses = settings.CompletionStatuses;
                filterChanges.Add(nameof(CompletionStatuses));
            }

            if (UserScore?.Equals(settings.UserScore) != true)
            {
                UserScore = settings.UserScore;
                filterChanges.Add(nameof(UserScore));
            }

            if (CriticScore?.Equals(settings.CriticScore) != true)
            {
                CriticScore = settings.CriticScore;
                filterChanges.Add(nameof(CriticScore));
            }

            if (CommunityScore?.Equals(settings.CommunityScore) != true)
            {
                CommunityScore = settings.CommunityScore;
                filterChanges.Add(nameof(CommunityScore));
            }

            if (LastActivity?.Equals(settings.LastActivity) != true)
            {
                LastActivity = settings.LastActivity;
                filterChanges.Add(nameof(LastActivity));
            }

            if (Added?.Equals(settings.Added) != true)
            {
                Added = settings.Added;
                filterChanges.Add(nameof(Added));
            }

            if (Modified?.Equals(settings.Modified) != true)
            {
                Modified = settings.Modified;
                filterChanges.Add(nameof(Modified));
            }

            if (PlayTime?.Equals(settings.PlayTime) != true)
            {
                PlayTime = settings.PlayTime;
                filterChanges.Add(nameof(PlayTime));
            }

            if (Feature?.Equals(settings.Feature) != true)
            {
                Feature = settings.Feature;
                filterChanges.Add(nameof(Feature));
            }

            SuppressFilterChanges = false;
            OnFilterChanged(filterChanges);
        }

        #region Serialization Conditions

        public bool ShouldSerializeName()
        {
            return !Name.IsNullOrEmpty();
        }

        public bool ShouldSerializeReleaseYear()
        {
            return ReleaseYear?.IsSet == true;
        }

        public bool ShouldSerializeVersion()
        {
            return !Version.IsNullOrEmpty();
        }

        public bool ShouldSerializeSeries()
        {
            return Series?.IsSet == true;
        }

        public bool ShouldSerializeSource()
        {
            return Source?.IsSet == true;
        }

        public bool ShouldSerializeAgeRating()
        {
            return AgeRating?.IsSet == true;
        }

        public bool ShouldSerializeRegion()
        {
            return Region?.IsSet == true;
        }

        public bool ShouldSerializeGenre()
        {
            return Genre?.IsSet == true;
        }

        public bool ShouldSerializePublisher()
        {
            return Publisher?.IsSet == true;
        }

        public bool ShouldSerializeDeveloper()
        {
            return Developer?.IsSet == true;
        }

        public bool ShouldSerializeCategory()
        {
            return Category?.IsSet == true;
        }

        public bool ShouldSerializeTag()
        {
            return Tag?.IsSet == true;
        }

        public bool ShouldSerializePlatform()
        {
            return Platform?.IsSet == true;
        }

        public bool ShouldSerializeLibrary()
        {
            return Library?.IsSet == true;
        }

        public bool ShouldSerializeCompletionStatuses()
        {
            return CompletionStatuses?.IsSet == true;
        }

        public bool ShouldSerializeUserScore()
        {
            return UserScore?.IsSet == true;
        }

        public bool ShouldSerializeCriticScore()
        {
            return CriticScore?.IsSet == true;
        }

        public bool ShouldSerializeCommunityScore()
        {
            return CommunityScore?.IsSet == true;
        }

        public bool ShouldSerializeLastActivity()
        {
            return LastActivity?.IsSet == true;
        }

        public bool ShouldSerializeAdded()
        {
            return Added?.IsSet == true;
        }

        public bool ShouldSerializeModified()
        {
            return Modified?.IsSet == true;
        }

        public bool ShouldSerializePlayTime()
        {
            return PlayTime?.IsSet == true;
        }

        public bool ShouldSerializeFeature()
        {
            return Feature?.IsSet == true;
        }

        #endregion Serialization Conditions
    }
}
