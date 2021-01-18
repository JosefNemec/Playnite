using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
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
    public class GenericPlayController : PlayController
    {
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;
        private GameDatabase database;
        private static ILogger logger = LogManager.GetLogger();

        public GenericPlayController(GameDatabase db, Game game)
        {
            database = db;
            Game = game;
        }

        public override void Play(PlayAction playAction)
        {
            throw new NotSupportedException("This shouldn't be called.");
        }

        public void PlayCustom(GenericPlayAction playAction)
        {
            PlayCustom(new GameAction
            {
                Type = playAction.Type == GenericPlayActionType.Url ? GameActionType.URL : GameActionType.File,
                Arguments = playAction.Arguments,
                Path = playAction.Path,
                WorkingDir = playAction.WorkingDir,
                TrackingMode = playAction.TrackingMode,
                TrackingPath = playAction.TrackingPath
            });
        }

        public void PlayCustom(GameAction playAction)
        {
            if (playAction == null)
            {
                throw new Exception("Cannot start game without play action.");
            }

            var gameClone = Game.GetClone();
            var action = playAction.GetClone();
            var emulators = database.Emulators.ToList();
            var profileClone = GameActionActivator.GetGameActionEmulatorConfig(action, emulators)?.GetClone();

            CheckGameImagePath(gameClone);
            CheckGameAction(action);
            if (action.Type == GameActionType.Emulator && profileClone != null)
            {
                CheckEmulatorConfig(profileClone);
            }

            action = action.ExpandVariables(gameClone);
            profileClone = profileClone?.ExpandVariables(gameClone);

            Dispose();

            InvokeOnStarting(this, new GameStartingEventArgs(this));
            var proc = GameActionActivator.ActivateAction(action, profileClone);
            procMon = new ProcessMonitor();
            procMon.TreeStarted += ProcMon_TreeStarted;
            procMon.TreeDestroyed += Monitor_TreeDestroyed;

            if (action.TrackingMode == TrackingMode.Default)
            {
                if (action.Type != GameActionType.URL)
                {
                    stopWatch = Stopwatch.StartNew();

                    // Handle Windows store apps
                    var uwpMatch = Regex.Match(action.Arguments ?? string.Empty, @"shell:AppsFolder\\(.+)!.+");
                    if (action.Path == "explorer.exe" && uwpMatch.Success)
                    {
                        var scanDirectory = gameClone.InstallDirectory;

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
                            InvokeOnStopped(this, new GameStoppedEventArgs(this));
                        }
                    }
                    else
                    {
                        if (proc != null)
                        {
                            InvokeOnStarted(this, new GameStartedEventArgs(this));
                            procMon.WatchProcessTree(proc);
                        }
                        else
                        {
                            InvokeOnStopped(this, new GameStoppedEventArgs(this));
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(gameClone.InstallDirectory) && Directory.Exists(gameClone.InstallDirectory))
                    {
                        InvokeOnStarted(this, new GameStartedEventArgs(this));
                        stopWatch = Stopwatch.StartNew();
                        procMon.WatchDirectoryProcesses(gameClone.InstallDirectory, false);
                    }
                    else
                    {
                        InvokeOnStopped(this, new GameStoppedEventArgs(this));
                    }
                }
            }
            else if (action.TrackingMode == TrackingMode.Process)
            {
                if (proc != null)
                {
                    InvokeOnStarted(this, new GameStartedEventArgs(this));
                    stopWatch = Stopwatch.StartNew();
                    procMon.WatchProcessTree(proc);
                }
                else
                {
                    InvokeOnStopped(this, new GameStoppedEventArgs(this));
                }
            }
            else if (action.TrackingMode == TrackingMode.Directory)
            {
                var watchDir = action.TrackingPath.IsNullOrEmpty() ? gameClone.InstallDirectory : action.TrackingPath;
                if (!watchDir.IsNullOrEmpty() && Directory.Exists(watchDir))
                {
                    stopWatch = Stopwatch.StartNew();
                    procMon.WatchDirectoryProcesses(watchDir, false);
                }
                else
                {
                    InvokeOnStopped(this, new GameStoppedEventArgs(this));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override void Dispose()
        {
            ReleaseResources();
        }

        public void ReleaseResources()
        {
            watcherToken?.Cancel();
            procMon?.Dispose();
            stopWatch?.Stop();
        }

        private void ProcMon_TreeStarted(object sender, EventArgs e)
        {
            InvokeOnStarted(this, new GameStartedEventArgs(this));
        }

        private void Monitor_TreeDestroyed(object sender, EventArgs args)
        {
            stopWatch.Stop();
            InvokeOnStopped(this, new GameStoppedEventArgs(this) { SessionLength = Convert.ToInt64(stopWatch.Elapsed.TotalSeconds) });
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
