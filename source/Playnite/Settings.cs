using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Targets;
using Playnite.Providers;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using CefSharp;
using System.Configuration;

namespace Playnite
{
    public class Settings : INotifyPropertyChanged
    {
        public class WindowPosition
        {
            public class Point
            {
                public double X
                {
                    get; set;
                }

                public double Y
                {
                    get; set;
                }
            }

            public Point Position
            {
                get; set;
            }

            public Point Size
            {
                get; set;
            }
        }

        private Dictionary<string, WindowPosition> windowPositions = new Dictionary<string, WindowPosition>();
        public Dictionary<string, WindowPosition> WindowPositions
        {
            get
            {
                return windowPositions;
            }

            set
            {
                windowPositions = value;
                OnPropertyChanged("WindowPositions");
            }
        }

        private bool firstTimeWizardComplete;
        public bool FirstTimeWizardComplete
        {
            get
            {
                return firstTimeWizardComplete;
            }

            set
            {
                firstTimeWizardComplete = value;
                OnPropertyChanged("FirstTimeWizardComplete");
            }
        }

        private string databasePath;
        public string DatabasePath
        {
            get
            {
                return databasePath;
            }

            set
            {
                databasePath = value;
                OnPropertyChanged("DatabasePath");
            }
        }

        private SortOrder sortingOrder;
        public SortOrder SortingOrder
        {
            get
            {
                return sortingOrder;
            }

            set
            {
                sortingOrder = value;
                OnPropertyChanged("SortingOrder");
            }
        }

        private GroupOrder groupingOrder;
        public GroupOrder GroupingOrder
        {
            get
            {
                return groupingOrder;
            }

            set
            {
                groupingOrder = value;
                OnPropertyChanged("GroupingOrder");
            }
        }

        private GameImageSize imageViewSize;
        public GameImageSize ImageViewSize
        {
            get
            {
                return imageViewSize;
            }

            set
            {
                imageViewSize = value;
                OnPropertyChanged("ImageViewSize");
            }
        }

        private ViewType gamesViewType;
        public ViewType GamesViewType
        {
            get
            {
                return gamesViewType;
            }

            set
            {
                gamesViewType = value;
                OnPropertyChanged("GamesViewType");
                OnPropertyChanged("IsListViewSet");
                OnPropertyChanged("IsImagesViewSet");
                OnPropertyChanged("IsGridViewSet");
            }
        }

        [JsonIgnore]
        public bool IsListViewSet
        {
            get
            {
                return GamesViewType == ViewType.List;
            }
        }

        [JsonIgnore]
        public bool IsImagesViewSet
        {
            get
            {
                return GamesViewType == ViewType.Images;
            }
        }

        [JsonIgnore]
        public bool IsGridViewSet
        {
            get
            {
                return GamesViewType == ViewType.Grid;
            }
        }

        private GogSettings gogSettings = new GogSettings();
        public GogSettings GOGSettings
        {
            get
            {
                return gogSettings;
            }

            set
            {
                gogSettings = value;
            }
        }

        private SteamSettings steamSettings = new SteamSettings();
        public SteamSettings SteamSettings
        {
            get
            {
                return steamSettings;
            }

            set
            {
                steamSettings = value;
            }
        }

        private OriginSettings originSettings = new OriginSettings();
        public OriginSettings OriginSettings
        {
            get
            {
                return originSettings;
            }

            set
            {
                originSettings = value;
            }
        }

        private FilterSettings filterSettings = new FilterSettings();
        public FilterSettings FilterSettings
        {
            get
            {
                return filterSettings;
            }

            set
            {
                filterSettings = value;
            }
        }

        private ObservableConcurrentDictionary<string, bool> gridViewHeaders = new ObservableConcurrentDictionary<string, bool>()
        {
            { "Icon", true },
            { "Name", true },
            { "Developer", true },
            { "Publisher", false },
            { "ReleaseDate", true },
            { "Genre", true },
            { "LastPlayed", true },
            { "Categories", false },
            { "StoreURL", false },
            { "WikiURL", false },
            { "ForumsURL", false },
            { "Installed", false },
            { "Directory", false },
        };

        public ObservableConcurrentDictionary<string, bool> GridViewHeaders
        {
            get
            {
                return gridViewHeaders;
            }

            set
            {
                if (gridViewHeaders != null)
                {
                    gridViewHeaders.PropertyChanged -= GridViewHeaders_PropertyChanged;
                }

                gridViewHeaders = value;
                gridViewHeaders.PropertyChanged += GridViewHeaders_PropertyChanged;
                OnPropertyChanged("GridViewHeaders");
            }
        }

        [JsonIgnore]
        public static bool IsPortable
        {
            get
            {
                return !File.Exists(Paths.UninstallerPath);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                }

                return instance;
            }   
        }

        public Settings()
        {
            GridViewHeaders.PropertyChanged += GridViewHeaders_PropertyChanged;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void GridViewHeaders_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Values")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GridViewHeaders"));
            }
        }

        public static void LoadSettings()
        {
            if (File.Exists(Paths.ConfigFilePath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Paths.ConfigFilePath));
                instance = settings;
            }
        }

        public void SaveSettings()
        {
            FileSystem.CreateFolder(Paths.ConfigRootPath);
            File.WriteAllText(Paths.ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${logger}:${message}${exception}"
            };

            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);
#endif
            var fileTarget = new FileTarget()
            {
                FileName = Path.Combine(Paths.ConfigRootPath, "playnite.log"),
                Layout = "${longdate}|${level:uppercase=true}:${message}${exception:format=toString}",
                KeepFileOpen = false,
                ArchiveFileName = Path.Combine(Paths.ConfigRootPath, "playnite.{#####}.log"),
                ArchiveAboveSize = 4096000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2
            };

            config.AddTarget("file", fileTarget);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;
        }

        public static void ConfigureCef()
        {
            FileSystem.CreateFolder(Paths.BrowserCachePath);
            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1");
            settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
            settings.CachePath = Paths.BrowserCachePath;
            settings.PersistSessionCookies = true;
            settings.LogFile = Path.Combine(Paths.ConfigRootPath, "cef.log");
            Cef.Initialize(settings);
        }

        public static string GetAppConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
