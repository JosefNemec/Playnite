using Playnite;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private UplayLibrary uplay;
        private static ILogger logger = LogManager.GetLogger();

        public UplayGameController(UplayLibrary library, Game game) : base(game)
        {
            uplay = library;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            if (Directory.Exists(Game.InstallDirectory))
            {
                var requiresUplay = Uplay.GetGameRequiresUplay(Game);
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                ProcessStarter.StartUrl(Uplay.GetLaunchString(Game.GameId));
                StartRunningWatcher(requiresUplay);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public async void StartRunningWatcher(bool waitForUplay)
        {
            if (waitForUplay)
            {
                logger.Debug("Game requires UbisoftGameLauncher to run, waiting for it to start properly.");
                // Solves issues with game process being started/shutdown multiple times during startup via Uplay
                watcherToken = new CancellationTokenSource();
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (ProcessExtensions.IsRunning("UbisoftGameLauncher"))
                    {
                        procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                        return;
                    }

                    await Task.Delay(5000);
                }
            }
            else
            {
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
        }

        public override void Install()
        {
            ReleaseResources();
            ProcessStarter.StartUrl("uplay://install/" + Game.GameId);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartUrl("uplay://uninstall/" + Game.GameId);
            StartUninstallWatcher();
        }

        private void ProcMon_TreeStarted(object sender, EventArgs args)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var installedGame = uplay.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId);
                if (installedGame != null)
                {
                    var installInfo = new GameInfo()
                    {
                        PlayAction = installedGame.PlayAction,
                        InstallDirectory = installedGame.InstallDirectory
                    };

                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                if (uplay.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId) == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
