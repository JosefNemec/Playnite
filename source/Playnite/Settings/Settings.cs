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
using Playnite.Providers.Uplay;
using Playnite.Providers.BattleNet;

namespace Playnite
{
    public enum AfterLaunchOptions
    {
        None,
        Minimize,
        Close
    }

    public class Settings : INotifyPropertyChanged, IEditableObject
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

            public System.Windows.WindowState State
            {
                get; set;
            } = System.Windows.WindowState.Normal;
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

        private List<string> collapsedCategories = new List<string>();
        public List<string> CollapsedCategories
        {
            get
            {
                return collapsedCategories;
            }

            set
            {
                collapsedCategories = value;
                OnPropertyChanged("CollapsedCategories");
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

        private bool emulatorWizardComplete;
        public bool EmulatorWizardComplete
        {
            get
            {
                return emulatorWizardComplete;
            }

            set
            {
                emulatorWizardComplete = value;
                OnPropertyChanged("EmulatorWizardComplete");
            }
        }

        private bool disableHwAcceleration = false;
        public bool DisableHwAcceleration
        {
            get
            {
                return disableHwAcceleration;
            }

            set
            {
                disableHwAcceleration = value;
                OnPropertyChanged("DisableHwAcceleration");
            }
        }

        private bool asyncImageLoading = false;
        public bool AsyncImageLoading
        {
            get
            {
                return asyncImageLoading;
            }

            set
            {
                asyncImageLoading = value;
                OnPropertyChanged("AsyncImageLoading");
            }
        }

        private bool showNameEmptyCover = true;
        public bool ShowNameEmptyCover
        {
            get
            {
                return showNameEmptyCover;
            }

            set
            {
                showNameEmptyCover = value;
                OnPropertyChanged("ShowNameEmptyCover");
            }
        }

        private bool migrationV2PcPlatformAdded = false;
        public bool MigrationV2PcPlatformAdded
        {
            get
            {
                return migrationV2PcPlatformAdded;
            }

            set
            {
                migrationV2PcPlatformAdded = value;
                OnPropertyChanged("MigrationV2PcPlatformAdded");
            }
        }

        private bool showIconsOnList = false;
        public bool ShowIconsOnList
        {
            get
            {
                return showIconsOnList;
            }

            set
            {
                showIconsOnList = value;
                OnPropertyChanged("ShowIconsOnList");
            }
        }

        private bool startInFullscreen = false;
        public bool StartInFullscreen
        {
            get
            {
                return startInFullscreen;
            }

            set
            {
                startInFullscreen = value;
                OnPropertyChanged("StartInFullscreen");
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

        private SortOrder sortingOrder = SortOrder.Name;
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

        private SortOrderDirection sortingOrderDirection = SortOrderDirection.Ascending;
        public SortOrderDirection SortingOrderDirection
        {
            get
            {
                return sortingOrderDirection;
            }

            set
            {
                sortingOrderDirection = value;
                OnPropertyChanged("SortingOrderDirection");
            }
        }

        private GroupOrder groupingOrder = GroupOrder.None;
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

        private double coversZoom = 180;
        public double CoversZoom
        {
            get
            {
                return coversZoom;
            }

            set
            {
                coversZoom = value;
                OnPropertyChanged("CoversZoom");
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

        public GogSettings GOGSettings
        {
            get; set;
        } = new GogSettings();

        public SteamSettings SteamSettings
        {
            get; set;
        } = new SteamSettings();

        public OriginSettings OriginSettings
        {
            get; set;
        } = new OriginSettings();

        public UplaySettings UplaySettings
        {
            get; set;
        } = new UplaySettings();

        public BattleNetSettings BattleNetSettings
        {
            get; set;
        } = new BattleNetSettings();

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
            { "Platform", false },
            { "Provider", false },
            { "Developers", true },
            { "Publishers", false },
            { "ReleaseDate", true },
            { "Genres", true },
            { "LastActivity", true },
            { "IsInstalled", false },
            { "InstallDirectory", false },
            { "Categories", false },
            { "Tags", false }
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

        private bool filterPanelVisible = false;
        public bool FilterPanelVisible
        {
            get
            {
                return filterPanelVisible;
            }

            set
            {
                filterPanelVisible = value;
                OnPropertyChanged("FilterPanelVisible");
            }
        }

        private bool minimizeToTray = false;
        public bool MinimizeToTray
        {
            get
            {
                return minimizeToTray;
            }

            set
            {
                minimizeToTray = value;
                OnPropertyChanged("MinimizeToTray");
            }
        }

        private bool closeToTray = true;
        public bool CloseToTray
        {
            get
            {
                return closeToTray;
            }

            set
            {
                closeToTray = value;
                OnPropertyChanged("CloseToTray");
            }
        }

        private bool enableTray = true;
        public bool EnableTray
        {
            get
            {
                return enableTray;
            }

            set
            {
                enableTray = value;
                OnPropertyChanged("EnableTray");
            }
        }

        private string language = "english";
        public string Language
        {
            get
            {
                return language;
            }

            set
            {
                language = value;
                OnPropertyChanged("Language");
            }
        }

        private bool updateLibStartup = true;
        public bool UpdateLibStartup
        {
            get
            {
                return updateLibStartup;
            }

            set
            {
                updateLibStartup = value;
                OnPropertyChanged("UpdateLibStartup");
            }
        }

        private AfterLaunchOptions afterLaunch = AfterLaunchOptions.Minimize;
        public AfterLaunchOptions AfterLaunch
        {
            get
            {
                return afterLaunch;
            }

            set
            {
                afterLaunch = value;
                OnPropertyChanged("AfterLaunch");
            }
        }

        private string skin = "Classic";
        public string Skin
        {
            get
            {
                return skin;
            }

            set
            {
                skin = value;
                OnPropertyChanged("Skin");
            }
        }

        private string skinColor = "Default";
        public string SkinColor
        {
            get
            {
                return skinColor;
            }

            set
            {
                skinColor = value;
                OnPropertyChanged("SkinColor");
            }
        }

        private string skinFullscreen = "Playnite";
        public string SkinFullscreen
        {
            get
            {
                return skinFullscreen;
            }

            set
            {
                skinFullscreen = value;
                OnPropertyChanged("SkinFullscreen");
            }
        }

        private string skinColorFullscreen = "Default";
        public string SkinColorFullscreen
        {
            get
            {
                return skinColorFullscreen;
            }

            set
            {
                skinColorFullscreen = value;
                OnPropertyChanged("SkinColorFullscreen");
            }
        }

        private string igdbApiKey;
        public string IGDBApiKey
        {
            get
            {
                return igdbApiKey;
            }

            set
            {
                igdbApiKey = value;
                OnPropertyChanged("IGDBApiKey");
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
        public List<string> EditedFields;
        private bool isEditing = false;
        private Settings editingCopy;

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

        public void BeginEdit()
        {
            isEditing = true;
            EditedFields = new List<string>();
            editingCopy = this.CloneJson();
        }

        public void EndEdit()
        {
            isEditing = false;
            foreach (var prop in EditedFields)
            {
                OnPropertyChanged(prop);
            }
        }

        public void CancelEdit()
        {
            editingCopy.CopyProperties(this, false, new List<string>() { "FilterSettings" });
            isEditing = false;
            EditedFields = new List<string>();
        }

        public void OnPropertyChanged(string name, bool force = false)
        {
            if (isEditing && !force)
            {
                if (!EditedFields.Contains(name))
                {
                    EditedFields.Add(name);
                }
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }            
        }

        private void GridViewHeaders_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Values")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GridViewHeaders"));
            }
        }

        public static Settings LoadSettings()
        {
            if (File.Exists(Paths.ConfigFilePath))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Paths.ConfigFilePath));
                instance = settings;
            }

            return Instance;
        }

        public void SaveSettings()
        {
            FileSystem.CreateDirectory(Paths.ConfigRootPath);
            File.WriteAllText(Paths.ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${level:uppercase=true}|${logger}:${message}${exception}"
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
            FileSystem.CreateDirectory(Paths.BrowserCachePath);
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
