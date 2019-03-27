using Hardcodet.Wpf.TaskbarNotification;
using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp
{
    public class DesktopApplication : PlayniteApplication
    {
        private ILogger logger = LogManager.GetLogger();
        private TaskbarIcon trayIcon;
        public const string DefaultThemeName = "Default";

        public List<ThirdPartyTool> ThirdPartyTools { get; private set; }
        public DesktopAppViewModel MainModel { get; set; }
        public new static DesktopApplication Current
        {
            get => (DesktopApplication)PlayniteApplication.Current;
        }

        public DesktopApplication(App nativeApp) : base(nativeApp, ApplicationMode.Desktop, DefaultThemeName)
        {
        }

        public override void Startup()
        {            
            ProgressWindowFactory.SetWindowType(typeof(ProgressWindow));
            CrashHandlerWindowFactory.SetWindowType(typeof(CrashHandlerWindow));
            UpdateWindowFactory.SetWindowType(typeof(UpdateWindow));
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
            LoadExtensions();
            SetupInputs();
            OpenMainViewAsync(isFirstStart);
            LoadTrayIcon();
            StartUpdateCheckerAsync();            
            SendUsageDataAsync();
            ProcessArguments(); 
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

            // TODO
            Api.Notifications.Add(new NotificationMessage("design1", "Design notification message 1", NotificationType.Error));
            Api.Notifications.Add(new NotificationMessage("design2", "Design message 2", NotificationType.Info));
            Api.Notifications.Add(new NotificationMessage("design3", "Design notification message 3, long message that does to multiple lies. Long message that does to multiple lies.", NotificationType.Error));
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
                Icon = new Icon(ThemeFile.GetFilePath("Images/applogo.ico", ThemeManager.DefaultTheme, ThemeManager.CurrentTheme)),
                Visibility = Visibility.Visible,
                ContextMenu = new TrayContextMenu(MainModel)
            };
        }

        private async void OpenMainViewAsync(bool isFirstStart)
        {
            try
            {
                MainModel.ThirdPartyTools = ThirdPartyToolsList.GetTools(Extensions.LibraryPlugins?.Select(a => a.Value.Plugin));
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
                var metaSettings = new MetadataDownloaderSettings();
                metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                await MainModel.DownloadMetadata(metaSettings);
            }
            else
            {
                if (AppSettings.UpdateLibStartup)
                {
                    await MainModel.UpdateDatabase(AppSettings.DownloadMetadataOnImport);
                }
            }
        }

        private bool ProcessStartupWizard()
        {
            var isFirstStart = AppSettings.DatabasePath.IsNullOrEmpty();
            if (isFirstStart)
            {
                if (PlayniteSettings.IsPortable)
                {
                    AppSettings.DatabasePath = @"{PlayniteDir}\library";
                }
                else
                {
                    AppSettings.DatabasePath = Path.Combine(PlaynitePaths.ConfigRootPath, "library");
                }

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

            return isFirstStart;
        }
    }
}
