﻿using Playnite;
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
        private CancellationTokenSource watcherToken;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private readonly IPlayniteAPI api;
        private readonly Game game;

        public EpicGameController(Game game, IPlayniteAPI api) : base(game)
        {
            this.api = api;
            this.game = game;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
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

            EpicLauncher.StartClient();
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            if (!EpicLauncher.IsInstalled)
            {
                throw new Exception("Epic Launcher is not installed.");
            }

            EpicLauncher.StartClient();
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
