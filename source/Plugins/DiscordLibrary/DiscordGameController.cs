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
using System.IO;

namespace DiscordLibrary
{
    public class DiscordGameController : BaseGameController
    {
        private static ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private Game game;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private DiscordLibrary library;

        public DiscordGameController(Game game, DiscordLibrary library) : base(game)
        {
            this.game = game;
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
            OnStarting(this, new GameControllerEventArgs(this, 0));
            stopWatch = Stopwatch.StartNew();
            ProcessStarter.StartUrl($"discord:///library/{Game.GameId}/launch");
            procMon = new ProcessMonitor();
            procMon.TreeStarted += ProcMon_TreeStarted;
            procMon.TreeDestroyed += Monitor_TreeDestroyed;
            if (Directory.Exists(Game.InstallDirectory))
            {
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public override void Install()
        {
            //TODO: is there a way to ask discord to install a specific game?
            ReleaseResources();
            ProcessStarter.StartUrl($"discord:///library/");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartUrl($"discord:///library/{Game.GameId}/uninstall");
            StartUninstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            var stopWatch = Stopwatch.StartNew();
            var id = Game.GameId;

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var installed = library.GetInstalledGames();
                if (installed.TryGetValue(id, out var installedGame))
                {
                    if (Game.PlayAction == null)
                    {
                        Game.PlayAction = installedGame.PlayAction;
                    }

                    Game.InstallDirectory = installedGame.InstallDirectory;
                    stopWatch.Stop();
                    OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                    return;
                }

                await Task.Delay(Playnite.Timer.SecondsToMilliseconds(10));
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            stopWatch = Stopwatch.StartNew();
            var id = Game.GameId;

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                if (!library.GetInstalledGames().ContainsKey(id))
                {
                    stopWatch.Stop();
                    OnUninstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                    return;
                }

                await Task.Delay(Playnite.Timer.SecondsToMilliseconds(5));
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
