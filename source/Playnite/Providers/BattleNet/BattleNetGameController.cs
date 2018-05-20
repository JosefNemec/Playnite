using Microsoft.Win32;
using NLog;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace Playnite.Providers.BattleNet
{
    public class BattleNetGameController : GameController
    {
        private BattleNetLibrary battleNet = new BattleNetLibrary();

        public BattleNetGameController(Game game) : base(game)
        {
            if (game.Provider != Provider.BattleNet)
            {
                throw new Exception("Cannot use non-BattleNet game for BattleNet game controller.");
            }
        }

        public override void Play(List<Emulator> emulators)
        {
            Dispose();
            stopWatch = Stopwatch.StartNew();
            procMon = new ProcessMonitor();
            procMon.TreeDestroyed += Monitor_TreeDestroyed;
            var app = BattleNetLibrary.GetAppDefinition(Game.ProviderId);

            if (Game.PlayTask.Type == GameTaskType.URL && Game.PlayTask.Path.StartsWith("battlenet", StringComparison.InvariantCultureIgnoreCase))
            {
                GameHandler.ActivateTask(Game.PlayTask, Game, emulators);
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else if (app.Type == BattleNetLibrary.BNetAppType.Classic && Game.PlayTask.Path.Contains(app.ClassicExecutable))
            {
                var proc = GameHandler.ActivateTask(Game.PlayTask, Game, emulators);
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, true);
                OnStarted(this, new GameControllerEventArgs(this, 0));
            }
            else
            {
                base.Play(emulators);
            }
        }

        public override void Install()
        {
            Dispose();
            var product = BattleNetLibrary.GetAppDefinition(Game.ProviderId);
            if (product.Type == BattleNetLibrary.BNetAppType.Classic)
            {
                ProcessStarter.StartUrl(@"https://battle.net/account/management/download/");
            }
            else
            {
                ProcessStarter.StartProcess(BattleNetSettings.ClientExecPath, $"--game={product.InternalId}");
            }

            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            Dispose();
            var product = BattleNetLibrary.GetAppDefinition(Game.ProviderId);
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
            await Task.Run(async () =>
            {
                var app = BattleNetLibrary.GetAppDefinition(Game.ProviderId);

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
                        if (Game.PlayTask == null)
                        {
                            if (app.Type == BattleNetLibrary.BNetAppType.Classic)
                            {

                                Game.PlayTask = new GameTask()
                                {
                                    Type = GameTaskType.File,
                                    WorkingDir = @"{InstallDir}",
                                    Path = @"{InstallDir}\" + app.ClassicExecutable
                                };
                            }
                            else
                            {
                                Game.PlayTask = battleNet.GetGamePlayTask(Game.ProviderId);
                            }
                        }

                        Game.InstallDirectory = install.InstallLocation;
                        OnInstalled(this, new GameControllerEventArgs(this, 0));
                        return;
                    }                    
                }
            });
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                var app = BattleNetLibrary.GetAppDefinition(Game.ProviderId);

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
            });
        }
    }
}
