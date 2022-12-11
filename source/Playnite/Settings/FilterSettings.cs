using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.Common;
using SdkModels = Playnite.SDK.Models;

namespace Playnite
{
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

    public class StringFilterItemProperties : ObservableObject
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

        public StringFilterItemProperties()
        {
        }

        public StringFilterItemProperties(List<string> values)
        {
            Values = values;
        }

        public StringFilterItemProperties(string value)
        {
            Values = new List<string>() { value };
        }

        public bool Equals(StringFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }

        public bool Equals(SdkModels.StringFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }

        public SdkModels.StringFilterItemProperties ToSdkModel()
        {
            if (Values.HasItems())
            {
                return new SdkModels.StringFilterItemProperties(Values);
            }
            else
            {
                return null;
            }
        }

        public static StringFilterItemProperties FromSdkModel(SdkModels.StringFilterItemProperties sdk)
        {
            if (sdk != null)
            {
                if (sdk.Values.HasItems())
                {
                    return new StringFilterItemProperties(sdk.Values);
                }
            }

            return null;
        }
    }

    public class EnumFilterItemProperties : ObservableObject
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

        public EnumFilterItemProperties()
        {
        }

        public EnumFilterItemProperties(List<int> values)
        {
            Values = values;
        }

        public EnumFilterItemProperties(int value)
        {
            Values = new List<int>() { value };
        }

        public bool Equals(EnumFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }

        public bool Equals(SdkModels.EnumFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Values.IsListEqual(obj?.Values);
        }

        public SdkModels.EnumFilterItemProperties ToSdkModel()
        {
            if (Values.HasItems())
            {
                return new SdkModels.EnumFilterItemProperties(Values);
            }
            else
            {
                return null;
            }
        }

        public static EnumFilterItemProperties FromSdkModel(SdkModels.EnumFilterItemProperties sdk)
        {
            if (sdk != null)
            {
                if (sdk.Values.HasItems())
                {
                    return new EnumFilterItemProperties(sdk.Values);
                }
            }

            return null;
        }
    }

    public class IdItemFilterItemProperties : ObservableObject
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
                    Texts = new List<string> { text };
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSet));
                OnPropertyChanged(nameof(Texts));
            }
        }

        public IdItemFilterItemProperties()
        {
        }

        public IdItemFilterItemProperties(List<Guid> ids)
        {
            Ids = ids;
        }

        public IdItemFilterItemProperties(Guid id)
        {
            Ids = new List<Guid>() { id };
        }

        public IdItemFilterItemProperties(string text)
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

        public bool Equals(IdItemFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Ids.IsListEqual(obj?.Ids) && Text == obj.Text;
        }

        public bool Equals(SdkModels.IdItemFilterItemProperties obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Ids.IsListEqual(obj?.Ids) && Text == obj.Text;
        }

        public SdkModels.IdItemFilterItemProperties ToSdkModel()
        {
            if (!Text.IsNullOrEmpty())
            {
                return new SdkModels.IdItemFilterItemProperties(Text);
            }
            else if (Ids.HasItems())
            {
                return new SdkModels.IdItemFilterItemProperties(Ids);
            }
            else
            {
                return null;
            }
        }

        public static IdItemFilterItemProperties FromSdkModel(SdkModels.IdItemFilterItemProperties sdk)
        {
            if (sdk != null)
            {
                if (sdk.Ids.HasItems())
                {
                    return new IdItemFilterItemProperties(sdk.Ids);
                }
                else if (!sdk.Text.IsNullOrEmpty())
                {
                    return new IdItemFilterItemProperties(sdk.Text);
                }
            }

            return null;
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
                    RecentActivity?.IsSet == true ||
                    Added?.IsSet == true ||
                    Modified?.IsSet == true ||
                    ReleaseYear?.IsSet == true ||
                    PlayTime?.IsSet == true ||
                    InstallSize?.IsSet == true ||
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

        private IdItemFilterItemProperties genre;
        public IdItemFilterItemProperties Genre
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

        private IdItemFilterItemProperties platforms;
        public IdItemFilterItemProperties Platform
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

        private StringFilterItemProperties releaseDate;
        public StringFilterItemProperties ReleaseYear
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

        private IdItemFilterItemProperties publishers;
        public IdItemFilterItemProperties Publisher
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

        private IdItemFilterItemProperties developers;
        public IdItemFilterItemProperties Developer
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

        private IdItemFilterItemProperties categories;
        public IdItemFilterItemProperties Category
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

        private IdItemFilterItemProperties tags;
        public IdItemFilterItemProperties Tag
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

        private IdItemFilterItemProperties series;
        public IdItemFilterItemProperties Series
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

        private IdItemFilterItemProperties region;
        public IdItemFilterItemProperties Region
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

        private IdItemFilterItemProperties source;
        public IdItemFilterItemProperties Source
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

        private IdItemFilterItemProperties ageRating;
        public IdItemFilterItemProperties AgeRating
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

        private bool useAndFilteringStyle;
        public bool UseAndFilteringStyle
        {
            get
            {
                return useAndFilteringStyle;
            }

            set
            {
                if (useAndFilteringStyle != value)
                {
                    useAndFilteringStyle = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(UseAndFilteringStyle));
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

        private IdItemFilterItemProperties library;
        public IdItemFilterItemProperties Library
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

        private IdItemFilterItemProperties completionStatus;
        public IdItemFilterItemProperties CompletionStatuses
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

        private EnumFilterItemProperties userScore;
        public EnumFilterItemProperties UserScore
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

        private EnumFilterItemProperties criticScore;
        public EnumFilterItemProperties CriticScore
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

        private EnumFilterItemProperties communityScore;
        public EnumFilterItemProperties CommunityScore
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

        private EnumFilterItemProperties lastActivity;
        public EnumFilterItemProperties LastActivity
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

        private EnumFilterItemProperties recentActivity;
        public EnumFilterItemProperties RecentActivity
        {
            get
            {
                return recentActivity;
            }

            set
            {
                if (recentActivity != value)
                {
                    recentActivity = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(RecentActivity));
                }
            }
        }

        private EnumFilterItemProperties added;
        public EnumFilterItemProperties Added
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

        private EnumFilterItemProperties modified;
        public EnumFilterItemProperties Modified
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

        private EnumFilterItemProperties playTime;
        public EnumFilterItemProperties PlayTime
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

        private EnumFilterItemProperties installSize;
        public EnumFilterItemProperties InstallSize
        {
            get
            {
                return installSize;
            }

            set
            {
                if (installSize != value)
                {
                    installSize = value;
                    OnPropertyChanged();
                    OnFilterChanged(nameof(InstallSize));
                }
            }
        }

        private IdItemFilterItemProperties feature;
        public IdItemFilterItemProperties Feature
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

            if (UseAndFilteringStyle != false)
            {
                UseAndFilteringStyle = false;
                filterChanges.Add(nameof(UseAndFilteringStyle));
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

            if (RecentActivity != null)
            {
                RecentActivity = null;
                filterChanges.Add(nameof(RecentActivity));
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

            if (InstallSize != null)
            {
                InstallSize = null;
                filterChanges.Add(nameof(InstallSize));
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

        public SdkModels.FilterPresetSettings AsPresetSettings()
        {
            return new SdkModels.FilterPresetSettings
            {
                UseAndFilteringStyle = UseAndFilteringStyle,
                IsInstalled = IsInstalled,
                IsUnInstalled = IsUnInstalled,
                Hidden = Hidden,
                Favorite = Favorite,
                Name = Name,
                Version = Version,
                ReleaseYear = ReleaseYear?.ToSdkModel(),
                Genre = Genre?.ToSdkModel(),
                Platform = Platform?.ToSdkModel(),
                Publisher = Publisher?.ToSdkModel(),
                Developer = Developer?.ToSdkModel(),
                Category = Category?.ToSdkModel(),
                Tag = Tag?.ToSdkModel(),
                Series = Series?.ToSdkModel(),
                Region = Region?.ToSdkModel(),
                Source = Source?.ToSdkModel(),
                AgeRating = AgeRating?.ToSdkModel(),
                Library = Library?.ToSdkModel(),
                CompletionStatuses = CompletionStatuses?.ToSdkModel(),
                Feature = Feature?.ToSdkModel(),
                UserScore = UserScore?.ToSdkModel(),
                CriticScore = CriticScore?.ToSdkModel(),
                CommunityScore = CommunityScore?.ToSdkModel(),
                LastActivity = LastActivity?.ToSdkModel(),
                RecentActivity = RecentActivity?.ToSdkModel(),
                Added = Added?.ToSdkModel(),
                Modified = Modified?.ToSdkModel(),
                PlayTime = PlayTime?.ToSdkModel(),
                InstallSize = InstallSize?.ToSdkModel()
            };
        }

        public static FilterSettings FromSdkFilterSettings(SdkModels.FilterPresetSettings settings)
        {
            return new FilterSettings
            {
                IsInstalled = settings.IsInstalled,
                IsUnInstalled = settings.IsUnInstalled,
                Hidden = settings.Hidden,
                Favorite = settings.Favorite,
                Name = settings.Name,
                Version = settings.Version,
                ReleaseYear = StringFilterItemProperties.FromSdkModel(settings.ReleaseYear),
                Genre = IdItemFilterItemProperties.FromSdkModel(settings.Genre),
                Platform = IdItemFilterItemProperties.FromSdkModel(settings.Platform),
                Publisher = IdItemFilterItemProperties.FromSdkModel(settings.Publisher),
                Developer = IdItemFilterItemProperties.FromSdkModel(settings.Developer),
                Category = IdItemFilterItemProperties.FromSdkModel(settings.Category),
                Tag = IdItemFilterItemProperties.FromSdkModel(settings.Tag),
                Series = IdItemFilterItemProperties.FromSdkModel(settings.Series),
                Region = IdItemFilterItemProperties.FromSdkModel(settings.Region),
                Source = IdItemFilterItemProperties.FromSdkModel(settings.Source),
                AgeRating = IdItemFilterItemProperties.FromSdkModel(settings.AgeRating),
                Library = IdItemFilterItemProperties.FromSdkModel(settings.Library),
                CompletionStatuses = IdItemFilterItemProperties.FromSdkModel(settings.CompletionStatuses),
                Feature = IdItemFilterItemProperties.FromSdkModel(settings.Feature),
                UserScore = EnumFilterItemProperties.FromSdkModel(settings.UserScore),
                CriticScore = EnumFilterItemProperties.FromSdkModel(settings.CriticScore),
                CommunityScore = EnumFilterItemProperties.FromSdkModel(settings.CommunityScore),
                LastActivity = EnumFilterItemProperties.FromSdkModel(settings.LastActivity),
                RecentActivity = EnumFilterItemProperties.FromSdkModel(settings.RecentActivity),
                Added = EnumFilterItemProperties.FromSdkModel(settings.Added),
                Modified = EnumFilterItemProperties.FromSdkModel(settings.Modified),
                PlayTime = EnumFilterItemProperties.FromSdkModel(settings.PlayTime),
                InstallSize = EnumFilterItemProperties.FromSdkModel(settings.InstallSize)
            };
        }

        public void ApplyFilter(SdkModels.FilterPresetSettings settings)
        {
            var filterChanges = new List<string>();
            SuppressFilterChanges = true;

            if (UseAndFilteringStyle != settings.UseAndFilteringStyle)
            {
                UseAndFilteringStyle = settings.UseAndFilteringStyle;
                filterChanges.Add(nameof(UseAndFilteringStyle));
            }

            if (Name != settings.Name)
            {
                Name = settings.Name;
                filterChanges.Add(nameof(Name));
            }

            if (Genre?.Equals(settings.Genre) != true)
            {
                Genre = IdItemFilterItemProperties.FromSdkModel(settings.Genre);
                filterChanges.Add(nameof(Genre));
            }
            if (Platform?.Equals(settings.Platform) != true)
            {
                Platform = IdItemFilterItemProperties.FromSdkModel(settings.Platform);
                filterChanges.Add(nameof(Platform));
            }

            if (ReleaseYear?.Equals(settings.ReleaseYear) != true)
            {
                ReleaseYear = StringFilterItemProperties.FromSdkModel(settings.ReleaseYear);
                filterChanges.Add(nameof(ReleaseYear));
            }

            if (Version != settings.Version)
            {
                Version = settings.Version;
                filterChanges.Add(nameof(Version));
            }

            if (Publisher?.Equals(settings.Publisher) != true)
            {
                Publisher = IdItemFilterItemProperties.FromSdkModel(settings.Publisher);
                filterChanges.Add(nameof(Publisher));
            }

            if (Developer?.Equals(settings.Developer) != true)
            {
                Developer = IdItemFilterItemProperties.FromSdkModel(settings.Developer);
                filterChanges.Add(nameof(Developer));
            }

            if (Category?.Equals(settings.Category) != true)
            {
                Category = IdItemFilterItemProperties.FromSdkModel(settings.Category);
                filterChanges.Add(nameof(Category));
            }

            if (Tag?.Equals(settings.Tag) != true)
            {
                Tag = IdItemFilterItemProperties.FromSdkModel(settings.Tag);
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
                Series = IdItemFilterItemProperties.FromSdkModel(settings.Series);
                filterChanges.Add(nameof(Series));
            }

            if (Region?.Equals(settings.Region) != true)
            {
                Region = IdItemFilterItemProperties.FromSdkModel(settings.Region);
                filterChanges.Add(nameof(Region));
            }

            if (Source?.Equals(settings.Source) != true)
            {
                Source = IdItemFilterItemProperties.FromSdkModel(settings.Source);
                filterChanges.Add(nameof(Source));
            }

            if (AgeRating?.Equals(settings.AgeRating) != true)
            {
                AgeRating = IdItemFilterItemProperties.FromSdkModel(settings.AgeRating);
                filterChanges.Add(nameof(AgeRating));
            }

            if (Library?.Equals(settings.Library) != true)
            {
                Library = IdItemFilterItemProperties.FromSdkModel(settings.Library);
                filterChanges.Add(nameof(Library));
            }

            if (CompletionStatuses?.Equals(settings.CompletionStatuses) != true)
            {
                CompletionStatuses = IdItemFilterItemProperties.FromSdkModel(settings.CompletionStatuses);
                filterChanges.Add(nameof(CompletionStatuses));
            }

            if (UserScore?.Equals(settings.UserScore) != true)
            {
                UserScore = EnumFilterItemProperties.FromSdkModel(settings.UserScore);
                filterChanges.Add(nameof(UserScore));
            }

            if (CriticScore?.Equals(settings.CriticScore) != true)
            {
                CriticScore = EnumFilterItemProperties.FromSdkModel(settings.CriticScore);
                filterChanges.Add(nameof(CriticScore));
            }

            if (CommunityScore?.Equals(settings.CommunityScore) != true)
            {
                CommunityScore = EnumFilterItemProperties.FromSdkModel(settings.CommunityScore);
                filterChanges.Add(nameof(CommunityScore));
            }

            if (LastActivity?.Equals(settings.LastActivity) != true)
            {
                LastActivity = EnumFilterItemProperties.FromSdkModel(settings.LastActivity);
                filterChanges.Add(nameof(LastActivity));
            }

            if (RecentActivity?.Equals(settings.RecentActivity) != true)
            {
                RecentActivity = EnumFilterItemProperties.FromSdkModel(settings.RecentActivity);
                filterChanges.Add(nameof(RecentActivity));
            }

            if (Added?.Equals(settings.Added) != true)
            {
                Added = EnumFilterItemProperties.FromSdkModel(settings.Added);
                filterChanges.Add(nameof(Added));
            }

            if (Modified?.Equals(settings.Modified) != true)
            {
                Modified = EnumFilterItemProperties.FromSdkModel(settings.Modified);
                filterChanges.Add(nameof(Modified));
            }

            if (PlayTime?.Equals(settings.PlayTime) != true)
            {
                PlayTime = EnumFilterItemProperties.FromSdkModel(settings.PlayTime);
                filterChanges.Add(nameof(PlayTime));
            }

            if (InstallSize?.Equals(settings.InstallSize) != true)
            {
                InstallSize = EnumFilterItemProperties.FromSdkModel(settings.InstallSize);
                filterChanges.Add(nameof(InstallSize));
            }

            if (Feature?.Equals(settings.Feature) != true)
            {
                Feature = IdItemFilterItemProperties.FromSdkModel(settings.Feature);
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

        public bool ShouldSerializeRecentActivity()
        {
            return RecentActivity?.IsSet == true;
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

        public bool ShouldSerializeInstallSize()
        {
            return InstallSize?.IsSet == true;
        }

        public bool ShouldSerializeFeature()
        {
            return Feature?.IsSet == true;
        }

        #endregion Serialization Conditions
    }
}
