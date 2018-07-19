//using NLog;
//using Playnite.Database;
//using Playnite.SDK.Models;
//using Polly;
//using SteamKit2;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Playnite.Providers.Steam
//{
//    public class SteamGameController : GameController
//    {
//        private SteamLibrary steam = new SteamLibrary();

//        public SteamGameController(Game game) : base(game)
//        {
//            if (game.Provider != Provider.Steam)
//            {
//                throw new Exception("Cannot use non-Steam game for Steam game controller.");
//            }
//        }

//        public override void Play(List<Emulator> emulators)
//        {
//            ReleaseResources();
//            if (Game.PlayTask.Type == GameTaskType.URL && Game.PlayTask.Path.StartsWith("steam", StringComparison.InvariantCultureIgnoreCase))
//            {
//                OnStarting(this, new GameControllerEventArgs(this, 0));
//                GameHandler.ActivateTask(Game.PlayTask, Game);
//                StartRunningWatcher();
//            }
//            else
//            {
//                base.Play(emulators);
//            }
//        }

//        public override void Install()
//        {
//            ReleaseResources();
//            ProcessStarter.StartUrl(@"steam://install/" + Game.GameId);
//            StartInstallWatcher();
//        }

//        public override void Uninstall()
//        {
//            ReleaseResources();
//            ProcessStarter.StartUrl(@"steam://uninstall/" + Game.GameId);
//            StartUninstallWatcher();
//        }

//        public async void StartInstallWatcher()
//        {
//            watcherToken = new CancellationTokenSource();
//            await Task.Run(async () =>
//            {
//                var stopWatch = Stopwatch.StartNew();
//                var id = int.Parse(Game.GameId);

//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    var gameState = steam.GetAppState(id);
//                    if (gameState.Installed == true)
//                    {
//                        if (Game.PlayTask == null)
//                        {
//                            Game.PlayTask = steam.GetPlayTask(int.Parse(Game.GameId));
//                        }

//                        stopWatch.Stop();
//                        OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
//                        return;
//                    }

//                    await Task.Delay(Timer.SecondsToMilliseconds(5));
//                }
//            });
//        }

//        public async void StartUninstallWatcher()
//        {
//            watcherToken = new CancellationTokenSource();
//            await Task.Run(async () =>
//            {
//                var stopWatch = Stopwatch.StartNew();
//                var id = int.Parse(Game.GameId);

//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    var gameState = steam.GetAppState(id);
//                    if (gameState.Installed == false)
//                    {
//                        stopWatch.Stop();
//                        OnUninstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
//                        return;
//                    }

//                    await Task.Delay(Timer.SecondsToMilliseconds(5));
//                }
//            });
//        }

//        public async void StartRunningWatcher()
//        {
//            watcherToken = new CancellationTokenSource();
//            await Task.Run(async () =>
//            {
//                var stopWatch = Stopwatch.StartNew();
//                var id = int.Parse(Game.GameId);
//                var gameState = steam.GetAppState(id);

//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    gameState = steam.GetAppState(id);
//                    if (gameState.Running == true)
//                    {
//                        OnStarted(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
//                        stopWatch.Restart();
//                        break;
//                    }

//                    await Task.Delay(Timer.SecondsToMilliseconds(2));
//                }

//                while (true)
//                {
//                    if (watcherToken.IsCancellationRequested)
//                    {
//                        return;
//                    }

//                    gameState = steam.GetAppState(id);
//                    if (gameState.Running == false)
//                    {
//                        stopWatch.Stop();
//                        OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
//                        return;
//                    }

//                    await Task.Delay(Timer.SecondsToMilliseconds(5));
//                }
//            });
//        }
//    }
//}
