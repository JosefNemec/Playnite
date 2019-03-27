using Playnite;
using Playnite.Common;
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

namespace BethesdaLibrary
{
    public class BethesdaGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private BethesdaLibrary BethesdaLib;

        public BethesdaGameController(BethesdaLibrary library, Game game) : base(game)
        {
            BethesdaLib = library;
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
            if (Game.PlayAction.Type == GameActionType.URL && Game.PlayAction.Path.StartsWith("bethesda", StringComparison.OrdinalIgnoreCase))
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));
                GameActionActivator.ActivateAction(Game.PlayAction);
                if (Directory.Exists(Game.InstallDirectory))
                {
                    stopWatch = Stopwatch.StartNew();
                    procMon = new ProcessMonitor();
                    procMon.TreeStarted += ProcMon_TreeStarted;
                    procMon.TreeDestroyed += Monitor_TreeDestroyed;
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                }
                else
                {
                    OnStopped(this, new GameControllerEventArgs(this, 0));
                }
            }
            else
            {
                throw new Exception("Unknown Play action configuration.");
            }
        }

        public override void Install()
        {
            ReleaseResources();
            ProcessStarter.StartUrl(Bethesda.ClientExecPath);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartUrl("bethesdanet://uninstall/" + Game.GameId);
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

                var installedGame = BethesdaLib.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId);
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

                if (BethesdaLib.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId) == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
