using NLog;
using Polly;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Steam
{
    public class SteamGameStateMonitor : IGameStateMonitor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event EventHandler GameUninstalled;

        public event GameInstalledEventHandler GameInstalled;

        private List<FileSystemWatcher> watchers;

        private ISteamLibrary library;

        private string oldGameState = string.Empty;

        private string id;

        public SteamGameStateMonitor(string id, ISteamLibrary steamLibrary)
        {
            this.id = id;
            library = steamLibrary;
            watchers = new List<FileSystemWatcher>();

            foreach (var folder in library.GetLibraryFolders())
            {
                var watcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Path = Path.Combine(folder, "steamapps"),
                    Filter = string.Format("appmanifest_{0}.acf", id)
                };

                watchers.Add(watcher);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var kv = new KeyValue();

            Policy.Handle<IOException>().WaitAndRetry(4, r => TimeSpan.FromSeconds(1)).Execute(() =>
            {
                kv.ReadFileAsText(e.FullPath);
            });            

            var state = kv["StateFlags"].Value;
            if (state == "4" && oldGameState != state)
            {
                oldGameState = state;
                var newGame = library.GetInstalledGameFromFile(e.FullPath);
                logger.Info("Steam app {0} installed.", id);
                GameInstalled?.Invoke(this, new GameInstalledEventArgs(newGame));
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            logger.Info("Steam app {0} uninstalled.", id);
            GameUninstalled?.Invoke(this, null);
        }

        public void StartInstallMonitoring()
        {
            logger.Info("Starting install monitoring of Steam app " + id);

            foreach (var watcher in watchers)
            {
                watcher.Changed += Watcher_Changed;
                watcher.EnableRaisingEvents = true;
            }
        }

        public void StartUninstallMonitoring()
        {
            logger.Info("Starting uninstall monitoring of Steam app " + id);

            foreach (var watcher in watchers)
            {
                watcher.Deleted += Watcher_Deleted;
                watcher.EnableRaisingEvents = true;
            }
        }

        public void StopMonitoring()
        {
            logger.Info("Stopping monitoring of Steam app " + id);

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        public void Dispose()
        {
            if (watchers == null)
            {
                return;
            }

            foreach (var watcher in watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
    }
}
