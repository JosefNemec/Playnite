﻿using Hardcodet.Wpf.TaskbarNotification;
using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.DesktopApp.API;
using Playnite.DesktopApp.Controls;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.DesktopApp.Windows;
using Playnite.Metadata;
using Playnite.Plugins;
using Playnite.SDK;
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
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp
{
    public class DesktopApplication : PlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private TaskbarIcon trayIcon;
        public const string DefaultThemeName = "Default";
        private SplashScreen splashScreen;

        public List<ThirdPartyTool> ThirdPartyTools { get; private set; }
        public DesktopAppViewModel MainModel { get; set; }
        public new static DesktopApplication Current
        {
            get => (DesktopApplication)PlayniteApplication.Current;
        }

        public DesktopApplication(App nativeApp, SplashScreen splashScreen) : base(nativeApp, ApplicationMode.Desktop, DefaultThemeName)
        {
            this.splashScreen = splashScreen;
        }

        public override void Startup()
        {
            ProgressWindowFactory.SetWindowType<ProgressWindow>();
            CrashHandlerWindowFactory.SetWindowType<CrashHandlerWindow>();
            UpdateWindowFactory.SetWindowType<UpdateWindow>();
            Dialogs = new DesktopDialogs();
            Playnite.Dialogs.SetHandler(Dialogs);

            if (CheckOtherInstances())
            {
                return;
            }

            ConfigureApplication();
            if (AppSettings.DisableDpiAwareness)
            {
                DisableDpiAwareness();
            }

            InstantiateApp();
            var isFirstStart = ProcessStartupWizard();
            MigrateDatabase();
            SetupInputs(AppSettings.EnableControllerInDesktop);
            OpenMainViewAsync(isFirstStart);
            LoadTrayIcon();
            StartUpdateCheckerAsync();            
            SendUsageDataAsync();
            ProcessArguments();
            splashScreen.Close(new TimeSpan(0));
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

        public override void ReleaseResources()
        {
            trayIcon?.Dispose();
            base.ReleaseResources();
        }

        public override void Restart()
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.DesktopExecutablePath);
            CurrentNative.Shutdown(0);
        }

        public override void Restart(CmdLineOptions options)
        {
            ReleaseResources();
            Process.Start(PlaynitePaths.DesktopExecutablePath, options.ToString());
            CurrentNative.Shutdown(0);
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
            GamesEditor = new DesktopGamesEditor(
                Database,
                Controllers,
                AppSettings,
                Dialogs,
                Extensions,
                this);
            Game.DatabaseReference = Database;
            ImageSourceManager.SetDatabase(Database);
            MainModel = new DesktopAppViewModel(
                Database,
                new MainWindowFactory(),
                Dialogs,
                new ResourceProvider(),
                AppSettings,
                (DesktopGamesEditor)GamesEditor,
                Api,
                Extensions,
                this);
            Api.MainView = new MainViewAPI(MainModel);
        }

        private void LoadTrayIcon()
        {
            if (!AppSettings.EnableTray)
            {
                return;
            }

            trayIcon = new TaskbarIcon
            {
                MenuActivation = PopupActivationMode.LeftOrRightClick,
                DoubleClickCommand = MainModel.ShowWindowCommand,
                Icon = GetTrayIcon(),
                Visibility = Visibility.Visible,
                ContextMenu = new TrayContextMenu(MainModel)
            };
        }

        private async void OpenMainViewAsync(bool isFirstStart)
        {
            Extensions.LoadPlugins(Api, AppSettings.DisabledPlugins);
            Extensions.LoadScripts(Api, AppSettings.DisabledPlugins);

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
                await MainModel.UpdateDatabase(false);
                await MainModel.DownloadMetadata(AppSettings.DefaultMetadataSettings);
            }
            else
            {
                if (AppSettings.UpdateLibStartup && !CmdLine.SkipLibUpdate)
                {
                    await MainModel.UpdateDatabase(AppSettings.DownloadMetadataOnImport);
                }
            }
        }

        private bool ProcessStartupWizard()
        {
            // TODO test db path recovery
            var firstStartup = true;
            var defaultPath = GameDatabase.GetFullDbPath(GameDatabase.GetDefaultPath(PlayniteSettings.IsPortable));
            if (!AppSettings.DatabasePath.IsNullOrEmpty())
            {
                firstStartup = false;
            }
            else if (AppSettings.DatabasePath.IsNullOrEmpty() && Directory.Exists(defaultPath))
            {
                AppSettings.DatabasePath = GameDatabase.GetDefaultPath(PlayniteSettings.IsPortable);
                firstStartup = false;
            }

            if (firstStartup)
            {
                AppSettings.DatabasePath = GameDatabase.GetDefaultPath(PlayniteSettings.IsPortable);
                AppSettings.SaveSettings();
                Database.SetDatabasePath(AppSettings.DatabasePath);
                Database.OpenDatabase();

                var wizardWindow = new FirstTimeStartupWindowFactory();
                var wizardModel = new FirstTimeStartupViewModel(
                    wizardWindow,
                    Dialogs,
                    new ResourceProvider(),
                    Extensions,
                    Api);
                if (wizardModel.OpenView() == true)
                {
                    var settings = wizardModel.Settings;
                    AppSettings.GridItemWidthRatio = settings.GridItemWidthRatio;
                    AppSettings.GridItemHeightRatio = settings.GridItemHeightRatio;
                    AppSettings.DefaultMetadataSettings = settings.DefaultMetadataSettings;
                    AppSettings.FirstTimeWizardComplete = true;
                    AppSettings.DisabledPlugins = settings.DisabledPlugins;
                    AppSettings.SaveSettings();
                }
                else
                {
                    AppSettings.FirstTimeWizardComplete = true;
                    AppSettings.SaveSettings();
                }

                // Emulator wizard
                if (wizardModel.StartEmulatorWizard)
                {
                    var model = new EmulatorImportViewModel(Database,
                       EmulatorImportViewModel.DialogType.Wizard,
                       new EmulatorImportWindowFactory(),
                       Dialogs,
                       new ResourceProvider());
                    model.OpenView();
                }
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
    }
}
