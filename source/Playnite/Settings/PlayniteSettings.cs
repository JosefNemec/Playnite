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
using Playnite.Common.System;

namespace Playnite.Settings
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

    public class PlayniteSettings : INotifyPropertyChanged, IEditableObject
    {
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
                OnPropertyChanged("ShowNamesUnderCovers");
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
                OnPropertyChanged("ShowBackgroundImage");
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
                OnPropertyChanged("DownloadMetadataOnImport");
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
            { "Provider", true }
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
                OnPropertyChanged("AfterGameClose");
            }
        }

        private string skin = "Modern";
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

        private FullscreenSettings fullscreenSettings = new FullscreenSettings();
        public FullscreenSettings FullscreenSettings
        {
            get
            {
                return fullscreenSettings;
            }

            set
            {
                fullscreenSettings = value;
                OnPropertyChanged("FullscreenSettings");
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
                OnPropertyChanged("DisabledPlugins");
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
                OnPropertyChanged("ShowSteamFriendsButton");
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
                OnPropertyChanged("StartMinimized");
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
                OnPropertyChanged("StartOnBoot");
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

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public List<string> EditedFields;
        private bool isEditing = false;
        private PlayniteSettings editingCopy;

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
            editingCopy.CopyProperties(this, false, new List<string>()
            {
                "FilterSettings", "FullScreenFilterSettings", "InstallInstanceId", "ViewSettings", "FullscreenViewSettings"
            });
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

        public static PlayniteSettings LoadSettings()
        {
            if (File.Exists(PlaynitePaths.ConfigFilePath))
            {
                var settings = JsonConvert.DeserializeObject<PlayniteSettings>(File.ReadAllText(PlaynitePaths.ConfigFilePath));
                instance = settings;
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
                FileSystem.CreateDirectory(path);
                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            try
            {
                if (!File.Exists(PlaynitePaths.ConfigFilePath))
                {
                    return;
                }

                var oldSettings = JsonConvert.DeserializeObject<OldSettings.Settings>(File.ReadAllText(PlaynitePaths.ConfigFilePath));
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
