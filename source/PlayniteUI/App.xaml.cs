using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using CefSharp;
using NLog;
using NLog.Config;
using Playnite;
using Playnite.Database;
using PlayniteUI.Windows;
using PlayniteUI.ViewModels;
using System.Threading.Tasks;
using Playnite.Services;
using System.Windows.Markup;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;
using Playnite.MetaProviders;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private MainViewModel mainModel;
        private FullscreenViewModel fullscreenModel;
        //private XInputDevice xdevice;

        public event PropertyChangedEventHandler PropertyChanged;

        public static GameDatabase Database
        {
            get;
            private set;
        }

        public static GamesEditor GamesEditor
        {
            get;
            private set;
        }

        public static Settings AppSettings
        {
            get;
            private set;
        }

        public static App CurrentApp
        {
            get => Current as App;
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsActive"));
            }
        }

        public App()
        {
            InitializeComponent();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (!resourcesReleased)
            {
                ReleaseResources();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Error(exception, "Unhandled exception occured.");

            var model = new CrashHandlerViewModel(
                CrashHandlerWindowFactory.Instance, new DialogsFactory(), new ResourceProvider());
            model.Exception = exception.ToString();
            model.OpenView();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            // Multi-instance checking
            if (Mutex.TryOpenExisting(instanceMuxet, out var mutex))
            {
                try
                {
                    var client = new PipeClient(Settings.GetAppConfigValue("PipeEndpoint"));
                    if (e.Args.Count() > 0 && e.Args.Contains("-command"))
                    {
                        var commandArgs = e.Args[1].Split(new char[] { ':' });
                        var command = commandArgs[0];
                        client.InvokeCommand(command, commandArgs.Count() > 1 ? commandArgs[1] : string.Empty);
                    }
                    else
                    {
                        client.InvokeCommand(CmdlineCommands.Focus, string.Empty);
                    }
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    PlayniteMessageBox.Show(
                        ResourceProvider.Instance.FindString("StartGenericError"),
                        ResourceProvider.Instance.FindString("StartupError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    logger.Error(exc, "Can't process communication with other instances.");
                }

                logger.Info("Application already running, shutting down.");
                Shutdown();
                return;
            }
            else
            {
                appMutex = new Mutex(true, instanceMuxet);
            }

            AppSettings = Settings.LoadSettings();
            Localization.SetLanguage(AppSettings.Language);
            Resources.Remove("AsyncImagesEnabled");
            Resources.Add("AsyncImagesEnabled", AppSettings.AsyncImageLoading);
            if (AppSettings.DisableHwAcceleration)
            {
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }

            Settings.ConfigureLogger();
            Settings.ConfigureCef();

            // Load skin
            LoadSkin(AppSettings.Skin, AppSettings.SkinColor, false);

            // First run wizard
            ulong steamCatImportId = 0;
            bool isFirstStart = !AppSettings.FirstTimeWizardComplete;
            if (!AppSettings.FirstTimeWizardComplete)
            {
                var wizardWindow = FirstTimeStartupWindowFactory.Instance;
                var wizardModel = new FirstTimeStartupViewModel(
                    wizardWindow,
                    new DialogsFactory(),
                    new ResourceProvider());
                if (wizardModel.OpenView() == true)
                {
                    var settings = wizardModel.Settings;
                    AppSettings.FirstTimeWizardComplete = true;
                    if (wizardModel.DatabaseLocation == FirstTimeStartupViewModel.DbLocation.Custom)
                    {
                        AppSettings.DatabasePath = settings.DatabasePath;
                    }
                    else
                    {
                        AppSettings.DatabasePath = Path.Combine(Paths.UserProgramDataPath, "games.db");
                    }

                    AppSettings.SteamSettings = settings.SteamSettings;
                    AppSettings.GOGSettings = settings.GOGSettings;
                    AppSettings.OriginSettings = settings.OriginSettings;
                    AppSettings.BattleNetSettings = settings.BattleNetSettings;
                    AppSettings.UplaySettings = settings.UplaySettings;
                    AppSettings.SaveSettings();

                    Database = new GameDatabase(AppSettings, AppSettings.DatabasePath);
                    Database.OpenDatabase();

                    if (wizardModel.ImportedGames.Count > 0)
                    {
                        foreach (var game in wizardModel.ImportedGames)
                        {
                            if (game.Icon != null)
                            {
                                var iconId = "images/custom/" + game.Icon.Name;
                                game.Game.Icon = Database.AddFileNoDuplicate(iconId, game.Icon.Name, game.Icon.Data); ;
                            }

                            Database.AddGame(game.Game);
                        }
                    }

                    if (wizardModel.SteamImportCategories)
                    {
                        steamCatImportId = wizardModel.SteamIdCategoryImport;
                    }
                }
                else
                {
                    AppSettings.DatabasePath = Path.Combine(Paths.UserProgramDataPath, "games.db");
                    AppSettings.SaveSettings();
                    Database = new GameDatabase(AppSettings, AppSettings.DatabasePath);
                }
            }
            else
            {
                Database = new GameDatabase(AppSettings, AppSettings.DatabasePath);
            }

            // Emulator wizard
            if (!AppSettings.EmulatorWizardComplete)
            {
                var model = new EmulatorImportViewModel(Database,
                       EmulatorImportViewModel.DialogType.Wizard,
                       EmulatorImportWindowFactory.Instance,
                       new DialogsFactory(),
                       new ResourceProvider());

                model.OpenView();                
                AppSettings.EmulatorWizardComplete = true;
                AppSettings.SaveSettings();
            }

            GamesEditor = new GamesEditor(Database, AppSettings);
            CustomImageStringToImageConverter.Database = Database;

            // Main view startup
            if (AppSettings.StartInFullscreen)
            {
                OpenFullscreenView();
            }
            else
            {
                OpenNormalView(steamCatImportId, isFirstStart);
            }

            // Update and stats
            CheckUpdate();
            SendUsageData();

            // Pipe server
            pipeService = new PipeService();
            pipeService.CommandExecuted += PipeService_CommandExecuted;
            pipeServer = new PipeServer(Settings.GetAppConfigValue("PipeEndpoint"));
            pipeServer.StartServer(pipeService);

            var args = Environment.GetCommandLineArgs();
            if (args.Count() > 0 && args.Contains("-command"))
            {
                var commandArgs = args[2].Split(new char[] { ':' });
                var command = commandArgs[0];
                var cmdArgs = commandArgs.Count() > 1 ? commandArgs[1] : string.Empty;
                PipeService_CommandExecuted(this, new CommandExecutedEventArgs(command, cmdArgs));
            }

            //xdevice = new XInputDevice(InputManager.Current);

            logger.Info("Application started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    mainModel.RestoreWindow();
                    break;

                case CmdlineCommands.Launch:
                    var game = Database.GamesCollection.FindById(int.Parse(args.Args));
                    if (game == null)
                    {
                        logger.Error("Cannot start game, game {0} not found.", args.Args);
                    }
                    else
                    {
                        GamesEditor.PlayGame(game);
                    }

                    break;

                default:
                    logger.Warn("Unknown command received");
                    break;
            }
        }

        private async void CheckUpdate()
        {
            await Task.Factory.StartNew(() =>
            {
                var update = new Update();

                while (true)
                {
                    try
                    {
                        if (update.IsUpdateAvailable)
                        {
                            update.DownloadUpdate();

                            try
                            {
                                update.DownloadReleaseNotes();
                            }
                            catch (Exception exc)
                            {
                                logger.Warn(exc, "Failed to download release notes.");
                            }

                            Dispatcher.Invoke(() =>
                            {
                                var model = new UpdateViewModel(update, UpdateWindowFactory.Instance);
                                model.OpenView();
                            });
                        }
                    }
                    catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(exc, "Failed to process update.");
                    }

                    Thread.Sleep(4 * 60 * 60 * 1000);
                }
            });
        }

        private async void SendUsageData()
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    var client = new ServicesClient();
                    client.PostUserUsage();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to post user usage data.");
                }
            });
        }

        public void Restart()
        {
            ReleaseResources();
            Process.Start(Paths.ExecutablePath);
            Shutdown(0);
        }

        private void ReleaseResources()
        {
            GamesLoaderHandler.CancelToken?.Cancel();
            Database?.CloseDatabase();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            AppSettings?.SaveSettings();
            appMutex?.ReleaseMutex();
            if (Cef.IsInitialized)
            {
                Cef.Shutdown();
            }

            resourcesReleased = true;
        }

        public async void OpenNormalView(ulong steamCatImportId, bool isFirstStart)
        {
            if (fullscreenModel != null)
            {
                fullscreenModel.CloseView();
                fullscreenModel = null;
                Current.MainWindow = null;
            }

            LoadSkin(AppSettings.Skin, AppSettings.SkinColor, false);
            var window = new MainWindowFactory();
            mainModel = new MainViewModel(
                Database,
                window,
                new DialogsFactory(),
                new ResourceProvider(),
                AppSettings,
                GamesEditor);
            mainModel.OpenView();
            Current.MainWindow = window.Window;
            await mainModel.LoadGames(AppSettings.UpdateLibStartup, steamCatImportId, !isFirstStart);

            if (isFirstStart)
            {
                var metaSettings = new MetadataDownloaderSettings();
                metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                await mainModel.DownloadMetadata(metaSettings);
            }
        }

        public async void OpenFullscreenView()
        {
            if (mainModel != null)
            {
                mainModel.CloseView();
                mainModel = null;
                Current.MainWindow = null;
            }

            LoadSkin(AppSettings.SkinFullscreen, AppSettings.SkinColorFullscreen, true);
            var window = new FullscreenWindowFactory();
            fullscreenModel = new FullscreenViewModel(
                Database,
                window,
                new DialogsFactory(),
                new ResourceProvider(),
                AppSettings,
                GamesEditor);

            fullscreenModel.OpenView();
            Current.MainWindow = window.Window;
            await fullscreenModel.LoadGames(AppSettings.UpdateLibStartup, 0, true);

            Current.MainWindow = window.Window;
            fullscreenModel.OpenView(false);
        }

        private void LoadSkin(string name, string profile, bool fullscreen)
        {
            try
            {
                if (fullscreen)
                {
                    Skins.ApplyFullscreenSkin(name, profile);
                }
                else
                {
                    Skins.ApplySkin(name, profile);
                }
            }
            catch (Exception exc)
            {
                PlayniteMessageBox.Show(
                    ResourceProvider.Instance.FindString(string.Format("SkinApplyError", AppSettings.Skin, AppSettings.SkinColor, exc.Message)),
                    ResourceProvider.Instance.FindString("SkinError"), MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }
        }

        private void Application_Activated(object sender, EventArgs e)
        {
            IsActive = true;
        }

        private void Application_Deactivated(object sender, EventArgs e)
        {
            IsActive = false;
        }
    }
}
