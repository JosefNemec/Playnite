//using Microsoft.Win32;
//using NLog;
//using Playnite.SDK.Models;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Playnite.Providers.Uplay
//{
//    public class UplayGameController : GameController
//    {
//        private UplayLibrary uplay = new UplayLibrary();

//        public UplayGameController(Game game) : base(game)
//        {
//            if (game.Provider != Provider.Uplay)
//            {
//                throw new Exception("Cannot use non-Uplay game for Uplay game controller.");
//            }
//        }

//        public override void Play(List<Emulator> emulators)
//        {
//            ReleaseResources();
//            if (Game.PlayTask.Type == GameTaskType.URL && Game.PlayTask.Path.StartsWith("uplay", StringComparison.InvariantCultureIgnoreCase))
//            {
//                OnStarting(this, new GameControllerEventArgs(this, 0));
//                stopWatch = Stopwatch.StartNew();
//                procMon = new ProcessMonitor();
//                procMon.TreeStarted += ProcMon_TreeStarted;
//                procMon.TreeDestroyed += Monitor_TreeDestroyed;
//                GameHandler.ActivateTask(Game.PlayTask, Game, emulators);
//                procMon.TreeStarted += ProcMon_TreeStarted;
//                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
//            }
//            else
//            {
//                base.Play(emulators);
//            }
//        }

//        public override void Install()
//        {
//            ReleaseResources();
//            ProcessStarter.StartUrl("uplay://install/" + Game.GameId);
//            StartInstallWatcher();
//        }

//        public override void Uninstall()
//        {
//            ReleaseResources();
//            ProcessStarter.StartUrl("uplay://uninstall/" + Game.GameId);
//            StartUninstallWatcher();
//        }

//        private void ProcMon_TreeStarted(object sender, EventArgs args)
//        {
//            OnStarted(this, new GameControllerEventArgs(this, 0));
//        }

//        private void Monitor_TreeDestroyed(object sender, EventArgs args)
//        {
//            stopWatch.Stop();
//            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
//        }

//        public async void StartInstallWatcher()
//        {
//            watcherToken = new CancellationTokenSource();
//            await Task.Run(async () =>
//            {
//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    var installedGame = uplay.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId);
//                    if (installedGame != null)
//                    {
//                        if (Game.PlayTask == null)
//                        {
//                            Game.PlayTask = installedGame.PlayTask;
//                        }

//                        Game.InstallDirectory = installedGame.InstallDirectory;
//                        OnInstalled(this, new GameControllerEventArgs(this, 0));
//                        return;
//                    }

//                    await Task.Delay(2000);
//                }
//            });
//        }

//        public async void StartUninstallWatcher()
//        {
//            watcherToken = new CancellationTokenSource();
//            await Task.Run(async () =>
//            {
//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    if (uplay.GetInstalledGames().FirstOrDefault(a => a.GameId == Game.GameId) == null)
//                    {
//                        OnUninstalled(this, new GameControllerEventArgs(this, 0));
//                        return;
//                    }

//                    await Task.Delay(2000);
//                }
//            });
//        }
//    }
//}
