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
using Newtonsoft.Json;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies <see cref="Game"/> fields.
    /// </summary>
    public enum GameField
    {
        ///
        BackgroundImage,
        ///
        Description,
        ///
        GenreIds,
        ///
        Hidden,
        ///
        Favorite,
        ///
        Icon,
        ///
        CoverImage,
        ///
        InstallDirectory,
        ///
        GameImagePath,
        ///
        LastActivity,
        ///
        SortingName,
        ///
        Gameid,
        ///
        PluginId,
        ///
        OtherActions,
        ///
        PlayAction,
        ///
        PlatformId,
        ///
        PublisherIds,
        ///
        DeveloperIds,
        ///
        ReleaseDate,
        ///
        CategoryIds,
        ///
        TagIds,
        ///
        Links,
        ///
        IsInstalling,
        ///
        IsUninstalling,
        ///
        IsLaunching,
        ///
        IsRunning,
        ///
        IsInstalled,
        ///
        IsCustomGame,
        ///
        Playtime,
        ///
        Added,
        ///
        Modified,
        ///
        PlayCount,
        ///
        SeriesId,
        ///
        Version,
        ///
        AgeRatingId,
        ///
        RegionId,
        ///
        SourceId,
        ///
        CompletionStatus,
        ///
        UserScore,
        ///
        CriticScore,
        ///
        CommunityScore,
        ///
        Genres,
        ///
        Developers,
        ///
        Publishers,
        ///
        Tags,
        ///
        Categories,
        ///
        Platform,
        ///
        Series,
        ///
        AgeRating,
        ///
        Region,
        ///
        Source,
        ///
        ReleaseYear,
        ///
        ActionsScriptLanguage,
        ///
        PreScript,
        ///
        PostScript,
        ///
        Name,
        ///
        Features,
        ///
        FeatureIds,
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
        /// Gets or sets avlue indicating if the game is marked as favorite in library.
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
                if (string.IsNullOrEmpty(installDirectory))
                {
                    if (PlayAction != null)
                    {
                        return PlayAction.WorkingDir;
                    }
                }

                return installDirectory;
            }

            set
            {
                installDirectory = value;
                OnPropertyChanged();
            }
        }

        private string gameImagePath;
        /// <summary>
        /// Gets or sets game's ISO, ROM or other type of executable image path.
        /// </summary>
        public string GameImagePath
        {
            get
            {
                return gameImagePath;
            }

            set
            {
                gameImagePath = value;
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

        private ObservableCollection<GameAction> otherActions;
        /// <summary>
        /// Gets or sets list of additional game actions.
        /// </summary>
        public ObservableCollection<GameAction> OtherActions
        {
            get
            {
                return otherActions;
            }

            set
            {
                otherActions = value;
                OnPropertyChanged();
            }
        }

        private GameAction playAction;
        /// <summary>
        /// Gets or sets game action used to starting the game.
        /// </summary>
        public GameAction PlayAction
        {
            get
            {
                return playAction;
            }

            set
            {
                playAction = value;
                OnPropertyChanged();
            }
        }

        private Guid platformId;
        /// <summary>
        /// Gets or sets platform id.
        /// </summary>
        public Guid PlatformId
        {
            get
            {
                return platformId;
            }

            set
            {
                platformId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Platform));
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

        private DateTime? releaseDate;
        /// <summary>
        /// Gets or set game's release date.
        /// </summary>
        public DateTime? ReleaseDate
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

        private bool isInstalling;
        /// <summary>
        /// Gets or sets value indicating wheter a game is being installed..
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
        /// Gets or sets value indicating wheter a game is being uninstalled.
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
        /// Gets or sets value indicating wheter a game is being launched.
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
        /// Gets or sets value indicating wheter a game is currently running.
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
        /// Gets or sets value indicating wheter a game is installed.
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

        private long playtime = 0;
        /// <summary>
        /// Gets or sets played time in seconds.
        /// </summary>
        public long Playtime
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
        /// Gets or sets date when game was added into library.
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

        private long playCount = 0;
        /// <summary>
        /// Gets or sets a number indicating how many times the game has been played.
        /// </summary>
        public long PlayCount
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

        private Guid seriesId;
        /// <summary>
        /// Gets or sets game series.
        /// </summary>
        public Guid SeriesId
        {
            get
            {
                return seriesId;
            }

            set
            {
                seriesId = value;
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

        private Guid ageRatingId;
        /// <summary>
        /// Gets or sets age rating for a game.
        /// </summary>
        public Guid AgeRatingId
        {
            get
            {
                return ageRatingId;
            }

            set
            {
                ageRatingId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeRating));
            }
        }

        private Guid regionId;
        /// <summary>
        /// Gets or sets game region.
        /// </summary>
        public Guid RegionId
        {
            get
            {
                return regionId;
            }

            set
            {
                regionId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Region));
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

        private CompletionStatus completionStatus = CompletionStatus.NotPlayed;
        /// <summary>
        /// Gets or sets game completion status.
        /// </summary>
        public CompletionStatus CompletionStatus
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

        private ScriptLanguage actionsScriptLanguage = ScriptLanguage.PowerShell;
        /// <summary>
        /// Gets or sets scripting language for <see cref="PreScript"/> and <see cref="PostScript"/> scripts.
        /// </summary>
        public ScriptLanguage ActionsScriptLanguage
        {
            get => actionsScriptLanguage;
            set
            {
                actionsScriptLanguage = value;
                OnPropertyChanged();
            }
        }

        private string preScript;
        /// <summary>
        /// Gets or sets pre-action script.
        /// </summary>
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
        public string PostScript
        {
            get => postScript;
            set
            {
                postScript = value;
                OnPropertyChanged();
            }
        }

        #region Expanded

        /// <summary>
        /// Gets game's genres.
        /// </summary>
        [JsonIgnore]
        public List<Genre> Genres
        {
            get
            {
                if (genreIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Genre>(DatabaseReference?.Genres.Where(a => genreIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's developers.
        /// </summary>
        [JsonIgnore]
        public List<Company> Developers
        {
            get
            {
                if (developerIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Company>(DatabaseReference?.Companies.Where(a => developerIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's publishers.
        /// </summary>
        [JsonIgnore]
        public List<Company> Publishers
        {
            get
            {
                if (publisherIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Company>(DatabaseReference?.Companies.Where(a => publisherIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's tags.
        /// </summary>
        [JsonIgnore]
        public List<Tag> Tags
        {
            get
            {
                if (tagIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Tag>(DatabaseReference?.Tags.Where(a => tagIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's features.
        /// </summary>
        [JsonIgnore]
        public List<GameFeature> Features
        {
            get
            {
                if (featureIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<GameFeature>(DatabaseReference?.Features.Where(a => featureIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's categories.
        /// </summary>
        [JsonIgnore]
        public List<Category> Categories
        {
            get
            {
                if (categoryIds?.Any() == true && DatabaseReference != null)
                {
                    return new List<Category>(DatabaseReference?.Categories.Where(a => categoryIds.Contains(a.Id)).OrderBy(a => a.Name));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets game's platform.
        /// </summary>
        [JsonIgnore]
        public Platform Platform
        {
            get => DatabaseReference?.Platforms[platformId];
        }

        /// <summary>
        /// Gets game's series.
        /// </summary>
        [JsonIgnore]
        public Series Series
        {
            get => DatabaseReference?.Series[seriesId];
        }

        /// <summary>
        /// Get game's age rating.
        /// </summary>
        [JsonIgnore]
        public AgeRating AgeRating
        {
            get => DatabaseReference?.AgeRatings[ageRatingId];
        }

        /// <summary>
        /// Gets game's region.
        /// </summary>
        [JsonIgnore]
        public Region Region
        {
            get => DatabaseReference?.Regions[regionId];
        }

        /// <summary>
        /// Gets game's source.
        /// </summary>
        [JsonIgnore]
        public GameSource Source
        {
            get => DatabaseReference?.Sources[sourceId];
        }

        /// <summary>
        /// Gets game's release year.
        /// </summary>
        [JsonIgnore]
        public int? ReleaseYear
        {
            get => ReleaseDate?.Year;
        }

        /// <summary>
        /// Gets game's user score rating.
        /// </summary>
        [JsonIgnore]
        public ScoreRating UserScoreRating
        {
            get => GetScoreRating(UserScore);
        }

        /// <summary>
        /// Gets game's community score rating.
        /// </summary>
        [JsonIgnore]
        public ScoreRating CommunityScoreRating
        {
            get => GetScoreRating(CommunityScore);
        }

        /// <summary>
        /// Gets game's critic score rating.
        /// </summary>
        [JsonIgnore]
        public ScoreRating CriticScoreRating
        {
            get => GetScoreRating(CriticScore);
        }

        /// <summary>
        /// Gets game's user score group.
        /// </summary>
        [JsonIgnore]
        public ScoreGroup UserScoreGroup
        {
            get => GetScoreGroup(UserScore);
        }

        /// <summary>
        /// Gets game's community score group.
        /// </summary>
        [JsonIgnore]
        public ScoreGroup CommunityScoreGroup
        {
            get => GetScoreGroup(CommunityScore);
        }

        /// <summary>
        /// Gets game's critic score group.
        /// </summary>
        [JsonIgnore]
        public ScoreGroup CriticScoreGroup
        {
            get => GetScoreGroup(CriticScore);
        }

        /// <summary>
        /// Gets time segment for games last activity.
        /// </summary>
        [JsonIgnore]
        public PastTimeSegment LastActivitySegment
        {
            get => GetPastTimeSegment(LastActivity);
        }

        /// <summary>
        /// Gets time segment for games added date.
        /// </summary>
        [JsonIgnore]
        public PastTimeSegment AddedSegment
        {
            get => GetPastTimeSegment(Added);
        }

        /// <summary>
        /// Gets time segment for games modified date..
        /// </summary>
        [JsonIgnore]
        public PastTimeSegment ModifiedSegment
        {
            get => GetPastTimeSegment(Modified);
        }

        /// <summary>
        /// Gets game's play time category.
        /// </summary>
        [JsonIgnore]
        public PlaytimeCategory PlaytimeCategory
        {
            get => GetPlayTimeCategory(Playtime);
        }

        /// <summary>
        /// Gets value indicating wheter the game is custom game.
        /// </summary>
        [JsonIgnore]
        public bool IsCustomGame
        {
            get => PluginId == Guid.Empty;
        }

        /// <summary>
        /// Gets game installation state.
        /// </summary>
        [JsonIgnore]
        public InstallationStatus InstallationStatus
        {
            get => IsInstalled ? InstallationStatus.Installed : InstallationStatus.Uninstalled;
        }

        #endregion Expanded

        /// <summary>
        /// Gets play time category.
        /// </summary>
        /// <param name="seconds">Play time in seconds.</param>
        /// <returns></returns>
        private PlaytimeCategory GetPlayTimeCategory(long seconds)
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

                if (!string.Equals(GameImagePath, tro.GameImagePath, StringComparison.Ordinal))
                {
                    tro.GameImagePath = GameImagePath;
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

                if (!OtherActions.IsListEqualExact(tro.OtherActions))
                {
                    tro.OtherActions = OtherActions;
                }

                if (!GameAction.Equals(PlayAction, tro.PlayAction))
                {
                    tro.PlayAction = PlayAction;
                }

                if (PlatformId != tro.PlatformId)
                {
                    tro.PlatformId = PlatformId;
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

                if (SeriesId != tro.SeriesId)
                {
                    tro.SeriesId = SeriesId;
                }

                if (Version != tro.Version)
                {
                    tro.Version = Version;
                }

                if (AgeRatingId != tro.AgeRatingId)
                {
                    tro.AgeRatingId = AgeRatingId;
                }

                if (RegionId != tro.RegionId)
                {
                    tro.RegionId = RegionId;
                }

                if (SourceId != tro.SourceId)
                {
                    tro.SourceId = SourceId;
                }

                if (CompletionStatus != tro.CompletionStatus)
                {
                    tro.CompletionStatus = CompletionStatus;
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

                if (ActionsScriptLanguage != tro.ActionsScriptLanguage)
                {
                    tro.ActionsScriptLanguage = ActionsScriptLanguage;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
