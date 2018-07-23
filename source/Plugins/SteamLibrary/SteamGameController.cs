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

namespace SteamLibrary
{
    public class SteamGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;

        public SteamGameController(Game game) : base(game)
        {
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            watcherToken?.Cancel();
        }

        public override void ActivateAction(GameAction action)
        {
            throw new NotImplementedException();
        }

        public override void Play()
        {
            ReleaseResources();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            ProcessStarter.StartUrl($"steam://run/{Game.GameId}");
            StartRunningWatcher();
        }

        public override void Install()
        {
            ReleaseResources();
            ProcessStarter.StartUrl($"steam://install/{Game.GameId}");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartUrl($"steam://uninstall/{Game.GameId}");
            StartUninstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.GameId);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var gameState = Steam.GetAppState(id);
                    if (gameState.Installed == true)
                    {
                        if (Game.PlayAction == null)
                        {
                            Game.PlayAction = SteamLibrary.CreatePlayTask(int.Parse(Game.GameId));
                        }

                        stopWatch.Stop();
                        OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    await Task.Delay(Playnite.Timer.SecondsToMilliseconds(5));
                }
            });
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.GameId);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var gameState = Steam.GetAppState(id);
                    if (gameState.Installed == false)
                    {
                        stopWatch.Stop();
                        OnUninstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    await Task.Delay(Playnite.Timer.SecondsToMilliseconds(5));
                }
            });
        }

        public async void StartRunningWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.GameId);
                var gameState = Steam.GetAppState(id);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    gameState = Steam.GetAppState(id);
                    if (gameState.Running == true)
                    {
                        OnStarted(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        stopWatch.Restart();
                        break;
                    }

                    await Task.Delay(Playnite.Timer.SecondsToMilliseconds(2));
                }

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    gameState = Steam.GetAppState(id);
                    if (gameState.Running == false)
                    {
                        stopWatch.Stop();
                        OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    await Task.Delay(Playnite.Timer.SecondsToMilliseconds(5));
                }
            });
        }
    }
}
