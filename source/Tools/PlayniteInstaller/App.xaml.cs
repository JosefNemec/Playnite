using Playnite.Common;
using Playnite.SDK;
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
        public static string InstallerDownloadPath => Path.Combine(Path.GetTempPath(), "PlayniteInstaller", "installer.exe");

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
            FileSystem.CreateDirectory(App.TempDir);
            LogManager.Initialize(Path.Combine(App.TempDir, "installer.log"));
            logger = LogManager.GetLogger();
            logger.Debug($"Installer started {CurrentVersion}");

            var window = new MainWindow();
            window.DataContext = new MainViewModel(window);
            window.ShowDialog();
            Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error((Exception)e.ExceptionObject, "Unhandled exception occured.");
            MessageBox.Show(
               "Unrecoverable error occured and installer will now close.",
               "Critical error",
               MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            LogManager.Dispose();
        }
    }
}
