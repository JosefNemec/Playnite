using Playnite.API;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
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

    public class ExtensionInstaller
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static List<ExtensionInstallQueueItem> currentQueue = new List<ExtensionInstallQueueItem>();

        public static List<BaseExtensionDescription> InstallExtensionQueue()
        {
            var anyFailed = false;
            var installedExts = new List<BaseExtensionDescription>();
            if (!File.Exists(PlaynitePaths.ExtensionQueueFilePath))
            {
                return installedExts;
            }

            var queue = Serialization.FromJsonFile<List<ExtensionInstallQueueItem>>(PlaynitePaths.ExtensionQueueFilePath);
            foreach (var queueItem in queue)
            {
                if (queueItem.InstallType == ExtInstallType.Install)
                {
                    if (queueItem.Path.EndsWith(PlaynitePaths.PackedThemeFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            installedExts.Add(ThemeManager.InstallFromPackedFile(queueItem.Path));
                            logger.Info($"Installed theme {queueItem}");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            anyFailed = true;
                            logger.Error(e, $"Failed to install theme {queueItem}");
                        }
                    }
                    else if (queueItem.Path.EndsWith(PlaynitePaths.PackedExtensionFileExtention, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            installedExts.Add(ExtensionFactory.InstallFromPackedFile(queueItem.Path));
                            logger.Info($"Installed extension {queueItem}");
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            anyFailed = true;
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
                            anyFailed = true;
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

            if (anyFailed)
            {
                throw new Exception("Failed to install or uninstall one or more extensions.");
            }

            return installedExts;
        }

        public static void QueueExtensionInstall(string extensionFile)
        {
            QueueExtensionOperation(extensionFile, ExtInstallType.Install);
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
    }
}