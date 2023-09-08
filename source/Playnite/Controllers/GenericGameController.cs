using Playnite.Common;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Exceptions;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Playnite.Controllers
{
    public class EmulationPlayAction : GameAction
    {
        public EmulatorProfile SelectedEmulatorProfile { get; set; }
        public string SelectedRomPath { get; set; }
    }

    public class GenericPlayController : PlayController
    {
        protected CancellationTokenSource watcherToken;
        protected Stopwatch stopWatch;
        protected ProcessMonitor procMon;
        private GameDatabase database;
        private static ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private IPowerShellRuntime scriptRuntime;
        private PowerShellRuntime playRuntime;
        private Task playTask;
        private bool isDisposed = false;
        private EmulatorProfile currentEmuProfile;

        internal OnGameStartingEventArgs StartingArgs { get; private set; }

        // These are stored for emulator scripts because they can be executed in non-linear fasion
        private string startedRomFile;
        private Emulator startedEmulator;
        private EmulatorProfile startedEmulatorProfile;
        private string startedEmulatorDir;
        private int startedEmuProcessId;

        public GenericPlayController(
            GameDatabase db,
            Game game,
            IPowerShellRuntime scriptRuntime,
            IPlayniteAPI playniteApi) : base(game)
        {
            execContext = SynchronizationContext.Current;
            database = db;
            this.scriptRuntime = scriptRuntime;
            this.playniteApi = playniteApi;
        }

        public override void Play(PlayActionArgs args)
        {
            throw new NotSupportedException("This shouldn't be called.");
        }

        public void StartEmulator(EmulationPlayAction action, bool asyncExec, OnGameStartingEventArgs startingArgs)
        {
            var emulator = database.Emulators[action.EmulatorId];
            if (emulator == null)
            {
                throw new Exception("Emulator not found.");
            }

            currentEmuProfile = null;
            if (action.SelectedEmulatorProfile is CustomEmulatorProfile customProfile)
            {
                currentEmuProfile = customProfile;
            }
            else if (action.SelectedEmulatorProfile is BuiltInEmulatorProfile builtinProfile)
            {
                currentEmuProfile = builtinProfile;
            }
            else
            {
                throw new Exception("Uknown play action configuration.");
            }

            emulator = emulator.GetClone();
            if (!emulator.InstallDir.IsNullOrEmpty())
            {
                emulator.InstallDir = Paths.FixSeparators(emulator.InstallDir.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath));
                emulator.InstallDir = CheckPath(emulator.InstallDir, nameof(emulator.InstallDir), FileSystemItem.Directory);
                emulator.InstallDir = emulator.InstallDir.TrimEnd(Path.DirectorySeparatorChar);
            }

            StartingArgs = startingArgs;

            var startupPath = "";
            var startupArgs = "";
            var startupDir = "";
            var romPath = Game.ExpandVariables(action.SelectedRomPath, true, emulator.InstallDir, null);
            // This is later passed to an emulator so it's up to it how it processes long paths.
            romPath = Paths.TrimLongPathPrefix(CheckPath(romPath, "ROM", FileSystemItem.File));

            if (currentEmuProfile is CustomEmulatorProfile emuProf)
            {
                var expandedProfile = emuProf.ExpandVariables(Game, emulator.InstallDir, romPath);
                expandedProfile.Executable = CheckPath(expandedProfile.Executable, nameof(expandedProfile.Executable), FileSystemItem.File);
                expandedProfile.WorkingDirectory = CheckPath(expandedProfile.WorkingDirectory, nameof(expandedProfile.WorkingDirectory), FileSystemItem.Directory);

                if (!emuProf.StartupScript.IsNullOrWhiteSpace())
                {
                    RunStartScript(
                        $"{emulator.Name} runtime for {Game.Name}",
                        emuProf.StartupScript,
                        emulator.InstallDir,
                        new Dictionary<string, object>
                        {
                            { "Emulator", emulator.GetClone() },
                            { "EmulatorProfile", expandedProfile.GetClone() },
                            { "RomPath", romPath }
                        },
                        asyncExec);
                }
                else
                {
                    if (action.OverrideDefaultArgs)
                    {
                        startupArgs = Game.ExpandVariables(action.Arguments, false, emulator.InstallDir, romPath);
                    }
                    else
                    {
                        startupArgs = expandedProfile.Arguments;
                        if (!action.AdditionalArguments.IsNullOrEmpty())
                        {
                            startupArgs += " " + Game.ExpandVariables(action.AdditionalArguments, false, emulator.InstallDir, romPath);
                        }
                    }

                    startupDir = expandedProfile.WorkingDirectory;
                    startupPath = expandedProfile.Executable;
                    StartEmulatorProcess(startupPath, startupArgs, startupDir, emulator.InstallDir, romPath, asyncExec, emulator.GetClone(), expandedProfile.GetClone(), expandedProfile.TrackingMode, expandedProfile.TrackingPath);
                }
            }
            else if (currentEmuProfile is BuiltInEmulatorProfile builtIn)
            {
                var profileDef = Emulation.GetProfile(emulator.BuiltInConfigId, builtIn.BuiltInProfileName);
                if (profileDef == null)
                {
                    throw new Exception($"Can't find built-in {builtIn.BuiltInProfileName} emulator profile.");
                }

                if (profileDef.ScriptStartup)
                {
                    var def = Emulation.GetDefition(emulator.BuiltInConfigId);
                    if (def == null || !FileSystem.FileExists(Emulation.GetStartupScriptPath(def)))
                    {
                        throw new FileNotFoundException(ResourceProvider.GetString(LOC.ErrorEmulatorStartupScriptNotFound));
                    }

                    RunStartScriptFile(
                        $"{emulator.Name} runtime for {Game.Name}",
                        Emulation.GetStartupScriptPath(def),
                        emulator.InstallDir,
                        new Dictionary<string, object>
                        {
                            { "Emulator", emulator.GetClone() },
                            { "EmulatorProfile", profileDef.GetClone() },
                            { "RomPath", romPath }
                        },
                        asyncExec);
                }
                else
                {
                    builtIn = builtIn.GetClone();
                    startupDir = emulator.InstallDir;
                    startupPath = Emulation.GetExecutable(emulator.InstallDir, profileDef, true);
                    if (startupPath.IsNullOrEmpty())
                    {
                        throw new FileNotFoundException(ResourceProvider.GetString(LOC.ErrorEmulatorExecutableNotFound));
                    }

                    if (action.OverrideDefaultArgs)
                    {
                        startupArgs = Game.ExpandVariables(action.Arguments, false, emulator.InstallDir, romPath);
                    }
                    else
                    {
                        if (builtIn.OverrideDefaultArgs)
                        {
                            startupArgs = Game.ExpandVariables(builtIn.CustomArguments, false, emulator.InstallDir, romPath);
                        }
                        else
                        {
                            startupArgs = Game.ExpandVariables(profileDef.StartupArguments, false, emulator.InstallDir, romPath);
                        }

                        if (!action.AdditionalArguments.IsNullOrEmpty())
                        {
                            startupArgs += " " + Game.ExpandVariables(action.AdditionalArguments, false, emulator.InstallDir, romPath);
                        }
                    }

                    StartEmulatorProcess(startupPath, startupArgs, startupDir, emulator.InstallDir, romPath, asyncExec, emulator.GetClone(), builtIn.GetClone(), TrackingMode.Process);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void StartEmulatorProcess(
            string path,
            string args,
            string workDir,
            string emulatorDir,
            string romPath,
            bool asyncExec,
            Emulator emulator,
            EmulatorProfile emuProfile,
            TrackingMode trackingMode,
            string trackingPath = null)
        {
            startedRomFile = romPath;
            startedEmulator = emulator;
            startedEmulatorProfile = emuProfile;
            startedEmulatorDir = emulatorDir;

            if (asyncExec)
            {
                ExecuteEmulatorScript(currentEmuProfile.PreScript, emulatorDir, romPath, emulator, emuProfile);
                Process process = null;
                try
                {
                    process = ProcessStarter.StartProcess(path, args, workDir);
                }
                catch (Win32Exception exc)
                {
                    // 2 is ERROR_FILE_NOT_FOUND
                    if (exc.NativeErrorCode == 2)
                    {
                        throw new FileNotFoundException(LOC.ErrorEmulatorExecutableNotFound.GetLocalized());
                    }
                    else
                    {
                        throw;
                    }
                }

                void gameStarted(int processId)
                {
                    startedEmuProcessId = processId;
                    stopWatch = Stopwatch.StartNew();
                    ExecuteEmulatorScript(currentEmuProfile.PostScript, emulatorDir, romPath, emulator, emuProfile);
                    InvokeOnStarted(new GameStartedEventArgs { StartedProcessId = processId });
                }

                procMon = new ProcessMonitor();
                procMon.TreeStarted += (_, treeArgs) => gameStarted(treeArgs.StartedId);
                procMon.TreeDestroyed += (_, __) =>
                {
                    stopWatch.Stop();
                    ExecuteEmulatorScript(currentEmuProfile?.ExitScript, startedEmulatorDir, startedRomFile, startedEmulator, startedEmulatorProfile);
                    InvokeOnStopped(new GameStoppedEventArgs { SessionLength = Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds) });
                };

                if (trackingMode == TrackingMode.Default || trackingMode == TrackingMode.Process)
                {
                    gameStarted(process.Id);
                    procMon.WatchProcessTree(process);
                }
                else if (trackingMode == TrackingMode.OriginalProcess)
                {
                    gameStarted(process.Id);
                    procMon.WatchSingleProcess(process);
                }
                else if (trackingMode == TrackingMode.Directory)
                {
                    var watchDir = trackingPath.IsNullOrEmpty() ? emulatorDir : trackingPath;
                    if (!watchDir.IsNullOrEmpty() && FileSystem.DirectoryExists(watchDir))
                    {
                        stopWatch = Stopwatch.StartNew();
                        procMon.WatchDirectoryProcesses(watchDir, false);
                    }
                    else
                    {
                        InvokeOnStopped(new GameStoppedEventArgs());
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                ExecuteEmulatorScript(currentEmuProfile.PreScript, emulatorDir, romPath, emulator, emuProfile);
                ProcessStarter.StartProcess(path, args, workDir);
                ExecuteEmulatorScript(currentEmuProfile.PostScript, emulatorDir, romPath, emulator, emuProfile);
                ExecuteEmulatorScript(currentEmuProfile.ExitScript, emulatorDir, romPath, emulator, emuProfile);
            }
        }

        private void ExecuteEmulatorScript(string script, string emulatorDir, string romPath, Emulator emulator, EmulatorProfile emuProfile)
        {
            if (script.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                var scriptVars = new Dictionary<string, object>
                {
                    {  "PlayniteApi", playniteApi },
                    {  "Game", Game.GetClone() },
                    {  "StartingArgs", StartingArgs },
                    {  "SourceAction", StartingArgs.SourceAction },
                    {  "SelectedRomFile", romPath },
                    {  "Emulator", emulator },
                    {  "EmulatorProfile", emuProfile },
                    {  "StartedProcessId", startedEmuProcessId }
                };

                var expandedScript = Game.ExpandVariables(script, false, emulatorDir, romPath);
                var dir = Game.ExpandVariables(Game.InstallDirectory, true);
                if (!dir.IsNullOrEmpty() && FileSystem.DirectoryExists(dir))
                {
                    scriptRuntime.Execute(expandedScript, dir, scriptVars);
                }
                else
                {
                    scriptRuntime.Execute(expandedScript, variables: scriptVars);
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Emulator script failed.");
                logger.Debug(script);
                Dialogs.ShowMessage(
                    exc.Message,
                    LOC.ErrorEmulatorScriptAction,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RunScript(string runtimeName, Action<PowerShellRuntime> startAction, Dictionary<string, object> variables, bool asyncExec = true)
        {
            variables.Add("Game", Game.GetClone());
            variables.Add("PlayniteApi", playniteApi);
            variables.Add("IsPlayAction", asyncExec);
            playRuntime = new PowerShellRuntime(runtimeName);

            if (asyncExec)
            {
                watcherToken = new CancellationTokenSource();
                variables.Add("CancelToken", watcherToken.Token);
                stopWatch = Stopwatch.StartNew();
                playTask = Task.Run(() =>
                {
                    try
                    {
                        startAction(playRuntime);

                        if (!isDisposed) // Should not be called when we reached this from cancel state.
                        {
                            execContext.Post((_) => InvokeOnStopped(new GameStoppedEventArgs() { SessionLength = Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds) }), null);
                        }
                    }
                    catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(exc, "Play script failed.");
                        execContext.Post((_) =>
                        {
                            InvokeOnStopped(new GameStoppedEventArgs() { SessionLength = Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds) });
                            Dialogs.ShowMessage(
                                exc.Message,
                                LOC.ErrorPlayScriptAction,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }, null);
                    }
                    finally
                    {
                        if (!playRuntime.IsDisposed)
                        {
                            playRuntime?.Dispose();
                        }
                    }
                });

                InvokeOnStarted(new GameStartedEventArgs());
            }
            else
            {
                try
                {
                    startAction(playRuntime);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Play script failed.");
                    InvokeOnStopped(new GameStoppedEventArgs() { SessionLength = Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds) });
                    Dialogs.ShowMessage(
                        exc.Message,
                        LOC.ErrorPlayScriptAction,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    if (!playRuntime.IsDisposed)
                    {
                        playRuntime?.Dispose();
                    }
                }
            }
        }

        private void RunStartScriptFile(string runtimeName, string scriptPath, string workDir, Dictionary<string, object> variables, bool asyncExec = true)
        {
            logger.Debug($"Starting game using script file {scriptPath}");
            RunScript(runtimeName, r => r.ExecuteFile(scriptPath, workDir, variables), variables, asyncExec);
        }

        private void RunStartScript(string runtimeName, string script, string workDir, Dictionary<string, object> variables, bool asyncExec = true)
        {
            logger.Debug($"Starting game using script.");
            logger.Trace(script);
            RunScript(runtimeName, r => r.Execute(script, workDir, variables), variables, asyncExec);
        }

        public void Start(AutomaticPlayController controller)
        {
            var action = new GameAction
            {
                Type = controller.Type == AutomaticPlayActionType.Url ? GameActionType.URL : GameActionType.File,
                Arguments = controller.Arguments,
                Path = controller.Path,
                WorkingDir = controller.WorkingDir,
                TrackingMode = controller.TrackingMode,
                TrackingPath = controller.TrackingPath
            };

            Start(action, true, new OnGameStartingEventArgs
            {
                SourceAction = action,
                Game = Game
            });
        }

        public void Start(GameAction playAction, bool asyncExec, OnGameStartingEventArgs startingArgs)
        {
            if (playAction == null)
            {
                throw new ArgumentNullException("Cannot start game without play action.");
            }

            if (playAction.Type == GameActionType.Emulator)
            {
                throw new Exception("Cannot start emulator using this configuration.");
            }

            StartingArgs = startingArgs;
            var gameClone = Game.GetClone();
            var action = playAction.GetClone();
            action = action.ExpandVariables(gameClone);
            action.Path = CheckPath(action.Path, nameof(action.Path), FileSystemItem.File);
            action.WorkingDir = CheckPath(action.WorkingDir, nameof(action.WorkingDir), FileSystemItem.Directory);

            if (playAction.Type == GameActionType.Script)
            {
                if (action.Script.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException("Game script is not defined.");
                }

                RunStartScript(
                    $"{Game.Name} play script",
                    action.Script,
                    gameClone.ExpandVariables(gameClone.InstallDirectory, true),
                    new Dictionary<string, object>(),
                    asyncExec);
            }
            else
            {
                Process proc;
                if (action.Type == GameActionType.File)
                {
                    proc = ProcessStarter.StartProcess(action.Path, action.Arguments, action.WorkingDir);
                }
                else if (action.Type == GameActionType.URL)
                {
                    proc = ProcessStarter.StartUrl(action.Path);
                }
                else
                {
                    throw new NotSupportedException();
                }

                procMon = new ProcessMonitor();
                procMon.TreeStarted += (_, treeArgs) => InvokeOnStarted(new GameStartedEventArgs { StartedProcessId = treeArgs.StartedId });
                procMon.TreeDestroyed += (_, __) =>
                {
                    stopWatch.Stop();
                    InvokeOnStopped(new GameStoppedEventArgs { SessionLength = Convert.ToUInt64(stopWatch.Elapsed.TotalSeconds) });
                };

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
                            if (FileSystem.DirectoryExists(scanDirectory) && ProcessMonitor.IsWatchableByProcessNames(scanDirectory))
                            {
                                procMon.WatchDirectoryProcesses(scanDirectory, false, true);
                            }
                            else
                            {
                                InvokeOnStopped(new GameStoppedEventArgs());
                            }
                        }
                        else
                        {
                            if (proc != null)
                            {
                                InvokeOnStarted(new GameStartedEventArgs() { StartedProcessId = proc.Id });
                                procMon.WatchProcessTree(proc);
                            }
                            else
                            {
                                InvokeOnStopped(new GameStoppedEventArgs());
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(gameClone.InstallDirectory) && FileSystem.DirectoryExists(gameClone.InstallDirectory))
                        {
                            stopWatch = Stopwatch.StartNew();
                            procMon.WatchDirectoryProcesses(gameClone.InstallDirectory, false);
                        }
                        else
                        {
                            InvokeOnStopped(new GameStoppedEventArgs());
                        }
                    }
                }
                else if (action.TrackingMode == TrackingMode.Process)
                {
                    if (proc != null)
                    {
                        InvokeOnStarted(new GameStartedEventArgs() { StartedProcessId = proc.Id });
                        stopWatch = Stopwatch.StartNew();
                        procMon.WatchProcessTree(proc);
                    }
                    else
                    {
                        InvokeOnStopped(new GameStoppedEventArgs());
                    }
                }
                else if (action.TrackingMode == TrackingMode.OriginalProcess)
                {
                    if (proc != null)
                    {
                        InvokeOnStarted(new GameStartedEventArgs() { StartedProcessId = proc.Id });
                        stopWatch = Stopwatch.StartNew();
                        procMon.WatchSingleProcess(proc);
                    }
                    else
                    {
                        InvokeOnStopped(new GameStoppedEventArgs());
                    }
                }
                else if (action.TrackingMode == TrackingMode.Directory)
                {
                    var watchDir = action.TrackingPath.IsNullOrEmpty() ? gameClone.InstallDirectory : action.TrackingPath;
                    if (!watchDir.IsNullOrEmpty() && FileSystem.DirectoryExists(watchDir))
                    {
                        stopWatch = Stopwatch.StartNew();
                        procMon.WatchDirectoryProcesses(watchDir, false);
                    }
                    else
                    {
                        InvokeOnStopped(new GameStoppedEventArgs());
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public override void Dispose()
        {
            isDisposed = true;
            watcherToken?.Cancel();
            playTask?.Wait(5000); // Give startup script time to gracefully shutdown.
            procMon?.Dispose();
            stopWatch?.Stop();
            if (playRuntime?.IsDisposed == false)
            {
                playRuntime?.Dispose();
            }

            watcherToken?.Dispose();
            currentEmuProfile = null;
        }

        private string CheckPath(string sourcePath, string changeProp, FileSystemItem pathType)
        {
            if (sourcePath.IsNullOrWhiteSpace())
            {
                return sourcePath;
            }

            var exists = false;
            var newPath = "";
            switch (pathType)
            {
                case FileSystemItem.File:
                    exists = FileSystem.FileExistsOnAnyDrive(sourcePath, out newPath);
                    break;
                case FileSystemItem.Directory:
                    exists = FileSystem.DirectoryExistsOnAnyDrive(sourcePath, out newPath);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (exists && !string.Equals(newPath, sourcePath, StringComparison.OrdinalIgnoreCase))
            {
                logger.Warn($"Replaced missing {changeProp} with new one:\n{{0}}\n{{1}}".Format(sourcePath, newPath));
                return newPath;
            }

            return sourcePath;
        }
    }
}
