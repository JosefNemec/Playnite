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

namespace Playnite.Database.OldModels.NewVer1
{
    /// <summary>
    /// Represents Playnite game object.
    /// </summary>
    public class OldGame : OldDatabaseObject
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

        private ComparableList<string> developers;
        /// <summary>
        /// Gets or sets list of developers.
        /// </summary>
        public ComparableList<string> Developers
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

        private ComparableList<string> genres;
        /// <summary>
        /// Gets or sets list of genres.
        /// </summary>
        public ComparableList<string> Genres
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

        private string name;
        /// <summary>
        /// Gets or sets game name.
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

        private ObservableCollection<OldGameAction> otherActions;
        /// <summary>
        /// Gets or sets list of additional game actions.
        /// </summary>
        public ObservableCollection<OldGameAction> OtherActions
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

        private OldGameAction playAction;
        /// <summary>
        /// Gets or sets game action used to starting the game.
        /// </summary>
        public OldGameAction PlayAction
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
            }
        }

        private ComparableList<string> publishers;
        /// <summary>
        /// Gets or sets list of publishers.
        /// </summary>
        public ComparableList<string> Publishers
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

        private ComparableList<string> categories;
        /// <summary>
        /// Gets or sets game categories.
        /// </summary>
        public ComparableList<string> Categories
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

        private ComparableList<string> tags;
        /// <summary>
        /// Gets or sets list of tags.
        /// </summary>
        public ComparableList<string> Tags
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

        private ObservableCollection<OldLink> links;
        /// <summary>
        /// Gets or sets list of game related web links.
        /// </summary>
        public ObservableCollection<OldLink> Links
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
            }
        }

        /// <summary>
        /// Gets value indicating wheter the game is custom game.
        /// </summary>
        [JsonIgnore]
        public bool IsCustomGame
        {
            get => PluginId == Guid.Empty;
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

        private string series;
        /// <summary>
        /// Gets or sets game series.
        /// </summary>
        public string Series
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

        private string ageRating;
        /// <summary>
        /// Gets or sets age rating for a game.
        /// </summary>
        public string AgeRating
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

        private string region;
        /// <summary>
        /// Gets or sets game region.
        /// </summary>
        public string Region
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

        private string source;
        /// <summary>
        /// Gets or sets source of the game.
        /// </summary>
        public string Source
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

        private OldCompletionStatus completionStatus = OldCompletionStatus.NotPlayed;
        /// <summary>
        /// Gets or sets game completion status.
        /// </summary>
        public OldCompletionStatus CompletionStatus
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

        /// <summary>
        /// Creates new instance of a Game object.
        /// </summary>
        public OldGame() : base()
        {
            GameId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates new instance of a Game object with specific name.
        /// </summary>
        /// <param name="name">Game name.</param>
        public OldGame(string name) : this()
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
}
