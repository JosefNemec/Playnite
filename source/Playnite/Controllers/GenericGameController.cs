using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Controllers
{
    public class GenericGameController : BaseGameController
    {
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;
        private GameDatabase database;
        private ILogger logger = LogManager.GetLogger();

        public GenericGameController(GameDatabase db, Game game) : base(game)
        {
            database = db;
        }

        public override void Play()
        {
            if (Game.PlayAction == null)
            {
                throw new Exception("Cannot start game without play action.");
            }

            //Solving Issue #1065 if removable drives are used for storing ROMs/Images
            CheckGameImagePath();

            var playAction = Game.PlayAction.ExpandVariables(Game);
            //Solving Issue #1065 if removable drives are used for manually added games
            CheckGameActionPath(playAction);

            Dispose();
            var emulators = database.Emulators.ToList();
            var profile = GameActionActivator.GetGameActionEmulatorConfig(playAction, emulators)?.ExpandVariables(Game);
            if (profile != null)
            {
                //Solving Issue #1065 if removable drives are used for emulators
                CheckEmulatorConfigExecutable(profile);
            }
            OnStarting(this, new GameControllerEventArgs(this, 0));
            var proc = GameActionActivator.ActivateAction(playAction, profile);

            if (playAction.Type != GameActionType.URL)
            {
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeDestroyed += Monitor_TreeDestroyed;

                // Handle Windows store apps
                var uwpMatch = Regex.Match(playAction.Arguments ?? string.Empty, @"shell:AppsFolder\\(.+)!.+");
                if (playAction.Path == "explorer.exe" && uwpMatch.Success)
                {
                    var scanDirectory = Game.InstallDirectory;
                    procMon.TreeStarted += ProcMon_TreeStarted;

                    if (!Game.GameId.IsNullOrEmpty())
                    {
                        var prg = Programs.GetUWPApps().FirstOrDefault(a => a.AppId == Game.GameId);
                        if (prg != null)
                        {
                            scanDirectory = prg.WorkDir;
                        }
                    }

                    // TODO switch to WatchUwpApp once we are building as 64bit app
                    //procMon.WatchUwpApp(uwpMatch.Groups[1].Value, false);
                    if (Directory.Exists(scanDirectory) && ProcessMonitor.IsWatchableByProcessNames(scanDirectory))
                    {
                        procMon.WatchDirectoryProcesses(scanDirectory, false, true);
                    }
                    else
                    {
                        OnStopped(this, new GameControllerEventArgs(this, 0));
                    }
                }
                else
                {
                    if (proc != null)
                    {
                        OnStarted(this, new GameControllerEventArgs(this, 0));
                        procMon.WatchProcessTree(proc);
                    }
                    else
                    {
                        OnStopped(this, new GameControllerEventArgs(this, 0));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(Game.InstallDirectory) && Directory.Exists(Game.InstallDirectory))
                {
                    OnStarted(this, new GameControllerEventArgs(this, 0));
                    stopWatch = Stopwatch.StartNew();
                    procMon = new ProcessMonitor();
                    procMon.TreeDestroyed += Monitor_TreeDestroyed;
                    procMon.WatchDirectoryProcesses(Game.InstallDirectory, false);
                }
                else
                {
                    OnStopped(this, new GameControllerEventArgs(this, 0));
                }
            }
        }

        public override void Install()
        {
        }

        public override void Uninstall()
        {
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

        private void ProcMon_TreeStarted(object sender, EventArgs e)
        {
            OnStarted(this, new GameControllerEventArgs(this, 0));
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            OnStopped(this, new GameControllerEventArgs(this, stopWatch.Elapsed.TotalSeconds));
        }

        private void CheckGameImagePath()
        {
            if (Game.PlayAction.Type == GameActionType.Emulator)
            {
                var gameImagePath = Game.GameImagePath;
                if (!FileSystem.CheckDrivesForValidFilePath(ref gameImagePath, false))
                {
                    throw new FileNotFoundException($"File \"{gameImagePath}\" does not exist", gameImagePath);
                }
                else if (!Game.GameImagePath.Equals(gameImagePath, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"ROM/Image \"{Game.GameImagePath}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to {gameImagePath}");
                    Game.GameImagePath = gameImagePath;
                }
            }
        }

        private void CheckGameActionPath(GameAction gameAction)
        {
            if (Game.PlayAction.Type == GameActionType.File)
            {
                var gameActionPath = gameAction.Path;
                if (!FileSystem.CheckDrivesForValidFilePath(ref gameActionPath, false))
                {
                    throw new FileNotFoundException($"File \"{gameActionPath}\" does not exist", gameActionPath);
                }
                else if (!gameAction.Path.Equals(gameActionPath, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Path \"{gameAction.Path}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to {gameActionPath}");
                    gameAction.Path = gameActionPath;
                }
            }
        }

        private void CheckEmulatorConfigExecutable(EmulatorProfile emulatorProfile)
        {
            if (Game.PlayAction.Type == GameActionType.Emulator)
            {
                var configExecutable = emulatorProfile.Executable;
                if (!FileSystem.CheckDrivesForValidFilePath(ref configExecutable, false))
                {
                    throw new FileNotFoundException($"Emulator \"{configExecutable}\" does not exist", configExecutable);
                }
                else if (!emulatorProfile.Executable.Equals(configExecutable, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn($"Emulator \"{configExecutable}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to {configExecutable}");
                    emulatorProfile.Executable = configExecutable;
                }
            }
        }
    }
}
