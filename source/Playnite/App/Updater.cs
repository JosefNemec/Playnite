using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using Flurl;
using System.Net;
using System.Threading;
using NSec.Cryptography;
using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Settings;
using Playnite.SDK;

namespace Playnite
{
    public class Updater
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly byte[] signingPubKey = [0x98, 0x68, 0xE8, 0x94, 0xBF, 0x90, 0xE0, 0x38, 0x5B, 0x40, 0xE6, 0x62, 0x6C, 0x4A, 0x8E, 0xBB, 0x90, 0x09, 0x62, 0x67, 0x52, 0x08, 0xFD, 0x88, 0x88, 0x6C, 0x64, 0x60, 0x4D, 0x7C, 0xB2, 0x64];

        private readonly Lazy<UpdateManifest> updateManifest;
        private readonly IPlayniteApplication playniteApp;
        private readonly IDownloader downloader = new Downloader();
        private readonly string updateBranch = ConfigurationManager.AppSettings["UpdateBranch"];
        private readonly string updateUrl = ConfigurationManager.AppSettings["UpdateUrl"];
        private readonly string updaterPath = Path.Combine(PlaynitePaths.TempPath, "update.exe");

        public bool IsUpdateAvailable
        {
            get
            {
                var latest = GetLatestVersion();
                var current = CurrentVersion;
                return latest > current;
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

        public Updater(IPlayniteApplication app)
        {
            playniteApp = app;
            updateManifest = new Lazy<UpdateManifest>(DownloadManifest, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public List<ReleaseNoteData> GetReleaseNotes()
        {
            var notes = new List<ReleaseNoteData>();
            foreach (var version in updateManifest.Value.VersionHistory)
            {
                if (version.CompareTo(CurrentVersion) > 0)
                {
                    var notUrl = Url.Combine(updateUrl, updateBranch, $"{version.Major}.{version.Minor}.html");
                    var note = downloader.DownloadString(notUrl);
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
            var newMD5 = FileSystem.GetSHA256(path);
            if (!newMD5.Equals(checksum, StringComparison.OrdinalIgnoreCase))
            {
                logger.Error($"Checksum of downloaded file doesn't match: {newMD5} vs {checksum}");
                return false;
            }

            return true;
        }

        public async Task DownloadUpdate(Action<DownloadProgressChangedEventArgs> progressHandler)
        {
            var manifest = updateManifest.Value;
            if (File.Exists(updaterPath) && VerifyUpdateFile(manifest.Sha256, updaterPath))
            {
                logger.Info("Update already downloaded skipping download.");
                return;
            }

            await downloader.DownloadFileAsync(manifest.PackageUrls, updaterPath, progressHandler);
            if (!VerifyUpdateFile(manifest.Sha256, updaterPath))
            {
                FileSystem.DeleteFile(updaterPath);
                throw new Exception($"Update file integrity check failed.");
            }
        }

        public void InstallUpdate(ApplicationMode mode)
        {
            var portable = PlayniteSettings.IsPortable ? "/PORTABLE" : "";
            var fullscreen = mode == ApplicationMode.Fullscreen ? "/FULLSCREEN" : "";
            logger.Info("Installing new update to {0}, in {1} mode".Format(PlaynitePaths.ProgramPath, portable));
            playniteApp.QuitAndStart(
                updaterPath,
                @"/SILENT /NOCANCEL /DIR=""{0}"" /UPDATE {1} {2}".Format(PlaynitePaths.ProgramPath, portable, fullscreen),
                !FileSystem.CanWriteToFolder(PlaynitePaths.ProgramPath));
        }

        public UpdateManifest DownloadManifest()
        {
            UpdateManifest manifest = null;
            var manUrl = Url.Combine(
                updateUrl,
                updateBranch,
                $"{CurrentVersion.Major}.{CurrentVersion.Minor}",
                UpdateManifest.ServerManifestFileName);
            logger.Info($"Downloading update manifest from {manUrl}");

            try
            {
                var manData = downloader.DownloadData(manUrl);
                var sigData = downloader.DownloadData(manUrl + ".sig");

                var algo = SignatureAlgorithm.Ed25519;
                var pubKey = PublicKey.Import(algo, signingPubKey, KeyBlobFormat.RawPublicKey);
                if (!algo.Verify(pubKey, manData, sigData))
                    throw new Exception("Update manifest integrity check failed.");

                using var stream = new MemoryStream(manData);
                manifest = Serialization.FromJsonStream<UpdateManifest>(stream);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to download or verify update manifest.");
                throw new Exception("Failed to download or verify update manifest.");
            }

            if (manifest is null)
                throw new Exception("Manifest data deserialization failed.");

            return manifest;
        }

        public Version GetLatestVersion()
        {
            return updateManifest.Value.Version;
        }
    }
}
