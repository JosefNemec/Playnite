using NLog;
using Playnite.Database;
using Playnite.SDK.Models;
using Polly;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Providers.Steam
{
    public class SteamGameController : GameController
    {
        private SteamLibrary steam = new SteamLibrary();

        public SteamGameController(Game game) : base(game)
        {
            if (game.Provider != Provider.Steam)
            {
                throw new Exception("Cannot use non-Steam game for Steam game controller.");
            }
        }

        public override void Play(List<Emulator> emulators)
        {
            Dispose();
            if (Game.PlayTask.Type == GameTaskType.URL && Game.PlayTask.Path.StartsWith("steam", StringComparison.InvariantCultureIgnoreCase))
            {
                GameHandler.ActivateTask(Game.PlayTask, Game);
                StartRunningWatcher();
            }
            else
            {
                base.Play(emulators);
            }
        }

        public override void Install()
        {
            Dispose();
            Process.Start(@"steam://install/" + Game.ProviderId);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            Dispose();
            Process.Start(@"steam://uninstall/" + Game.ProviderId);
            StartUninstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.ProviderId);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var gameState = steam.GetAppState(id);
                    if (gameState.Installed == true)
                    {
                        stopWatch.Stop();
                        OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    Thread.Sleep(2000);
                }
            });
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.ProviderId);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var gameState = steam.GetAppState(id);
                    if (gameState.Installed == false)
                    {
                        stopWatch.Stop();
                        OnUninstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    Thread.Sleep(2000);
                }
            });
        }

        public async void StartRunningWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
            {
                var stopWatch = Stopwatch.StartNew();
                var id = int.Parse(Game.ProviderId);
                var gameState = steam.GetAppState(id);

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    gameState = steam.GetAppState(id);
                    if (gameState.Running == true)
                    {
                        OnStarted(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        stopWatch.Restart();
                        break;
                    }

                    Thread.Sleep(1000);
                }

                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    gameState = steam.GetAppState(id);
                    if (gameState.Running == false)
                    {
                        stopWatch.Stop();
                        OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                        return;
                    }

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
