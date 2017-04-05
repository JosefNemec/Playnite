using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Playnite.Providers.GOG
{
    public class GogGameStateMonitor : IGameStateMonitor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event EventHandler GameUninstalled;

        public event GameInstalledEventHandler GameInstalled
        {
            add
            {
            }
            remove
            {
            }
        }

        private IGogLibrary library;

        private FileSystemWatcher watcher;

        private string id;

        private string installDirectory;

        public GogGameStateMonitor(string id, string installDirectory, IGogLibrary originLibrary)
        {
            library = originLibrary;
            this.id = id;
            this.installDirectory = installDirectory;
        }

        public void StartMonitoring()
        {
            logger.Info("Starting monitoring of GOG app " + id);
            Dispose();

            var infoFile = string.Format("goggame-{0}.info", id);
            if (File.Exists(Path.Combine(installDirectory, infoFile)))
            {
                logger.Info("GOG app {0} is currently not installed, starting install monitor.", id);
                watcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Path = installDirectory,
                    Filter = Path.GetFileName(infoFile)
                };

                watcher.Deleted += Watcher_Deleted;
                watcher.EnableRaisingEvents = true;
            }
            else
            {
                // Installation detection not implemented for several technical limitations
                // Maily because of shared access to GOG's local database
                logger.Warn("GOG app {0} is currently not installed, NOT starting install monitor", id);
            }                    
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            logger.Info("GOG app {0} uninstalled.", id);
            GameUninstalled?.Invoke(this, null);
        }

        public void StopMonitoring()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
    }
}
