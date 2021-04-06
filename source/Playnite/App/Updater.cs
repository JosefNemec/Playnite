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
using Flurl;
using System.Net;
using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Settings;

namespace Playnite
{
    public class Updater
    {
        private static string updateBranch
        {
            get
            {
                return ConfigurationManager.AppSettings["UpdateBranch"];
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private UpdateManifest updateManifest;
        private IPlayniteApplication playniteApp;
        private IDownloader downloader;

        private string updaterPath
        {
            get
            {
                return Path.Combine(PlaynitePaths.TempPath, "update.exe");
            }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(CurrentVersion) > 0;
            }
        }

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

        public Updater(IPlayniteApplication app) : this(app, new Downloader())
        {
        }

        public Updater(IPlayniteApplication app, IDownloader webDownloader)
        {
            playniteApp = app;
            downloader = webDownloader;
        }

        private string GetUpdateDataRootUrl(string configKey)
        {
            return Url.Combine(ConfigurationManager.AppSettings[configKey], updateBranch, $"{CurrentVersion.Major}.{CurrentVersion.Minor}");
        }

        public List<ReleaseNoteData> GetReleaseNotes()
        {
            var notes = new List<ReleaseNoteData>();
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            foreach (var version in updateManifest.VersionHistory)
            {
                if (version.CompareTo(CurrentVersion) > 0)
                {
                    var noteUrls = new List<string>
                    {
                        Url.Combine(ConfigurationManager.AppSettings["UpdateUrl"], updateBranch, $"{version.Major}.{version.Minor}.html"),
                        Url.Combine(ConfigurationManager.AppSettings["UpdateUrl2"], updateBranch, $"{version.Major}.{version.Minor}.html")
                    };

                    var note = downloader.DownloadString(noteUrls);
                    notes.Add(new ReleaseNoteData()
                    {
                        Version = version,
                        Note = note
                    });
                }
            }

            return notes;
        }

        private bool VerifyUpdateFile(string checksum, string path)
        {
            var newMD5 = FileSystem.GetMD5(path);
            if (newMD5 != checksum)
            {
                logger.Error($"Checksum of downloaded file doesn't match: {newMD5} vs {checksum}");
                return false;
            }

            return true;
        }

        public async Task DownloadUpdate(Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            if (File.Exists(updaterPath))
            {
                if (VerifyUpdateFile(updateManifest.Checksum, updaterPath))
                {
                    logger.Info("Update already downloaded skipping download.");
                    return;
                }
            }

            try
            {
                await downloader.DownloadFileAsync(updateManifest.PackageUrls, updaterPath, progressHandler);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to download update file.");
                throw new Exception("Failed to download update file.");
            }

            if (!VerifyUpdateFile(updateManifest.Checksum, updaterPath))
            {
                throw new Exception($"Update file integrity check failed.");
            }
        }

        public void InstallUpdate()
        {
            var portable = PlayniteSettings.IsPortable ? "/PORTABLE" : "";
            logger.Info("Installing new update to {0}, in {1} mode", PlaynitePaths.ProgramPath, portable);
            playniteApp.QuitAndStart(
                updaterPath,
                string.Format(@"/SILENT /NOCANCEL /DIR=""{0}"" /UPDATE {1}", PlaynitePaths.ProgramPath, portable),
                !FileSystem.CanWriteToFolder(PlaynitePaths.ProgramPath));
        }

        public UpdateManifest DownloadManifest()
        {
            var dataString = string.Empty;

            try
            {
                dataString = GetUpdateManifestData(GetUpdateDataRootUrl("UpdateUrl"));
            }
            catch (Exception e)
            {
                logger.Warn(e, "Failed to download update manifest from main URL");
            }

            try
            {
                if (string.IsNullOrEmpty(dataString))
                {
                    dataString = GetUpdateManifestData(GetUpdateDataRootUrl("UpdateUrl2"));
                }
            }
            catch (Exception e)
            {
                logger.Warn(e, "Failed to download update manifest from secondary URL");
            }

            if (string.IsNullOrEmpty(dataString))
            {
                throw new Exception("Failed to download update manifest.");
            }

            updateManifest = JsonConvert.DeserializeObject<UpdateManifest>(dataString);
            return updateManifest;
        }

        public Version GetLatestVersion()
        {
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            return updateManifest.Version;
        }

        private string GetUpdateManifestData(string url)
        {
            return downloader.DownloadString(Url.Combine(url, "update.json"));
        }
    }
}
