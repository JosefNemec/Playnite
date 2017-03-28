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
using Playnite.Providers.Custom;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;

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

        private string communityHubUrl;
        public string CommunityHubUrl
        {
            get
            {
                return communityHubUrl;
            }

            set
            {
                communityHubUrl = value;
                OnPropertyChanged("CommunityHubUrl");
            }
        }

        [BsonIgnore]
        public string DefaultIcon
        {
            get
            {
                switch (Provider)
                {
                    case Provider.GOG:
                        return GogSettings.DefaultIcon;
                    case Provider.Steam:
                        return SteamSettings.DefaultIcon;
                    case Provider.Origin:
                        return OriginSettings.DefaultIcon;
                    case Provider.Custom:
                    default:
                        return CustomGameSettings.DefaultIcon;
                }
            }
        }

        [BsonIgnore]
        public string DefaultImage
        {
            get
            {
                switch (Provider)
                {
                    case Provider.GOG:
                        return GogSettings.DefaultImage;
                    case Provider.Steam:
                        return SteamSettings.DefaultImage;
                    case Provider.Origin:
                        return OriginSettings.DefaultImage;
                    case Provider.Custom:
                    default:
                        return CustomGameSettings.DefaultImage;
                }
            }
        }

        [BsonIgnore]
        public string DefaultBackgroundImage
        {
            get
            {
                switch (Provider)
                {
                    case Provider.GOG:
                    case Provider.Steam:
                    case Provider.Origin:
                    case Provider.Custom:
                    default:
                        return CustomGameSettings.DefaultBackgroundImage;
                }
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
            }
        }

        [BsonIgnore]
        public string DescriptionView
        {
            get
            {
                switch (Provider)
                {
                    case Provider.GOG:
                        return string.IsNullOrEmpty(GogSettings.DescriptionTemplate) ? Description : GogSettings.DescriptionTemplate.Replace("{0}", Description);
                    case Provider.Custom:
                    case Provider.Origin:
                    case Provider.Steam:
                    default:
                        return string.IsNullOrEmpty(SteamSettings.DescriptionTemplate)? Description : SteamSettings.DescriptionTemplate.Replace("{0}", Description);
                }
            }
        }

        private List<string> developers;
        public List<string> Developers
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

        private List<string> genres;
        public List<string> Genres
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

        private List<string> publishers;
        public List<string> Publishers
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

        private string storeUrl;
        public string StoreUrl
        {
            get
            {
                return storeUrl;
            }

            set
            {
                storeUrl = value;
                OnPropertyChanged("StoreUrl");
            }
        }

        private List<string> categories;
        public List<string> Categories
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

        private string wikiUrl;
        public string WikiUrl
        {
            get
            {
                return wikiUrl;
            }

            set
            {
                wikiUrl = value;
                OnPropertyChanged("WikiUrl");
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            Provider = Provider.Custom;
        }

        public Game(string name)
        {
            Name = name;
            Provider = Provider.Custom;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void InstallGame()
        {
            switch (Provider)
            {
                case Provider.Steam:
                    Process.Start(@"steam://install/" + ProviderId);
                    break;
                case Provider.GOG:
                    Process.Start(@"goggalaxy://openGameView/" + ProviderId);
                    break;
                case Provider.Origin:
                    Process.Start(string.Format(@"origin2://game/launch?offerIds={0}&autoDownload=true", ProviderId));
                    break;
                case Provider.Custom:
                    break;
                default:
                    break;
            }
        }

        public void PlayGame()
        {
            if (PlayTask == null)
            {
                return;
            }

            LastActivity = DateTime.Now;
            if (Provider == Provider.GOG && Settings.Instance.GOGSettings.RunViaGalaxy)
            {
                var args = string.Format(@"/gameId={0} /command=launch /path=""{1}""", ProviderId, InstallDirectory);
                Process.Start(Path.Combine(GogSettings.InstallationPath, "GalaxyClient.exe"), args);
                return;
            }

            PlayTask.Activate();
        }

        public void UninstallGame()
        {
            switch (Provider)
            {
                case Provider.Steam:
                    Process.Start("steam://uninstall/" + ProviderId);
                    break;
                case Provider.GOG:
                    var uninstaller = Path.Combine(InstallDirectory, "unins000.exe");
                    if (!File.Exists(uninstaller))
                    {
                        throw new FileNotFoundException("Uninstaller not found.");
                    }

                    Process.Start(uninstaller).WaitForExit();
                    break;
                case Provider.Origin:
                    Process.Start("appwiz.cpl");
                    break;
                case Provider.Custom:
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
