﻿using System;
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
using Playnite.App;
using Flurl;
using Playnite.Web;
using System.Net;

namespace Playnite.App
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

        private static string updateDataUrl
        {
            get
            {
                return string.Format(ConfigurationManager.AppSettings["UpdateUrl"] ?? "", updateBranch);
            }
        }

        private static string updateDataUrl2
        {
            get
            {
                return string.Format(ConfigurationManager.AppSettings["UpdateUrl2"] ?? "", updateBranch);
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
                return Path.Combine(Paths.TempPath, "update.exe");
            }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(GetCurrentVersion()) > 0;
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

        public List<ReleaseNoteData> DownloadReleaseNotes(Version currentVersion)
        {
            var notes = new List<ReleaseNoteData>();
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            foreach (var version in updateManifest.ReleaseNotes)
            {
                if (version.Version.CompareTo(currentVersion) > 0)
                {
                    var noteUrls = updateManifest.ReleaseNotesUrlRoots.Select(a => Url.Combine(a,version.FileName));
                    var note = downloader.DownloadString(noteUrls);
                    notes.Add(new ReleaseNoteData()
                    {
                        Version = version.Version,
                        Note = note
                    });
                }
            }

            return notes;
        }

        public UpdateManifest.Package GetUpdatePackage(Version currentVersion)
        {
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            return GetUpdatePackage(updateManifest, currentVersion);
        }

        public UpdateManifest.Package GetUpdatePackage(UpdateManifest manifest, Version currentVersion)
        {
            var diff = manifest.Packages.FirstOrDefault(a => a.BaseVersion.ToString(2) == currentVersion.ToString(2));
            if (diff != null)
            {
                return diff;
            }

            return manifest.Packages.First(a => a.BaseVersion == manifest.LatestVersion);
        }

        public async Task DownloadUpdate(UpdateManifest.Package package, Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            if (updateManifest == null)
            {
                DownloadManifest();
            }

            if (File.Exists(updaterPath))
            {
                var md5 = FileSystem.GetMD5(updaterPath);
                if (md5 == package.Checksum)
                {
                    logger.Info("Update already downloaded skipping download.");
                    return;
                }
            }

            try
            {
                var downloadUrls = updateManifest.DownloadServers.Select(a => Url.Combine(a, updateManifest.LatestVersion.ToString(), package.FileName));
                await downloader.DownloadFileAsync(downloadUrls, updaterPath, progressHandler);
                var md5 = FileSystem.GetMD5(updaterPath);
                if (md5 != package.Checksum)
                {
                    throw new Exception($"Checksum of downloaded file doesn't match: {md5} vs {package.Checksum}");
                }
            }
            catch (Exception e)
            {
                logger.Warn(e, "Failed to download update file.");
                throw new Exception("Failed to download update file.");
            }
        }

        public void InstallUpdate()
        {            
            var portable = Settings.IsPortable ? "/PORTABLE" : "";
            logger.Info("Installing new update to {0}, in {1} mode", Paths.ProgramFolder, portable);

            Task.Run(() =>
            {
                Process.Start(updaterPath, string.Format(@"/SILENT /NOCANCEL /DIR=""{0}"" /UPDATE {1}", Paths.ProgramFolder, portable));
            });

            playniteApp.Quit();
        }

        public UpdateManifest DownloadManifest()
        {
            var dataString = string.Empty;

            try
            {
                dataString = GetUpdateManifestData(updateDataUrl);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Failed to download update manifest from main URL");
            }

            try
            {
                if (string.IsNullOrEmpty(dataString) && !string.IsNullOrEmpty(updateDataUrl2))
                {
                    dataString = GetUpdateManifestData(updateDataUrl2);
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

            return updateManifest.LatestVersion;
        }

        public static Version GetCurrentVersion()
        {
            return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }

        private string GetUpdateManifestData(string url)
        {
            return downloader.DownloadString(url);
        }
    }
}
