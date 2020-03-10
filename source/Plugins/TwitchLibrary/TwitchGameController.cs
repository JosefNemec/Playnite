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
using TwitchLibrary.Models;

namespace TwitchLibrary
{
    public class TwitchGameController : BaseGameController
    {
        private static ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private TwitchLibrary library;
        private Stopwatch stopWatch;
        private IPlayniteAPI api;

        public TwitchGameController(Game game, TwitchLibrary library, IPlayniteAPI api) : base(game)
        {
            this.library = library;
            this.api = api;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        private void ReleaseResources()
        {
            procMon?.Dispose();
        }

        public override void Install()
        {
            ProcessStarter.StartUrl($"twitch://fuel/");
            StartInstallWatcher();
        }

        public override void Play()
        {
            OnStarting(this, new GameControllerEventArgs(this, 0));
            var startViaLauncher = true;
            GameConfiguration gameConfig = null;
            if (library.LibrarySettings.StartGamesWithoutLauncher)
            {
                try
                {
                    gameConfig = Twitch.GetGameConfiguration(Game.InstallDirectory);
                    if (Twitch.GetGameRequiresClient(gameConfig))
                    {
                        startViaLauncher = true;
                    }
                    else
                    {
                        startViaLauncher = false;
                    }
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    logger.Error(e, "Failed to get local game configuration.");
                }
            }

            if (startViaLauncher)
            {
                ProcessStarter.StartUrl($"twitch://fuel-launch/{Game.GameId}");
            }
            else
            {
                var exePath = Path.Combine(Game.InstallDirectory, gameConfig.Main.Command);
                var workDir = Game.InstallDirectory;
                if (!gameConfig.Main.WorkingSubdirOverride.IsNullOrEmpty())
                {
                    workDir = Path.Combine(Game.InstallDirectory, gameConfig.Main.WorkingSubdirOverride);
                }

                string args = null;
                if (gameConfig.Main.Args.HasNonEmptyItems())
                {
                    args = string.Join(" ", gameConfig.Main.Args);
                }

                ProcessStarter.StartProcess(exePath, args, workDir);
            }

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

        public override void Uninstall()
        {
            ReleaseResources();
            ProcessStarter.StartProcess(Twitch.GameRemoverPath, $"-m Game -p {Game.GameId}");
            StartUninstallWatcher();
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

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }
                var program = Twitch.GetUninstallRecord(Game.GameId);
                if (program != null)
                {
                    var installInfo = new GameInfo()
                    {
                        PlayAction = TwitchLibrary.GetPlayAction(Game.GameId),
                        InstallDirectory = Paths.FixSeparators(program.InstallLocation)
                    };

                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                    return;
                }

                await Task.Delay(2000);
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

                var program = Twitch.GetUninstallRecord(Game.GameId);
                if (program == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
