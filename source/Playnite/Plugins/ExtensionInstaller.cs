using Playnite.API;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Plugins
{
    public enum ExtInstallType
    {
        Install,
        Uninstall
    }

    public class ExtensionInstallQueueItem
    {
        public ExtInstallType InstallType { get; set; }

        public string Path { get; set; }

        public ExtensionInstallQueueItem()
        {
        }

        public ExtensionInstallQueueItem(string path, ExtInstallType type)
        {
            Path = path;
            InstallType = type;
        }

        public override string ToString()
        {
            return Path;
        }
    }

    public class ExtensionInstallResult
    {
        public bool Updated { get; set; }
        public Exception InstallError { get; set; }
        public BaseExtensionManifest InstalledManifest { get; set; }
        public string PackagePath { get; set; }

        public ExtensionInstallResult(bool updated, BaseExtensionManifest installedManifest, string packagePath)
        {
            Updated = updated;
            InstalledManifest = installedManifest;
            PackagePath = packagePath;
        }

        public ExtensionInstallResult(Exception installError,  string packagePath)
        {
            InstallError = installError;
            PackagePath = packagePath;
        }
    }

    public class ExtensionInstaller
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static List<ExtensionInstallQueueItem> currentQueue = new List<ExtensionInstallQueueItem>();
        private static Dictionary<string, DateTime> agreedLicenses;

        static ExtensionInstaller()
        {
            if (File.Exists(PlaynitePaths.AddonLicenseAgreementsFilePath))
            {
                agreedLicenses = Serialization.FromJsonFile<Dictionary<string, DateTime>>(PlaynitePaths.AddonLicenseAgreementsFilePath);
            }
            else
            {
                agreedLicenses = new Dictionary<string, DateTime>();
            }
        }

        public static void AgreeAddonLicense(string addonId)
        {
            agreedLicenses[addonId] = DateTime.Today;
            File.WriteAllText(PlaynitePaths.AddonLicenseAgreementsFilePath, Serialization.ToJson(agreedLicenses, true));
        }

        public static void RemoveAddonLicenseAgreement(string addonId)
        {
            if (agreedLicenses.ContainsKey(addonId))
            {
                agreedLicenses.Remove(addonId);
                File.WriteAllText(PlaynitePaths.AddonLicenseAgreementsFilePath, Serialization.ToJson(agreedLicenses, true));
            }
        }

        public static DateTime? GetAddonLicenseAgreed(string addonId)
        {
            if (agreedLicenses.ContainsKey(addonId))
            {
                return agreedLicenses[addonId];
            }
            else
            {
                return null;
            }
        }

        public static List<ExtensionInstallQueueItem> GetQueuedItems()
        {
            if (!File.Exists(PlaynitePaths.ExtensionQueueFilePath))
            {
                return new List<ExtensionInstallQueueItem>();
            }

            return Serialization.FromJsonFile<List<ExtensionInstallQueueItem>>(PlaynitePaths.ExtensionQueueFilePath);
        }

        public static List<ExtensionInstallResult> InstallExtensionQueue()
        {
            var installedExts = new List<ExtensionInstallResult>();

            foreach (var queueItem in GetQueuedItems())
            {
                if (queueItem.InstallType == ExtInstallType.Install)
                {
                    if (queueItem.Path.EndsWith(PlaynitePaths.PackedThemeFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            installedExts.Add(InstallPackedTheme(queueItem.Path));
                            logger.Info($"Installed theme {queueItem}");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            installedExts.Add(new ExtensionInstallResult(e, queueItem.Path));
                            logger.Error(e, $"Failed to install theme {queueItem}");
                        }
                    }
                    else if (queueItem.Path.EndsWith(PlaynitePaths.PackedExtensionFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            installedExts.Add(InstallPackedExtension(queueItem.Path));
                            logger.Info($"Installed extension {queueItem}");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            installedExts.Add(new ExtensionInstallResult(e, queueItem.Path));
                            logger.Error(e, $"Failed to install extension {queueItem}");
                        }
                    }
                    else
                    {
                        logger.Warn($"Uknown extension file format {queueItem}");
                    }
                }
                else if (queueItem.InstallType == ExtInstallType.Uninstall)
                {
                    if (Directory.Exists(queueItem.Path))
                    {
                        try
                        {
                            Directory.Delete(queueItem.Path, true);
                            logger.Info($"Uninstalled theme or extension {queueItem}");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, $"Failed to uninstall extension {queueItem}");
                        }
                    }
                    else
                    {
                        logger.Warn($"Can't uninstall extension, directory doesn't exists anymore {queueItem}");
                    }
                }
            }

            File.Delete(PlaynitePaths.ExtensionQueueFilePath);
            currentQueue = new List<ExtensionInstallQueueItem>();
            return installedExts;
        }

        public static ExtensionInstallResult InstallPackedFile<T>(string packagePath, string nanifestFileName, string rootDir, Func<string, T> newMan) where T : BaseExtensionManifest
        {
            logger.Info($"Installing extenstion/theme {packagePath}");
            var manifest = GetPackedManifest<T>(packagePath, nanifestFileName);
            if (manifest == null)
            {
                throw new FileNotFoundException("Extenstion/theme manifest not found.");
            }

            var entries = Archive.GetArchiveFiles(packagePath);
            if (entries.Any(a => a.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)))
            {
                // Check for themes that are not packaged via Toolbox.
                throw new Exception("Package content invalid.");
            }

            if (manifest is ThemeManifest themeMan)
            {
                rootDir = Path.Combine(rootDir, themeMan.Mode.ToString());
            }

            var wasUpdated = false;
            var installDir = Path.Combine(rootDir, Paths.GetSafePathName(manifest.Id));
            if (Directory.Exists(installDir))
            {
                wasUpdated = true;
                logger.Debug($"Replacing existing extenstion/theme installation: {installDir}.");
            }

            FileSystem.CreateDirectory(installDir, true);
            ZipFile.ExtractToDirectory(packagePath, installDir);
            if (Paths.AreEqual(PlaynitePaths.TempPath, Path.GetDirectoryName(packagePath)))
            {
                File.Delete(packagePath);
            }

            return new ExtensionInstallResult(wasUpdated, newMan(Path.Combine(installDir, nanifestFileName)), packagePath);
        }

        public static ExtensionInstallResult InstallPackedExtension(string path)
        {
            return InstallPackedFile<ExtensionManifest>(
                path,
                PlaynitePaths.ExtensionManifestFileName,
                PlayniteSettings.IsPortable ? PlaynitePaths.ExtensionsProgramPath : PlaynitePaths.ExtensionsUserDataPath,
                (a) => ExtensionManifest.FromFile(a));
        }

        public static ExtensionInstallResult InstallPackedTheme(string path)
        {
            return InstallPackedFile<ThemeManifest>(
               path,
               PlaynitePaths.ThemeManifestFileName,
               PlayniteSettings.IsPortable ? PlaynitePaths.ThemesProgramPath : PlaynitePaths.ThemesUserDataPath,
               (a) => new ThemeManifest(a));
        }

        private static T GetPackedManifest<T>(string packagePath, string nanifestFileName) where T : class
        {
            using (var zip = ZipFile.OpenRead(packagePath))
            {
                var manifest = zip.GetEntry(nanifestFileName);
                if (manifest == null)
                {
                    return null;
                }

                using (var logStream = manifest.Open())
                {
                    using (TextReader tr = new StreamReader(logStream))
                    {
                        return Serialization.FromYaml<T>(tr.ReadToEnd());
                    }
                }
            }
        }

        private static BaseExtensionManifest GetManifestFromDir(string extDir)
        {
            if (!Directory.Exists(extDir))
            {
                return null;
            }

            var extMan = Path.Combine(extDir, PlaynitePaths.ExtensionManifestFileName);
            if (File.Exists(extMan))
            {
                return GetExtensionManifest(extMan);
            }

            var themeMan = Path.Combine(extDir, PlaynitePaths.ThemeManifestFileName);
            if (File.Exists(themeMan))
            {
                return GetThemeManifest(themeMan);
            }

            return null;
        }

        private static T GetManifestFile<T>(string manifestPath) where T : class
        {
            return Serialization.FromYaml<T>(File.ReadAllText(manifestPath));
        }

        public static ThemeManifest GetPackedThemeManifest(string packagePath)
        {
            return GetPackedManifest<ThemeManifest>(packagePath, PlaynitePaths.ThemeManifestFileName);
        }

        public static ThemeManifest GetThemeManifest(string manifestPath)
        {
            return GetManifestFile<ThemeManifest>(manifestPath);
        }

        public static ExtensionManifest GetPackedExtensionManifest(string packagePath)
        {
            return GetPackedManifest<ExtensionManifest>(packagePath, PlaynitePaths.ExtensionManifestFileName);
        }

        public static ExtensionManifest GetExtensionManifest(string manifestPath)
        {
            return GetManifestFile<ExtensionManifest>(manifestPath);
        }

        public static void QueuePackageInstall(string packagePath)
        {
            QueueExtensionOperation(packagePath, ExtInstallType.Install);
        }

        public static void QueueExtensionUninstall(string extensionDirectory)
        {
            QueueExtensionOperation(extensionDirectory, ExtInstallType.Uninstall);
        }

        private static void QueueExtensionOperation(string extensionPath, ExtInstallType installationType)
        {
            if (currentQueue.FirstOrDefault(a => a.Path == extensionPath) == null)
            {
                currentQueue.Add(new ExtensionInstallQueueItem(extensionPath, installationType));
            }

            FileSystem.WriteStringToFile(PlaynitePaths.ExtensionQueueFilePath, Serialization.ToJson(currentQueue));
        }

        public static void VerifyExtensionPackage(string packagePath)
        {
            using (var zip = ZipFile.OpenRead(packagePath))
            {
                var manifestEntry = zip.GetEntry(PlaynitePaths.ExtensionManifestFileName);
                if (manifestEntry == null)
                {
                    logger.Error("Extension package is invalid, no manifest found.");
                    throw new LocalizedException(LOC.GeneralExtensionPackageError);
                }

                using (var logStream = manifestEntry.Open())
                {
                    using (TextReader tr = new StreamReader(logStream))
                    {
                        var manifest = Serialization.FromYaml<ExtensionManifest>(tr.ReadToEnd());
                        if (manifest.Id.IsNullOrEmpty())
                        {
                            logger.Error("Extension package is invalid, no extension ID found.");
                            throw new LocalizedException(LOC.GeneralExtensionPackageError);
                        }

                        if (!Version.TryParse(manifest.Version, out var _))
                        {
                            logger.Error($"Extension package is invalid, version is not in correct format {manifest.Version}.");
                            throw new LocalizedException(LOC.GeneralExtensionPackageError);
                        }
                    }
                }

                if (zip.Entries.Any(a => a.Name == "Playnite.dll" || a.Name == "Playnite.Common.dll" || a.Name == "Playnite.SDK.dll"))
                {
                    logger.Error($"Extension package is invalid, includes not allowed Playnite dependencies.");
                    throw new LocalizedException(LOC.GeneralExtensionPackageError);
                }
            }
        }

        public static void VerifyThemePackage(string packagePath)
        {
            using (var zip = ZipFile.OpenRead(packagePath))
            {
                var manifestEntry = zip.GetEntry(PlaynitePaths.ThemeManifestFileName);
                if (manifestEntry == null)
                {
                    logger.Error("Theme package is invalid, no manifest found.");
                    throw new LocalizedException(LOC.GeneralThemePackageError);
                }

                using (var logStream = manifestEntry.Open())
                {
                    using (TextReader tr = new StreamReader(logStream))
                    {
                        var manifest = Serialization.FromYaml<ThemeManifest>(tr.ReadToEnd());
                        if (manifest.Id.IsNullOrEmpty())
                        {
                            logger.Error("Theme package is invalid, no extension ID found.");
                            throw new LocalizedException(LOC.GeneralThemePackageError);
                        }

                        if (!Version.TryParse(manifest.Version, out var _))
                        {
                            logger.Error($"Theme package is invalid, version is not in correct format {manifest.Version}.");
                            throw new LocalizedException(LOC.GeneralThemePackageError);
                        }
                    }
                }

                if (zip.Entries.Any(a =>
                    a.Name == PlaynitePaths.ThemeSlnFileName ||
                    a.Name == PlaynitePaths.ThemeProjFileName ||
                    a.Name == PlaynitePaths.AppXamlFileName))
                {
                    logger.Error($"Theme package is invalid, includes not allowed theme project files.");
                    throw new LocalizedException(LOC.GeneralThemePackageError);
                }

                if (zip.Entries.Any(a => a.Name == "Playnite.dll" || a.Name == "Playnite.Common.dll" || a.Name == "Playnite.SDK.dll"))
                {
                    logger.Error($"Theme package is invalid, includes not allowed Playnite dependencies.");
                    throw new LocalizedException(LOC.GeneralThemePackageError);
                }
            }
        }
    }
}