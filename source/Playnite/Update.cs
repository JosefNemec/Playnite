using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.Windows;

namespace Playnite
{
    public class UpdateData
    {
        public string version
        {
            get; set;
        }

        public string url
        {
            get; set;
        }
    }

    // TODO: cleanup and convert from static
    public class Update
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static string updateDataUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["UpdateUrl"];
            }
        }

        private static UpdateData latestData;
        private static string updaterPath = Path.Combine(Paths.TempPath, "playnite.exe");
        private static string downloadCompletePath = Path.Combine(Paths.TempPath, "download.done");

        public static bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(GetCurrentVersion()) > 0;
            }
        }

        public static void DownloadUpdate()
        {
            if (latestData == null)
            {
                GetLatestVersion();
            }

            DownloadUpdate(latestData.url);
        }

        public static void DownloadUpdate(string url)
        {
            logger.Info("Downloading new update from " + url);
            FileSystem.CreateFolder(Paths.TempPath);

            if (File.Exists(downloadCompletePath))
            {
                var info = FileVersionInfo.GetVersionInfo(updaterPath);
                if (info.FileVersion == GetLatestVersion().ToString())
                {
                    logger.Info("Update already ready to install");
                    return;
                }
                else
                {
                    File.Delete(downloadCompletePath);
                }
            }

            Web.DownloadFile(url, updaterPath);
            File.Create(downloadCompletePath);
        }

        public static void InstallUpdate()
        {            
            var portable = Settings.IsPortable ? "/Portable 1" : "/Portable 0";
            logger.Info("Installing new update to {0}, in {1} mode", Paths.ProgramFolder, portable);

            Task.Factory.StartNew(() =>
            {
                Process.Start(updaterPath, string.Format(@"/ProgressOnly 1 {0} /D={1}", portable, Paths.ProgramFolder));
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Close();
            });
        }

        public static Version GetCurrentVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }        
        
        public static Version GetLatestVersion()
        {
            var dataString = Web.DownloadString(updateDataUrl);
            latestData = JsonConvert.DeserializeObject<Dictionary<string, UpdateData>>(dataString)["stable"];
            return new Version(latestData.version);
        }
    }
}
