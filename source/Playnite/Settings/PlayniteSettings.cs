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
using System.Configuration;
using Playnite.Common;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Playnite
{
    public enum AfterLaunchOptions
    {
        None,
        Minimize,
        Close
    }

    public enum AfterGameCloseOptions
    {
        None,
        Restore
    }

    public class PlayniteSettings : ObservableObject
    {
        public class DetailsVisibilitySettings : ObservableObject
        {
            private bool playTime = true;
            public bool PlayTime
            {
                get
                {
                    return playTime;
                }

                set
                {
                    playTime = value;
                    OnPropertyChanged();
                }
            }

            private bool platform = true;
            public bool Platform
            {
                get
                {
                    return platform;
                }

                set
                {
                    platform = value;
                    OnPropertyChanged();
                }
            }

            private bool genres = true;
            public bool Genres
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

            private bool developers = true;
            public bool Developers
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

            private bool publishers = true;
            public bool Publishers
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

            private bool releaseDate = true;
            public bool ReleaseDate
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

            private bool categories = true;
            public bool Categories
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

            private bool tags = true;
            public bool Tags
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

            private bool links = true;
            public bool Links
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

            private bool description = true;
            public bool Description
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
        }

        private static ILogger logger = LogManager.GetCurrentClassLogger();

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

        public int Version
        {
            get; set;
        } = 1;

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
                OnPropertyChanged();
            }
        }

        public DetailsVisibilitySettings detailsVisibility = new DetailsVisibilitySettings();
        public DetailsVisibilitySettings DetailsVisibility
        {
            get
            {
                return detailsVisibility;
            }

            set
            {
                detailsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Dock gridViewDetailsPosition = Dock.Right;
        public Dock GridViewDetailsPosition
        {
            get
            {
                return gridViewDetailsPosition;
            }

            set
            {
                gridViewDetailsPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock sideBarPosition = Dock.Left;
        public Dock SideBarPosition
        {
            get
            {
                return sideBarPosition;
            }

            set
            {
                sideBarPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock filterPanelPosition = Dock.Right;
        public Dock FilterPanelPosition
        {
            get
            {
                return filterPanelPosition;
            }

            set
            {
                filterPanelPosition = value;
                OnPropertyChanged();
            }
        }

        private Dock explorerPanelPosition = Dock.Left;
        public Dock ExplorerPanelPosition
        {
            get
            {
                return explorerPanelPosition;
            }

            set
            {
                explorerPanelPosition = value;
                OnPropertyChanged();
            }
        }

        private bool explorerPanelVisible = false;
        public bool ExplorerPanelVisible
        {
            get
            {
                return explorerPanelVisible;
            }

            set
            {
                explorerPanelVisible = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public double CoverArtHeight
        {
            get
            {
                if (CoversZoom != 0)
                {
                    return CoversZoom * ((double)coverArtHeightRatio / CoverArtWidthRatio);
                }
                else
                {
                    return 0;
                }
            }
        }

        // TODO rename
        private double coversZoom = ViewSettings.DefaultCoversZoom;
        public double CoversZoom
        {
            get
            {
                return coversZoom;
            }

            set
            {
                coversZoom = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoverArtHeight));
            }
        }

        private uint coverArtWidthRatio = 92;
        public uint CoverArtWidthRatio
        {
            get
            {
                return coverArtWidthRatio;
            }

            set
            {
                coverArtWidthRatio = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoverArtHeight));
            }
        }

        private uint coverArtHeightRatio = 43;
        public uint CoverArtHeightRatio
        {
            get
            {
                return coverArtHeightRatio;
            }

            set
            {
                coverArtHeightRatio = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CoverArtHeight));
            }
        }

        private Stretch coverArtStretch = Stretch.UniformToFill;
        public Stretch CoverArtStretch
        {
            get
            {
                return coverArtStretch;
            }

            set
            {
                coverArtStretch = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private bool disableDpiAwareness = false;
        public bool DisableDpiAwareness
        {
            get
            {
                return disableDpiAwareness;
            }

            set
            {
                disableDpiAwareness = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }


        private bool showNamesUnderCovers = false;
        public bool ShowNamesUnderCovers
        {
            get
            {
                return showNamesUnderCovers;
            }

            set
            {
                showNamesUnderCovers = value;
                OnPropertyChanged();
            }
        }

        private bool showBackgroundImage = true;
        public bool ShowBackgroundImage
        {
            get
            {
                return showBackgroundImage;
            }

            set
            {
                showBackgroundImage = value;
                OnPropertyChanged();
            }
        }

        private bool downloadMetadataOnImport = true;
        public bool DownloadMetadataOnImport
        {
            get
            {
                return downloadMetadataOnImport;
            }

            set
            {
                downloadMetadataOnImport = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private bool showIconsOnList = true;
        public bool ShowIconsOnList
        {
            get
            {
                return showIconsOnList;
            }

            set
            {
                showIconsOnList = value;
                OnPropertyChanged();
            }
        }

        private bool showGroupCount = false;
        public bool ShowGroupCount
        {
            get
            {
                return showGroupCount;
            }

            set
            {
                showGroupCount = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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

        private FilterSettings fullScreenFilterSettings = new FilterSettings();
        public FilterSettings FullScreenFilterSettings
        {
            get
            {
                return fullScreenFilterSettings;
            }

            set
            {
                fullScreenFilterSettings = value;
            }
        }

        private ViewSettings desktopViewSettings = new ViewSettings();
        public ViewSettings ViewSettings
        {
            get
            {
                return desktopViewSettings;
            }

            set
            {
                desktopViewSettings = value;
            }
        }

        private ViewSettings fullscreenViewSettings = new ViewSettings();
        public ViewSettings FullscreenViewSettings
        {
            get
            {
                return fullscreenViewSettings;
            }

            set
            {
                fullscreenViewSettings = value;
            }
        }

        private ObservableConcurrentDictionary<string, bool> gridViewHeaders = new ObservableConcurrentDictionary<string, bool>()
        {
            { "Icon", true },
            { "Name", true },
            { "Platform", false },
            { "Developers", false },
            { "Publishers", false },
            { "ReleaseDate", true },
            { "Genres", true },
            { "LastActivity", true },
            { "IsInstalled", false },
            { "InstallDirectory", false },
            { "Categories", false },
            { "Playtime", true },
            { "Added", false },
            { "Modified", false },
            { "PlayCount", false },
            { "Series", false },
            { "Version", false },
            { "AgeRating", false },
            { "Region", false },
            { "Source", false },
            { "CompletionStatus", false },
            { "UserScore", false },
            { "CriticScore", false },
            { "CommunityScore", false },
            { "Tags", false },
            { "Library", true }
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
                OnPropertyChanged();
            }
        }

        private bool gridViewSideBarVisible = false;
        public bool GridViewSideBarVisible
        {
            get
            {
                return gridViewSideBarVisible;
            }

            set
            {
                gridViewSideBarVisible = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }
                
        private bool notificationPanelVisible = false;
        [JsonIgnore]
        public bool NotificationPanelVisible
        {
            get
            {
                return notificationPanelVisible;
            }

            set
            {
                notificationPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private bool sidebarVisible = false;
        [JsonIgnore]
        public bool SidebarVisible
        {
            get
            {
                return sidebarVisible;
            }

            set
            {
                sidebarVisible = value;
                OnPropertyChanged();
            }
        }

        private Dock sidebarPosition = Dock.Left;
        [JsonIgnore]
        public Dock SidebarPosition
        {
            get
            {
                return sidebarPosition;
            }

            set
            {
                sidebarPosition = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private AfterGameCloseOptions afterGameClose = AfterGameCloseOptions.Restore;
        public AfterGameCloseOptions AfterGameClose
        {
            get
            {
                return afterGameClose;
            }

            set
            {
                afterGameClose = value;
                OnPropertyChanged();
            }
        }

        private string theme = "Default";
        public string Theme
        {
            get
            {
                return theme;
            }

            set
            {
                theme = value;
                OnPropertyChanged();
            }
        }

        public string InstallInstanceId
        {
            get; set;
        }

        private List<string> disabledPlugins = new List<string>();
        public List<string> DisabledPlugins
        {
            get
            {
                return disabledPlugins;
            }

            set
            {
                disabledPlugins = value;
                OnPropertyChanged();
            }
        }

        private bool showSteamFriendsButton = true;
        public bool ShowSteamFriendsButton
        {
            get
            {
                return showSteamFriendsButton;
            }

            set
            {
                showSteamFriendsButton = value;
                OnPropertyChanged();
            }
        }

        private bool startMinimized = false;
        public bool StartMinimized
        {
            get
            {
                return startMinimized;
            }

            set
            {
                startMinimized = value;
                OnPropertyChanged();
            }
        }

        private bool startOnBoot = false;
        public bool StartOnBoot
        {
            get
            {
                return startOnBoot;
            }

            set
            {
                startOnBoot = value;
                OnPropertyChanged();
            }
        }

        private bool forcePlayTimeSync = false;
        public bool ForcePlayTimeSync
        {
            get
            {
                return forcePlayTimeSync;
            }

            set
            {
                forcePlayTimeSync = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public static bool IsPortable
        {
            get
            {
                return !File.Exists(PlaynitePaths.UninstallerNsisPath) && !File.Exists(PlaynitePaths.UninstallerInnoPath);
            }
        }        

        private static PlayniteSettings instance;
        public static PlayniteSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayniteSettings();
                }

                return instance;
            }   
        }

        public PlayniteSettings()
        {
            InstallInstanceId = Guid.NewGuid().ToString();
            GridViewHeaders.PropertyChanged += GridViewHeaders_PropertyChanged;
        }

        private void GridViewHeaders_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO
            //if (e.PropertyName == "Values")
            //{
            //    OnPropertyChanged(nameof(GridViewHeaders));
            //}
        }

        public static PlayniteSettings LoadSettings()
        {
            try
            {
                if (File.Exists(PlaynitePaths.ConfigFilePath))
                {
                    var settings = JsonConvert.DeserializeObject<PlayniteSettings>(File.ReadAllText(PlaynitePaths.ConfigFilePath));
                    instance = settings;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to load application settings.");
                instance = new PlayniteSettings();
            }

            return Instance;
        }

        public void SaveSettings()
        {
            FileSystem.CreateDirectory(PlaynitePaths.ConfigRootPath);
            File.WriteAllText(PlaynitePaths.ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
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
                FileName = Path.Combine(PlaynitePaths.ConfigRootPath, "playnite.log"),
                Layout = "${longdate}|${level:uppercase=true}:${message}${exception:format=toString}",
                KeepFileOpen = false,
                ArchiveFileName = Path.Combine(PlaynitePaths.ConfigRootPath, "playnite.{#####}.log"),
                ArchiveAboveSize = 4096000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2
            };

            config.AddTarget("file", fileTarget);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;
        }

        public static string GetAppConfigValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetAppConfigBoolValue(string key)
        {
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out var result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }

        public static void MigrateSettingsConfig()
        {
            void WriteConfig(string id, Dictionary<string, object> config)
            {
                var path = Path.Combine(PlaynitePaths.ExtensionsDataPath, id, "config.json");
                if (!File.Exists(path))
                {
                    FileSystem.CreateDirectory(path);
                    File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                }
            }

            try
            {
                if (!File.Exists(PlaynitePaths.ConfigFilePath))
                {
                    return;
                }

                var oldSettings = JsonConvert.DeserializeObject<Settings.OldSettings.Settings>(File.ReadAllText(PlaynitePaths.ConfigFilePath));
                if (oldSettings.BattleNetSettings != null)
                {
                    var config = new Dictionary<string, object>()
                    {
                        { "ImportInstalledGames", oldSettings.BattleNetSettings.IntegrationEnabled },
                        { "ImportUninstalledGames", oldSettings.BattleNetSettings.LibraryDownloadEnabled }
                    };

                    WriteConfig("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD", config);
                }

                if (oldSettings.OriginSettings != null)
                {
                    var config = new Dictionary<string, object>()
                    {
                        { "ImportInstalledGames", oldSettings.OriginSettings.IntegrationEnabled },
                        { "ImportUninstalledGames", oldSettings.OriginSettings.LibraryDownloadEnabled }
                    };

                    WriteConfig("85DD7072-2F20-4E76-A007-41035E390724", config);
                }

                if (oldSettings.GOGSettings != null)
                {
                    var config = new Dictionary<string, object>()
                    {
                        { "ImportInstalledGames", oldSettings.GOGSettings.IntegrationEnabled },
                        { "ImportUninstalledGames", oldSettings.GOGSettings.LibraryDownloadEnabled },
                        { "StartGamesUsingGalaxy", oldSettings.GOGSettings.RunViaGalaxy }
                    };

                    WriteConfig("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E", config);
                }

                if (oldSettings.SteamSettings != null)
                {
                    var config = new Dictionary<string, object>()
                    {
                        { "ImportInstalledGames", oldSettings.SteamSettings.IntegrationEnabled },
                        { "ImportUninstalledGames", oldSettings.SteamSettings.LibraryDownloadEnabled },
                        { "PreferScreenshotForBackground", oldSettings.SteamSettings.PreferScreenshotForBackground },
                        { "ApiKey", oldSettings.SteamSettings.APIKey },
                        { "IsPrivateAccount", oldSettings.SteamSettings.PrivateAccount },
                        { "AccountId", oldSettings.SteamSettings.AccountId },
                        { "AccountName", oldSettings.SteamSettings.AccountName },
                        { "IdSource", oldSettings.SteamSettings.IdSource }
                    };

                    WriteConfig("CB91DFC9-B977-43BF-8E70-55F46E410FAB", config);
                }

                if (oldSettings.UplaySettings != null)
                {
                    var config = new Dictionary<string, object>()
                    {
                        { "ImportInstalledGames", oldSettings.UplaySettings.IntegrationEnabled }
                    };

                    WriteConfig("C2F038E5-8B92-4877-91F1-DA9094155FC5", config);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to migrade plugin configuration.");
            }
        }

        public static void SetBootupStateRegistration(bool runOnBootup)
        {
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortcutPath = Path.Combine(startupPath, "Playnite.lnk");
            if (runOnBootup)
            {
                FileSystem.DeleteFile(shortcutPath);
                Programs.CreateShortcut(PlaynitePaths.ExecutablePath, "", "", shortcutPath);
            }
            else
            {
                FileSystem.DeleteFile(shortcutPath);
            }
        }
    }
}
