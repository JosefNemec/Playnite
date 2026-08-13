using Hardcodet.Wpf.TaskbarNotification;
using Playnite.API;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Controls;
using Playnite.Database;
using Playnite.DesktopApp.API;
using Playnite.DesktopApp.Controls;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.DesktopApp.Windows;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.ViewModels;
using Playnite.WebView;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Playnite.Input;
using static SDL2.SDL;

namespace Playnite.DesktopApp
{
    public class DesktopApplication : PlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private TaskbarIcon trayIcon;
        private SplashScreen splashScreen;
        private bool sdlInitialized = false;
        private bool exitSDLEventLoop = false;

        private DesktopAppViewModel mainModel;
        public DesktopAppViewModel MainModel
        {
            get => mainModel;
            set
            {
                mainModel = value;
                MainModelBase = value;
            }
        }

        public new static DesktopApplication Current
        {
            get => PlayniteApplication.Current == null ? null : (DesktopApplication)PlayniteApplication.Current;
        }

        public DesktopApplication(Func<Application> appInitializer, SplashScreen splashScreen, CmdLineOptions cmdLine)
            : base(appInitializer, ApplicationMode.Desktop, cmdLine)
        {
            this.splashScreen = splashScreen;
        }

        public override void ConfigureViews()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashHandlerWindow>();
            ExtensionCrashHandlerWindowFactory.SetWindowType<ExtensionCrashHandlerWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            LicenseAgreementWindowFactory.SetWindowType<LicenseAgreementWindow>();
            SingleItemSelectionWindowFactory.SetWindowType<SingleItemSelectionWindow>();
            MultiItemSelectionWindowFactory.SetWindowType<MultiItemSelectionWindow>();
            Dialogs = new DesktopDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);
        }

        public override bool Startup()
        {
            if (!ConfigureApplication())
            {
                return false;
            }

            InstantiateApp();
            AppUriHandler = MainModel.ProcessUriRequest;
            var isFirstStart = ProcessStartupWizard();
            MigrateDatabase();
#pragma warning disable CS4014
            if (AppSettings.EnableGameControllerSupport)
            {
                InitSDL();
                SetupInputs();
            }
            OpenMainViewAsync(isFirstStart);
            LoadTrayIcon();
            StartUpdateCheckerAsync();
#pragma warning restore CS4014
            ProcessArguments();
            splashScreen?.Close(new TimeSpan(0));
            return true;
        }

        public override void InitializeNative()
        {
            ((App)CurrentNative).InitializeComponent();
        }

        public override void Restore()
        {
            MainModel?.RestoreWindow();
        }

        public override void Minimize()
        {
            MainModel.WindowState = WindowState.Minimized;
        }

        public override void ReleaseResources(bool releaseCefSharp = true)
        {
            trayIcon?.Dispose();
            MainModel?.UnregisterSystemSearchHotkey();
            exitSDLEventLoop = true;
            GameController?.Dispose();
            if (sdlInitialized) SDL_Quit();
            base.ReleaseResources(releaseCefSharp);
        }

        public override void Restart(bool saveSettings)
        {
            Restart(new CmdLineOptions { MasterInstance = true }, saveSettings);
        }

        public override void Restart(CmdLineOptions options, bool saveSettings)
        {
            options.MasterInstance = true;
            options.UserDataDir = CmdLine.UserDataDir;
            QuitAndStart(PlaynitePaths.DesktopExecutablePath, options.ToString(), saveSettings: saveSettings);
        }

        public override void InstantiateApp()
        {
            Database = new GameDatabase();
            Database.SetAsSingletonInstance();
            Controllers = new GameControllerFactory(Database);
            Extensions = new ExtensionFactory(Database, Controllers, GetApiInstance);
            GamesEditor = new DesktopGamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this,
                new DesktopActionSelector());
            Game.DatabaseReference = Database;
            ImageSourceManager.SetDatabase(Database);
            MainModel = new DesktopAppViewModel(
                Database,
                new MainWindowFactory(),
                Dialogs,
                new ResourceProvider(),
                AppSettings,
                (DesktopGamesEditor)GamesEditor,
                Extensions,
                this);
            PlayniteApiGlobal = GetApiInstance();
            SDK.API.Instance = PlayniteApiGlobal;
        }

        private void LoadTrayIcon()
        {
            if (AppSettings.EnableTray)
            {
                try
                {
                    trayIcon = new TaskbarIcon
                    {
                        MenuActivation = PopupActivationMode.LeftOrRightClick,
                        DoubleClickCommand = MainModel.ShowWindowCommand,
                        Icon = GetTrayIcon(),
                        Visibility = Visibility.Visible,
                        ContextMenu = new TrayContextMenu(MainModel)
                    };
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to initialize tray icon.");
                }
            }
        }

        private async void OpenMainViewAsync(bool isFirstStart)
        {
            if (!isFirstStart)
            {
                Extensions.LoadPlugins(
                    AppSettings.DisabledPlugins,
                    CmdLine.SafeStartup,
                    AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            }

            Extensions.LoadScripts(
                AppSettings.DisabledPlugins,
                CmdLine.SafeStartup,
                AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            OnExtensionsLoaded();

            try
            {
                MainModel.ThirdPartyTools = ThirdPartyToolsList.GetTools(Extensions.LibraryPlugins);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to load third party tools.");
            }

            MainModel.OpenView();
            CurrentNative.MainWindow = MainModel.Window.Window;

            if (isFirstStart)
            {
                await MainModel.UpdateLibrary(false, true, false);
                await MainModel.DownloadMetadata(AppSettings.MetadataSettings);
            }
            else
            {
                await MainModel.ProcessStartupLibUpdate();
            }

            // This is most likely safe place to consider application to be started properly
            FileSystem.DeleteFile(PlaynitePaths.SafeStartupFlagFile);
        }

        private bool ProcessStartupWizard()
        {
            // TODO test db path recovery
            var firstStartup = true;
            var defaultDbDir = GameDatabase.GetDefaultPath(
                PlayniteSettings.IsPortable,
                CmdLine.UserDataDir.IsNullOrWhiteSpace() ? null : PlaynitePaths.ConfigRootPath);

            if (!AppSettings.DatabasePath.IsNullOrEmpty())
            {
                AppSettings.FirstTimeWizardComplete = true;
                firstStartup = false;
            }
            else if (AppSettings.DatabasePath.IsNullOrEmpty() && Directory.Exists(GameDatabase.GetFullDbPath(defaultDbDir)))
            {
                AppSettings.DatabasePath = defaultDbDir;
                AppSettings.FirstTimeWizardComplete = true;
                firstStartup = false;
            }

            if (firstStartup)
            {
                AppSettings.DatabasePath = defaultDbDir;
                AppSettings.SaveSettings();
                Database.SetDatabasePath(AppSettings.DatabasePath);
                Database.OpenDatabase();

                var wizardWindow = new FirstTimeStartupWindowFactory();
                var wizardModel = new FirstTimeStartupViewModel(
                    wizardWindow,
                    Dialogs,
                    new ResourceProvider(),
                    Extensions,
                    ServicesClient);
                if (wizardModel.OpenView() == true)
                {
                    var settings = wizardModel.Settings;
                    AppSettings.DisabledPlugins = settings.DisabledPlugins;
                }

                AppSettings.AutoBackupEnabled = true;
                AppSettings.LastAutoBackup = DateTime.Now.AddDays(1); // Postpone first backup to not interrupt initial user experience
                AppSettings.RotatingBackups = 3;
                AppSettings.AutoBackupDir = Path.Combine(PlaynitePaths.ConfigRootPath, "Backup");
                AppSettings.AutoBackupFrequency = AutoBackupFrequency.OnceADay;
                AppSettings.AutoBackupIncludeExtensions = false;
                AppSettings.AutoBackupIncludeExtensionsData = false;
                AppSettings.AutoBackupIncludeLibFiles = false;
                AppSettings.AutoBackupIncludeThemes = false;

                AppSettings.FirstTimeWizardComplete = true;
                AppSettings.SaveSettings();
            }
            else
            {
                Database.SetDatabasePath(AppSettings.DatabasePath);
            }

            return firstStartup;
        }

        public override void ShowWindowsNotification(string title, string body, Action action)
        {
            var icon = GetTrayIcon();
            if (AppSettings.EnableTray)
            {
                trayIcon.ShowBalloonTip(title, body, icon, true);
            }
            else
            {
                WindowsNotifyIconManager.Notify(icon, title, body, action);
            }
        }

        private Icon GetTrayIcon()
        {
            var trayIconImage =
                ResourceProvider.GetResource(AppSettings.TrayIcon.GetDescription()) as BitmapImage ??
                ResourceProvider.GetResource("TrayIcon") as BitmapImage;
            return new Icon(trayIconImage.UriSource.LocalPath);
        }

        public override void SwitchAppMode(ApplicationMode mode)
        {
            if (mode == ApplicationMode.Fullscreen)
            {
                MainModel?.SwitchToFullscreenMode();
            }
            else
            {
                Restore();
            }
        }

        public override PlayniteAPI GetApiInstance(ExtensionManifest pluginOwner)
        {
            return new PlayniteAPI
            {
                Addons = new AddonsAPI(Extensions, AppSettings),
                ApplicationInfo = new PlayniteInfoAPI(),
                ApplicationSettings = new PlayniteSettingsAPI(AppSettings, Database),
                Database = new DatabaseAPI(Database),
                Dialogs = Dialogs,
                Emulation = new Emulators.Emulation(),
                MainView = new MainViewAPI(MainModel),
                Notifications = Notifications,
                Paths = new PlaynitePathsAPI(),
                Resources = new ResourceProvider(),
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database, MainModel),
                UriHandler = UriHandler,
                WebViews = new WebViewFactory(AppSettings)
            };
        }

        public override PlayniteAPI GetApiInstance()
        {
            return new PlayniteAPI
            {
                Addons = new AddonsAPI(Extensions, AppSettings),
                ApplicationInfo = new PlayniteInfoAPI(),
                ApplicationSettings = new PlayniteSettingsAPI(AppSettings, Database),
                Database = new DatabaseAPI(Database),
                Dialogs = Dialogs,
                Emulation = new Emulators.Emulation(),
                MainView = new MainViewAPI(MainModel),
                Notifications = Notifications,
                Paths = new PlaynitePathsAPI(),
                Resources = new ResourceProvider(),
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database, MainModel),
                UriHandler = UriHandler,
                WebViews = new WebViewFactory(AppSettings)
            };
        }


        private void InitSDL()
        {
            if (SDL_Init(SDL_INIT_GAMECONTROLLER) < 0)
            {
                logger.Error("SDL2 failed to initialize:");
                logger.Error(SDL_GetError());
                return;
            }

            if (SDL_GameControllerAddMappingsFromFile("gamecontrollerdb.txt") == -1)
            {
                logger.Error("Failed to load game controller mappings:");
                logger.Error(SDL_GetError());
            }

            // This should fix some random XInput controller issues
            // https://github.com/libsdl-org/SDL/issues/13047
            // https://github.com/JosefNemec/Playnite/issues/3794
            SDL_SetHint(SDL_HINT_JOYSTICK_RAWINPUT, "0");
            SDL_GameControllerEventState(SDL_IGNORE);
            SDLEventLoop();
            sdlInitialized = true;
        }

        private void SDLEventLoop()
        {
            Task.Run(async () =>
            {
                while (!exitSDLEventLoop)
                {
                    while (SDL_PollEvent(out var sdlEvent) == 1)
                    {
                        if (sdlEvent.type == SDL_EventType.SDL_CONTROLLERDEVICEADDED)
                        {
                            GameController?.AddController(sdlEvent.cdevice.which, AppSettings.DisabledGameControllers);
                        }

                        if (sdlEvent.type == SDL_EventType.SDL_CONTROLLERDEVICEREMOVED)
                        {
                            GameController?.RemoveController(sdlEvent.cdevice.which);
                        }
                    }

                    GameController?.ProcessInputs();
                    await Task.Delay(16);
                }
            });
        }

        public void SetupInputs()
        {
            if (!sdlInitialized)
            {
                return;
            }

            try
            {
                if (GameController == null)
                {
                    GameController = new GameControllerManager(InputManager.Current, AppSettings.DisabledGameControllers)
                    {
                        SimulateAllKeys = false,
                        SimulateNavigationKeys = false,
                        StandardProcessingEnabled = false
                    };
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed intitialize game controller devices.");
            }
        }

    }
}
