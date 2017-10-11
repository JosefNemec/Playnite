using Microsoft.Win32;
using NLog;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Providers.Uplay
{
    public class UplayGameStateMonitor : IGameStateMonitor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IUplayLibrary library;
        private string id;
        private CancellationTokenSource cancelToken;

        public event EventHandler GameUninstalled;
        public event GameInstalledEventHandler GameInstalled;

        public UplayGameStateMonitor(string id, IUplayLibrary uplayLibrary)
        {
            this.id = id;
            library = uplayLibrary;
        }

        public void Dispose()
        {
            cancelToken?.Cancel(false);
        }

        public void StartInstallMonitoring()
        {
            logger.Info("Starting install monitoring of Uplay app " + id);
            Dispose();

            cancelToken = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                // Uplay is currently 32bit only, but this will future proof this feature
                var root32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);                
                var root64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                while (!cancelToken.Token.IsCancellationRequested)
                {
                    var installsKey32 = root32.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
                    if (installsKey32 != null)
                    {
                        var gameKey = installsKey32.OpenSubKey(id);
                        if (gameKey != null)
                        {
                            logger.Info($"Uplay app {id} has been installed.");
                            GameInstalled?.Invoke(this, new GameInstalledEventArgs(new Game()
                            {
                                PlayTask = library.GetGamePlayTask(id),
                                InstallDirectory = (gameKey.GetValue("InstallDir") as string).Replace('/', Path.DirectorySeparatorChar)
                            }));
                            return;
                        }                             
                    }

                    if (Environment.Is64BitOperatingSystem)
                    {
                        var installsKey64 = root64.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
                        if (installsKey64 != null)
                        {
                            var gameKey = installsKey64.OpenSubKey(id);
                            if (gameKey != null)
                            {
                                logger.Info($"Uplay app {id} has been installed.");
                                GameInstalled?.Invoke(this, new GameInstalledEventArgs(new Game()
                                {
                                    PlayTask = library.GetGamePlayTask(id),
                                    InstallDirectory = (gameKey.GetValue("InstallDir") as string).Replace('/', Path.DirectorySeparatorChar)
                                }));
                                return;
                            }
                        }
                    }

                    Thread.Sleep(5000);
                }
            }, cancelToken.Token);
        }

        public void StartUninstallMonitoring()
        {

            logger.Info("Starting uninstall monitoring of Uplay app " + id);
            Dispose();

            cancelToken = new CancellationTokenSource();
            var gameInstalled = library.GetInstalledGames().FirstOrDefault(a => a.ProviderId == id) != null;

            Task.Factory.StartNew(() =>
            {
                // Uplay is currently 32bit only, but this will future proof this feature
                var root32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var root64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                while (!cancelToken.Token.IsCancellationRequested)
                {
                    var installsKey32 = root32.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
                    if (installsKey32 != null)
                    {
                        var gameKey = installsKey32.OpenSubKey(id);
                        if (gameKey == null)
                        {
                            logger.Info($"Uplay app {id} has been uninstalled.");
                            GameUninstalled?.Invoke(this, null);
                            return;
                        }
                    }

                    if (Environment.Is64BitOperatingSystem)
                    {
                        var installsKey64 = root64.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
                        if (installsKey64 != null)
                        {
                            var gameKey = installsKey64.OpenSubKey(id);
                            if (gameKey == null)
                            {
                                logger.Info($"Uplay app {id} has been uninstalled.");
                                GameUninstalled?.Invoke(this, null);
                                return;
                            }
                        }
                    }

                    Thread.Sleep(5000);
                }
            }, cancelToken.Token);
        }

        public void StopMonitoring()
        {
            logger.Info("Stopping monitoring of Uplay app " + id);
            Dispose();
        }
    }
}
