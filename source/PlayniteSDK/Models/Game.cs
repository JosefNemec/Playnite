using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Playnite.SDK.Data;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies <see cref="Game"/> fields.
    /// </summary>
    public enum GameField : int
    {
        ///
        BackgroundImage = 0,
        ///
        Description = 1,
        ///
        GenreIds = 2,
        ///
        Hidden = 3,
        ///
        Favorite = 4,
        ///
        Icon = 5,
        ///
        CoverImage = 6,
        ///
        InstallDirectory = 7,
        ///
        LastActivity = 9,
        ///
        SortingName = 10,
        ///
        Gameid = 11,
        ///
        PluginId = 12,
        ///
        PublisherIds = 16,
        ///
        DeveloperIds = 17,
        ///
        ReleaseDate = 18,
        ///
        CategoryIds = 19,
        ///
        TagIds = 20,
        ///
        Links = 21,
        ///
        IsInstalling = 22,
        ///
        IsUninstalling = 23,
        ///
        IsLaunching = 24,
        ///
        IsRunning = 25,
        ///
        IsInstalled = 26,
        ///
        IsCustomGame = 27,
        ///
        Playtime = 28,
        ///
        Added = 29,
        ///
        Modified = 30,
        ///
        PlayCount = 31,
        ///
        Version = 33,
        ///
        SourceId = 36,
        ///
        CompletionStatus = 37,
        ///
        UserScore = 38,
        ///
        CriticScore = 39,
        ///
        CommunityScore = 40,
        ///
        Genres = 41,
        ///
        Developers = 42,
        ///
        Publishers = 43,
        ///
        Tags = 44,
        ///
        Categories = 45,
        ///
        Source = 50,
        ///
        ReleaseYear = 51,
        ///
        PreScript = 53,
        ///
        PostScript = 54,
        ///
        Name = 55,
        ///
        Features = 56,
        ///
        FeatureIds = 57,
        ///
        UseGlobalPostScript = 58,
        ///
        UseGlobalPreScript = 59,
        ///
        UserScoreRating = 60,
        ///
        CommunityScoreRating = 61,
        ///
        CriticScoreRating = 62,
        ///
        UserScoreGroup = 63,
        ///
        CommunityScoreGroup = 64,
        ///
        CriticScoreGroup = 65,
        ///
        LastActivitySegment = 66,
        ///
        AddedSegment = 67,
        ///
        ModifiedSegment = 68,
        ///
        PlaytimeCategory = 69,
        ///
        InstallationStatus = 70,
        ///
        None = 71,
        ///
        GameStartedScript = 72,
        ///
        UseGlobalGameStartedScript = 73,
        ///
        Notes = 74,
        ///
        Manual = 75,
        ///
        GameActions = 76,
        ///
        IncludeLibraryPluginAction = 77,
        ///
        Roms = 78,
        ///
        AgeRatingIds = 79,
        ///
        AgeRatings = 80,
        ///
        SeriesIds = 81,
        ///
        Series = 82,
        ///
        RegionIds = 83,
        ///
        Regions = 84,
        ///
        PlatformIds = 85,
        ///
        Platforms = 86,
        ///
        CompletionStatusId = 87,
        ///
        OverrideInstallState = 88,
        ///
        InstallSize = 89,
        ///
        LastSizeScanDate = 90,
        ///
        RecentActivity = 91
    }

    /// <summary>
    /// Represents Playnite game object.
    /// </summary>
    public class Game : DatabaseObject
    {
        private string backgroundImage;
        /// <summary>
        /// Gets or sets background image. Local file path, HTTP URL or database file ids are supported.
        /// </summary>
        public string BackgroundImage
        {
            get
            {
                return backgroundImage;
            }

            set
            {
                backgroundImage = value;
                OnPropertyChanged();
            }
        }

        private string description;
        /// <summary>
        /// Gets or sets HTML game description.
        /// </summary>
        [DefaultValue("")]
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private string notes;
        /// <summary>
        /// Gets or sets user notes.
        /// </summary>
        [DefaultValue("")]
        public string Notes
        {
            get
            {
                return notes;
            }

            set
            {
                notes = value;
                OnPropertyChanged();
            }
        }

        private List<Guid> genreIds;
        /// <summary>
        /// Gets or sets list of genres.
        /// </summary>
        public List<Guid> GenreIds
        {
            get
            {
                return genreIds;
            }

            set
            {
                genreIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Genres));
            }
        }

        private bool hidden;
        /// <summary>
        /// Gets or sets value indicating if the game is hidden in library.
        /// </summary>
        public bool Hidden
        {
            get
            {
                return hidden;
            }

            set
            {
                hidden = value;
                OnPropertyChanged();
            }
        }

        private bool favorite;
        /// <summary>
        /// Gets or sets value indicating if the game is marked as favorite in library.
        /// </summary>
        public bool Favorite
        {
            get
            {
                return favorite;
            }

            set
            {
                favorite = value;
                OnPropertyChanged();
            }
        }

        private string icon;
        /// <summary>
        /// Gets or sets game icon. Local file path, HTTP URL or database file ids are supported.
        /// </summary>
        public string Icon
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

        private string coverImage;
        /// <summary>
        /// Gets or sets game cover image. Local file path, HTTP URL or database file ids are supported.
        /// </summary>
        public string CoverImage
        {
            get
            {
                return coverImage;
            }

            set
            {
                coverImage = value;
                OnPropertyChanged();
            }
        }

        private string installDirectory;
        /// <summary>
        /// Gets or sets game installation directory path.
        /// </summary>
        public string InstallDirectory
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

        private DateTime? lastActivity;
        /// <summary>
        /// Gets or sets last played date.
        /// </summary>
        public DateTime? LastActivity
        {
            get
            {
                return lastActivity;
            }

            set
            {
                lastActivity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastActivitySegment));
                OnPropertyChanged(nameof(RecentActivity));
            }
        }

        private string sortingName;
        /// <summary>
        /// Gets or sets optional name used for sorting the game by name.
        /// </summary>
        public string SortingName
        {
            get
            {
                return sortingName;
            }

            set
            {
                sortingName = value;
                OnPropertyChanged();
            }
        }

        private string gameId;
        /// <summary>
        /// Gets or sets provider id. For example game's Steam ID.
        /// </summary>
        public string GameId
        {
            get
            {
                return gameId;
            }

            set
            {
                gameId = value;
                OnPropertyChanged();
            }
        }

        private Guid pluginId = Guid.Empty;
        /// <summary>
        /// Gets or sets id of plugin responsible for handling this game.
        /// </summary>
        public Guid PluginId
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

        private bool includeLibraryPluginAction = true;
        /// <summary>
        /// Gets or sets id of plugin responsible for handling this game.
        /// </summary>
        public bool IncludeLibraryPluginAction
        {
            get
            {
                return includeLibraryPluginAction;
            }

            set
            {
                includeLibraryPluginAction = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GameAction> gameActions;
        /// <summary>
        /// Gets or sets list of additional game actions.
        /// </summary>
        public ObservableCollection<GameAction> GameActions
        {
            get
            {
                return gameActions;
            }

            set
            {
                gameActions = value;
                OnPropertyChanged();
            }
        }

        private List<Guid> platformIds;
        /// <summary>
        /// Gets or sets platform id.
        /// </summary>
        public List<Guid> PlatformIds
        {
            get
            {
                return platformIds;
            }

            set
            {
                platformIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Platforms));
            }
        }

        private List<Guid> publisherIds;
        /// <summary>
        /// Gets or sets list of publishers.
        /// </summary>
        public List<Guid> PublisherIds
        {
            get
            {
                return publisherIds;
            }

            set
            {
                publisherIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Publishers));
            }
        }

        private List<Guid> developerIds;
        /// <summary>
        /// Gets or sets list of developers.
        /// </summary>
        public List<Guid> DeveloperIds
        {
            get
            {
                return developerIds;
            }

            set
            {
                developerIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Developers));
            }
        }

        private ReleaseDate? releaseDate;
        /// <summary>
        /// Gets or set game's release date.
        /// </summary>
        public ReleaseDate? ReleaseDate
        {
            get
            {
                return releaseDate;
            }

            set
            {
                releaseDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReleaseYear));
            }
        }

        private List<Guid> categoryIds;
        /// <summary>
        /// Gets or sets game categories.
        /// </summary>
        public List<Guid> CategoryIds
        {
            get
            {
                return categoryIds;
            }

            set
            {
                categoryIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Categories));
            }
        }

        private List<Guid> tagIds;
        /// <summary>
        /// Gets or sets list of tags.
        /// </summary>
        public List<Guid> TagIds
        {
            get
            {
                return tagIds;
            }

            set
            {
                tagIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Tags));
            }
        }

        private List<Guid> featureIds;
        /// <summary>
        /// Gets or sets list of game features.
        /// </summary>
        public List<Guid> FeatureIds
        {
            get
            {
                return featureIds;
            }

            set
            {
                featureIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Features));
            }
        }

        private ObservableCollection<Link> links;
        /// <summary>
        /// Gets or sets list of game related web links.
        /// </summary>
        public ObservableCollection<Link> Links
        {
            get
            {
                return links;
            }

            set
            {
                links = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GameRom> roms;
        /// <summary>
        /// Gets or sets list of game ROM files.
        /// </summary>
        public ObservableCollection<GameRom> Roms
        {
            get
            {
                return roms;
            }

            set
            {
                roms = value;
                OnPropertyChanged();
            }
        }

        private bool isInstalling;
        /// <summary>
        /// Gets or sets value indicating whether a game is being installed.
        /// </summary>
        public bool IsInstalling
        {
            get => isInstalling;
            set
            {
                isInstalling = value;
                OnPropertyChanged();
            }
        }

        private bool isUninstalling;
        /// <summary>
        /// Gets or sets value indicating whether a game is being uninstalled.
        /// </summary>
        public bool IsUninstalling
        {
            get => isUninstalling;
            set
            {
                isUninstalling = value;
                OnPropertyChanged();
            }
        }

        private bool isLaunching;
        /// <summary>
        /// Gets or sets value indicating whether a game is being launched.
        /// </summary>
        public bool IsLaunching
        {
            get => isLaunching;
            set
            {
                isLaunching = value;
                OnPropertyChanged();
            }
        }

        private bool isRunning;
        /// <summary>
        /// Gets or sets value indicating whether a game is currently running.
        /// </summary>
        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }

        private bool isInstalled;
        /// <summary>
        /// Gets or sets value indicating whether a game is installed.
        /// </summary>
        public bool IsInstalled
        {
            get => isInstalled;
            set
            {
                isInstalled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InstallationStatus));
            }
        }

        private bool overrideInstallState;
        /// <summary>
        /// Gets or sets value indicating whether installation state set by integration plugin should be ignored.
        /// </summary>
        public bool OverrideInstallState
        {
            get => overrideInstallState;
            set
            {
                overrideInstallState = value;
                OnPropertyChanged();
            }
        }

        private ulong playtime = 0;
        /// <summary>
        /// Gets or sets played time in seconds.
        /// </summary>
        public ulong Playtime
        {
            get
            {
                return playtime;
            }

            set
            {
                playtime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlaytimeCategory));
            }
        }

        private DateTime? added;
        /// <summary>
        /// Gets or sets date when game was added to library.
        /// </summary>
        public DateTime? Added
        {
            get
            {
                return added;
            }

            set
            {
                added = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AddedSegment));
                OnPropertyChanged(nameof(RecentActivity));
            }
        }

        private DateTime? modified;
        /// <summary>
        /// Gets or sets date of last modification made to a game.
        /// </summary>
        public DateTime? Modified
        {
            get
            {
                return modified;
            }

            set
            {
                modified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModifiedSegment));
            }
        }

        private ulong playCount = 0;
        /// <summary>
        /// Gets or sets a number indicating how many times the game has been played.
        /// </summary>
        public ulong PlayCount
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

        private ulong? installSize = null;
        /// <summary>
        /// Gets or sets the install size in bytes of the game.
        /// </summary>
        public ulong? InstallSize
        {
            get
            {
                return installSize;
            }

            set
            {
                installSize = value;
                OnPropertyChanged();
            }
        }

        private DateTime? lastSizeScanDate;
        /// <summary>
        /// Gets or sets date of last date of install size scan made to a game.
        /// </summary>
        public DateTime? LastSizeScanDate
        {
            get
            {
                return lastSizeScanDate;
            }

            set
            {
                lastSizeScanDate = value;
                OnPropertyChanged();
            }
        }

        private List<Guid> seriesIds;
        /// <summary>
        /// Gets or sets game series.
        /// </summary>
        public List<Guid> SeriesIds
        {
            get
            {
                return seriesIds;
            }

            set
            {
                seriesIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Series));
            }
        }

        private string version;
        /// <summary>
        /// Gets or sets game version.
        /// </summary>
        public string Version
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

        private List<Guid> ageRatingIds;
        /// <summary>
        /// Gets or sets age rating for a game.
        /// </summary>
        public List<Guid> AgeRatingIds
        {
            get
            {
                return ageRatingIds;
            }

            set
            {
                ageRatingIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeRatings));
            }
        }

        private List<Guid> regionIds;
        /// <summary>
        /// Gets or sets game region.
        /// </summary>
        public List<Guid> RegionIds
        {
            get
            {
                return regionIds;
            }

            set
            {
                regionIds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Regions));
            }
        }

        private Guid sourceId;
        /// <summary>
        /// Gets or sets source of the game.
        /// </summary>
        public Guid SourceId
        {
            get
            {
                return sourceId;
            }

            set
            {
                sourceId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Source));
            }
        }

        private Guid completionStatusId;
        /// <summary>
        /// Gets or sets game completion status.
        /// </summary>
        public Guid CompletionStatusId
        {
            get
            {
                return completionStatusId;
            }

            set
            {
                completionStatusId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CompletionStatus));
            }
        }

        private int? userScore = null;
        /// <summary>
        /// Gets or sets user's rating score.
        /// </summary>
        public int? UserScore
        {
            get
            {
                return userScore;
            }

            set
            {
                userScore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UserScoreGroup));
                OnPropertyChanged(nameof(UserScoreRating));
            }
        }

        private int? criticScore = null;
        /// <summary>
        /// Gets or sets critic based rating score.
        /// </summary>
        public int? CriticScore
        {
            get
            {
                return criticScore;
            }

            set
            {
                criticScore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CriticScoreGroup));
                OnPropertyChanged(nameof(CriticScoreRating));
            }
        }

        private int? communityScore = null;
        /// <summary>
        /// Gets or sets community rating score.
        /// </summary>
        public int? CommunityScore
        {
            get
            {
                return communityScore;
            }

            set
            {
                communityScore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CommunityScoreGroup));
                OnPropertyChanged(nameof(CommunityScoreRating));
            }
        }

        private string preScript;
        /// <summary>
        /// Gets or sets pre-action script.
        /// </summary>
        [DefaultValue("")]
        public string PreScript
        {
            get => preScript;
            set
            {
                preScript = value;
                OnPropertyChanged();
            }
        }

        private string postScript;
        /// <summary>
        /// Gets or sets post-action script.
        /// </summary>
        [DefaultValue("")]
        public string PostScript
        {
            get => postScript;
            set
            {
                postScript = value;
                OnPropertyChanged();
            }
        }

        private string gameStartedScript;
        /// <summary>
        /// Gets or sets script to be executed after game started.
        /// </summary>
        [DefaultValue("")]
        public string GameStartedScript
        {
            get => gameStartedScript;
            set
            {
                gameStartedScript = value;
                OnPropertyChanged();
            }
        }

        private bool useGlobalPostScript = true;
        /// <summary>
        /// Gets or sets value indicating whether global post script should be executed.
        /// </summary>
        [DefaultValue(true)]
        public bool UseGlobalPostScript
        {
            get => useGlobalPostScript;
            set
            {
                useGlobalPostScript = value;
                OnPropertyChanged();
            }
        }

        private bool useGlobalPreScript = true;
        /// <summary>
        /// Gets or sets value indicating whether global pre script should be executed.
        /// </summary>
        [DefaultValue(true)]
        public bool UseGlobalPreScript
        {
            get => useGlobalPreScript;
            set
            {
                useGlobalPreScript = value;
                OnPropertyChanged();
            }
        }

        private bool useGameStartedScript = true;
        /// <summary>
        /// Gets or sets value indicating whether global pre script should be executed.
        /// </summary>
        [DefaultValue(true)]
        public bool UseGlobalGameStartedScript
        {
            get => useGameStartedScript;
            set
            {
                useGameStartedScript = value;
                OnPropertyChanged();
            }
        }

        private string manual;
        /// <summary>
        /// Gets or sets game manual.
        /// </summary>
        [DefaultValue("")]
        public string Manual
        {
            get
            {
                return manual;
            }

            set
            {
                manual = value;
                OnPropertyChanged();
            }
        }

        #region Expanded

        /// <summary>
        /// Gets game's genres.
        /// </summary>
        [DontSerialize]
        public List<Genre> Genres
        {
            get
            {
                if (genreIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Genre>(DatabaseReference?.Genres.Get(genreIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's developers.
        /// </summary>
        [DontSerialize]
        public List<Company> Developers
        {
            get
            {
                if (developerIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Company>(DatabaseReference?.Companies.Get(developerIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's publishers.
        /// </summary>
        [DontSerialize]
        public List<Company> Publishers
        {
            get
            {
                if (publisherIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Company>(DatabaseReference?.Companies.Get(publisherIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's tags.
        /// </summary>
        [DontSerialize]
        public List<Tag> Tags
        {
            get
            {
                if (tagIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Tag>(DatabaseReference?.Tags.Get(tagIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's features.
        /// </summary>
        [DontSerialize]
        public List<GameFeature> Features
        {
            get
            {
                if (featureIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<GameFeature>(DatabaseReference?.Features.Get(featureIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's categories.
        /// </summary>
        [DontSerialize]
        public List<Category> Categories
        {
            get
            {
                if (categoryIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Category>(DatabaseReference?.Categories.Get(categoryIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's platform.
        /// </summary>
        [DontSerialize]
        public List<Platform> Platforms
        {
            get
            {
                if (platformIds?.Any() == true && DatabaseReference != null)
                {
                    return DatabaseReference?.Platforms.Get(platformIds).OrderBy(a => a.Name).ToList();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's series.
        /// </summary>
        [DontSerialize]
        public List<Series> Series
        {
            get
            {
                if (seriesIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Series>(DatabaseReference?.Series.Get(seriesIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's age rating.
        /// </summary>
        [DontSerialize]
        public List<AgeRating> AgeRatings
        {
            get
            {
                if (ageRatingIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<AgeRating>(DatabaseReference?.AgeRatings.Get(ageRatingIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's region.
        /// </summary>
        [DontSerialize]
        public List<Region> Regions
        {
            get
            {
                if (regionIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Region>(DatabaseReference?.Regions.Get(regionIds).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's source.
        /// </summary>
        [DontSerialize]
        public GameSource Source
        {
            get => DatabaseReference?.Sources[sourceId];
        }

        /// <summary>
        /// Gets game's completion status.
        /// </summary>
        [DontSerialize]
        public CompletionStatus CompletionStatus
        {
            get => DatabaseReference?.CompletionStatuses[completionStatusId];
        }

        /// <summary>
        /// Gets game's release year.
        /// </summary>
        [DontSerialize]
        public int? ReleaseYear
        {
            get => ReleaseDate?.Year;
        }

        /// <summary>
        /// Gets the most recent date between the last played and added dates.
        /// </summary>
        [DontSerialize]
        public DateTime? RecentActivity
        {
            get => GetGameRecentActivity();
        }

        /// <summary>
        /// Gets game's user score rating.
        /// </summary>
        [DontSerialize]
        public ScoreRating UserScoreRating
        {
            get => GetScoreRating(UserScore);
        }

        /// <summary>
        /// Gets game's community score rating.
        /// </summary>
        [DontSerialize]
        public ScoreRating CommunityScoreRating
        {
            get => GetScoreRating(CommunityScore);
        }

        /// <summary>
        /// Gets game's critic score rating.
        /// </summary>
        [DontSerialize]
        public ScoreRating CriticScoreRating
        {
            get => GetScoreRating(CriticScore);
        }

        /// <summary>
        /// Gets game's user score group.
        /// </summary>
        [DontSerialize]
        public ScoreGroup UserScoreGroup
        {
            get => GetScoreGroup(UserScore);
        }

        /// <summary>
        /// Gets game's community score group.
        /// </summary>
        [DontSerialize]
        public ScoreGroup CommunityScoreGroup
        {
            get => GetScoreGroup(CommunityScore);
        }

        /// <summary>
        /// Gets game's critic score group.
        /// </summary>
        [DontSerialize]
        public ScoreGroup CriticScoreGroup
        {
            get => GetScoreGroup(CriticScore);
        }

        /// <summary>
        /// Gets time segment for games last activity.
        /// </summary>
        [DontSerialize]
        public PastTimeSegment LastActivitySegment
        {
            get => GetPastTimeSegment(LastActivity);
        }

        /// <summary>
        /// Gets time segment for games recent activity.
        /// </summary>
        [DontSerialize]
        public PastTimeSegment RecentActivitySegment
        {
            get => GetPastTimeSegment(RecentActivity);
        }

        /// <summary>
        /// Gets time segment for games added date.
        /// </summary>
        [DontSerialize]
        public PastTimeSegment AddedSegment
        {
            get => GetPastTimeSegment(Added);
        }

        /// <summary>
        /// Gets time segment for games modified date..
        /// </summary>
        [DontSerialize]
        public PastTimeSegment ModifiedSegment
        {
            get => GetPastTimeSegment(Modified);
        }

        /// <summary>
        /// Gets game's play time category.
        /// </summary>
        [DontSerialize]
        public PlaytimeCategory PlaytimeCategory
        {
            get => GetPlayTimeCategory(Playtime);
        }

        /// <summary>
        /// Gets game's install size group.
        /// </summary>
        [DontSerialize]
        public InstallSizeGroup InstallSizeGroup
        {
            get => GetInstallSizeGroup();
        }

        /// <summary>
        /// Gets value indicating whether the game is custom game.
        /// </summary>
        [DontSerialize]
        public bool IsCustomGame
        {
            get => PluginId == Guid.Empty;
        }

        /// <summary>
        /// Gets game installation state.
        /// </summary>
        [DontSerialize]
        public InstallationStatus InstallationStatus
        {
            get => IsInstalled ? InstallationStatus.Installed : InstallationStatus.Uninstalled;
        }

        #endregion Expanded

        /// <summary>
        /// Gets the most recent date between the last played and added dates.
        /// </summary>
        /// <returns></returns>
        public DateTime? GetGameRecentActivity()
        {
            if (lastActivity == null)
            {
                return added;
            }
            else if (added == null || lastActivity > added)
            {
                return lastActivity;
            }
            else
            {
                return added;
            }
        }

        /// <summary>
        /// Gets play time category.
        /// </summary>
        /// <param name="seconds">Play time in seconds.</param>
        /// <returns></returns>
        private PlaytimeCategory GetPlayTimeCategory(ulong seconds)
        {
            if (seconds == 0)
            {
                return PlaytimeCategory.NotPlayed;
            }

            var hours = seconds / 3600;
            if (hours < 1)
            {
                return PlaytimeCategory.LessThenHour;
            }
            else if (hours >= 1 && hours <= 10)
            {
                return PlaytimeCategory.O1_10;
            }
            else if (hours >= 10 && hours <= 100)
            {
                return PlaytimeCategory.O10_100;
            }
            else if (hours >= 100 && hours <= 500)
            {
                return PlaytimeCategory.O100_500;
            }
            else if (hours >= 500 && hours <= 1000)
            {
                return PlaytimeCategory.O500_1000;
            }
            else
            {
                return PlaytimeCategory.O1000plus;
            }
        }

        /// <summary>
        /// Gets time segment.
        /// </summary>
        /// <param name="dateTime">Date time to be measured.</param>
        /// <returns></returns>
        private PastTimeSegment GetPastTimeSegment(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return PastTimeSegment.Never;
            }

            if (dateTime.Value.Date == DateTime.Today)
            {
                return PastTimeSegment.Today;
            }

            if (dateTime.Value.Date.AddDays(1) == DateTime.Today)
            {
                return PastTimeSegment.Yesterday;
            }

            var diff = DateTime.Now - dateTime.Value;
            if (diff.TotalDays < 7)
            {
                return PastTimeSegment.PastWeek;
            }

            if (diff.TotalDays < 31)
            {
                return PastTimeSegment.PastMonth;
            }

            if (diff.TotalDays < 365)
            {
                return PastTimeSegment.PastYear;
            }

            return PastTimeSegment.MoreThenYear;
        }

        /// <summary>
        /// Gets score rating.
        /// </summary>
        /// <param name="score">Score.</param>
        /// <returns></returns>
        private ScoreRating GetScoreRating(int? score)
        {
            if (score == null)
            {
                return ScoreRating.None;
            }
            else if (score > 75)
            {
                return ScoreRating.Positive;
            }
            else if (score > 25)
            {
                return ScoreRating.Mixed;
            }
            else
            {
                return ScoreRating.Negative;
            }
        }

        /// <summary>
        /// Gets score group.
        /// </summary>
        /// <param name="score">Score.</param>
        /// <returns></returns>
        private ScoreGroup GetScoreGroup(int? score)
        {
            if (score >= 0 && score < 10)
            {
                return ScoreGroup.O0x;
            }

            if (score >= 10 && score < 20)
            {
                return ScoreGroup.O1x;
            }

            if (score >= 20 && score < 30)
            {
                return ScoreGroup.O2x;
            }

            if (score >= 30 && score < 40)
            {
                return ScoreGroup.O3x;
            }

            if (score >= 40 && score < 50)
            {
                return ScoreGroup.O4x;
            }

            if (score >= 50 && score < 60)
            {
                return ScoreGroup.O5x;
            }

            if (score >= 60 && score < 70)
            {
                return ScoreGroup.O6x;
            }

            if (score >= 70 && score < 80)
            {
                return ScoreGroup.O7x;
            }

            if (score >= 80 && score < 90)
            {
                return ScoreGroup.O8x;
            }

            if (score >= 90)
            {
                return ScoreGroup.O9x;
            }

            return ScoreGroup.None;
        }

        /// <summary>
        /// Gets or sets game database reference.
        /// </summary>
        internal static IGameDatabase DatabaseReference
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of a Game object.
        /// </summary>
        public Game() : base()
        {
            GameId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates new instance of a Game object with specific name.
        /// </summary>
        /// <param name="name">Game name.</param>
        public Game(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Game GetCopy()
        {
            return new Game
            {
                Id = Id,
                GameId = GameId,
                PluginId = PluginId,
                Name = Name,
                Icon = Icon,
                CoverImage = CoverImage,
                BackgroundImage = BackgroundImage,
                Description = Description,
                Notes = Notes,
                Hidden = Hidden,
                Favorite = Favorite,
                InstallDirectory = InstallDirectory,
                LastActivity = LastActivity,
                SortingName = SortingName,
                ReleaseDate = ReleaseDate,
                IsInstalled = IsInstalled,
                IsInstalling = IsInstalling,
                IsLaunching = IsLaunching,
                IsUninstalling = IsUninstalling,
                IsRunning = IsRunning,
                Playtime = Playtime,
                Added = Added,
                Modified = Modified,
                PlayCount = PlayCount,
                InstallSize = InstallSize,
                LastSizeScanDate = LastSizeScanDate,
                Version = Version,
                GenreIds = GenreIds?.ToList(),
                PlatformIds = PlatformIds?.ToList(),
                PublisherIds = PublisherIds?.ToList(),
                DeveloperIds = DeveloperIds?.ToList(),
                CategoryIds = CategoryIds?.ToList(),
                TagIds = TagIds?.ToList(),
                FeatureIds = FeatureIds?.ToList(),
                SeriesIds = SeriesIds?.ToList(),
                AgeRatingIds = AgeRatingIds?.ToList(),
                RegionIds = RegionIds?.ToList(),
                SourceId = SourceId,
                CompletionStatusId = CompletionStatusId,
                UserScore = UserScore,
                CriticScore = CriticScore,
                CommunityScore = CommunityScore,
                PreScript = PreScript,
                PostScript = PostScript,
                GameStartedScript = GameStartedScript,
                UseGlobalPostScript = UseGlobalPostScript,
                UseGlobalPreScript = UseGlobalPreScript,
                UseGlobalGameStartedScript = UseGlobalGameStartedScript,
                Manual = Manual,
                IncludeLibraryPluginAction = IncludeLibraryPluginAction,
                OverrideInstallState = OverrideInstallState,
                GameActions = GameActions?.Select(a => a.GetCopy()).ToObservable(),
                Links = Links?.Select(a => a.GetCopy()).ToObservable(),
                Roms = Roms?.Select(a => a.GetCopy()).ToObservable(),
            };
        }

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is Game tro)
            {
                if (!string.Equals(BackgroundImage, tro.BackgroundImage, StringComparison.Ordinal))
                {
                    tro.BackgroundImage = BackgroundImage;
                }

                if (!string.Equals(Description, tro.Description, StringComparison.Ordinal))
                {
                    tro.Description = Description;
                }

                if (!string.Equals(Notes, tro.Notes, StringComparison.Ordinal))
                {
                    tro.Notes = Notes;
                }

                if (!GenreIds.IsListEqual(tro.GenreIds))
                {
                    tro.GenreIds = GenreIds;
                }

                if (Hidden != tro.Hidden)
                {
                    tro.Hidden = Hidden;
                }

                if (Favorite != tro.Favorite)
                {
                    tro.Favorite = Favorite;
                }

                if (!string.Equals(Icon, tro.Icon, StringComparison.Ordinal))
                {
                    tro.Icon = Icon;
                }

                if (!string.Equals(CoverImage, tro.CoverImage, StringComparison.Ordinal))
                {
                    tro.CoverImage = CoverImage;
                }

                if (!string.Equals(InstallDirectory, tro.InstallDirectory, StringComparison.Ordinal))
                {
                    tro.InstallDirectory = InstallDirectory;
                }

                if (LastActivity != tro.LastActivity)
                {
                    tro.LastActivity = LastActivity;
                }

                if (!string.Equals(SortingName, tro.SortingName, StringComparison.Ordinal))
                {
                    tro.SortingName = SortingName;
                }

                if (!string.Equals(GameId, tro.GameId, StringComparison.Ordinal))
                {
                    tro.GameId = GameId;
                }

                if (PluginId != tro.PluginId)
                {
                    tro.PluginId = PluginId;
                }

                if (!GameActions.IsListEqualExact(tro.GameActions))
                {
                    tro.GameActions = GameActions;
                }

                if (!PlatformIds.IsListEqual(tro.PlatformIds))
                {
                    tro.PlatformIds = PlatformIds;
                }

                if (!PublisherIds.IsListEqual(tro.PublisherIds))
                {
                    tro.PublisherIds = PublisherIds;
                }

                if (!DeveloperIds.IsListEqual(tro.DeveloperIds))
                {
                    tro.DeveloperIds = DeveloperIds;
                }

                if (ReleaseDate != tro.ReleaseDate)
                {
                    tro.ReleaseDate = ReleaseDate;
                }

                if (!CategoryIds.IsListEqual(tro.CategoryIds))
                {
                    tro.CategoryIds = CategoryIds;
                }

                if (!TagIds.IsListEqual(tro.TagIds))
                {
                    tro.TagIds = TagIds;
                }

                if (!FeatureIds.IsListEqual(tro.FeatureIds))
                {
                    tro.FeatureIds = FeatureIds;
                }

                if (!Links.IsListEqualExact(tro.Links))
                {
                    tro.Links = Links;
                }

                if (!Roms.IsListEqualExact(tro.Roms))
                {
                    tro.Roms = Roms;
                }

                if (IsInstalling != tro.IsInstalling)
                {
                    tro.IsInstalling = IsInstalling;
                }

                if (IsUninstalling != tro.IsUninstalling)
                {
                    tro.IsUninstalling = IsUninstalling;
                }

                if (IsLaunching != tro.IsLaunching)
                {
                    tro.IsLaunching = IsLaunching;
                }

                if (IsRunning != tro.IsRunning)
                {
                    tro.IsRunning = IsRunning;
                }

                if (IsInstalled != tro.IsInstalled)
                {
                    tro.IsInstalled = IsInstalled;
                }

                if (Playtime != tro.Playtime)
                {
                    tro.Playtime = Playtime;
                }

                if (Added != tro.Added)
                {
                    tro.Added = Added;
                }

                if (Modified != tro.Modified)
                {
                    tro.Modified = Modified;
                }

                if (PlayCount != tro.PlayCount)
                {
                    tro.PlayCount = PlayCount;
                }

                if (InstallSize != tro.InstallSize)
                {
                    tro.InstallSize = InstallSize;
                }

                if (LastSizeScanDate != tro.lastSizeScanDate)
                {
                    tro.LastSizeScanDate = LastSizeScanDate;
                }

                if (!SeriesIds.IsListEqual(tro.SeriesIds))
                {
                    tro.SeriesIds = SeriesIds;
                }

                if (Version != tro.Version)
                {
                    tro.Version = Version;
                }

                if (!AgeRatingIds.IsListEqual(tro.AgeRatingIds))
                {
                    tro.AgeRatingIds = AgeRatingIds;
                }

                if (!RegionIds.IsListEqual(tro.RegionIds))
                {
                    tro.RegionIds = RegionIds;
                }

                if (SourceId != tro.SourceId)
                {
                    tro.SourceId = SourceId;
                }

                if (CompletionStatusId != tro.CompletionStatusId)
                {
                    tro.CompletionStatusId = CompletionStatusId;
                }

                if (UserScore != tro.UserScore)
                {
                    tro.UserScore = UserScore;
                }

                if (CriticScore != tro.CriticScore)
                {
                    tro.CriticScore = CriticScore;
                }

                if (CommunityScore != tro.CommunityScore)
                {
                    tro.CommunityScore = CommunityScore;
                }

                if (!string.Equals(PreScript, tro.PreScript, StringComparison.Ordinal))
                {
                    tro.PreScript = PreScript;
                }

                if (!string.Equals(PostScript, tro.PostScript, StringComparison.Ordinal))
                {
                    tro.PostScript = PostScript;
                }

                if (!string.Equals(GameStartedScript, tro.GameStartedScript, StringComparison.Ordinal))
                {
                    tro.GameStartedScript = GameStartedScript;
                }

                if (UseGlobalPostScript != tro.UseGlobalPostScript)
                {
                    tro.UseGlobalPostScript = UseGlobalPostScript;
                }

                if (UseGlobalPreScript != tro.UseGlobalPreScript)
                {
                    tro.UseGlobalPreScript = UseGlobalPreScript;
                }

                if (UseGlobalGameStartedScript != tro.UseGlobalGameStartedScript)
                {
                    tro.UseGlobalGameStartedScript = UseGlobalGameStartedScript;
                }

                if (!string.Equals(Manual, tro.Manual, StringComparison.Ordinal))
                {
                    tro.Manual = Manual;
                }

                if (IncludeLibraryPluginAction != tro.IncludeLibraryPluginAction)
                {
                    tro.IncludeLibraryPluginAction = IncludeLibraryPluginAction;
                }

                if (OverrideInstallState != tro.OverrideInstallState)
                {
                    tro.OverrideInstallState = OverrideInstallState;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }

        /// <summary>
        /// Gets differences in game objects.
        /// </summary>
        /// <param name="otherGame">Game object to compare to.</param>
        /// <returns>List of fields that differ.</returns>
        public List<GameField> GetDifferences(Game otherGame)
        {
            var changes = new List<GameField>();
            if (!string.Equals(BackgroundImage, otherGame.BackgroundImage, StringComparison.Ordinal))
            {
                changes.Add(GameField.BackgroundImage);
            }

            if (!string.Equals(Description, otherGame.Description, StringComparison.Ordinal))
            {
                changes.Add(GameField.Description);
            }

            if (!string.Equals(Notes, otherGame.Notes, StringComparison.Ordinal))
            {
                changes.Add(GameField.Notes);
            }

            if (!GenreIds.IsListEqual(otherGame.GenreIds))
            {
                changes.Add(GameField.GenreIds);
                changes.Add(GameField.Genres);
            }

            if (Hidden != otherGame.Hidden)
            {
                changes.Add(GameField.Hidden);
            }

            if (Favorite != otherGame.Favorite)
            {
                changes.Add(GameField.Favorite);
            }

            if (!string.Equals(Icon, otherGame.Icon, StringComparison.Ordinal))
            {
                changes.Add(GameField.Icon);
            }

            if (!string.Equals(CoverImage, otherGame.CoverImage, StringComparison.Ordinal))
            {
                changes.Add(GameField.CoverImage);
            }

            if (!string.Equals(InstallDirectory, otherGame.InstallDirectory, StringComparison.Ordinal))
            {
                changes.Add(GameField.InstallDirectory);
            }

            if (LastActivity != otherGame.LastActivity)
            {
                changes.Add(GameField.LastActivity);
                if (LastActivitySegment != otherGame.LastActivitySegment)
                {
                    changes.Add(GameField.LastActivitySegment);
                }
            }

            if (!string.Equals(SortingName, otherGame.SortingName, StringComparison.Ordinal))
            {
                changes.Add(GameField.SortingName);
            }

            if (!string.Equals(GameId, otherGame.GameId, StringComparison.Ordinal))
            {
                changes.Add(GameField.Gameid);
            }

            if (PluginId != otherGame.PluginId)
            {
                changes.Add(GameField.PluginId);
            }

            if (!GameActions.IsListEqualExact(otherGame.GameActions))
            {
                changes.Add(GameField.GameActions);
            }

            if (!PlatformIds.IsListEqual(otherGame.PlatformIds))
            {
                changes.Add(GameField.PlatformIds);
                changes.Add(GameField.Platforms);
            }

            if (!PublisherIds.IsListEqual(otherGame.PublisherIds))
            {
                changes.Add(GameField.PublisherIds);
                changes.Add(GameField.Publishers);
            }

            if (!DeveloperIds.IsListEqual(otherGame.DeveloperIds))
            {
                changes.Add(GameField.DeveloperIds);
                changes.Add(GameField.Developers);
            }

            if (ReleaseDate != otherGame.ReleaseDate)
            {
                changes.Add(GameField.ReleaseDate);
                if (ReleaseYear != otherGame.ReleaseYear)
                {
                    changes.Add(GameField.ReleaseYear);
                }
            }

            if (!CategoryIds.IsListEqual(otherGame.CategoryIds))
            {
                changes.Add(GameField.CategoryIds);
                changes.Add(GameField.Categories);
            }

            if (!TagIds.IsListEqual(otherGame.TagIds))
            {
                changes.Add(GameField.TagIds);
                changes.Add(GameField.Tags);
            }

            if (!FeatureIds.IsListEqual(otherGame.FeatureIds))
            {
                changes.Add(GameField.FeatureIds);
                changes.Add(GameField.Features);
            }

            if (!Links.IsListEqualExact(otherGame.Links))
            {
                changes.Add(GameField.Links);
            }

            if (!Roms.IsListEqualExact(otherGame.Roms))
            {
                changes.Add(GameField.Roms);
            }

            if (IsInstalling != otherGame.IsInstalling)
            {
                changes.Add(GameField.IsInstalling);
            }

            if (IsUninstalling != otherGame.IsUninstalling)
            {
                changes.Add(GameField.IsUninstalling);
            }

            if (IsLaunching != otherGame.IsLaunching)
            {
                changes.Add(GameField.IsLaunching);
            }

            if (IsRunning != otherGame.IsRunning)
            {
                changes.Add(GameField.IsRunning);
            }

            if (IsInstalled != otherGame.IsInstalled)
            {
                changes.Add(GameField.IsInstalled);
                changes.Add(GameField.InstallationStatus);
            }

            if (Playtime != otherGame.Playtime)
            {
                changes.Add(GameField.Playtime);
                if (PlaytimeCategory != otherGame.PlaytimeCategory)
                {
                    changes.Add(GameField.PlaytimeCategory);
                }
            }

            if (Added != otherGame.Added)
            {
                changes.Add(GameField.Added);
                if (AddedSegment != otherGame.AddedSegment)
                {
                    changes.Add(GameField.AddedSegment);
                }
            }

            if (Modified != otherGame.Modified)
            {
                changes.Add(GameField.Modified);
                if (Modified != otherGame.Modified)
                {
                    changes.Add(GameField.ModifiedSegment);
                }
            }

            if (PlayCount != otherGame.PlayCount)
            {
                changes.Add(GameField.PlayCount);
            }

            if (InstallSize != otherGame.InstallSize)
            {
                changes.Add(GameField.InstallSize);
            }

            if (LastSizeScanDate != otherGame.LastSizeScanDate)
            {
                changes.Add(GameField.LastSizeScanDate);
            }

            if (!SeriesIds.IsListEqual(otherGame.SeriesIds))
            {
                changes.Add(GameField.SeriesIds);
                changes.Add(GameField.Series);
            }

            if (Version != otherGame.Version)
            {
                changes.Add(GameField.Version);
            }

            if (!AgeRatingIds.IsListEqual(otherGame.AgeRatingIds))
            {
                changes.Add(GameField.AgeRatingIds);
                changes.Add(GameField.AgeRatings);
            }

            if (!RegionIds.IsListEqual(otherGame.RegionIds))
            {
                changes.Add(GameField.RegionIds);
                changes.Add(GameField.Regions);
            }

            if (SourceId != otherGame.SourceId)
            {
                changes.Add(GameField.SourceId);
                changes.Add(GameField.Source);
            }

            if (CompletionStatusId != otherGame.CompletionStatusId)
            {
                changes.Add(GameField.CompletionStatusId);
                changes.Add(GameField.CompletionStatus);
            }

            if (UserScore != otherGame.UserScore)
            {
                changes.Add(GameField.UserScore);
                if (UserScoreGroup != otherGame.UserScoreGroup)
                {
                    changes.Add(GameField.UserScoreGroup);
                }

                if (UserScoreRating != otherGame.UserScoreRating)
                {
                    changes.Add(GameField.UserScoreRating);
                }
            }

            if (CriticScore != otherGame.CriticScore)
            {
                changes.Add(GameField.CriticScore);
                if (CriticScoreGroup != otherGame.CriticScoreGroup)
                {
                    changes.Add(GameField.CriticScoreGroup);
                }

                if (CriticScoreRating != otherGame.CriticScoreRating)
                {
                    changes.Add(GameField.CriticScoreRating);
                }
            }

            if (CommunityScore != otherGame.CommunityScore)
            {
                changes.Add(GameField.CommunityScore);
                if (CommunityScoreGroup != otherGame.CommunityScoreGroup)
                {
                    changes.Add(GameField.CommunityScoreGroup);
                }

                if (CommunityScoreRating != otherGame.CommunityScoreRating)
                {
                    changes.Add(GameField.CommunityScoreRating);
                }
            }

            if (!string.Equals(PreScript, otherGame.PreScript, StringComparison.Ordinal))
            {
                changes.Add(GameField.PreScript);
            }

            if (!string.Equals(PostScript, otherGame.PostScript, StringComparison.Ordinal))
            {
                changes.Add(GameField.PostScript);
            }

            if (!string.Equals(GameStartedScript, otherGame.GameStartedScript, StringComparison.Ordinal))
            {
                changes.Add(GameField.GameStartedScript);
            }

            if (UseGlobalPostScript != otherGame.UseGlobalPostScript)
            {
                changes.Add(GameField.UseGlobalPostScript);
            }

            if (UseGlobalPreScript != otherGame.UseGlobalPreScript)
            {
                changes.Add(GameField.UseGlobalPreScript);
            }

            if (UseGlobalGameStartedScript != otherGame.UseGlobalGameStartedScript)
            {
                changes.Add(GameField.UseGlobalGameStartedScript);
            }

            if (!string.Equals(Manual, otherGame.Manual, StringComparison.Ordinal))
            {
                changes.Add(GameField.Manual);
            }

            if (IncludeLibraryPluginAction != otherGame.IncludeLibraryPluginAction)
            {
                changes.Add(GameField.IncludeLibraryPluginAction);
            }

            if (OverrideInstallState != otherGame.OverrideInstallState)
            {
                changes.Add(GameField.OverrideInstallState);
            }

            return changes;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public char GetNameGroup()
        {
            var nameMatch = string.IsNullOrEmpty(SortingName) ? Name : SortingName;
            if (string.IsNullOrEmpty(nameMatch))
            {
                return '#';
            }
            else
            {
                var firstChar = char.ToUpper(nameMatch[0]);
                return char.IsLetter(firstChar) ? firstChar : '#';
            }
        }

        /// <summary>
        /// Gets game Install Size group.
        /// </summary>
        public InstallSizeGroup GetInstallSizeGroup()
        {
            if (installSize == null || installSize == 0)
            {
                return InstallSizeGroup.None;
            }
            else if (installSize <= 0x6400000) //100MB
            {
                return InstallSizeGroup.S0_0MB_100MB;
            }
            else if (installSize <= 0x40000000) //1GB
            {
                return InstallSizeGroup.S1_100MB_1GB;
            }
            else if (installSize <= 0x140000000) //5GB
            {
                return InstallSizeGroup.S2_1GB_5GB;
            }
            else if (installSize <= 0x280000000) //10GB
            {
                return InstallSizeGroup.S3_5GB_10GB;
            }
            else if (installSize <= 0x500000000) //20GB
            {
                return InstallSizeGroup.S4_10GB_20GB;
            }
            else if (installSize <= 0xA00000000) //40GB
            {
                return InstallSizeGroup.S5_20GB_40GB;
            }
            else if (installSize <= 0x1900000000) //100GB
            {
                return InstallSizeGroup.S6_40GB_100GB;
            }

            return InstallSizeGroup.S7_100GBPlus;
        }

        /// <summary>
        /// Gets game Install Drive group.
        /// </summary>
        public string GetInstallDriveGroup()
        {
            var installDrive = GetInstallDrive();
            if (string.IsNullOrEmpty(installDrive))
            {
                return ResourceProvider.GetString("LOCNone");
            }
            else
            {
                return installDrive;
            }
        }

        /// <summary>
        /// Gets game Install Drive.
        /// </summary>
        public string GetInstallDrive()
        {
            if (!isInstalled)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(InstallDirectory))
            {
                return string.Empty;
            }

            try
            {
                return Path.GetPathRoot(InstallDirectory).ToUpperInvariant();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
