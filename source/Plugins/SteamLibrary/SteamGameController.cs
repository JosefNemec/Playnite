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
using SteamKit2;

namespace SteamLibrary
{
    public class SteamGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private GameID gameId;

        public SteamGameController(Game game) : base(game)
        {
            gameId = game.ToSteamGameID();
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            watcherToken?.Cancel();
        }

        public override void Play()
        {
            ReleaseResources();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            ProcessStarter.StartUrl($"steam://rungameid/{Game.GameId}");
            StartRunningWatcher();
        }

        public override void Install()
        {
            if (gameId.IsMod)
            {
                throw new NotSupportedException("Installing mods is not supported.");
            }
            else
            {
                ReleaseResources();
                ProcessStarter.StartUrl($"steam://install/{gameId.AppID}");
                StartInstallWatcher();
            }
        }

        public override void Uninstall()
        {
            if (gameId.IsMod)
            {
                throw new NotSupportedException("Uninstalling mods is not supported.");
            }
            else
            {
                ReleaseResources();
                ProcessStarter.StartUrl($"steam://uninstall/{gameId.AppID}");
                StartUninstallWatcher();
            }
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();      
            var stopWatch = Stopwatch.StartNew();
            var id = Game.ToSteamGameID();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var gameState = Steam.GetAppState(id);
                if (gameState.Installed == true)
                {
                    var installInfo = new GameInfo()
                    {
                        PlayAction = SteamLibrary.CreatePlayTask(Game.ToSteamGameID()),
                        // TODO: update install directory here
                    };

                    stopWatch.Stop();
                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, stopWatch.Elapsed.TotalSeconds));
                    return;
                }

                await Task.Delay(Playnite.Timer.SecondsToMilliseconds(5));
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();    
            var stopWatch = Stopwatch.StartNew();
            var id = Game.ToSteamGameID();

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
        }

        public async void StartRunningWatcher()
        {
            watcherToken = new CancellationTokenSource(); 
            var stopWatch = Stopwatch.StartNew();
            var id = Game.ToSteamGameID();
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
        }
    }
}
