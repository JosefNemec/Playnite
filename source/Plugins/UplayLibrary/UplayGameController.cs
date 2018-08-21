using Playnite;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public UplayGameController(UplayLibrary library, Game game) : base(game)
        {
            uplay = library;
        }

        public override void ActivateAction(GameAction action)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();
            if (Game.PlayAction.Type == GameActionType.URL && Game.PlayAction.Path.StartsWith("uplay", StringComparison.InvariantCultureIgnoreCase))
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                GameActionActivator.ActivateAction(Game.PlayAction, Game);
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else
                throw new Exception("Unknoww Play action configuration");
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
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var installedGame = uplay.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId);
                    if (installedGame != null)
                    {
                        if (Game.PlayAction == null)
                        {
                            Game.PlayAction = installedGame.PlayAction;
                        }

                        Game.InstallDirectory = installedGame.InstallDirectory;
                        OnInstalled(this, new GameControllerEventArgs(this, 0));
                        return;
                    }

                    await Task.Delay(2000);
                }
            });
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
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
            });
        }
    }
}
