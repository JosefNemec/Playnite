using Newtonsoft.Json;
using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite
{
    public class AddonInstallerManifest : AddonInstallerManifestBase
    {
        public AddonType AddonType { get; set; }

        public AddonInstallerPackage GetLatestCompatiblePackage()
        {
            if (!Packages.HasItems())
            {
                return null;
            }

            var apiVersion = GetApiVersion(AddonType);
            return GetLatestCompatiblePackage(apiVersion);
        }

        public AddonInstallerPackage GetLatestCompatiblePackage(Version apiVersion)
        {
            if (!Packages.HasItems())
            {
                return null;
            }

            return Packages.
                Where(a => a.RequiredApiVersion != null && a.RequiredApiVersion.Major == apiVersion.Major && a.RequiredApiVersion <= apiVersion).
                OrderByDescending(a => a.Version).FirstOrDefault();
        }

        private static Version GetApiVersion(AddonType type)
        {
            switch (type)
            {
                case AddonType.GameLibrary:
                case AddonType.MetadataProvider:
                case AddonType.Generic:
                    return SdkVersions.SDKVersion;
                case AddonType.ThemeDesktop:
                    return ThemeManager.DesktopApiVersion;
                case AddonType.ThemeFullscreen:
                    return ThemeManager.FullscreenApiVersion;
            }

            return new Version(999, 0);
        }
    }

    public class AddonManifest : AddonManifestBase
    {
        private static ILogger logger = LogManager.GetLogger();

        private AddonInstallerManifest installerManifest;
        [YamlIgnore]
        [JsonIgnore]
        public AddonInstallerManifest InstallerManifest
        {
            get
            {
                if (installerManifest == null)
                {
                    DownloadInstallerManifest();
                }

                return installerManifest;
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public AddonInstallerPackage LatestPackage
        {
            get
            {
                var manifest = InstallerManifest;
                if (!manifest.Packages.HasItems())
                {
                    return null;
                }

                return manifest.Packages.OrderByDescending(a => a.Version).FirstOrDefault();
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public bool IsQueuedForInstall
        {
            get
            {
                var fileName = Path.GetFileName(GetTargetDownloadPath());
                return ExtensionInstaller.GetQueuedItems().Any(a => Path.GetFileName(a.Path) == fileName);
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public bool IsInstalled
        {
            get
            {
                if (IsTheme)
                {
                    if (Type == AddonType.ThemeDesktop)
                    {
                        return ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).Any(a => a.Id == AddonId);
                    }
                    else
                    {
                        return ThemeManager.GetAvailableThemes(ApplicationMode.Fullscreen).Any(a => a.Id == AddonId);
                    }
                }
                else
                {
                    return ExtensionFactory.GetInstalledManifests().Any(a => a.Id == AddonId);
                }
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public bool IsTheme => Type == AddonType.ThemeDesktop || Type == AddonType.ThemeFullscreen;

        [YamlIgnore]
        [JsonIgnore]
        public bool IsExtension => Type == AddonType.Generic || Type == AddonType.GameLibrary || Type == AddonType.MetadataProvider;

        public string GetTargetDownloadPath()
        {
            return Path.Combine(PlaynitePaths.TempPath, Paths.GetSafePathName(AddonId) + GetAddonPackageExtension(Type));
        }

        public static string GetAddonPackageExtension(AddonType type)
        {
            switch (type)
            {
                case AddonType.GameLibrary:
                case AddonType.MetadataProvider:
                case AddonType.Generic:
                    return PlaynitePaths.PackedExtensionFileExtention;
                case AddonType.ThemeDesktop:
                case AddonType.ThemeFullscreen:
                    return PlaynitePaths.PackedThemeFileExtention;
                default:
                    throw new Exception($"Uknown addon type {type}");
            }
        }

        public bool? CheckAddonLicense()
        {
            try
            {
                if (UserAgreement != null)
                {
                    var acceptState = ExtensionInstaller.GetAddonLicenseAgreed(AddonId);
                    if (acceptState == null || acceptState < UserAgreement.Updated)
                    {
                        var license = HttpDownloader.DownloadString(UserAgreement.AgreementUrl);
                        var licenseAgree = new LicenseAgreementViewModel(
                            new LicenseAgreementWindowFactory(),
                            license,
                            Name);

                        if (licenseAgree.OpenView() == true)
                        {
                            ExtensionInstaller.AgreeAddonLicense(AddonId);
                            return true;
                        }
                        else
                        {
                            ExtensionInstaller.RemoveAddonLicenseAgreement(AddonId);
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to process addon license.");
                return null;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void DownloadInstallerManifest()
        {
            DownloadInstallerManifest(new CancellationToken());
        }

        public void DownloadInstallerManifest(CancellationToken cancelToken)
        {
            try
            {
                if (InstallerManifestUrl.IsNullOrEmpty())
                {
                    throw new Exception("No addon manifest installer url.");
                }

                if (InstallerManifestUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    installerManifest = Serialization.FromYaml<AddonInstallerManifest>(HttpDownloader.DownloadString(InstallerManifestUrl, cancelToken));
                }
                else if (File.Exists(InstallerManifestUrl))
                {
                    installerManifest = Serialization.FromYamlFile<AddonInstallerManifest>(InstallerManifestUrl);
                }
                else
                {
                    throw new Exception($"Uknown installer manifest url format {InstallerManifestUrl}.");
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to get addon installer manifest data.");
                installerManifest = new AddonInstallerManifest();
            }

            installerManifest.AddonType = Type;
        }
    }
}
