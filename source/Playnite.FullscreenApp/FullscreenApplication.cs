using Playnite.API;
using Playnite.Audio;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.FullscreenApp.API;
using Playnite.FullscreenApp.Markup;
using Playnite.FullscreenApp.ViewModels;
using Playnite.FullscreenApp.Windows;
using Playnite.Input;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.WebView;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static SDL2.SDL;
using static SDL2.SDL_mixer;

namespace Playnite.FullscreenApp
{
    public class FullscreenApplication : PlayniteApplication
    {
        private static ILogger logger = LogManager.GetLogger();

        private FullscreenAppViewModel mainModel;
        public FullscreenAppViewModel MainModel
        {
            get => mainModel;
            set
            {
                mainModel = value;
                MainModelBase = value;
            }
        }

        private ExtendedSplashScreen splashScreen;
        private bool sdlInitialized = false;
        public static AudioEngine Audio { get; private set; }
        public static IntPtr NavigateSound { get; private set; }
        public static IntPtr ActivateSound { get; private set; }
        public static IntPtr BackgroundMusic { get; private set; }
        public GameControllerManager GameController { get; private set; }
        private bool exitSDLEventLoop = false;

        public new static FullscreenApplication Current
        {
            get => PlayniteApplication.Current == null ? null : (FullscreenApplication)PlayniteApplication.Current;
        }

        public FullscreenApplication(Func<Application> appInitializer, ExtendedSplashScreen splashScreen, CmdLineOptions cmdLine)
            : base(appInitializer, ApplicationMode.Fullscreen, cmdLine)
        {
            this.splashScreen = splashScreen;            
        }

        public override void ConfigureViews()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashWindow>();
            ExtensionCrashHandlerWindowFactory.SetWindowType<ExtensionCrashWindow>();
            LicenseAgreementWindowFactory.SetWindowType<LicenseAgreementWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            SingleItemSelectionWindowFactory.SetWindowType<SingleItemSelectionWindow>();
            MultiItemSelectionWindowFactory.SetWindowType<MultiItemSelectionWindow>();
            Dialogs = new FullscreenDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);
        }

        public override bool Startup()
        {
            if (!AppSettings.FirstTimeWizardComplete)
            {
                Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCFullscreenFirstTimeError"), "");
                ReleaseResources();
                FileSystem.DeleteFile(PlaynitePaths.SafeStartupFlagFile);
                ProcessStarter.StartProcess(PlaynitePaths.DesktopExecutablePath, new CmdLineOptions() { MasterInstance = true }.ToString());
                CurrentNative.Shutdown(0);
                return false;
            }

            if (!ConfigureApplication())
            {
                return false;
            }

            InstantiateApp();
            AppUriHandler = MainModel.ProcessUriRequest;
            MigrateDatabase();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitSDL();
            SetupInputs();
            InitializeAudio();
            OpenMainViewAsync();
            StartUpdateCheckerAsync();
#pragma warning restore CS4014
            ProcessArguments();
            PropertyChanged += FullscreenApplication_PropertyChanged;
            return true;
        }

        public override void InstantiateApp()
        {
            Database = new GameDatabase();
            Database.SetAsSingletonInstance();
            Controllers = new GameControllerFactory(Database);
            Extensions = new ExtensionFactory(Database, Controllers, GetApiInstance);
            GamesEditor = new GamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this,
                new FullscreenActionSelector());
            Game.DatabaseReference = Database;
            ImageSourceManager.SetDatabase(Database);
            MainModel = new FullscreenAppViewModel(
                Database,
                new MainWindowFactory(),
                Dialogs,
                new ResourceProvider(),
                AppSettings,
                GamesEditor,
                Extensions,
                this);
            PlayniteApiGlobal = GetApiInstance();
            SDK.API.Instance = PlayniteApiGlobal;
        }

        private void FullscreenApplication_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteApplication.IsActive))
            {
                if (AppSettings.Fullscreen.MuteInBackground && IsActive == false)
                {
                    Mix_PauseMusic();
                }
                else if (AppSettings.Fullscreen.MuteInBackground && IsActive == true)
                {
                    Mix_ResumeMusic();
                }

                if (GameController != null && AppSettings.Fullscreen.EnableGameControllerSupport)
                {
                    GameController.StandardProcessingEnabled = IsActive;
                }
            }
        }

        private async void OpenMainViewAsync()
        {
            Extensions.LoadPlugins(AppSettings.DisabledPlugins, CmdLine.SafeStartup, AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            Extensions.LoadScripts(AppSettings.DisabledPlugins, CmdLine.SafeStartup, AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            OnExtensionsLoaded();

            MainModel.OpenView();
            CurrentNative.MainWindow = MainModel.Window.Window;
            CurrentNative.MainWindow.Activate();
            splashScreen?.Close(new TimeSpan(0));

            await MainModel.ProcessStartupLibUpdate();
            
            // This is most likely safe place to consider application to be started properly
            FileSystem.DeleteFile(PlaynitePaths.SafeStartupFlagFile);
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
            MainModel?.MinimizeWindow();
        }

        public override void Restart(bool saveSettings)
        {
            Restart(new CmdLineOptions { MasterInstance = true }, saveSettings);
        }

        public override void Restart(CmdLineOptions options, bool saveSettings)
        {
            options.MasterInstance = true;
            QuitAndStart(PlaynitePaths.FullscreenExecutablePath, options.ToString(), saveSettings: saveSettings);
        }

        public override void ShowWindowsNotification(string title, string body, Action action)
        {
            // Fullscreen mode shouldn't show anything since user has no way to interact with it
        }

        public override void SwitchAppMode(ApplicationMode mode)
        {
            if (mode == ApplicationMode.Desktop)
            {
                MainModel.SwitchToDesktopMode();
            }
            else
            {
                Restore();
            }
        }

        public override void ReleaseResources(bool releaseCefSharp = true)
        {
            if (ResourcesReleased)
            {
                return;
            }

            exitSDLEventLoop = true;
            GameController?.Dispose();
            if (Audio != null)
            {
                Mix_FreeChunk(NavigateSound);
                Mix_FreeChunk(ActivateSound);
                Mix_FreeMusic(BackgroundMusic);
                Audio.Dispose();
            }

            SDL_Quit();
            base.ReleaseResources(releaseCefSharp);
        }

        public static void PlayNavigateSound()
        {
            if (Current.AppSettings.Fullscreen.InterfaceVolume == 0)
            {
                return;
            }

            try
            {
                Mix_PlayChannel(-1, NavigateSound, 0);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play navigation sound.");
            }
        }

        public static void PlayActivateSound()
        {
            if (Current.AppSettings.Fullscreen.InterfaceVolume == 0)
            {
                return;
            }

            try
            {
                Mix_PlayChannel(-1, ActivateSound, 0);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play activation sound.");
            }
        }

        private void InitSDL()
        {
            if (SDL_Init(SDL_INIT_GAMECONTROLLER | SDL_INIT_AUDIO) < 0)
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
                            GameController?.AddController(sdlEvent.cdevice.which);
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
                    GameController = new GameControllerManager(InputManager.Current, AppSettings)
                    {
                        SimulateAllKeys = false,
                        SimulateNavigationKeys = true,
                        StandardProcessingEnabled = AppSettings.Fullscreen.EnableGameControllerSupport
                    };
                }

                UpdateConfirmCancelBindings();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed intitialize game controller devices.");
            }
        }

        public void UpdateConfirmCancelBindings()
        {
            GameControllerGesture.ConfirmationBinding = AppSettings.Fullscreen.SwapConfirmCancelButtons ? ControllerInput.B : ControllerInput.A;
            GameControllerGesture.CancellationBinding = AppSettings.Fullscreen.SwapConfirmCancelButtons ? ControllerInput.A : ControllerInput.B;
        }

        private void InitializeAudio()
        {
            if (!sdlInitialized)
            {
                return;
            }

            try
            {
                Audio = new AudioEngine();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to initialize audio interface.");
                Dialogs.ShowErrorMessage(LOC.ErrorAudioInterfaceInit, "");
                return;
            }

            var navigationFile = ThemeFile.GetFilePath($@"audio\\navigation\.({AudioEngine.SupportedFileTypesRegex})", matchByRegex: true);
            if (!navigationFile.IsNullOrEmpty())
            {
                try
                {
                    NavigateSound = Mix_LoadWAV(navigationFile);
                    Mix_VolumeChunk(NavigateSound, AudioEngine.GetVolume(AppSettings.Fullscreen.InterfaceVolume));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to sound file {navigationFile}");
                }
            }

            var activationFile = ThemeFile.GetFilePath($@"audio\\activation\.({AudioEngine.SupportedFileTypesRegex})", matchByRegex: true);
            if (!activationFile.IsNullOrEmpty())
            {
                try
                {
                    ActivateSound = Mix_LoadWAV(activationFile);
                    Mix_VolumeChunk(ActivateSound, AudioEngine.GetVolume(AppSettings.Fullscreen.InterfaceVolume));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to sound file {ActivateSound}");
                }
            }

            var backgroundSoundPath = ThemeFile.GetFilePath($@"audio\\background\.({AudioEngine.SupportedFileTypesRegex})", matchByRegex: true);
            if (!backgroundSoundPath.IsNullOrEmpty())
            {
                try
                {
                    BackgroundMusic = Mix_LoadMUS(backgroundSoundPath);
                    Mix_VolumeMusic(AudioEngine.GetVolume(AppSettings.Fullscreen.BackgroundVolume));
                    if (Current.AppSettings.Fullscreen.BackgroundVolume > 0)
                    {
                        Mix_PlayMusic(BackgroundMusic, -1);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to music file {ActivateSound}");
                }
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
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database),
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
                RootApi = new PlayniteApiRoot(GamesEditor, Extensions, Database),
                UriHandler = UriHandler,
                WebViews = new WebViewFactory(AppSettings)
            };
        }
    }
}
