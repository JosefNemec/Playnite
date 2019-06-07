using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
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

namespace Playnite.FullscreenApp
{
    public class FullscreenApplication : PlayniteApplication
    {
        private static ILogger logger = LogManager.GetLogger();

        public FullscreenAppViewModel MainModel { get; set; }
        public const string DefaultThemeName = "Default";

        public new static FullscreenApplication Current
        {
            get => (FullscreenApplication)PlayniteApplication.Current;
        }

        public FullscreenApplication(App nativeApp) : base(nativeApp, ApplicationMode.Fullscreen, DefaultThemeName)
        {
        }

        public override void Startup()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            Dialogs = new FullscreenDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);

            if (CheckOtherInstances())
            {
                return;
            }

            if (!AppSettings.FirstTimeWizardComplete)
            {
                Dialogs.ShowErrorMessage(ResourceProvider.GetString("LOCFullscreenFirstTimeError"), "");
                ReleaseResources();
                Process.Start(PlaynitePaths.DesktopExecutablePath);
                CurrentNative.Shutdown(0);
                return;
            }

            ConfigureApplication();
            DisableDpiAwareness();
            InstantiateApp();
            MigrateDatabase();
            SetupInputs(true);
            OpenMainViewAsync();
            StartUpdateCheckerAsync();
            SendUsageDataAsync();
            ProcessArguments();
        }

        public override void InstantiateApp()
        {
            Database = new GameDatabase();
            Controllers = new GameControllerFactory(Database);
            Api = new PlayniteAPI(
                new DatabaseAPI(Database),
                Dialogs,
                null,
                new PlayniteInfoAPI(),
                new PlaynitePathsAPI(),
                new WebViewFactory(),
                new ResourceProvider(),
                new NotificationsAPI());
            Extensions = new ExtensionFactory(Database, Controllers);
            Extensions.LoadPlugins(Api, AppSettings.DisabledPlugins);
            Extensions.LoadScripts(Api, AppSettings.DisabledPlugins);
            GamesEditor = new GamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this);
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
            Extensions.LoadPlugins(Api, AppSettings.DisabledPlugins);
            Extensions.LoadScripts(Api, AppSettings.DisabledPlugins);
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
    }
}
