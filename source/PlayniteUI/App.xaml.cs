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
using Playnite.API;
using PlayniteUI.API;
using Playnite.Plugins;
using Playnite.Scripting;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged, IPlayniteApplication
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private XInputDevice xdevice;
        private DialogsFactory dialogs;

        public PlayniteAPI Api
        {
            get; set;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;


        public static MainViewModel MainModel
        {
            get;
            private set;
        }

        public static FullscreenViewModel FullscreenModel
        {
            get;
            private set;
        }

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
            ReleaseResources();
            appMutex?.ReleaseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Error(exception, "Unhandled exception occured.");

            var model = new CrashHandlerViewModel(
                CrashHandlerWindowFactory.Instance, dialogs, new ResourceProvider());
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
                        ResourceProvider.Instance.FindString("LOCStartGenericError"),
                        ResourceProvider.Instance.FindString("LOCStartupError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    logger.Error(exc, "Can't process communication with other instances.");
                }

                logger.Info("Application already running, shutting down.");
                Quit();
                return;
            }
            else
            {
                appMutex = new Mutex(true, instanceMuxet);
            }

            Time.Instance = new Time();
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
            dialogs = new DialogsFactory(AppSettings.StartInFullscreen);

            // Create directories
            try
            {
                Plugins.CreatePluginFolders();
                Scripts.CreateScriptFolders();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to script and plugin directories.");
            }

            // Load theme
            ApplyTheme(AppSettings.Skin, AppSettings.SkinColor, false);

            // First run wizard
            ulong steamCatImportId = 0;
            bool isFirstStart = !AppSettings.FirstTimeWizardComplete;
            if (!AppSettings.FirstTimeWizardComplete)
            {
                var wizardWindow = FirstTimeStartupWindowFactory.Instance;
                var wizardModel = new FirstTimeStartupViewModel(
                    wizardWindow,
                    dialogs,
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

                            Database.AssignPcPlatform(game.Game);
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
                       dialogs,
                       new ResourceProvider());

                model.OpenView();                
                AppSettings.EmulatorWizardComplete = true;
                AppSettings.SaveSettings();
            }

            GamesEditor = new GamesEditor(Database, AppSettings, dialogs);
            CustomImageStringToImageConverter.Database = Database;
            Api = new PlayniteAPI(Database, GamesEditor.Controllers, dialogs, null);

            // Main view startup
            if (AppSettings.StartInFullscreen)
            {
                OpenFullscreenView(true);
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

            xdevice = new XInputDevice(InputManager.Current)
            {
                SimulateAllKeys = true,
                SimulateNavigationKeys = true
            };

            logger.Info("Application started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:                    
                    MainModel?.RestoreWindow();
                    FullscreenModel?.RestoreWindow();
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
                Thread.Sleep(10000);
                if (GlobalTaskHandler.IsActive)
                {
                    GlobalTaskHandler.Wait();
                }

                var update = new Update(this);

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
                                var model = new UpdateViewModel(update, UpdateWindowFactory.Instance, new ResourceProvider(), dialogs);
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
                    client.PostUserUsage(AppSettings.InstallInstanceId);
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

        public void Quit()
        {
            ReleaseResources();
            Shutdown(0);
        }

        private void ReleaseResources()
        {
            if (resourcesReleased)
            {
                return;
            }

            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), () =>
            {
                try
                {
                    GlobalTaskHandler.CancelAndWait();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to cancel global progress task.");
                    throw;
                }

                GamesEditor?.Dispose();
                Database?.CloseDatabase();
                AppSettings?.SaveSettings();
                Api?.Dispose();
            }, ResourceProvider.Instance.FindString("LOCClosingPlaynite"));

            progressModel.ActivateProgress();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            if (Cef.IsInitialized)
            {
                Cef.Shutdown();
            }

            resourcesReleased = true;
        }

        public async void OpenNormalView(ulong steamCatImportId, bool isFirstStart)
        {
            if (Database.IsOpen)
            {
                FullscreenModel = null;
                Database.CloseDatabase();
            }

            GamesEditor.IsFullscreen = false;
            dialogs.IsFullscreen = false;
            ApplyTheme(AppSettings.Skin, AppSettings.SkinColor, false);
            var window = new MainWindowFactory();
            MainModel = new MainViewModel(
                Database,
                window,
                dialogs,
                new ResourceProvider(),
                AppSettings,
                GamesEditor);
            Api.MainView = new MainViewAPI(MainModel);
            MainModel.OpenView();
            Current.MainWindow = window.Window;
            if (AppSettings.UpdateLibStartup)
            {
                await MainModel.UpdateDatabase(AppSettings.UpdateLibStartup, steamCatImportId, !isFirstStart);
            }

            if (isFirstStart)
            {
                var metaSettings = new MetadataDownloaderSettings();
                metaSettings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                metaSettings.CoverImage.Source = MetadataSource.IGDBOverStore;
                metaSettings.Name = new MetadataFieldSettings(true, MetadataSource.Store);
                await MainModel.DownloadMetadata(metaSettings);
            }
        }

        public async void OpenFullscreenView(bool updateDb)
        {
            if (Database.IsOpen)
            {
                MainModel = null;
                Database.CloseDatabase();
            }

            GamesEditor.IsFullscreen = true;
            dialogs.IsFullscreen = true;
            ApplyTheme(AppSettings.SkinFullscreen, AppSettings.SkinColorFullscreen, true);
            var window = new FullscreenWindowFactory();
            FullscreenModel = new FullscreenViewModel(
                Database,
                window,
                dialogs,
                new ResourceProvider(),
                AppSettings,
                GamesEditor);
            Api.MainView = new MainViewAPI(MainModel);
            FullscreenModel.OpenView(!PlayniteEnvironment.IsDebugBuild);
            Current.MainWindow = window.Window;

            if (updateDb)
            {
                await FullscreenModel.UpdateDatabase(AppSettings.UpdateLibStartup, 0, true);
            }            
        }

        private void ApplyTheme(string name, string profile, bool fullscreen)
        {
            bool isThemeValid = true;
            string themeName = "Classic";
            string themeProfile = "Default";

            if (fullscreen)
            {
                if (Themes.CurrentFullscreenTheme == name && Themes.CurrentFullscreenColor == profile)
                {
                    return;
                }
            }
            else
            {
                if (Themes.CurrentTheme == name && Themes.CurrentColor == profile)
                {
                    return;
                }
            }

            var themeValid = Themes.IsThemeValid(name, fullscreen);
            if (themeValid.Item1 == false)
            {
                PlayniteMessageBox.Show(
                    string.Format(ResourceProvider.Instance.FindString("LOCSkinApplyError"), AppSettings.Skin, AppSettings.SkinColor, themeValid.Item2),
                    ResourceProvider.Instance.FindString("LOCSkinError"), MessageBoxButton.OK, MessageBoxImage.Error);
                isThemeValid = false;
            }

            var profileValid = Themes.IsColorProfileValid(name, profile, fullscreen);
            if (profileValid.Item1 == false)
            {
                PlayniteMessageBox.Show(
                    string.Format(ResourceProvider.Instance.FindString("LOCSkinApplyError"), AppSettings.Skin, AppSettings.SkinColor, profileValid.Item2),
                    ResourceProvider.Instance.FindString("LOCSkinError"), MessageBoxButton.OK, MessageBoxImage.Error);
                isThemeValid = false;
            }

            if (isThemeValid)
            {
                themeName = name;
                themeProfile = profile;
            }

            if (fullscreen)
            {
                Themes.ApplyFullscreenTheme(themeName, themeProfile, true);
            }
            else
            {
                Themes.ApplyTheme(themeName, themeProfile, true);
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
