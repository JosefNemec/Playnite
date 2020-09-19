using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.FullscreenApp.API;
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

        public FullscreenAppViewModel MainModel { get; set; }
        public const string DefaultThemeName = "Default";
        private SplashScreen splashScreen;

        public new static FullscreenApplication Current
        {
            get => PlayniteApplication.Current == null ? null : (FullscreenApplication)PlayniteApplication.Current;
        }

        public FullscreenApplication(App nativeApp, SplashScreen splashScreen, CmdLineOptions cmdLine)
            : base(nativeApp, ApplicationMode.Fullscreen, DefaultThemeName, cmdLine)
        {
            this.splashScreen = splashScreen;
        }

        public override void Startup()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashWindow>();
            ExtensionCrashHandlerWindowFactory.SetWindowType<ExtensionCrashWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            Dialogs = new FullscreenDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);
            if (!AppSettings.FirstTimeWizardComplete)
            {
                Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCFullscreenFirstTimeError"), "");
                ReleaseResources();
                Process.Start(PlaynitePaths.DesktopExecutablePath);
                CurrentNative.Shutdown(0);
                return;
            }

            ConfigureApplication();
            InstantiateApp();
            MigrateDatabase();
            SetupInputs(true);
            OpenMainViewAsync();
#pragma warning disable CS4014
            StartUpdateCheckerAsync();
            SendUsageDataAsync();
#pragma warning restore CS4014
            ProcessArguments();
        }

        public override void InstantiateApp()
        {
            Database = new GameDatabase();
            Controllers = new GameControllerFactory(Database);
            Extensions = new ExtensionFactory(Database, Controllers);
            GamesEditor = new GamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this);
            Api = new PlayniteAPI(
                new DatabaseAPI(Database),
                Dialogs,
                null,
                new PlayniteInfoAPI(),
                new PlaynitePathsAPI(),
                new WebViewFactory(),
                new ResourceProvider(),
                new NotificationsAPI(),
                GamesEditor,
                new PlayniteUriHandler(),
                new PlayniteSettingsAPI(AppSettings));
            Game.DatabaseReference = Database;
            ImageSourceManager.SetDatabase(Database);
            MainModel = new FullscreenAppViewModel(
                Database,
                new MainWindowFactory(),
                Dialogs,
                new ResourceProvider(),
                AppSettings,
                GamesEditor,
                Api,
                Extensions,
                this);
            Api.MainView = new MainViewAPI(MainModel);
        }

        private async void OpenMainViewAsync()
        {
            Extensions.LoadPlugins(Api, AppSettings.DisabledPlugins, CmdLine.SafeStartup);
            Extensions.LoadScripts(Api, AppSettings.DisabledPlugins, CmdLine.SafeStartup);
            splashScreen?.Close(new TimeSpan(0));
            MainModel.OpenView();
            CurrentNative.MainWindow = MainModel.Window.Window;

            if (AppSettings.UpdateLibStartup && !CmdLine.SkipLibUpdate)
            {
                await MainModel.UpdateDatabase(AppSettings.DownloadMetadataOnImport);
            }
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

        public override void Restart()
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.FullscreenExecutablePath);
            CurrentNative.Shutdown(0);
        }

        public override void Restart(CmdLineOptions options)
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.FullscreenExecutablePath, options.ToString());
            CurrentNative.Shutdown(0);
        }

        public override void ShowWindowsNotification(string title, string body, Action action)
        {
            // Fullscreen mode shoulnd't show anything since user has no way how inteact with it
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
    }
}
