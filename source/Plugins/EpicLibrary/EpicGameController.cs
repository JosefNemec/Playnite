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

namespace EpicLibrary
{
    public class EpicGameController : BaseGameController
    {
        private static List<string> launchelessExceptions = new List<string>
        {
            "Duckbill",     // Yooka-Laylee and the Impossible Lair
            "Vulture",      // Faeria
            "Stellula",     // Farming Simulator 19
            "Albacore",     // Assassins Creed Syndicate
            "Sundrop",      // For Honor
            "Wombat",       // World War Z
            "Eel",          // Kingdom Come Deliverance
            "Dodo",         // Borderlands 2
            "Turkey",       // Borderlands TPS
            "Kinglet",      // Civ 6
            "9d2d0eb64d5c44529cece33fe2a46482", // GTA 5
            "UnrealTournamentDev",  // Unreal Tournament 4
            "AzaleaAlpha",  // The Cycle
            "11e598b192324994abce05bad4f81b50", // A Total War Saga: TROY
        };

        private static ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private readonly IPlayniteAPI api;
        private readonly Game game;
        private readonly EpicLibrarySettings settings;

        public EpicGameController(Game game, IPlayniteAPI api, EpicLibrarySettings settings) : base(game)
        {
            this.api = api;
            this.game = game;
            this.settings = settings;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();
            OnStarting(this, new GameControllerEventArgs(this, 0));
            var startUri = string.Format(EpicLauncher.GameLaunchUrlMask, game.GameId);
            ProcessStarter.StartUrl(startUri);
            if (Directory.Exists(Game.InstallDirectory))
            {
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public override void Install()
        {
            if (!EpicLauncher.IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            ProcessStarter.StartUrl(EpicLauncher.LibraryLaunchUrl);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            if (!EpicLauncher.IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            ProcessStarter.StartUrl(EpicLauncher.LibraryLaunchUrl);
            StartUninstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var installed = EpicLauncher.GetInstalledAppList();
                var app = installed?.FirstOrDefault(a => a.AppName == Game.GameId);
                if (app != null)
                {
                    var installInfo = new GameInfo
                    {
                        InstallDirectory = app.InstallLocation,
                        PlayAction = new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = string.Format(EpicLauncher.GameLaunchUrlMask, app.AppName),
                            IsHandledByPlugin = true
                        }
                    };

                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                    return;
                };

                await Task.Delay(5000);
            }
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var installed = EpicLauncher.GetInstalledAppList();
                var app = installed?.FirstOrDefault(a => a.AppName == Game.GameId);
                if (app == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
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
