using Playnite;
using Playnite.Common.System;
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

namespace TwitchLibrary
{
    public class TwitchGameController : BaseGameController
    {
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
            ProcessStarter.StartUrl($"twitch://fuel-launch/{Game.GameId}");
            stopWatch = Stopwatch.StartNew();
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
                    if (Game.PlayAction == null)
                    {
                        Game.PlayAction = TwitchLibrary.GetPlayAction(Game.GameId);
                    }

                    Game.InstallDirectory = Paths.FixSeparators(program.InstallLocation);
                    OnInstalled(this, new GameControllerEventArgs(this, 0));
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
