using Playnite;
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

namespace EpicLibrary
{
    public class EpicGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private readonly IPlayniteAPI api;
        private readonly Game game;

        public EpicGameController(Game game, IPlayniteAPI api) : base(game)
        {
            this.api = api;
            this.game = game;
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
            if (Game.PlayAction.Type == GameActionType.URL && Game.PlayAction.Path.StartsWith("com.epicgames.launcher", StringComparison.OrdinalIgnoreCase))
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
                throw new Exception("Unknown Epic action configuration.");
            }
        }

        public override void Install()
        {
            if (!EpicLauncher.IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            EpicLauncher.StartClient();
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            if (!EpicLauncher.IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            EpicLauncher.StartClient();
            StartUninstallWatcher();
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

                var installed = EpicLauncher.GetInstalledAppList();
                var app = installed?.FirstOrDefault(a => a.AppName == Game.GameId);
                if (app != null)
                {
                    if (Game.PlayAction == null)
                    {
                        Game.PlayAction = new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = string.Format(EpicLauncher.GameLaunchUrlMask, app.AppName),
                            IsHandledByPlugin = true
                        };
                    }

                    Game.InstallDirectory = app.InstallLocation;
                    OnInstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(5000);
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

                var installed = EpicLauncher.GetInstalledAppList();
                var app = installed?.FirstOrDefault(a => a.AppName == Game.GameId);
                if (app == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
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
    }
}
