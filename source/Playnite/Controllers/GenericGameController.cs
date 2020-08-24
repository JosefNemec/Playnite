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
        private static ILogger logger = LogManager.GetLogger();

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

            var gameClone = Game.GetClone();
            var playAction = gameClone.PlayAction;
            var emulators = database.Emulators.ToList();
            var profileClone = GameActionActivator.GetGameActionEmulatorConfig(playAction, emulators)?.GetClone();

            CheckGameImagePath(gameClone);
            CheckGameAction(playAction);
            if (playAction.Type == GameActionType.Emulator && profileClone != null)
            {
                CheckEmulatorConfig(profileClone);
            }

            playAction = gameClone.PlayAction.ExpandVariables(gameClone);
            profileClone = profileClone?.ExpandVariables(gameClone);

            Dispose();

            OnStarting(this, new GameControllerEventArgs(this, 0));
            var proc = GameActionActivator.ActivateAction(playAction, profileClone);

            if (playAction.Type != GameActionType.URL)
            {
                stopWatch = Stopwatch.StartNew();
                procMon = new ProcessMonitor();
                procMon.TreeDestroyed += Monitor_TreeDestroyed;

                // Handle Windows store apps
                var uwpMatch = Regex.Match(playAction.Arguments ?? string.Empty, @"shell:AppsFolder\\(.+)!.+");
                if (playAction.Path == "explorer.exe" && uwpMatch.Success)
                {
                    var scanDirectory = gameClone.InstallDirectory;
                    procMon.TreeStarted += ProcMon_TreeStarted;

                    if (!gameClone.GameId.IsNullOrEmpty())
                    {
                        var prg = Programs.GetUWPApps().FirstOrDefault(a => a.AppId == gameClone.GameId);
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
                if (!string.IsNullOrEmpty(gameClone.InstallDirectory) && Directory.Exists(gameClone.InstallDirectory))
                {
                    OnStarted(this, new GameControllerEventArgs(this, 0));
                    stopWatch = Stopwatch.StartNew();
                    procMon = new ProcessMonitor();
                    procMon.TreeDestroyed += Monitor_TreeDestroyed;
                    procMon.WatchDirectoryProcesses(gameClone.InstallDirectory, false);
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

        private void CheckGameImagePath(Game game)
        {
            if (!string.IsNullOrWhiteSpace(game.GameImagePath) && !File.Exists(game.GameImagePath))
            {
                var gameImagePath = FileSystem.LookupAlternativeFilePath(game.GameImagePath);

                if (!string.IsNullOrWhiteSpace(gameImagePath))
                {
                    logger.Warn($"ROM/Image \"{Game.GameImagePath}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to \"{gameImagePath}\"");
                    game.GameImagePath = gameImagePath;
                }
            }
        }

        private void CheckGameAction(GameAction gameAction)
        {
            if (!string.IsNullOrWhiteSpace(gameAction.Path) && !File.Exists(gameAction.Path))
            {
                var gameActionPath = FileSystem.LookupAlternativeFilePath(gameAction.Path);
                if (!string.IsNullOrWhiteSpace(gameActionPath))
                {
                    logger.Warn($"Path \"{gameAction.Path}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to \"{gameActionPath}\"");
                    gameAction.Path = gameActionPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(gameAction.WorkingDir) && !Directory.Exists(gameAction.WorkingDir))
            {
                var workingDir = FileSystem.LookupAlternativeDirectoryPath(gameAction.WorkingDir);
                if (!string.IsNullOrWhiteSpace(workingDir))
                {
                    logger.Warn($"WorkingDir \"{gameAction.WorkingDir}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to \"{workingDir}\"");
                    gameAction.WorkingDir = workingDir;
                }
            }
        }

        private void CheckEmulatorConfig(EmulatorProfile emulatorProfile)
        {
            if (!File.Exists(emulatorProfile.Executable))
            {
                var configExecutable = FileSystem.LookupAlternativeFilePath(emulatorProfile.Executable);
                if (!string.IsNullOrWhiteSpace(configExecutable))
                {
                    logger.Warn($"Emulator \"{configExecutable}\" does not exist for game \"{Game.Name}\"" +
                        $" and is temporarily changed to \"{configExecutable}\"");
                    emulatorProfile.Executable = configExecutable;
                }
            }

            if (!string.IsNullOrWhiteSpace(emulatorProfile.WorkingDirectory) && !Directory.Exists(emulatorProfile.WorkingDirectory))
            {
                var workingDir = FileSystem.LookupAlternativeDirectoryPath(emulatorProfile.WorkingDirectory);
                if (!string.IsNullOrWhiteSpace(workingDir))
                {
                    logger.Warn($"WorkingDir \"{emulatorProfile.WorkingDirectory}\" does not exist for emulator \"{emulatorProfile.Name}\"" +
                        $" and is temporarily changed to \"{workingDir}\"");
                    emulatorProfile.WorkingDirectory = workingDir;
                }
            }
        }
    }
}
