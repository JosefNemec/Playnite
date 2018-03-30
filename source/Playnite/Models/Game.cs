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
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using Playnite.Providers;
using System.Collections.Concurrent;
using Playnite.Providers.Uplay;
using Playnite.Providers.BattleNet;
using Newtonsoft.Json;
using Playnite.Converters;

namespace Playnite.Models
{
    public class Game : IGame
    {
        private string backgroundImage;
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
        public string InstallDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(installDirectory))
                {
                    if (PlayTask != null)
                    {
                        return PlayTask.WorkingDir;
                    }
                }

                return installDirectory;
            }

            set
            {
                installDirectory = value;
                OnPropertyChanged("InstallDirectory");
                if (Provider == Provider.Custom)
                {
                    OnPropertyChanged("IsInstalled");
                }
            }
        }

        private string isoPath;
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

        private string providerId;
        public string ProviderId
        {
            get
            {
                return providerId;
            }

            set
            {
                providerId = value;
                OnPropertyChanged("ProviderId");
            }
        }

        private ObservableCollection<GameTask> otherTasks;
        public ObservableCollection<GameTask> OtherTasks
        {
            get
            {
                return otherTasks;
            }

            set
            {
                otherTasks = value;
                OnPropertyChanged("OtherTasks");
            }
        }

        private GameTask playTask;
        public GameTask PlayTask
        {
            get
            {
                return playTask;
            }

            set
            {
                playTask = value;
                OnPropertyChanged("PlayTask");
                OnPropertyChanged("IsInstalled");
            }
        }

        private Provider provider;
        public Provider Provider
        {
            get
            {
                return provider;
            }

            set
            {
                provider = value;
                OnPropertyChanged("Provider");
            }
        }

        private ObjectId platformId;
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

        [JsonIgnore]
        [BsonIgnore]
        public bool IsInstalling
        {
            get
            {
                return State.Installing;
            }
        }

        [JsonIgnore]
        [BsonIgnore]
        public bool IsUninstalling
        {
            get
            {
                return State.Uninstalling;
            }
        }

        [JsonIgnore]
        [BsonIgnore]
        public bool IsLaunching
        {
            get
            {
                return State.Launching;
            }
        }

        [JsonIgnore]
        [BsonIgnore]
        public bool IsRunning
        {
            get
            {
                return State.Running;
            }
        }

        [JsonIgnore]
        [BsonIgnore]
        public bool IsInstalled
        {
            get
            {
                if (Provider == Provider.Custom)
                {
                    return !string.IsNullOrEmpty(InstallDirectory);
                }
                else
                {
                    return State.Installed;
                }
            }
        }

        private GameState state = new GameState();
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

                if (Provider != Provider.Custom)
                {
                    OnPropertyChanged("IsInstalled");
                }
            }
        }

        private long playtime = 0;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            Provider = Provider.Custom;
            ProviderId = Guid.NewGuid().ToString();
        }

        public Game(string name)
        {
            Name = name;
            Provider = Provider.Custom;
            ProviderId = Guid.NewGuid().ToString();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }    

        public override string ToString()
        {
            return Name;
        }
    }
}
