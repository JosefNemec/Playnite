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

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string instanceMuxet = "PlayniteInstaceMutex";
        private Mutex appMutex;
        private bool resourcesReleased = false;
        private PipeService pipeService;
        private PipeServer pipeServer;
        private MainViewModel mainModel;
        private FullscreenViewModel fullscreenModel;
        private XInputDevice xdevice;

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
            model.ShowDialog();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppSettings = Settings.LoadSettings();
            Localization.SetLanguage(AppSettings.Language);
            Resources.Remove("AsyncImagesEnabled");
            Resources.Add("AsyncImagesEnabled", AppSettings.AsyncImageLoading);
            if (AppSettings.DisableHwAcceleration)
            {
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            }

            Database = new GameDatabase(AppSettings);
            GamesEditor = new GamesEditor(Database);
            CustomImageStringToImageConverter.Database = Database;
            Settings.ConfigureLogger();
            Settings.ConfigureCef();

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
                    PlayniteMessageBox.Show("Playnite failed to start. Please close all running instances and try again.",
                        "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Load skin
            try
            {
                Skins.ApplySkin(AppSettings.Skin, AppSettings.SkinColor);
            }
            catch (Exception exc)
            {
                PlayniteMessageBox.Show(
                    $"Failed to apply skin \"{AppSettings.Skin}\", color profile \"{AppSettings.SkinColor}\"\n\n{exc.Message}",
                    "Skin Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // First run wizard
            ulong steamCatImportId = 0;
            if (!AppSettings.FirstTimeWizardComplete)
            {
                var wizardWindow = FirstTimeStartupWindowFactory.Instance;
                var wizardModel = new FirstTimeStartupViewModel(
                    wizardWindow,
                    new DialogsFactory(),
                    new ResourceProvider());
                if (wizardModel.ShowDialog() == true)
                {
                    var settings = wizardModel.Settings;
                    AppSettings.FirstTimeWizardComplete = true;
                    if (wizardModel.DatabaseLocation == FirstTimeStartupViewModel.DbLocation.Custom)
                    {
                        AppSettings.DatabasePath = settings.DatabasePath;
                    }
                    else
                    {
                        AppSettings.DatabasePath = System.IO.Path.Combine(Paths.UserProgramDataPath, "games.db");
                    }

                    AppSettings.SteamSettings = settings.SteamSettings;
                    AppSettings.GOGSettings = settings.GOGSettings;
                    AppSettings.OriginSettings = settings.OriginSettings;
                    AppSettings.BattleNetSettings = settings.BattleNetSettings;
                    AppSettings.UplaySettings = settings.UplaySettings;
                    AppSettings.SaveSettings();

                    if (wizardModel.ImportedGames.Count > 0)
                    {
                        Database.OpenDatabase(AppSettings.DatabasePath);
                        foreach (var game in wizardModel.ImportedGames)
                        {
                            if (game.Icon != null)
                            {
                                var iconId = "images/custom/" + game.Icon.Name;
                                Database.AddImage(iconId, game.Icon.Name, game.Icon.Data);
                                game.Game.Icon = iconId;
                            }

                            Database.AddGame(game.Game);
                        }
                    }

                    if (wizardModel.SteamImportCategories)
                    {
                        steamCatImportId = wizardModel.SteamIdCategoryImport;
                    }
                }
            }

            // Main view startup
            if (AppSettings.StartInFullscreen)
            {
                OpenFullscreenView();
            }
            else
            {
                OpenNormalView(steamCatImportId);
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

            xdevice = new XInputDevice(InputManager.Current);

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
                                model.ShowDialog();
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
            GamesLoaderHandler.ProgressTask.Wait();
            Database.CloseDatabase();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            Cef.Shutdown();
            AppSettings?.SaveSettings();
            appMutex?.ReleaseMutex();
            resourcesReleased = true;
        }

        public void OpenNormalView(ulong steamCatImportId)
        {
            if (fullscreenModel != null)
            {
                fullscreenModel.CloseView();
                fullscreenModel = null;
                Current.MainWindow = null;
            }

            var window = new MainWindowFactory();
            mainModel = new MainViewModel(
                Database,
                window,
                new DialogsFactory(),
                new ResourceProvider(),
                new NotificationFactory(),
                AppSettings,
                GamesEditor);
            mainModel.ShowView();
            Current.MainWindow = window.Window;
            mainModel.LoadGames(AppSettings.UpdateLibStartup, steamCatImportId);
        }

        public void OpenFullscreenView()
        {
            if (mainModel != null)
            {
                mainModel.CloseView();
                mainModel = null;
                Current.MainWindow = null;
            }

            var window = new FullscreenWindowFactory();
            fullscreenModel = new FullscreenViewModel(
                Database,
                AppSettings,
                window,
                new ResourceProvider());
            Current.MainWindow = window.Window;
            fullscreenModel.OpenView(false);
        }
    }
}
