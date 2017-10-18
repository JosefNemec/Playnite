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
        
        public static ThirdPartyToolsList ThirdPartyTools
        {
            get; set;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GameDatabase.Instance.CloseDatabase();
            GamesLoaderHandler.CancelToken.Cancel();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            Cef.Shutdown();
            Settings.Instance.SaveSettings();

            if (appMutex != null)
            {
                appMutex.ReleaseMutex();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            logger.Error(exception, "Unhandled exception occured.");

            var window = new CrashHandlerWindow();
            window.TextDetails.Text = exception.ToString();
            window.ShowDialog();
            Process.GetCurrentProcess().Kill();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var config = Settings.LoadSettings();
            Localization.SetLanguage(config.Language);
            Resources.Remove("AsyncImagesEnabled");
            Resources.Add("AsyncImagesEnabled", config.AsyncImageLoading);
            if (config.DisableHwAcceleration)
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
                    var args = commandArgs.Count() > 1 ? commandArgs[1] : string.Empty;                    
                    client.InvokeCommand(command, args);
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


            new ThemeTesterWindow().ShowDialog();
            Shutdown();
            return;

            LoadThirdPartyTools();
            var mainWindow = new MainWindow();
            Current.MainWindow = MainWindow;
            mainWindow.Show();

            logger.Info("Application started");
        }

        private void LoadThirdPartyTools()
        {
            try
            {
                ThirdPartyTools = new ThirdPartyToolsList();
                ThirdPartyTools.SetTools(ThirdPartyTools.GetDefaultInstalledTools());
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to load 3rd party tool list.");
            }
        }
    }
}
