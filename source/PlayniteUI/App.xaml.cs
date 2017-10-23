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
        private PipeService pipeService;
        private PipeServer pipeServer;        
        private MainViewModel mainModel;

        public static Settings AppSettings
        {
            get;
            private set;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GameDatabase.Instance.CloseDatabase();
            GamesLoaderHandler.CancelToken.Cancel();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            Cef.Shutdown();
            AppSettings.SaveSettings();

            if (appMutex != null)
            {
                appMutex.ReleaseMutex();
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

            Settings.ConfigureLogger();
            Settings.ConfigureCef();

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif

            if (Mutex.TryOpenExisting(instanceMuxet, out var mutex))
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

                logger.Info("Application already running, shutting down.");
                Shutdown();
                return;
            }
            else
            {
                appMutex = new Mutex(true, instanceMuxet);
            }

            var mainWindow = new MainWindowFactory();
            mainModel = new MainViewModel(
                mainWindow,
                new DialogsFactory(),
                new ResourceProvider(),
                new NotificationFactory(),
                AppSettings);
            Current.MainWindow = mainWindow.Window;
            mainWindow.Show(mainModel);            
            mainModel.LoadGames(AppSettings.UpdateLibStartup);

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

            logger.Info("Application started");
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    mainModel.ShowWindow();
                    break;

                case CmdlineCommands.Launch:
                    var game = GameDatabase.Instance.GamesCollection.FindById(int.Parse(args.Args));
                    if (game == null)
                    {
                        logger.Error("Cannot start game, game {0} not found.", args.Args);
                    }
                    else
                    {
                        GamesEditor.Instance.PlayGame(game);
                    }

                    break;

                default:
                    logger.Warn("Unknown command received");
                    break;
            }
        }

        private void CheckUpdate()
        {
            Task.Factory.StartNew(() =>
            {
                var update = new Update();
                UpdateWindow updateWindow = null;

                while (true)
                {
                    try
                    {
                        if ((updateWindow == null || !updateWindow.IsVisible) && update.IsUpdateAvailable)
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
                                    updateWindow = new UpdateWindow()
                                    {
                                        Owner = MainWindow
                                    };

                                    updateWindow.SetUpdate(update);
                                    updateWindow.Show();
                                    updateWindow.Focus();
                                });
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Failed to process update.");
                    }

                    Thread.Sleep(4 * 60 * 60 * 1000);
                }
            });
        }

        private void SendUsageData()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var client = new ServicesClient();
                    client.PostUserUsage();
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to post user usage data.");
                }
            });
        }
    }
}
