﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Playnite;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;

namespace GogLibrary
{
    public class GogGameController : BaseGameController
    {
        private CancellationTokenSource watcherToken;
        private FileSystemWatcher fileWatcher;
        private GogLibrarySettings settings;
        private ProcessMonitor procMon;
        private Stopwatch stopWatch;
        private GogLibrary library;
        private IPlayniteAPI api;

        public GogGameController(Game game, GogLibrary library, GogLibrarySettings settings, IPlayniteAPI api) : base(game)
        {
            this.settings = settings;
            this.library = library;
            this.api = api;
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            fileWatcher?.Dispose();
            procMon?.Dispose();
        }

        public override void Play()
        {
            ReleaseResources();
            if (settings.StartGamesUsingGalaxy == true)
            {
                OnStarting(this, new GameControllerEventArgs(this, 0));
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeStarted += ProcMon_TreeStarted;
                procMon.TreeDestroyed += Monitor_TreeDestroyed;
                var args = string.Format(@"/gameId={0} /command=runGame /path=""{1}""", Game.GameId, Game.InstallDirectory);
                ProcessStarter.StartProcess(Path.Combine(Gog.InstallationPath, "GalaxyClient.exe"), args);
                if (Directory.Exists(Game.InstallDirectory))
                {
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                }
                else
                {
                    OnStopped(this, new GameControllerEventArgs(this, 0));
                }
            }
            else
            {
                if (Game.PlayAction.Type == GameActionType.Emulator)
                {
                    throw new NotSupportedException();
                }

                var playAction = api.ExpandGameVariables(Game, Game.PlayAction);
                OnStarting(this, new GameControllerEventArgs(this, 0));
                var proc = GameActionActivator.ActivateAction(playAction, Game);
                OnStarted(this, new GameControllerEventArgs(this, 0));

                if (Game.PlayAction.Type != GameActionType.URL)
                {
                    stopWatch = Stopwatch.StartNew();
                    procMon = new ProcessMonitor();
                    procMon.TreeDestroyed += Monitor_TreeDestroyed;
                    procMon.WatchProcessTree(proc);
                }
                else
                {
                    OnStopped(this, new GameControllerEventArgs(this, 0));
                }
            }
        }

        public override void Install()
        {
            ReleaseResources();
            ProcessStarter.StartUrl(@"goggalaxy://openGameView/" + Game.GameId);
            StartInstallWatcher();
        }

        public override void Uninstall()
        {
            ReleaseResources();
            var uninstaller = Path.Combine(Game.InstallDirectory, "unins000.exe");
            if (!File.Exists(uninstaller))
            {
                throw new FileNotFoundException("Uninstaller not found.");
            }

            Process.Start(uninstaller);
            var infoFile = string.Format("goggame-{0}.info", Game.GameId);
            if (File.Exists(Path.Combine(Game.InstallDirectory, infoFile)))
            {
                fileWatcher = new FileSystemWatcher()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Path = Game.InstallDirectory,
                    Filter = Path.GetFileName(infoFile)
                };

                fileWatcher.Deleted += FileWatcher_Deleted;
                fileWatcher.EnableRaisingEvents = true;
            }
            else
            {
                OnUninstalled(this, new GameControllerEventArgs(this, 0));
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

        private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            OnUninstalled(this, new GameControllerEventArgs(this, 0));
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();  
            var stopWatch = Stopwatch.StartNew();

            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    return;
                }

                var games = library.GetInstalledGames();
                if (games.ContainsKey(Game.GameId))
                {
                    var game = games[Game.GameId];
                    stopWatch.Stop();
                    Game.PlayAction = game.PlayAction;
                    Game.OtherActions = game.OtherActions;
                    Game.InstallDirectory = game.InstallDirectory;
                    OnInstalled(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
