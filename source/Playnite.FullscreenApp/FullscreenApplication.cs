using Playnite.API;
using Playnite.Audio;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.FullscreenApp.API;
using Playnite.FullscreenApp.Markup;
using Playnite.FullscreenApp.ViewModels;
using Playnite.FullscreenApp.Windows;
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

        private SplashScreen splashScreen;
        public static AudioPlaybackEngine Audio;
        private static CachedSound navigateSound;
        private static CachedSound activateSound;
        private static PlayingSound backgroundSound;
        private static string backgroundSoundPath;

        public new static FullscreenApplication Current
        {
            get => PlayniteApplication.Current == null ? null : (FullscreenApplication)PlayniteApplication.Current;
        }

        public FullscreenApplication(Func<Application> appInitializer, SplashScreen splashScreen, CmdLineOptions cmdLine)
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
                Process.Start(PlaynitePaths.DesktopExecutablePath);
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
            SetupInputs();
            OpenMainViewAsync();
#pragma warning disable CS4014
            StartUpdateCheckerAsync();
            SendUsageDataAsync();
#pragma warning restore CS4014
            ProcessArguments();
            InitializeAudio();
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
                    Audio?.PausePlayback();
                }
                else if (AppSettings.Fullscreen.MuteInBackground && IsActive == true)
                {
                    Audio?.ResumePlayback();
                }

                if (XInputDevice != null)
                {
                    XInputDevice.StandardProcessingEnabled = IsActive;
                }
            }
        }

        private async void OpenMainViewAsync()
        {
            Extensions.LoadPlugins(AppSettings.DisabledPlugins, CmdLine.SafeStartup, AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            Extensions.LoadScripts(AppSettings.DisabledPlugins, CmdLine.SafeStartup, AppSettings.DevelExtenions.Where(a => a.Selected == true).Select(a => a.Item).ToList());
            OnExtensionsLoaded();

            splashScreen?.Close(new TimeSpan(0));
            MainModel.OpenView();
            CurrentNative.MainWindow = MainModel.Window.Window;
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
            StopBackgroundSound();
            Audio?.Dispose();
            base.ReleaseResources(releaseCefSharp);
        }

        public static void PlayNavigateSound()
        {
            if (Audio == null || navigateSound == null)
            {
                return;
            }

            if (!SoundsEnabled || Current.AppSettings.Fullscreen.InterfaceVolume == 0)
            {
                return;
            }

            try
            {
                Audio?.PlaySound(navigateSound, Current.AppSettings.Fullscreen.InterfaceVolume);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play navigation sound.");
            }
        }

        public static void PlayActivateSound()
        {
            if (Audio == null || activateSound == null)
            {
                return;
            }

            if (!SoundsEnabled || Current.AppSettings.Fullscreen.InterfaceVolume == 0)
            {
                return;
            }

            try
            {
                Audio?.PlaySound(activateSound, Current.AppSettings.Fullscreen.InterfaceVolume);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play activation sound.");
            }
        }

        public static void PlayBackgroundSound()
        {
            if (Audio == null)
            {
                return;
            }

            if (backgroundSoundPath.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                if (backgroundSound == null)
                {
                    backgroundSound = Audio.PlaySound(
                        backgroundSoundPath,
                        Current.AppSettings.Fullscreen.BackgroundVolume,
                        true);
                }

                if (backgroundSound != null)
                {
                    backgroundSound.Volume = Current.AppSettings.Fullscreen.BackgroundVolume;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play background sound.");
            }
        }

        public static void StopBackgroundSound()
        {
            if (Audio == null)
            {
                return;
            }

            if (backgroundSound != null)
            {
                Audio.StopPlayback(backgroundSound);
                backgroundSound.Dispose();
                backgroundSound = null;
            }
        }

        public static void SetBackgroundSoundVolume(float volume)
        {
            if (Audio == null)
            {
                return;
            }

            if (backgroundSound != null)
            {
                backgroundSound.Volume = volume;
            }
        }

        private void InitializeAudio()
        {
            try
            {
                Audio = new AudioPlaybackEngine(AppSettings.Fullscreen.AudioInterfaceApi);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to initialize audio interface.");
                Dialogs.ShowErrorMessage(LOC.ErrorAudioInterfaceInit, "");
                return;
            }

            var navigationFile = ThemeFile.GetFilePath(@"audio\navigation.wav", true);
            if (navigationFile.IsNullOrEmpty())
            {
                navigationFile = ThemeFile.GetFilePath(@"audio\navigation.mp3", true);
            }

            if (!navigationFile.IsNullOrEmpty())
            {
                try
                {
                    navigateSound = new CachedSound(navigationFile);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to sound file {navigationFile}");
                }
            }

            var activationFile = ThemeFile.GetFilePath(@"audio\activation.wav", true);
            if (activationFile.IsNullOrEmpty())
            {
                activationFile = ThemeFile.GetFilePath(@"audio\activation.mp3", true);
            }

            if (!activationFile.IsNullOrEmpty())
            {
                try
                {
                    activateSound = new CachedSound(activationFile);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to sound file {activateSound}");
                }
            }

            backgroundSoundPath = ThemeFile.GetFilePath(@"audio\background.wma", true);
            if (backgroundSoundPath.IsNullOrEmpty())
            {
                backgroundSoundPath = ThemeFile.GetFilePath(@"audio\background.mp3", true);
            }

            if (AppSettings.Fullscreen.BackgroundVolume > 0)
            {
                PlayBackgroundSound();
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
