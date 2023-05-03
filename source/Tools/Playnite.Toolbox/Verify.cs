using Playnite.Common;
using Playnite.Common.Web;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public class Verify
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool VerifyInstallerManifest(string manifestFile, out string addonId)
        {
            logger.Info($"Verifying installer manifest {manifestFile} ...");
            addonId = string.Empty;
            var passed = true;
            if (manifestFile.IsHttpUrl())
            {
                var newManifestFile = Path.Combine(PlaynitePaths.TempPath, "httpmanifest.yaml");
                HttpDownloader.DownloadFile(manifestFile, newManifestFile);
                manifestFile = newManifestFile;
            }

            if (!File.Exists(manifestFile))
            {
                logger.Error("Manifest file not found!");
                passed = false;
                return false;
            }

            AddonInstallerManifestBase manifest = null;
            try
            {
                manifest = Serialization.FromYamlFile<AddonInstallerManifestBase>(manifestFile);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to parse installer manifest YAML:");
                logger.Error(e.Message);
                passed = false;
                return false;
            }

            addonId = manifest.AddonId;
            CheckPropertyString(manifest.AddonId, nameof(manifest.AddonId), true, ref passed);
            if (manifest.Packages.HasItems())
            {
                foreach (var package in manifest.Packages)
                {
                    CheckPropertyURL(package.PackageUrl, $"Package > {nameof(package.PackageUrl)}", true, false, ref passed);
                    CheckPropertyDateTime(package.ReleaseDate, $"Package > {nameof(package.ReleaseDate)}", true, ref passed);
                    CheckPropertyVersion(package.Version, $"Package > {nameof(package.Version)}", true, ref passed);
                    CheckPropertyVersion(package.RequiredApiVersion, $"Package > {nameof(package.RequiredApiVersion)}", true, ref passed);
                    if (!package.Changelog.HasNonEmptyItems())
                    {
                        logger.Error($"Installer package is missing changelog.");
                        passed = false;
                    }
                }
            }
            else
            {
                logger.Error("Installer manifest doesn't have any packages specified.");
                passed = false;
            }

            if (passed)
            {
                logger.Info("Installer manifest passed verification.");
            }
            else
            {
                logger.Error("Installer manifest didn't pass verification.");
            }

            return passed;
        }

        public static bool VerifyAddonManifest(string manifestFile)
        {
            logger.Info($"Verifying addon manifest {manifestFile} ...");
            var passed = true;
            if (manifestFile.IsHttpUrl())
            {
                var newManifestFile = Path.Combine(PlaynitePaths.TempPath, "httpmanifest.yaml");
                HttpDownloader.DownloadFile(manifestFile, newManifestFile);
                manifestFile = newManifestFile;
            }

            if (!File.Exists(manifestFile))
            {
                logger.Error("Manifest file not found!");
                passed = false;
                return false;
            }

            AddonManifestBase manifest = null;
            try
            {
                manifest = Serialization.FromYamlFile<AddonManifestBase>(manifestFile);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to parse addon manifest YAML:");
                logger.Error(e.Message);
                passed = false;
                return false;
            }

            CheckPropertyString(manifest.AddonId, nameof(manifest.AddonId), true, ref passed);
            CheckPropertyString(manifest.Name, nameof(manifest.Name), true, ref passed);
            CheckPropertyString(manifest.Author, nameof(manifest.Author), true, ref passed);
            CheckPropertyString(manifest.ShortDescription, nameof(manifest.ShortDescription), true, ref passed);
            CheckPropertyURL(manifest.IconUrl, nameof(manifest.IconUrl), false, false, ref passed);
            CheckPropertyURL(manifest.SourceUrl, nameof(manifest.SourceUrl), true, true, ref passed);
            foreach (var screenshot in manifest.Screenshots ?? new List<AddonManifestBase.AddonScreenshot>())
            {
                CheckPropertyURL(screenshot.Thumbnail, $"Screenshots > {nameof(screenshot.Thumbnail)}", true, false, ref passed);
                CheckPropertyURL(screenshot.Image, $"Screenshots > {nameof(screenshot.Image)}", true, false, ref passed);
            }

            if (manifest.UserAgreement != null)
            {
                CheckPropertyURL(manifest.UserAgreement.AgreementUrl, $"UserAgreement > {nameof(manifest.UserAgreement.AgreementUrl)}", true, false, ref passed);
                CheckPropertyDateTime(manifest.UserAgreement.Updated, $"UserAgreement > {nameof(manifest.UserAgreement.Updated)}", true, ref passed);
            }

            if (CheckPropertyURL(manifest.InstallerManifestUrl, nameof(manifest.InstallerManifestUrl), true, false, ref passed))
            {
                if (!VerifyInstallerManifest(manifest.InstallerManifestUrl, out var installerAddonId))
                {
                    passed = false;
                }

                if (manifest.AddonId != installerAddonId)
                {
                    logger.Error($"Addon manifest ID and installer maniefst ID do not match:\n{manifest.AddonId} vs {installerAddonId}");
                    passed = false;
                }
            }

            if (passed)
            {
                logger.Info("Addon manifest passed verifiction.");
            }
            else
            {
                logger.Error("Addon manifest didn't pass verification.");
            }

            return passed;
        }

        private static bool CheckPropertyURL(string url, string propertyName, bool mandatory, bool allowHtmlContent, ref bool passRes)
        {
            if (url.IsNullOrWhiteSpace() && mandatory)
            {
                logger.Error($"{propertyName} URL is missing.");
                passRes = false;
                return false;
            }

            if (url.IsNullOrWhiteSpace() && !mandatory)
            {
                return true;
            }

            if (!url.IsHttpUrl())
            {
                logger.Error($"{propertyName} is not HTTP URL.\n{url}");
                passRes = false;
                return false;
            }

            if (!HttpDownloader.GetResponseCode(url, out var _).IsSuccess())
            {
                logger.Error($"{propertyName} doesn't point to reachable HTTP location.\n{url}");
                passRes = false;
                return false;
            }

            if (!allowHtmlContent)
            {
                var testFile = Path.Combine(PlaynitePaths.TempPath, "webfiletest.file");
                HttpDownloader.DownloadFile(url, testFile);
                if (File.ReadAllText(testFile).Contains("<html"))
                {
                    logger.Error($"{propertyName} doesn't point to corret file. It needs to point to the actual file content, not its repository page.\n{url}");
                    passRes = false;
                    return false;
                }
            }

            return true;
        }

        private static bool CheckPropertyString(string value, string propertyName, bool mandatory, ref bool passRes)
        {
            if (value.IsNullOrWhiteSpace() && mandatory)
            {
                logger.Error($"{propertyName} property is missing.");
                passRes = false;
                return false;
            }

            return true;
        }

        private static bool CheckPropertyDateTime(DateTime value, string propertyName, bool mandatory, ref bool passRes)
        {
            if (value == default(DateTime) && mandatory)
            {
                logger.Error($"{propertyName} property is missing.");
                passRes = false;
                return false;
            }

            return true;
        }

        private static bool CheckPropertyVersion(Version value, string propertyName, bool mandatory, ref bool passRes)
        {
            if (value == null && mandatory)
            {
                logger.Error($"{propertyName} property is missing.");
                passRes = false;
                return false;
            }

            return true;
        }
    }
}
