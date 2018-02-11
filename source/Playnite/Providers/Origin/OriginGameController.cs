using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;

namespace Playnite.Providers.Origin
{
    public class OriginGameController : GameController
    {
        private OriginLibrary origin = new OriginLibrary();

        public OriginGameController(Game game) : base(game)
        {
            if (game.Provider != Provider.Origin)
            {
                throw new Exception("Cannot use non-Origin game for Origin game controller.");
            }
        }

        public override void Play(List<Emulator> emulators)
        {
            Dispose();
            stopWatch = Stopwatch.StartNew();
            procMon = new ProcessMonitor();
            procMon.TreeDestroyed += ProcMon_TreeDestroyed;
            procMon.TreeStarted += ProcMon_TreeStarted;
            var proc = GameHandler.ActivateTask(Game.PlayTask, Game, emulators);
            procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
        }

        public override void Install()
        {
            Dispose();
            ProcessStarter.StartUrl($"origin2://game/launch?offerIds={Game.ProviderId}&autoDownload=true");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            Dispose();
            ProcessStarter.StartProcess("appwiz.cpl", string.Empty);
            StartUninstallWatcher();
        }

        private void ProcMon_TreeStarted(object sender, EventArgs args)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
        }

        private void ProcMon_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                var manifest = origin.GetLocalManifest(Game.ProviderId, null, true);
                var platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var executablePath = origin.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);
                    if (!string.IsNullOrEmpty(executablePath))
                    {
                        if (File.Exists(executablePath))
                        {
                            if (Game.PlayTask == null)
                            {
                                origin.GetGamePlayTask(manifest);
                            }

                            Game.InstallDirectory = Path.GetDirectoryName(executablePath);
                            OnInstalled(this, new GameControllerEventArgs(this, 0));
                            return;
                        }
                    }
                    else
                    {
                        if (PlayniteEnvironment.ThrowAllErrors)
                        {
                            throw new Exception($"Cannot start installation of {Game.Name} Origin game, cannot determine install location.");
                        }

                        logger.Error($"Cannot start installation of {Game.Name} Origin game, cannot determine install location.");
                        return;
                    }

                    Thread.Sleep(2000);
                }
            });
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                var manifest = origin.GetLocalManifest(Game.ProviderId, null, true);
                var platform = manifest.publishing.softwareList.software.FirstOrDefault(a => a.softwarePlatform == "PCWIN");
                var executablePath = origin.GetPathFromPlatformPath(platform.fulfillmentAttributes.installCheckOverride);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    if (string.IsNullOrEmpty(executablePath))
                    {
                        OnUninstalled(this, new GameControllerEventArgs(this, 0));
                        return;
                    }
                    else
                    {
                        if (!File.Exists(executablePath))
                        {
                            OnUninstalled(this, new GameControllerEventArgs(this, 0));
                            return;
                        }
                    }

                    Thread.Sleep(2000);
                }
            });
        }
    }
}
