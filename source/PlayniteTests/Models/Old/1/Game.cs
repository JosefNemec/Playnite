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
using Playnite.SDK;
using Playnite.SDK.Models;

namespace Playnite.Models.Old1
{
    public class Game
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

        public string DescriptionView
        {
            get;
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

        [JsonIgnore]
        [BsonIgnore]
        public bool IsInstalled
        {
            get
            {
                return PlayTask != null;
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

        private int? platformId;
        public int? PlatformId
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

        private bool isProviderDataReady;
        public bool IsProviderDataUpdated
        {
            get
            {
                return isProviderDataReady;
            }

            set
            {
                isProviderDataReady = value;
                OnPropertyChanged("IsProviderDataReady");
            }
        }

        private bool isSetupInProgress = false;
        [JsonIgnore]
        [BsonIgnore]
        public bool IsSetupInProgress
        {
            get
            {
                return isSetupInProgress;
            }

            set
            {
                isSetupInProgress = value;
                OnPropertyChanged("IsSetupInProgress");
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
    }
}
