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

namespace Playnite.Providers.BattleNet
{
    public class BattleNetGameStateMonitor : IGameStateMonitor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IBattleNetLibrary library;
        private BattleNetLibrary.BNetApp app;
        private CancellationTokenSource cancelToken;

        public event EventHandler GameUninstalled;
        public event GameInstalledEventHandler GameInstalled;

        public BattleNetGameStateMonitor(BattleNetLibrary.BNetApp app, IBattleNetLibrary library)
        {
            this.app = app;
            this.library = library;
        }

        public void Dispose()
        {
            cancelToken?.Cancel(false);
        }

        public void StartInstallMonitoring()
        {
            logger.Info("Starting install monitoring of BattleNet app " + app.ProductId);
            Dispose();

            cancelToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (!cancelToken.Token.IsCancellationRequested)
                {
                    var entry = BattleNetLibrary.GetUninstallEntry(app);
                    if (entry != null)
                    {
                        logger.Info($"BattleNet app {app.ProductId} has been installed.");

                        GameTask playTask;
                        if (app.Type == BattleNetLibrary.BNetAppType.Classic)
                        {
                            playTask = new GameTask()
                            {
                                Type = GameTaskType.File,
                                WorkingDir = @"{InstallDir}",
                                Path = @"{InstallDir}\" + app.ClassicExecutable
                            };
                        }
                        else
                        {
                            playTask = library.GetGamePlayTask(app.ProductId);
                        }

                        GameInstalled?.Invoke(this, new GameInstalledEventArgs(new Game()
                        {
                            PlayTask = playTask,
                            InstallDirectory = entry.InstallLocation
                        }));
                        return;
                    }

                    Thread.Sleep(5000);
                }
            }, cancelToken.Token);
        }

        public void StartUninstallMonitoring()
        {
            logger.Info("Starting uninstall monitoring of BattleNet app " + app.ProductId);
            Dispose();

            cancelToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (!cancelToken.Token.IsCancellationRequested)
                {
                    var entry = BattleNetLibrary.GetUninstallEntry(app);
                    if (entry == null)
                    {
                        logger.Info($"BattleNet app {app.ProductId} has been uninstalled.");
                        GameUninstalled?.Invoke(this, null);
                        return;
                    }

                    Thread.Sleep(5000);
                }
            }, cancelToken.Token);
        }

        public void StopMonitoring()
        {
            logger.Info("Stopping monitoring of BattleNet app " + app.ProductId);
            Dispose();
        }
    }
}
