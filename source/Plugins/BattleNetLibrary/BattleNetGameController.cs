using BattleNetLibrary.Models;
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

namespace BattleNetLibrary
{
    public class BattleNetGameController : BaseGameController
    {
        private static ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private IPlayniteAPI api;

        public BattleNetGameController(Game game, IPlayniteAPI api) : base(game)
        {
            this.api = api;
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
            stopWatch = Stopwatch.StartNew();
            procMon = new ProcessMonitor();
            procMon.TreeDestroyed += Monitor_TreeDestroyed;
            var app = BattleNetGames.GetAppDefinition(Game.GameId);

            if (Game.PlayAction.Type == GameActionType.URL && Game.PlayAction.Path.StartsWith("battlenet", StringComparison.OrdinalIgnoreCase))
            {
                if (!BattleNet.IsInstalled)
                {
                    throw new Exception("Cannot start game, Battle.net launcher is not installed properly.");
                }

                OnStarting(this, new GameControllerEventArgs(this, 0));     
                StartBnetRunningWatcher();
            }
            else if (app.Type == BNetAppType.Classic && Game.PlayAction.Path.Contains(app.ClassicExecutable))
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));
                GameActionActivator.ActivateAction(Game.PlayAction);
                OnStarted(this, new GameControllerEventArgs(this, 0));
                if (Directory.Exists(Game.InstallDirectory))
                {
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, true);
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

        public async void StartBnetRunningWatcher()
        {
            if (!BattleNet.IsRunning)
            {
                logger.Info("Battle.net is not running, starting it first.");
                BattleNet.StartClient();
                while (BattleNet.RunningProcessesCount < 3)
                {
                    await Task.Delay(500);
                }
            }

            var task = new GameAction()
            {
                Path = BattleNet.ClientExecPath,
                Arguments = string.Format("--exec=\"launch {0}\"", Game.GameId)
            };

            GameActionActivator.ActivateAction(task);
            if (Directory.Exists(Game.InstallDirectory))
            {
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }

        }

        public override void Install()
        {
            ReleaseResources();
            var product = BattleNetGames.GetAppDefinition(Game.GameId);
            if (product.Type == BNetAppType.Classic)
            {
                ProcessStarter.StartUrl(@"https://battle.net/account/management/download/");
            }
            else
            {
                if (!BattleNet.IsInstalled)
                {
                    throw new Exception("Cannot install game, Battle.net launcher is not installed properly.");
                }

                ProcessStarter.StartProcess(BattleNet.ClientExecPath, $"--game={product.InternalId}");
            }

            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            var product = BattleNetGames.GetAppDefinition(Game.GameId);
            var entry = BattleNetLibrary.GetUninstallEntry(product);
            if (entry != null)
            {
                var args = string.Format("/C \"{0}\"", entry.UninstallString);
                ProcessStarter.StartProcess("cmd", args);
                StartUninstallWatcher();
            }
            else
            {
                OnUninstalled(this, new GameControllerEventArgs(this, 0));
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

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            var app = BattleNetGames.GetAppDefinition(Game.GameId);
            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var install = BattleNetLibrary.GetUninstallEntry(app);
                if (install == null)
                {
                    await Task.Delay(2000);
                    continue;
                }
                else
                {
                    if (Game.PlayAction == null)
                    {
                        if (app.Type == BNetAppType.Classic)
                        {
                            Game.PlayAction = new GameAction()
                            {
                                Type = GameActionType.File,
                                WorkingDir = @"{InstallDir}",
                                Path = app.ClassicExecutable
                            };
                        }
                        else
                        {
                            Game.PlayAction = BattleNetLibrary.GetGamePlayTask(Game.GameId);
                        }
                    }

                    Game.InstallDirectory = install.InstallLocation;
                    OnInstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            var app = BattleNetGames.GetAppDefinition(Game.GameId);
            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var entry = BattleNetLibrary.GetUninstallEntry(app);
                if (entry == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
