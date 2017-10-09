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

namespace Playnite.Models
{
    public class Game : IGame
    {
        private IGameStateMonitor stateMonitor;

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
                    case Provider.Uplay:
                    case Provider.Steam:
                    default:
                        return string.IsNullOrEmpty(SteamSettings.DescriptionTemplate) ? Description : SteamSettings.DescriptionTemplate.Replace("{0}", Description);
                }
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

        public void InstallGame()
        {
            switch (Provider)
            {
                case Provider.Steam:
                    Process.Start(@"steam://install/" + ProviderId);
                    RegisterStateMonitor(new SteamGameStateMonitor(ProviderId, new SteamLibrary()));
                    break;
                case Provider.GOG:
                    Process.Start(@"goggalaxy://openGameView/" + ProviderId);
                    RegisterStateMonitor(new GogGameStateMonitor(ProviderId, InstallDirectory, new GogLibrary()));
                    break;
                case Provider.Origin:
                    Process.Start(string.Format(@"origin2://game/launch?offerIds={0}&autoDownload=true", ProviderId));
                    RegisterStateMonitor(new OriginGameStateMonitor(ProviderId, new OriginLibrary()));
                    break;
                case Provider.Uplay:
                    Process.Start("uplay://install/" + ProviderId);
                    RegisterStateMonitor(new UplayGameStateMonitor(ProviderId, new UplayLibrary()));
                    break;
                case Provider.Custom:
                    break;
                default:
                    break;
            }
        }

        public void PlayGame()
        {
            PlayGame(null);
        }

        public void PlayGame(List<Emulator> emulators)
        {
            if (PlayTask == null)
            {
                return;
            }

            LastActivity = DateTime.Now;
            if (Provider == Provider.GOG && Settings.Instance.GOGSettings.RunViaGalaxy)
            {
                var args = string.Format(@"/gameId={0} /command=runGame /path=""{1}""", ProviderId, InstallDirectory);
                Process.Start(Path.Combine(GogSettings.InstallationPath, "GalaxyClient.exe"), args);
                return;
            }

            GameHandler.ActivateTask(PlayTask, this, emulators);
        }

        public void UninstallGame()
        {
            switch (Provider)
            {
                case Provider.Steam:
                    Process.Start("steam://uninstall/" + ProviderId);
                    RegisterStateMonitor(new SteamGameStateMonitor(ProviderId, new SteamLibrary()));
                    break;
                case Provider.GOG:
                    var uninstaller = Path.Combine(InstallDirectory, "unins000.exe");
                    if (!File.Exists(uninstaller))
                    {
                        throw new FileNotFoundException("Uninstaller not found.");
                    }

                    Process.Start(uninstaller);
                    RegisterStateMonitor(new GogGameStateMonitor(ProviderId, InstallDirectory, new GogLibrary()));
                    break;
                case Provider.Origin:
                    Process.Start("appwiz.cpl");
                    RegisterStateMonitor(new OriginGameStateMonitor(ProviderId, new OriginLibrary()));
                    break;
                case Provider.Uplay:
                    Process.Start("uplay://uninstall/" + ProviderId);
                    RegisterStateMonitor(new UplayGameStateMonitor(ProviderId, new UplayLibrary()));
                    break;
                case Provider.Custom:
                    break;
                default:
                    break;
            }
        }

        public void RegisterStateMonitor(IGameStateMonitor monitor)
        {
            if (stateMonitor != null)
            {
                stateMonitor.Dispose();
            }

            stateMonitor = monitor;
            stateMonitor.GameInstalled += StateMonitor_GameInstalled;
            stateMonitor.GameUninstalled += StateMonitor_GameUninstalled;
            stateMonitor.StartMonitoring();
            IsSetupInProgress = true;
        }

        public void UnregisetrStateMonitor()
        {
            if (stateMonitor != null)
            {
                stateMonitor.StopMonitoring();
                stateMonitor.Dispose();
            }

            IsSetupInProgress = false;
        }

        private void StateMonitor_GameUninstalled(object sender, EventArgs e)
        {
            IsSetupInProgress = false;
            PlayTask = null;
            InstallDirectory = string.Empty;

            if (OtherTasks != null)
            {
                OtherTasks = new ObservableCollection<GameTask>(OtherTasks.Where(a => !a.IsBuiltIn));
            }
        }

        private void StateMonitor_GameInstalled(object sender, GameInstalledEventArgs e)
        {
            IsSetupInProgress = false;
            var game = e.NewGame;
            PlayTask = game.PlayTask;
            InstallDirectory = game.InstallDirectory;

            if (game.OtherTasks != null)
            {
                OtherTasks = new ObservableCollection<GameTask>(OtherTasks.Where(a => !a.IsBuiltIn));
                foreach (var task in game.OtherTasks.Reverse())
                {
                    OtherTasks.Insert(0, task);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public string ResolveVariables(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return inputString;
            }

            var result = inputString;
            result = result.Replace("{InstallDir}", InstallDirectory);
            result = result.Replace("{ImagePath}", IsoPath);
            return result;
        }
    }
}
