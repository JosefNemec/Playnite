using Playnite;
using Playnite.Common;
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
using System.IO;

namespace SteamLibrary
{
    public class SteamGameController : BaseGameController
    {
        private static ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private GameID gameId;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private SteamLibrary library;

        public SteamGameController(Game game, SteamLibrary library) : base(game)
        {
            gameId = game.ToSteamGameID();
            this.library = library;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();

            var installDirectory = Game.InstallDirectory;
            if (gameId.IsMod)
            {
                var allGames = library.GetInstalledGames(false);
                if (allGames.TryGetValue(gameId.AppID.ToString(), out GameInfo realGame))
                {
                    installDirectory = realGame.InstallDirectory;
                }
            }

            OnStarting(this, new GameControllerEventArgs(this, 0));
            stopWatch = Stopwatch.StartNew();
            ProcessStarter.StartUrl($"steam://rungameid/{Game.GameId}");
            procMon = new ProcessMonitor();
            procMon.TreeStarted += ProcMon_TreeStarted;
            procMon.TreeDestroyed += Monitor_TreeDestroyed;
            if (Directory.Exists(installDirectory))
            {
                procMon.WatchDirectoryProcesses(installDirectory, false);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
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

                var installed = library.GetInstalledGames(false);
                if (installed.TryGetValue(id, out var installedGame))
                {
                    var installInfo = new GameInfo
                    {
                        InstallDirectory = installedGame.InstallDirectory,
                        PlayAction = installedGame.PlayAction
                    };

                    stopWatch.Stop();
                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, stopWatch.Elapsed.TotalSeconds));
                    return;
                }

                await Task.Delay(Playnite.Common.Timer.SecondsToMilliseconds(10));
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();    
            stopWatch = Stopwatch.StartNew();
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

                await Task.Delay(Playnite.Common.Timer.SecondsToMilliseconds(5));
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
