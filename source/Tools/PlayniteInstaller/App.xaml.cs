using Playnite.SDK;
using PlayniteInstaller.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ILogger logger;

        public static string TempDir => Path.Combine(Path.GetTempPath(), "PlayniteInstaller");

        private static Version currentVersion;
        public static Version CurrentVersion
        {
            get
            {
                if (currentVersion == null)
                {
                    currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                }

                return currentVersion;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            NLogLogger.ConfigureLogger();
            LogManager.Init(new NLogLogProvider());
            logger = LogManager.GetLogger();
            logger.Debug($"Installer started {CurrentVersion}");
            var window = new MainWindow();
            window.DataContext = new MainViewModel(window);
            window.ShowDialog();
            Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }
    }
}
