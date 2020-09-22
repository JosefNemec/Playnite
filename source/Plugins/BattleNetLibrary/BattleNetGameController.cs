using BattleNetLibrary.Models;
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
                var playAction = api.ExpandGameVariables(Game, Game.PlayAction);
                OnStarting(this, new GameControllerEventArgs(this, 0));
                GameActionActivator.ActivateAction(playAction);
                OnStarted(this, new GameControllerEventArgs(this, 0));
                if (Directory.Exists(Game.InstallDirectory))
                {
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, true, true);
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

            try
            {
                GameActionActivator.ActivateAction(task);
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to start battle.net game.");
                OnStopped(this, new GameControllerEventArgs(this, 0));
                return;
            }

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
                    var installInfo = new GameInfo()
                    {
                        InstallDirectory = install.InstallLocation
                    };

                    if (app.Type == BNetAppType.Classic)
                    {
                        installInfo.PlayAction = new GameAction()
                        {
                            Type = GameActionType.File,
                            WorkingDir = ExpandableVariables.InstallationDirectory,
                            Path = app.ClassicExecutable
                        };
                    }
                    else
                    {
                        installInfo.PlayAction = BattleNetLibrary.GetGamePlayTask(Game.GameId);
                    }

                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
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
