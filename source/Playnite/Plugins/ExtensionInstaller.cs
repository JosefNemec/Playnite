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
    public class ExtensionInstallQueueItem
    {
        public string Path { get; set; }

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
                if (queueItem.Path.EndsWith(PlaynitePaths.PackedThemeFileExtention, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        installedExts.Add(ThemeManager.InstallFromPackedFile(queueItem.Path));
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

            File.Delete(PlaynitePaths.ExtensionQueueFilePath);

            if (anyFailed)
            {
                throw new Exception("Failed to install one or more extensions.");
            }

            return installedExts;
        }

        public static void QueueExetnsionInstall(string extensionFile)
        {
            if (currentQueue.FirstOrDefault(a => a.Path == extensionFile) == null)
            {
                currentQueue.Add(new ExtensionInstallQueueItem() { Path = extensionFile });
            }

            FileSystem.WriteStringToFile(PlaynitePaths.ExtensionQueueFilePath, Serialization.ToJson(currentQueue));
        }
    }
}