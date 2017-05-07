using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GameDatabase.Instance.CloseDatabase();
            GamesLoaderHandler.CancelToken.Cancel();
            Playnite.Providers.Steam.SteamApiClient.Instance.Logout();
            Cef.Shutdown();
            Settings.Instance.SaveSettings();
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
            Settings.ConfigureLogger();
            Settings.ConfigureCef();
            Settings.LoadSettings();

#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            logger.Info("Application started");
        }
    }
}
