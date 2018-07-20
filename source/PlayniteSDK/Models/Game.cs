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
using LiteDB;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Playnite.SDK.Converters;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents Playnite game object.
    /// </summary>
    public class Game : ObservableObject
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
                OnPropertyChanged("BackgroundImage");
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
                OnPropertyChanged("Description");
                OnPropertyChanged("DescriptionView");
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
                OnPropertyChanged("Developers");
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
                OnPropertyChanged("Genres");
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
                OnPropertyChanged("Hidden");
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
                OnPropertyChanged("Favorite");
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
                OnPropertyChanged("Icon");
            }
        }

        private int id;
        /// <summary>
        /// Gets or sets game database id.
        /// </summary>
        [BsonId]
        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private string image;
        /// <summary>
        /// Gets or sets game cover image. Local file path, HTTP URL or database file ids are supported.
        /// </summary>
        public string Image
        {
            get
            {
                return image;
            }

            set
            {
                image = value;
                OnPropertyChanged("Image");
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
                OnPropertyChanged("InstallDirectory");
            }
        }

        private string isoPath;
        /// <summary>
        /// Gets or sets game's ISO, ROM or other type of executable image path.
        /// </summary>
        public string IsoPath
        {
            get
            {
                return isoPath;
            }

            set
            {
                isoPath = value;
                OnPropertyChanged("IsoPath");
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
                OnPropertyChanged("LastActivity");
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
                OnPropertyChanged("Name");
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
                OnPropertyChanged("SortingName");
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
                OnPropertyChanged("GameId");
            }
        }


        private Dictionary<string, object> pluginMetadata = new Dictionary<string, object>();
        /// <summary>
        /// Gets or sets metadata assigned by the plugin responsible for handling this game.
        /// </summary>
        public Dictionary<string, object> PluginMetadata
        {
            get
            {
                return pluginMetadata;
            }

            set
            {
                pluginMetadata = value;
                OnPropertyChanged("PluginMetadata");
            }
        }

        private Guid pluginId;
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
                OnPropertyChanged("PluginId");
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
                OnPropertyChanged("OtherActions");
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
                OnPropertyChanged("PlayAction");
            }
        }

        //private Provider provider;
        ///// <summary>
        ///// Gets or sets original library provider.
        ///// </summary>
        //public Provider Provider
        //{
        //    get
        //    {
        //        return provider;
        //    }

        //    set
        //    {
        //        provider = value;
        //        OnPropertyChanged("Provider");
        //    }
        //}

        private ObjectId platformId;
        /// <summary>
        /// Gets or sets platform id.
        /// </summary>
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId PlatformId
        {
            get
            {
                return platformId;
            }

            set
            {
                platformId = value;
                OnPropertyChanged("PlatformId");
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
                OnPropertyChanged("Publishers");
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
                OnPropertyChanged("ReleaseDate");
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
                OnPropertyChanged("Categories");
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
                OnPropertyChanged("Tags");
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
                OnPropertyChanged("Links");
            }
        }

        /// <summary>
        /// Gets value indicating wheter a game is being installed..
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public bool IsInstalling
        {
            get => State.Installing;
        }

        /// <summary>
        /// Gets value indicating wheter a game is being uninstalled.
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public bool IsUninstalling
        {
            get => State.Uninstalling;
        }

        /// <summary>
        /// Gets value indicating wheter a game is being launched.
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public bool IsLaunching
        {
            get => State.Launching;
        }

        /// <summary>
        /// Gets value indicating wheter a game is currently running.
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public bool IsRunning
        {
            get => State.Running;
        }

        /// <summary>
        /// Gets value indicating wheter a game is installed.
        /// </summary>
        [JsonIgnore]
        [BsonIgnore]
        public bool IsInstalled
        {
            get => State.Installed;
        }
        
        private GameState state = new GameState();
        /// <summary>
        /// Gets or sets game state.
        /// </summary>
        public GameState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("IsRunning");
                OnPropertyChanged("IsInstalling");
                OnPropertyChanged("IsUninstalling");
                OnPropertyChanged("IsLaunching");
                OnPropertyChanged("IsInstalled");
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
                OnPropertyChanged("Playtime");
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
                OnPropertyChanged("Added");
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
                OnPropertyChanged("Modified");
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
                OnPropertyChanged("PlayCount");
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
                OnPropertyChanged("Series");
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
                OnPropertyChanged("Version");
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
                OnPropertyChanged("AgeRating");
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
                OnPropertyChanged("Region");
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
                OnPropertyChanged("Source");
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
                OnPropertyChanged("CompletionStatus");
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
                OnPropertyChanged("UserScore");
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
                OnPropertyChanged("CriticScore");
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
                OnPropertyChanged("CommunityScore");
            }
        }

        /// <summary>
        /// Creates new instance of a Game object.
        /// </summary>
        public Game()
        {
        }

        /// <summary>
        /// Creates new instance of a Game object with specific name.
        /// </summary>
        /// <param name="name">Game name.</param>
        public Game(string name)
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
