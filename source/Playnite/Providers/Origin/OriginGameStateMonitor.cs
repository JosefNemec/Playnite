using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Playnite.Providers.Origin
{
    public class OriginGameStateMonitor : IGameStateMonitor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event EventHandler GameUninstalled;

        public event GameInstalledEventHandler GameInstalled;

        private IOriginLibrary library;

        private GameLocalDataResponse manifest;

        private FileSystemWatcher watcher;

        private GameLocalDataResponse.Publishing.Software platform;

        private string executablePath;

        private CancellationTokenSource installWaitToken;

        private string id;

        public OriginGameStateMonitor(string id, IOriginLibrary originLibrary)
        {
            this.id = id;
            library = originLibrary;
            manifest = library.GetLocalManifest(id, null, true);
            platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            logger.Info("Origin app {0} installed.", id);
            GameInstalled?.Invoke(this, new GameInstalledEventArgs(new Game()
            {
                PlayTask = library.GetGamePlayTask(manifest),
                InstallDirectory = Path.GetDirectoryName(executablePath)
            }));
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            logger.Info("Origin app {0} uninstalled.", id);
            GameUninstalled?.Invoke(this, null);
        }
       
        public void StartInstallMonitoring()
        {
            logger.Info("Starting install monitoring of Origin app " + id);
            Dispose();

            executablePath = library.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
            installWaitToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (!installWaitToken.Token.IsCancellationRequested)
                {
                    executablePath = library.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
                    if (!string.IsNullOrEmpty(executablePath))
                    {
                        if (File.Exists(executablePath))
                        {
                            Watcher_Created(this, null);
                            return;
                        }
                        else
                        {
                            watcher = new FileSystemWatcher()
                            {
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                Path = Path.GetDirectoryName(executablePath),
                                Filter = Path.GetFileName(executablePath)
                            };

                            watcher.Created += Watcher_Created;
                            watcher.EnableRaisingEvents = true;
                            return;
                        }
                    }

                    Thread.Sleep(2000);
                }
            }, installWaitToken.Token);
        }

        public void StartUninstallMonitoring()
        {
            logger.Info("Starting uninstall monitoring of Origin app " + id);
            Dispose();

            executablePath = library.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);            
            if (string.IsNullOrEmpty(executablePath))
            {
                Watcher_Deleted(this, null);
            }
            else
            {
                watcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Path = Path.GetDirectoryName(executablePath),
                    Filter = Path.GetFileName(executablePath)
                };

                watcher.Deleted += Watcher_Deleted;
                watcher.EnableRaisingEvents = true;
            }
        }

        public void StopMonitoring()
        {
            logger.Info("Stopping monitoring of Origin app " + id);
            Dispose();
        }

        public void Dispose()
        {
            if (installWaitToken != null)
            {
                installWaitToken.Cancel(false);
            }

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
    }
}
