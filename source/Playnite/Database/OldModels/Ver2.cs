using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.OldModels
{
    public enum Ver2_ScriptLanguage
    {
        /// <summary>
        /// Represents PowerShell scripting language.
        /// </summary>
        PowerShell,
        /// <summary>
        /// Represents IronPython scripting language.
        /// </summary>
        IronPython,
        /// <summary>
        /// Represents Batch scripting language.
        /// </summary>
        [Description("Batch (.bat script)")]
        Batch
    }

    public class Ver2_ImportExclusionList
    {
        public List<Ver2_ImportExclusionItem> Items { get; set; }
    }

    public class Ver2_ImportExclusionItem
    {
        public string GameId { get; set; }
        public string GameName { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
    }

    public interface Ver2_IIdentifiable
    {
        /// <summary>
        /// Gets unique object identifier.
        /// </summary>
        Guid Id { get; }
    }

    public class Ver2_DatabaseObject : ObservableObject, IComparable, Ver2_IIdentifiable
    {
        /// <summary>
        /// Gets or sets identifier of database object.
        /// </summary>
        public Guid Id { get; set; }

        private string name;
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name
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

        /// <summary>
        /// Creates new instance of <see cref="Ver2_DatabaseObject"/>.
        /// </summary>
        public Ver2_DatabaseObject()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Compares Names of database object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var objName = (obj as Ver2_DatabaseObject).Name;
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(objName))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(Name))
            {
                return 1;
            }

            if (string.IsNullOrEmpty(objName))
            {
                return -1;
            }

            return string.Compare(Name, objName, true);
        }

        /// <summary>
        /// DO NOT use for actual equality check, this only checks if db Ids are equal!
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Ver2_DatabaseObject dbObj)
            {
                return dbObj.Id == Id;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Id == Guid.Empty)
            {
                return 0;
            }
            else
            {
                return Id.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }

    public class Ver2_AgeRating : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_AgeRating"/>.
        /// </summary>
        public Ver2_AgeRating() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_AgeRating"/>.
        /// </summary>
        /// <param name="name">Rating name.</param>
        public Ver2_AgeRating(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty age rating.
        /// </summary>
        public static readonly Ver2_AgeRating Empty = new Ver2_AgeRating { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_AppSoftware : Ver2_DatabaseObject
    {
        private string icon;
        /// <summary>
        /// Gets or sets application icon.
        /// </summary>
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string arguments;
        /// <summary>
        /// Gets or sets application arguments.
        /// </summary>
        public string Arguments
        {
            get => arguments;
            set
            {
                arguments = value;
                OnPropertyChanged();
            }
        }

        private string path;
        /// <summary>
        /// Gets or sets application path.
        /// </summary>
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        private string workingDir;
        /// <summary>
        /// Gets or sets application working directory.
        /// </summary>
        public string WorkingDir
        {
            get => workingDir;
            set
            {
                workingDir = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_AppSoftware"/>.
        /// </summary>
        public Ver2_AppSoftware() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_AppSoftware"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_AppSoftware(string name) : base()
        {
            Name = name;
        }
    }

    public class Ver2_Category : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Category"/>.
        /// </summary>
        public Ver2_Category() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Category"/>.
        /// </summary>
        /// <param name="name">Category name.</param>
        public Ver2_Category(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty category.
        /// </summary>
        public static readonly Ver2_Category Empty = new Ver2_Category { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_Company : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Company"/>.
        /// </summary>
        public Ver2_Company() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Company"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Company(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly Ver2_Company Empty = new Ver2_Company { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_Developer : Ver2_Company
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Developer"/>.
        /// </summary>
        public Ver2_Developer() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Developer"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Developer(string name) : base()
        {
        }
    }

    public class Ver2_Publisher : Ver2_Company
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Publisher"/>.
        /// </summary>
        public Ver2_Publisher() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Publisher"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Publisher(string name) : base()
        {
        }
    }

    public enum Ver2_CompletionStatus : int
    {
        [Description("Not Played")]
        NotPlayed = 0,
        /// <summary>
        /// Represents Played completion status.
        /// </summary>
        [Description("Played")]
        Played = 1,
        /// <summary>
        /// Represents Beaten completion status.
        /// </summary>
        [Description("Beaten")]
        Beaten = 2,
        /// <summary>
        /// Represents Completed completion status.
        /// </summary>
        [Description("Completed")]
        Completed = 3,
        /// <summary>
        /// Represents Playing completion status.
        /// </summary>
        [Description("Playing")]
        Playing = 4,
        /// <summary>
        /// Represents Abandoned completion status.
        /// </summary>
        [Description("Abandoned")]
        Abandoned = 5,
        /// <summary>
        /// Represents "On hold" completion status.
        /// </summary>
        [Description("On Hold")]
        OnHold = 6,
        /// <summary>
        /// Represents "Plan to Play" completion status.
        /// </summary>
        [Description("Plan to Play")]
        PlanToPlay = 7
    }

    public class Ver2_EmulatorProfile : Ver2_DatabaseObject, IEquatable<Ver2_EmulatorProfile>
    {
        private List<Guid> platforms;
        /// <summary>
        /// Gets or sets platforms supported by profile.
        /// </summary>
        public List<Guid> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
                OnPropertyChanged();
            }
        }

        private List<string> imageExtensions;
        /// <summary>
        /// Gets or sets file extension supported by profile.
        /// </summary>
        public List<string> ImageExtensions
        {
            get => imageExtensions;
            set
            {
                imageExtensions = value;
                OnPropertyChanged();
            }
        }

        private string executable;
        /// <summary>
        /// Gets or sets executable path used to launch emulator.
        /// </summary>
        public string Executable
        {
            get => executable;
            set
            {
                executable = value;
                OnPropertyChanged();
            }
        }

        private string arguments;
        /// <summary>
        /// Gets or sets arguments for emulator executable.
        /// </summary>
        public string Arguments
        {
            get => arguments;
            set
            {
                arguments = value;
                OnPropertyChanged();
            }
        }

        private string workingDirectory;
        /// <summary>
        /// Gets or sets working directory of emulator process.
        /// </summary>
        public string WorkingDirectory
        {
            get => workingDirectory;
            set
            {
                workingDirectory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates new instance of EmulatorProfile.
        /// </summary>
        public Ver2_EmulatorProfile() : base()
        {
        }

        /// <inheritdoc/>
        public bool Equals(Ver2_EmulatorProfile other)
        {
            if (other is null)
            {
                return false;
            }

            if (!Platforms.IsListEqual(other.Platforms))
            {
                return false;
            }

            if (!ImageExtensions.IsListEqual(other.ImageExtensions))
            {
                return false;
            }

            if (!string.Equals(Executable, other.Executable, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Arguments, other.Arguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(WorkingDirectory, other.WorkingDirectory, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents system emulator.
    /// </summary>
    public class Ver2_Emulator : Ver2_DatabaseObject
    {
        private ObservableCollection<Ver2_EmulatorProfile> profile;
        /// <summary>
        /// Gets or sets list of emulator profiles.
        /// </summary>
        public ObservableCollection<Ver2_EmulatorProfile> Profiles
        {
            get => profile;
            set
            {
                profile = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of Emulator.
        /// </summary>
        public Ver2_Emulator() : base()
        {
        }

        /// <summary>
        /// Creates new instance of Emulator with specific name.
        /// </summary>
        /// <param name="name">Emulator name.</param>
        public Ver2_Emulator(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    public enum Ver2_GameField
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
        ///
        UseGlobalPostScript,
        ///
        UseGlobalPreScript,
        ///
        UserScoreRating,
        ///
        CommunityScoreRating,
        ///
        CriticScoreRating,
        ///
        UserScoreGroup,
        ///
        CommunityScoreGroup,
        ///
        CriticScoreGroup,
        ///
        LastActivitySegment,
        ///
        AddedSegment,
        ///
        ModifiedSegment,
        ///
        PlaytimeCategory,
        ///
        InstallationStatus,
        ///
        None,
        ///
        GameStartedScript,
        ///
        UseGlobalGameStartedScript,
        ///
        Notes,
        ///
        Manual
    }

    /// <summary>
    /// Represents Playnite game object.
    /// </summary>
    public class Ver2_Game : Ver2_DatabaseObject
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

        private ObservableCollection<Ver2_GameAction> otherActions;
        /// <summary>
        /// Gets or sets list of additional game actions.
        /// </summary>
        public ObservableCollection<Ver2_GameAction> OtherActions
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

        private Ver2_GameAction playAction;
        /// <summary>
        /// Gets or sets game action used to starting the game.
        /// </summary>
        public Ver2_GameAction PlayAction
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
                OnPropertyChanged(nameof(Ver2_Platform));
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
            }
        }

        private ObservableCollection<Ver2_Link> links;
        /// <summary>
        /// Gets or sets list of game related web links.
        /// </summary>
        public ObservableCollection<Ver2_Link> Links
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
                OnPropertyChanged(nameof(Ver2_InstallationStatus));
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
                OnPropertyChanged(nameof(Ver2_PlaytimeCategory));
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
                OnPropertyChanged(nameof(Ver2_Series));
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
                OnPropertyChanged(nameof(Ver2_Region));
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
            }
        }

        private Ver2_CompletionStatus completionStatus = Ver2_CompletionStatus.NotPlayed;
        /// <summary>
        /// Gets or sets game completion status.
        /// </summary>
        public Ver2_CompletionStatus CompletionStatus
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
            }
        }

        private Ver2_ScriptLanguage actionsScriptLanguage = Ver2_ScriptLanguage.PowerShell;
        /// <summary>
        /// Gets or sets scripting language for <see cref="PreScript"/> and <see cref="PostScript"/> scripts.
        /// </summary>
        public Ver2_ScriptLanguage ActionsScriptLanguage
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

        /// <summary>
        /// Creates new instance of a Game object.
        /// </summary>
        public Ver2_Game() : base()
        {
            GameId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates new instance of a Game object with specific name.
        /// </summary>
        /// <param name="name">Game name.</param>
        public Ver2_Game(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    public enum Ver2_GameActionType : int
    {
        /// <summary>
        /// Game action executes a file.
        /// </summary>
        File = 0,
        /// <summary>
        /// Game action navigates to a web based URL.
        /// </summary>
        URL = 1,
        /// <summary>
        /// Game action starts an emulator.
        /// </summary>
        Emulator = 2
    }

    /// <summary>
    /// Represents executable game action.
    /// </summary>
    public class Ver2_GameAction : ObservableObject, IEquatable<Ver2_GameAction>
    {
        private readonly Guid id = Guid.NewGuid();

        private Ver2_GameActionType type;
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public Ver2_GameActionType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private string arguments;
        /// <summary>
        /// Gets or sets executable arguments for File type tasks.
        /// </summary>
        public string Arguments
        {
            get => arguments;
            set
            {
                arguments = value;
                OnPropertyChanged();
            }
        }

        private string additionalArguments;
        /// <summary>
        /// Gets or sets additional executable arguments used for Emulator action type.
        /// </summary>
        public string AdditionalArguments
        {
            get => additionalArguments;
            set
            {
                additionalArguments = value;
                OnPropertyChanged();
            }
        }

        private bool overrideDefaultArgs;
        /// <summary>
        /// Gets or sets value indicating wheter emulator arguments should be completely overwritten with action arguments.
        /// Applies only to Emulator action type.
        /// </summary>
        public bool OverrideDefaultArgs
        {
            get => overrideDefaultArgs;
            set
            {
                overrideDefaultArgs = value;
                OnPropertyChanged();
            }
        }

        private string path;
        /// <summary>
        /// Gets or sets executable path for File action type or URL for URL action type.
        /// </summary>
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        private string workingDir;
        /// <summary>
        /// Gets or sets working directory for File action type executable.
        /// </summary>
        public string WorkingDir
        {
            get => workingDir;
            set
            {
                workingDir = value;
                OnPropertyChanged();
            }
        }

        private string name;
        /// <summary>
        /// Gets or sets action name.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private bool isHandledByPlugin;
        /// <summary>
        /// Gets or sets value indicating wheter a action's execution should be handled by a plugin.
        /// </summary>
        public bool IsHandledByPlugin
        {
            get => isHandledByPlugin;
            set
            {
                isHandledByPlugin = value;
                OnPropertyChanged();
            }
        }

        private Guid emulatorId;
        /// <summary>
        /// Gets or sets emulator id for Emulator action type execution.
        /// </summary>
        public Guid EmulatorId
        {
            get => emulatorId;
            set
            {
                emulatorId = value;
                OnPropertyChanged();
            }
        }

        private Guid emulatorProfileId;
        /// <summary>
        /// Gets or sets emulator profile id for Emulator action type execution.
        /// </summary>
        public Guid EmulatorProfileId
        {
            get => emulatorProfileId;
            set
            {
                emulatorProfileId = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            switch (Type)
            {
                case Ver2_GameActionType.File:
                    return $"File: {Path}, {Arguments}, {WorkingDir}";
                case Ver2_GameActionType.URL:
                    return $"Url: {Path}";
                case Ver2_GameActionType.Emulator:
                    return $"Emulator: {EmulatorId}, {EmulatorProfileId}, {OverrideDefaultArgs}, {AdditionalArguments}";
                default:
                    return Path;
            }
        }

        /// <summary>
        /// Compares two <see cref="Ver2_GameAction"/> objects for equality.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool Equals(Ver2_GameAction obj1, Ver2_GameAction obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            else
            {
                return obj1?.Equals(obj2) == true;
            }
        }

        /// <inheritdoc/>
        public bool Equals(Ver2_GameAction other)
        {
            if (other is null)
            {
                return false;
            }

            if (Type != other.Type)
            {
                return false;
            }

            if (!string.Equals(Arguments, other.Arguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(AdditionalArguments, other.AdditionalArguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Path, other.Path, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(WorkingDir, other.WorkingDir, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (IsHandledByPlugin != other.IsHandledByPlugin)
            {
                return false;
            }

            if (EmulatorId != other.EmulatorId)
            {
                return false;
            }

            if (EmulatorProfileId != other.EmulatorProfileId)
            {
                return false;
            }

            if (OverrideDefaultArgs != other.OverrideDefaultArgs)
            {
                return false;
            }

            return true;
        }
    }

    public class Ver2_GameFeature : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_GameFeature"/>.
        /// </summary>
        public Ver2_GameFeature() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_GameFeature"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_GameFeature(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty tag.
        /// </summary>
        public static readonly Ver2_GameFeature Empty = new Ver2_GameFeature { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_GameSource : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_GameSource"/>.
        /// </summary>
        public Ver2_GameSource() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_GameSource"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_GameSource(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty game source.
        /// </summary>
        public static readonly Ver2_GameSource Empty = new Ver2_GameSource { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_Genre : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Genre"/>.
        /// </summary>
        public Ver2_Genre() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Genre"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Genre(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty genre.
        /// </summary>
        public static readonly Ver2_Genre Empty = new Ver2_Genre { Id = Guid.Empty, Name = string.Empty };
    }

    public enum Ver2_InstallationStatus
    {
        /// <summary>
        /// Game is installed.
        /// </summary>
        [Description("LOCGameIsInstalledTitle")]
        Installed = 0,

        /// <summary>
        /// Game is not installed.
        /// </summary>
        [Description("LOCGameIsUnInstalledTitle")]
        Uninstalled = 1
    }

    public class Ver2_Link : ObservableObject, IEquatable<Ver2_Link>
    {
        private string name;
        /// <summary>
        /// Gets or sets name of the link.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string url;
        /// <summary>
        /// Gets or sets web based URL.
        /// </summary>
        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of Link.
        /// </summary>
        public Ver2_Link()
        {
        }

        /// <summary>
        /// Creates new instance of Link with specific values.
        /// </summary>
        /// <param name="name">Link name.</param>
        /// <param name="url">Link URL.</param>
        public Ver2_Link(string name, string url)
        {
            Name = name;
            Url = url;
        }

        /// <inheritdoc/>
        public bool Equals(Ver2_Link other)
        {
            if (other is null)
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Url, other.Url, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }

    public enum Ver2_PastTimeSegment : int
    {
        /// <summary>
        /// Idicates time occurig today.
        /// </summary>
        [Description("LOCToday")]
        Today = 0,

        /// <summary>
        /// Idicates time occurig yesterday.
        /// </summary>
        [Description("LOCYesterday")]
        Yesterday = 1,

        /// <summary>
        /// Idicates time occurig past week.
        /// </summary>
        [Description("LOCPastWeek")]
        PastWeek = 2,

        /// <summary>
        /// Idicates time occurig past month.
        /// </summary>
        [Description("LOCPastMonth")]
        PastMonth = 3,

        /// <summary>
        /// Idicates time occurig past year.
        /// </summary>
        [Description("LOCPastYear")]
        PastYear = 4,

        /// <summary>
        /// Idicates time occurig past year.
        /// </summary>
        [Description("LOCMoreThenYear")]
        MoreThenYear = 5,

        /// <summary>
        /// Idicates time that never happened.
        /// </summary>
        [Description("LOCNever")]
        Never = 6
    }

    public class Ver2_Platform : Ver2_DatabaseObject
    {
        private string icon;
        /// <summary>
        /// Gets or sets platform icon.
        /// </summary>
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string cover;
        /// <summary>
        /// Gets or sets default game cover.
        /// </summary>
        public string Cover
        {
            get => cover;
            set
            {
                cover = value;
                OnPropertyChanged();
            }
        }

        private string background;
        /// <summary>
        /// Gets or sets default game background image.
        /// </summary>
        public string Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of Platform.
        /// </summary>
        public Ver2_Platform() : base()
        {
        }

        /// <summary>
        /// Creates new instance of Platform with specific name.
        /// </summary>
        /// <param name="name">Platform name.</param>
        public Ver2_Platform(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets empty platform.
        /// </summary>
        public static readonly Ver2_Platform Empty = new Ver2_Platform { Id = Guid.Empty, Name = string.Empty };
    }

    public enum Ver2_PlaytimeCategory : int
    {
        /// <summary>
        /// Not playtime.
        /// </summary>
        [Description("LOCPlayedNone")]
        NotPlayed = 0,

        /// <summary>
        /// Less then an hour played.
        /// </summary>
        [Description("LOCPLaytimeLessThenAnHour")]
        LessThenHour = 1,

        /// <summary>
        /// Played 1 to 10 hours.
        /// </summary>
        [Description("LOCPLaytime1to10")]
        O1_10 = 2,

        /// <summary>
        /// Played 10 to 100 hours.
        /// </summary>
        [Description("LOCPLaytime10to100")]
        O10_100 = 3,

        /// <summary>
        /// Played 100 to 500 hours.
        /// </summary>
        [Description("LOCPLaytime100to500")]
        O100_500 = 4,

        /// <summary>
        /// Played 500 to 1000 hours.
        /// </summary>
        [Description("LOCPLaytime500to1000")]
        O500_1000 = 5,

        /// <summary>
        /// Played more then 1000 hours.
        /// </summary>
        [Description("LOCPLaytime1000plus")]
        O1000plus = 6
    }

    public class Ver2_Region : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Region"/>.
        /// </summary>
        public Ver2_Region() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Region"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Region(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty region.
        /// </summary>
        public static readonly Ver2_Region Empty = new Ver2_Region { Id = Guid.Empty, Name = string.Empty };
    }

    public enum Ver2_ScoreRating
    {
        /// <summary>
        /// No score.
        /// </summary>
        None,
        /// <summary>
        /// Negative rating.
        /// </summary>
        Negative,
        /// <summary>
        /// Positive rating.
        /// </summary>
        Positive,
        /// <summary>
        /// Mixed rating.
        /// </summary>
        Mixed
    }

    /// <summary>
    /// Scpecifies rating score groups.
    /// </summary>
    public enum Ver2_ScoreGroup : int
    {
        /// <summary>
        /// Score rage from 0 to 10.
        /// </summary>
        [Description("0x")]
        O0x = 0,

        /// <summary>
        /// Score rage from 10 to 20.
        /// </summary>
        [Description("1x")]
        O1x = 1,

        /// <summary>
        /// Score rage from 20 to 30.
        /// </summary>
        [Description("2x")]
        O2x = 2,

        /// <summary>
        /// Score rage from 30 to 40.
        /// </summary>
        [Description("3x")]
        O3x = 3,

        /// <summary>
        /// Score rage from 40 to 50.
        /// </summary>
        [Description("4x")]
        O4x = 4,

        /// <summary>
        /// Score rage from 50 to 60.
        /// </summary>
        [Description("5x")]
        O5x = 5,

        /// <summary>
        /// Score rage from 60 to 70.
        /// </summary>
        [Description("6x")]
        O6x = 6,

        /// <summary>
        /// Score rage from 70 to 80.
        /// </summary>
        [Description("7x")]
        O7x = 7,

        /// <summary>
        /// Score rage from 80 to 90.
        /// </summary>
        [Description("8x")]
        O8x = 8,

        /// <summary>
        /// Score rage from 90 to 100.
        /// </summary>
        [Description("9x")]
        O9x = 9,

        /// <summary>
        /// No score.
        /// </summary>
        [Description("LOCNone")]
        None = 10
    }

    public class Ver2_Series : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Series"/>.
        /// </summary>
        public Ver2_Series() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Series"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Series(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty series.
        /// </summary>
        public static readonly Ver2_Series Empty = new Ver2_Series { Id = Guid.Empty, Name = string.Empty };
    }

    public class Ver2_Tag : Ver2_DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Ver2_Tag"/>.
        /// </summary>
        public Ver2_Tag() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Ver2_Tag"/>.
        /// </summary>
        /// <param name="name"></param>
        public Ver2_Tag(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty tag.
        /// </summary>
        public static readonly Ver2_Tag Empty = new Ver2_Tag { Id = Guid.Empty, Name = string.Empty };
    }
}
