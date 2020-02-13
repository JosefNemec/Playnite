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

namespace XboxLibrary
{
    public class XboxGameController : BaseGameController
    {
        private readonly IPlayniteAPI api;
        private readonly Game game;
        private readonly XboxLibrarySettings settings;
        protected ProcessMonitor procMon;
        protected Stopwatch stopWatch;
        private CancellationTokenSource watcherToken;

        public XboxGameController(Game game, IPlayniteAPI api, XboxLibrarySettings settings) : base(game)
        {
            this.api = api;
            this.game = game;
            this.settings = settings;
        }

        public override void Install()
        {
            if (Game.GameId.StartsWith("CONSOLE"))
            {
                throw new Exception("We can't install console only games, the technology is not there yet.");
            }

            ProcessStarter.StartUrl($"ms-windows-store://pdp/?PFN={Game.GameId}");
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            if (Game.GameId.StartsWith("CONSOLE"))
            {
                throw new Exception("We can't uninstall console only games, the technology is not there yet.");
            }

            ProcessStarter.StartUrl("ms-settings:appsfeatures");
            StartUninstallWatcher();
        }

        public override void Play()
        {
            if (Game.GameId.StartsWith("CONSOLE"))
            {
                throw new Exception("We can't start console only games, the technology is not there yet.");
            }

            var prg = Programs.GetUWPApps().FirstOrDefault(a => a.AppId == Game.GameId);
            if (prg == null)
            {
                throw new Exception("Cannot start UWP game, installation not found.");
            }

            ProcessStarter.StartProcess(prg.Path, prg.Arguments);
            stopWatch = Stopwatch.StartNew();
            procMon = new ProcessMonitor();
            procMon.TreeDestroyed += Monitor_TreeDestroyed;
            procMon.TreeStarted += ProcMon_TreeStarted;

            // TODO switch to WatchUwpApp once we are building as 64bit app
            //procMon.WatchUwpApp(uwpMatch.Groups[1].Value, false);
            if (Directory.Exists(prg.WorkDir) && ProcessMonitor.IsWatchableByProcessNames(prg.WorkDir))
            {
                procMon.WatchDirectoryProcesses(prg.WorkDir, false, true);
            }
            else
            {
                OnStopped(this, new GameControllerEventArgs(this, 0));
            }
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            procMon?.Dispose();
            watcherToken?.Cancel();
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

                var app = Programs.GetUWPApps().FirstOrDefault(a => a.AppId == Game.GameId);
                if (app != null)
                {
                    var installInfo = new GameInfo
                    {
                        InstallDirectory = app.WorkDir,
                        PlayAction = new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = app.Path,
                            Arguments = app.Arguments,
                            IsHandledByPlugin = true
                        }
                    };

                    OnInstalled(this, new GameInstalledEventArgs(installInfo, this, 0));
                    return;
                };

                await Task.Delay(10000);
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

                var app = Programs.GetUWPApps().FirstOrDefault(a => a.AppId == Game.GameId);
                if (app == null)
                {
                    OnUninstalled(this, new GameControllerEventArgs(this, 0));
                    return;
                }

                await Task.Delay(10000);
            }
        }

        private void ProcMon_TreeStarted(object sender, EventArgs e)
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
