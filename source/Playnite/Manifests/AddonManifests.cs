using Newtonsoft.Json;
using Playnite.Common;
using Playnite.Common.Web;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite
{
    //public class ExtensionInstallerPackage : AddonInstallerPackage
    //{
    //}

    //public class ThemeInstallerPackage : AddonInstallerPackage
    //{
    //}

    public class AddonInstallerPackage
    {
        public Version Version { get; set; }
        public string PackageUrl { get; set; }
        public Version RequiredApiVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
    }

    public class AddonInstallerManifest
    {
        public string AddonId { get; set; }
        public List<AddonInstallerPackage> Packages { get; set; }
        public Dictionary<Version, List<string>> Changelog { get; set; }

        public AddonInstallerPackage GetLatestCompatiblePackage(Version apiVersion)
        {
            if (!Packages.HasItems())
            {
                return null;
            }

            return Packages.
                Where(a => a.RequiredApiVersion.Major == apiVersion.Major && a.RequiredApiVersion <= apiVersion).
                OrderByDescending(a => a.Version).FirstOrDefault();
        }
    }

    public enum AddonType
    {
        Library,
        Metadata,
        Generic,
        ThemeDesktop,
        ThemeFullscreen
    }

    public class AddonManifest : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();

        public string IconUrl { get; set; }
        public string ScreenshotUrl { get; set; }
        public AddonType Type { get; set; }
        public string InstallerManifestUrl { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string AddonId { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }

        private AddonInstallerManifest installerManifest;
        [YamlIgnore]
        [JsonIgnore]
        public AddonInstallerManifest InstallerManifest
        {
            get
            {
                if (installerManifest == null)
                {
                    try
                    {
                        if (InstallerManifestUrl.IsNullOrEmpty())
                        {
                            throw new Exception("No addon manifest installer url.");
                        }

                        if (InstallerManifestUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            installerManifest = Serialization.FromYaml<AddonInstallerManifest>(HttpDownloader.DownloadString(InstallerManifestUrl));
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

                return manifest.Packages.OrderBy(a => a.Version).FirstOrDefault();
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
                    return ExtensionFactory.GetExtensionDescriptors().Any(a => a.Id == AddonId);
                }
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public bool IsTheme => Type == AddonType.ThemeDesktop || Type == AddonType.ThemeFullscreen;

        [YamlIgnore]
        [JsonIgnore]
        public bool IsExtension => Type == AddonType.Generic || Type == AddonType.Library || Type == AddonType.Metadata;

        public string GetTargetDownloadPath()
        {
            return Path.Combine(PlaynitePaths.TempPath, Paths.GetSafePathName(AddonId) + GetAddonPackageExtension(Type));
        }

        public static string GetAddonPackageExtension(AddonType type)
        {
            switch (type)
            {
                case AddonType.Library:
                case AddonType.Metadata:
                case AddonType.Generic:
                    return PlaynitePaths.PackedExtensionFileExtention;
                case AddonType.ThemeDesktop:
                case AddonType.ThemeFullscreen:
                    return PlaynitePaths.PackedThemeFileExtention;
                default:
                    throw new Exception($"Uknown addon type {type}");
            }
        }
    }
}
