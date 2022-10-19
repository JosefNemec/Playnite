using Playnite;
using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.Controllers;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shell;
using Playnite.Scripting;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Playnite.SDK.Exceptions;
using System.Drawing.Imaging;
using Playnite.SDK.Plugins;
using System.Collections.ObjectModel;
using Playnite.Scripting.PowerShell;
using Playnite.Windows;
using System.Windows.Input;

namespace Playnite
{
    public enum GameScriptType
    {
        [Description(LOC.ScriptTypeStarting)]
        Starting,
        [Description(LOC.ScriptTypeStarted)]
        Started,
        [Description(LOC.ScriptTypeExit)]
        Exit,
        [Description("")]
        None
    }

    public interface IActionSelector
    {
        object SelectPlayAction(List<PlayController> controllers, List<GameAction> actions);
        InstallController SelectInstallAction(List<InstallController> pluginActions);
        UninstallController SelectUninstallAction(List<UninstallController> pluginActions);
    }

    public class ClientShutdownJob
    {
        public Guid PluginId { get; set; }
        public CancellationTokenSource CancelToken { get; set; }
        public Task CancelTask { get; set; }
    }

    public class GamesEditor : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private static bool showedPowerShellError = false;
        private IResourceProvider resources = new ResourceProvider();
        private GameControllerFactory controllers;
        private readonly ConcurrentDictionary<Guid, ClientShutdownJob> shutdownJobs = new ConcurrentDictionary<Guid, ClientShutdownJob>();
        private readonly ConcurrentDictionary<Guid, DateTime> gameStartups = new ConcurrentDictionary<Guid, DateTime>();
        private readonly ConcurrentDictionary<Guid, IPowerShellRuntime> scriptRuntimes = new ConcurrentDictionary<Guid, IPowerShellRuntime>();
        private readonly IActionSelector actionSelector;

        public PlayniteApplication Application;

        public ExtensionFactory Extensions { get; private set; }
        public GameDatabase Database { get; private set; }
        public IDialogsFactory Dialogs { get; private set; }
        public PlayniteSettings AppSettings { get; private set; }
        public List<Guid> RunningGames { get; } = new List<Guid>();

        public List<Game> QuickLaunchItems
        {
            get
            {
                if (AppSettings.QuickLaunchItems > 0)
                {
                    return Database.Games.
                        Where(a => a.LastActivity != null && a.IsInstalled &&
                            (!a.Hidden || (a.Hidden && AppSettings.ShowHiddenInQuickLaunch))).
                        OrderByDescending(a => a.LastActivity).
                        Take(AppSettings.QuickLaunchItems).
                        ToList();
                }
                else
                {
                    return new List<Game>();
                }
            }
        }

        public GamesEditor(
            GameDatabase database,
            GameControllerFactory controllerFactory,
            PlayniteSettings appSettings,
            IDialogsFactory dialogs,
            ExtensionFactory extensions,
            PlayniteApplication app,
            IActionSelector actionSelector)
        {
            this.Dialogs = dialogs;
            this.Database = database;
            this.AppSettings = appSettings;
            this.Extensions = extensions;
            this.Application = app;
            this.actionSelector = actionSelector;
            controllers = controllerFactory;
            controllers.Installed += Controllers_Installed;
            controllers.Uninstalled += Controllers_Uninstalled;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.ShowHiddenInQuickLaunch) ||
                e.PropertyName == nameof(PlayniteSettings.QuickLaunchItems))
            {
                UpdateJumpList();
            }
        }

        public void Dispose()
        {
            foreach (var controller in controllers.PlayControllers)
            {
                UpdateGameState(controller.Game.Id, null, false, false, false, false);
            }

            foreach (var controller in controllers.InstallControllers)
            {
                UpdateGameState(controller.Game.Id, null, false, false, false, false);
            }

            controllers.Installed -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Uninstalled;
            controllers.Started -= Controllers_Started;
            controllers.Stopped -= Controllers_Stopped;
        }

        public void StartContextAction(Game game)
        {
            if (game.IsInstalled)
            {
                PlayGame(game);
            }
            else
            {
                InstallGame(game);
            }
        }

        public void PlayGame(Game game)
        {
            if (!game.IsInstalled)
            {
                InstallGame(game);
                return;
            }

            logger.Info($"Starting {game.GetIdentifierInfo()}");
            var dbGame = Database.Games.Get(game.Id);
            if (dbGame == null)
            {
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameStartErrorNoGame"), game.Name),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateJumpList();
                return;
            }

            PlayController controller = null;

            try
            {
                if (game.IsRunning || game.IsLaunching)
                {
                    logger.Warn("Failed to start the game, game is already running/launching.");
                    return;
                }

                var gameActions = GetPlayActions(game);
                if (!gameActions.Item1.HasItems() && !gameActions.Item2.HasItems())
                {
                    Dialogs.ShowErrorMessage(LOC.ErrorNoPlayAction, LOC.GameError);
                    return;
                }

                object playAction = null;
                if ((gameActions.Item1.Count + gameActions.Item2.Count) > 1)
                {
                    playAction = actionSelector.SelectPlayAction(gameActions.Item1, gameActions.Item2);
                }
                else
                {
                    if (gameActions.Item1.Count > 0)
                    {
                        playAction = gameActions.Item1[0];
                    }
                    else
                    {
                        playAction = gameActions.Item2[0];
                    }
                }

                if (playAction == null)
                {
                    return;
                }

                try
                {
                    scriptRuntimes.TryAdd(game.Id, new PowerShellRuntime($"{game.Name} {game.Id} runtime"));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    // This should really only happen on Windows 7 without PS 5.1 installed, which is very small percentage of users.
                    // It should not prevent game startup.
                    logger.Error(e, "Failed to create PowerShell runtime.");
                    if (!showedPowerShellError)
                    {
                        Dialogs.ShowErrorMessage(resources.GetString(LOC.PowerShellCreationError) + "\n\n" + e.Message, "");
                        showedPowerShellError = true;
                    }

                    scriptRuntimes.TryAdd(game.Id, new DummyPowerShellRuntime());
                }

                if (playAction is AutomaticPlayController)
                {
                    logger.Debug("Using automatic plugin controller to start a game.");
                    controller = new GenericPlayController(Database, game, scriptRuntimes[game.Id], Application.PlayniteApiGlobal);
                }
                else if (playAction is PlayController plugAction)
                {
                    logger.Debug("Using plugin to start a game.");
                    controller = plugAction;
                }
                else if (playAction is EmulationPlayAction)
                {
                    logger.Debug("Using generic controller to start emulated game.");
                    controller = new GenericPlayController(Database, game, scriptRuntimes[game.Id], Application.PlayniteApiGlobal);
                }
                else if (playAction is GameAction gameAction)
                {
                    logger.Debug("Using generic controller start a game.");
                    controller = new GenericPlayController(Database, game, scriptRuntimes[game.Id], Application.PlayniteApiGlobal);
                }
                else
                {
                    logger.Error($"Uknown controller found to start a game: {controller.GetType()}");
                }

                foreach (var item in gameActions.Item1)
                {
                    if (item != controller)
                    {
                        item.Dispose();
                    }
                }

                if (controller == null)
                {
                    scriptRuntimes.TryRemove(game.Id, out _);
                    Dialogs.ShowErrorMessage(LOC.ErrorStartupNoController, LOC.StartupError);
                    return;
                }

                void cancelStartup(string message)
                {
                    logger.Warn(message);
                    controllers.RemovePlayController(game.Id);
                    UpdateGameState(game.Id, null, null, null, null, false);
                }

                controllers.RemovePlayController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, null, null, true);

                var startingArgs = new SDK.Events.OnGameStartingEventArgs
                {
                    Game = game.GetClone(),
                    SourceAction = (playAction as GameAction)?.GetClone(),
                    SelectedRomFile = (playAction as EmulationPlayAction)?.SelectedRomPath
                };

                controllers.InvokeOnStarting(this, startingArgs);
                if (startingArgs.CancelStartup)
                {
                    cancelStartup("Game startup cancelled by an extension.");
                    return;
                }

                if (!game.IsCustomGame && shutdownJobs.TryGetValue(game.PluginId, out var existingJob))
                {
                    logger.Debug($"Starting game with existing client shutdown job, canceling job {game.PluginId}.");
                    existingJob.CancelToken.Cancel();
                    shutdownJobs.TryRemove(game.PluginId, out var _);
                }

                var scriptVars = new Dictionary<string, object>
                {
                    {  "StartingArgs", startingArgs },
                    {  "SourceAction", startingArgs.SourceAction },
                    {  "SelectedRomFile", startingArgs.SelectedRomFile }
                };

                if (!ExecuteScriptAction(scriptRuntimes[game.Id], AppSettings.PreScript, game, game.UseGlobalPreScript, true, GameScriptType.Starting, scriptVars))
                {
                    cancelStartup("Game startup cancelled because global game script failed.");
                    return;
                }

                if (startingArgs.CancelStartup)
                {
                    cancelStartup("Game startup cancelled by global game script.");
                    return;
                }

                if (!ExecuteScriptAction(scriptRuntimes[game.Id], game.PreScript, game, true, false, GameScriptType.Starting, scriptVars))
                {
                    cancelStartup("Game startup cancelled because game script failed.");
                    return;
                }

                if (startingArgs.CancelStartup)
                {
                    cancelStartup("Game startup cancelled by game script.");
                    return;
                }

                if (controller is GenericPlayController genCtrl)
                {
                    if (playAction is EmulationPlayAction emuAct)
                    {
                        genCtrl.StartEmulator(emuAct, true, startingArgs);
                    }
                    else if (playAction is AutomaticPlayController autoAction)
                    {
                        genCtrl.Start(autoAction);
                    }
                    else if (playAction is GameAction act)
                    {
                        genCtrl.Start(act, true, startingArgs);
                    }
                    else
                    {
                        throw new NotSupportedException("Uknown play action type.");
                    }
                }
                else
                {
                    controller.Play(new PlayActionArgs());
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot start game: ");
                Dialogs.ShowMessage(
                    resources.GetString(LOC.GameStartError).Format(exc.Message), LOC.GameError,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (controller != null)
                {
                    controllers.RemoveController(controller);
                    UpdateGameState(game.Id, null, null, null, null, false);
                }

                return;
            }

            UpdateJumpList();
        }

        public void ActivateAction(Game game, GameAction action)
        {
            try
            {
                switch (action.Type)
                {
                    case GameActionType.URL:
                        ProcessStarter.StartUrl(action.Path);
                        break;
                    case GameActionType.File:
                    case GameActionType.Emulator:
                    case GameActionType.Script:
                        using (var scriptRuntime = new PowerShellRuntime("Custom action runtime"))
                        using (var controller = new GenericPlayController(Database, game, scriptRuntime, Application.PlayniteApiGlobal))
                        {
                            if (action.Type == GameActionType.Emulator)
                            {
                                var emulator = Database.Emulators[action.EmulatorId];
                                if (emulator == null)
                                {
                                    throw new Exception($"Emulator not found.");
                                }

                                var prof = emulator.AllProfiles.FirstOrDefault(a => a.Id == action.EmulatorProfileId);
                                var newAction = action.GetClone<GameAction, EmulationPlayAction>();
                                newAction.SelectedEmulatorProfile = prof ?? throw new Exception("Specified emulator config does't exists.");
                                newAction.SelectedRomPath = game.Roms.HasItems() ? game.Roms[0].Path : string.Empty;
                                controller.StartEmulator(newAction, false, new SDK.Events.OnGameStartingEventArgs
                                {
                                    Game = game,
                                    SelectedRomFile = newAction.SelectedRomPath,
                                    SourceAction = action
                                });
                            }
                            else
                            {
                                controller.Start(action, false,  new SDK.Events.OnGameStartingEventArgs
                                {
                                    Game = game,
                                    SourceAction = action
                                });
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot activate action: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameStartActionError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenGameLocation(Game game)
        {
            if (string.IsNullOrEmpty(game.InstallDirectory))
            {
                return;
            }

            try
            {
                var installDirectory = game.InstallDirectory;
                if (FileSystem.DirectoryExistsOnAnyDrive(installDirectory, out var newPath) &&
                    !string.Equals(newPath, installDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Warn("Replaced missing game dir with new one:\n{0}\n{1}".Format(installDirectory, newPath));
                    installDirectory = newPath;
                }

                installDirectory = game.ExpandVariables(installDirectory);
                Commands.GlobalCommands.NavigateDirectoryCommand.Execute(installDirectory);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot open game location: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameOpenLocationError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateGameSize(Game game, bool onlyIfDataMissing, bool updateGameOnLibrary, bool checkLastScanDate)
        {
            if (!game.IsInstalled)
            {
                return;
            }

            if (game.InstallSize != null && onlyIfDataMissing)
            {
                return;
            }

            ulong? scanSize;
            try
            {
                scanSize = CalculateGameSize(game, checkLastScanDate);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get InstallSize from game {game.Name} with install dir {game.InstallDirectory}");
                throw;
            }

            if (scanSize == null)
            {
                return;
            }

            game.LastSizeScanDate = DateTime.Now;
            if (game.InstallSize != scanSize)
            {
                game.InstallSize = scanSize;
            }

            if (updateGameOnLibrary)
            {
                Database.Games.Update(game);
            }
        }

        public ulong? CalculateGameSize(Game game, bool checkLastScanDate)
        {
            long size = 0;
            if (game.Roms.HasItems())
            {
                var expandedGame = GetExpandedGameForRomsSizeScan(game);
                var romsFilesPaths = new List<string>();
                foreach (var rom in expandedGame.Roms)
                {
                    if (rom.Path.IsNullOrWhiteSpace() || !FileSystem.FileExists(rom.Path))
                    {
                        continue;
                    }

                    if (rom.Path.EndsWith(".cue", StringComparison.OrdinalIgnoreCase))
                    {
                        AddPlaylistRomFilesPathsToList(rom, romsFilesPaths, CueSheet.GetFileEntries(rom.Path).Select(a => a.Path));
                    }
                    else if (rom.Path.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase))
                    {
                        AddPlaylistRomFilesPathsToList(rom, romsFilesPaths, M3U.GetEntries(rom.Path).Select(a => a.Path));
                    }
                    else if (rom.Path.EndsWith(".gdi", StringComparison.OrdinalIgnoreCase))
                    {
                        AddPlaylistRomFilesPathsToList(rom, romsFilesPaths, GdiFile.GetEntries(rom.Path).Select(a => a.Path));
                    }
                    else
                    {
                        romsFilesPaths.Add(rom.Path);
                    }
                }

                if (!romsFilesPaths.HasItems())
                {
                    return null;
                }

                if (checkLastScanDate)
                {
                    var addedAfterLastCheck = expandedGame.LastSizeScanDate == null || expandedGame.Added != null && expandedGame.Added > expandedGame.LastSizeScanDate;
                    if (!addedAfterLastCheck && expandedGame.LastSizeScanDate != null
                        && !romsFilesPaths.Any(x => FileSystem.FileGetLastWriteTime(x) > expandedGame.LastSizeScanDate))
                    {
                        return null;
                    }
                }

                foreach (var romFilePath in romsFilesPaths)
                {
                    if (AppSettings.InstallSizeScanUseSizeOnDisk)
                    {
                        size += FileSystem.GetFileSizeOnDisk(romFilePath);
                    }
                    else
                    {
                        size += FileSystem.GetFileSize(romFilePath);
                    }
                }
            }
            else
            {
                if (game.InstallDirectory.IsNullOrEmpty())
                {
                    return null;
                }

                var expandedGame = game.ExpandGame();
                if (!FileSystem.DirectoryExists(expandedGame.InstallDirectory))
                {
                    return null;
                }

                if (checkLastScanDate)
                {
                    var addedAfterLastCheck = expandedGame.LastSizeScanDate == null || expandedGame.Added != null && expandedGame.Added > expandedGame.LastSizeScanDate;
                    if (!addedAfterLastCheck && expandedGame.LastSizeScanDate != null && FileSystem.DirectoryGetLastWriteTime(expandedGame.InstallDirectory) < expandedGame.LastSizeScanDate)
                    {
                        return null;
                    }
                }

                size = FileSystem.GetDirectorySize(expandedGame.InstallDirectory, AppSettings.InstallSizeScanUseSizeOnDisk);
            }

            return (ulong)size;
        }

        private void AddPlaylistRomFilesPathsToList(GameRom rom, List<string> pathsList, IEnumerable<string> playlistChildren)
        {
            if (!playlistChildren.HasItems())
            {
                return;
            }

            var rootDir = Path.GetDirectoryName(rom.Path);
            pathsList.AddRange
            (
                playlistChildren
                .Select(a => Path.Combine(rootDir, a))
                .Where(x => FileSystem.FileExists(x))
             );
        }

        public Game GetExpandedGameForRomsSizeScan(Game game)
        {
            if (game.GameActions == null)
            {
                return game.ExpandGame();
            }

            var emulatorAction = game.GameActions.FirstOrDefault(x => x.Type == GameActionType.Emulator);
            if (emulatorAction == null)
            {
                return game.ExpandGame();
            }

            var emulator = Database.Emulators[emulatorAction.EmulatorId];
            if (emulator != null)
            {
                return game.ExpandGame(false, emulator.InstallDir);
            }

            return game.ExpandGame();
        }

        public void UpdateGameSizeWithDialog(Game game, bool onlyIfDataMissing, bool updateGameOnLibrary)
        {
            var textTitle = string.Format(ResourceProvider.GetString("LOCCalculatingInstallSizeOfGameMessage"), game.Name);
            Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    UpdateGameSize(game, onlyIfDataMissing, updateGameOnLibrary, false);
                }
                catch (Exception e)
                {
                    Dialogs.ShowMessage(
                        string.Format(resources.GetString("LOCCalculateGameSizeError"), e.Message),
                        resources.GetString("LOCCalculateGameSizeErrorCaption"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, new GlobalProgressOptions(textTitle, false) { IsIndeterminate = true });
        }

        public void UpdateGamesSizeWithDialog(List<Game> games, bool onlyIfDataMissing)
        {
            var textTitle = ResourceProvider.GetString("LOCCalculatingInstallSizeMessage");
            var errorStrings = new List<string>();
            var errorsCount = 0;
            Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = games.Count();
                using (Database.BufferedUpdate())
                {
                    foreach (var game in games)
                    {
                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        a.CurrentProgressValue++;
                        a.Text = $"{textTitle}\n\n{game.Name}\n{a.CurrentProgressValue}/{a.ProgressMaxValue}";

                        try
                        {
                            UpdateGameSize(game, onlyIfDataMissing, true, false);
                        }
                        catch (Exception e)
                        {
                            errorsCount++;
                            if (errorStrings.Count < 10)
                            {
                                errorStrings.Add($"{game.Name}: {e.Message}");
                            }
                        }
                    }
                }
            }, new GlobalProgressOptions(textTitle, true) { IsIndeterminate = false });

            if (errorsCount > 0)
            {
                var errorMessage = ResourceProvider.GetString("LOCCalculateGamesSizeErrorMessage").Format(errorsCount)
                    + $"\n\n" + string.Join("\n", errorStrings);
                if (errorsCount > 10)
                {
                    errorMessage += "\n...";
                }

                Dialogs.ShowMessage(
                    errorMessage,
                    resources.GetString("LOCCalculateGameSizeErrorCaption"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetHideGame(Game game, bool state)
        {
            game.Hidden = state;
            Database.Games.Update(game);
        }

        public void SetHideGames(List<Game> games, bool state)
        {
            using (Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    SetHideGame(game, state);
                }
            }
        }

        public void ToggleHideGame(Game game)
        {
            game.Hidden = !game.Hidden;
            Database.Games.Update(game);
        }

        public void ToggleHideGames(List<Game> games)
        {
            using (Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    ToggleHideGame(game);
                }
            }
        }

        public void SetFavoriteGame(Game game, bool state)
        {
            game.Favorite = state;
            Database.Games.Update(game);
        }

        public void SetFavoriteGames(List<Game> games, bool state)
        {
            using (Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    SetFavoriteGame(game, state);
                }
            }
        }

        public void ToggleFavoriteGame(Game game)
        {
            game.Favorite = !game.Favorite;
            Database.Games.Update(game);
        }

        public void ToggleFavoriteGame(List<Game> games)
        {
            using (Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    ToggleFavoriteGame(game);
                }
            }
        }

        public void SetCompletionStatus(Game game, CompletionStatus status)
        {
            if (game.CompletionStatusId != status.Id)
            {
                game.CompletionStatusId = status.Id;
                Database.Games.Update(game);
            }
        }

        public void SetCompletionStatus(List<Game> games, CompletionStatus status)
        {
            using (Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    SetCompletionStatus(game, status);
                }
            }
        }

        public void RemoveGame(Game game)
        {
            if (game.IsInstalling || game.IsRunning || game.IsLaunching || game.IsUninstalling)
            {
                Dialogs.ShowMessage(
                    "LOCGameRemoveRunningError",
                    "LOCGameError",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var addToExclusionList = false;
            if (game.IsCustomGame)
            {
                if (Dialogs.ShowMessage(
                    "LOCGameRemoveAskMessage",
                    "LOCGameRemoveAskTitle",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                var options = new List<MessageBoxOption>
                {
                    new MessageBoxOption("LOCRemoveAskAddToExlusionListYesResponse"),
                    new MessageBoxOption("LOCYesLabel", true),
                    new MessageBoxOption("LOCNoLabel", false, true)
                };
                var result = Dialogs.ShowMessage(
                    "LOCGameRemoveAskMessageIgnoreOption",
                    "LOCGameRemoveAskTitle",
                    MessageBoxImage.Question,
                    options);
                if (result == options[0])
                {
                    addToExclusionList = true;
                }
                else if (result == options[2])
                {
                    return;
                }
            }

            if (Database.Games[game.Id] == null)
            {
                logger.Warn($"Failed to remove game {game.Name} {game.Id}, game doesn't exists anymore.");
            }
            else
            {
                Database.Games.Remove(game);
                if (addToExclusionList)
                {
                    var exclusion = new ImportExclusionItem(game.GameId, game.Name, game.PluginId, Extensions.GetLibraryPlugin(game.PluginId)?.Name);
                    if (Database.ImportExclusions[exclusion.Id] == null)
                    {
                        Database.ImportExclusions.Add(exclusion);
                    }
                }
            }
        }

        public void RemoveGames(List<Game> games)
        {
            if (!games.HasItems())
            {
                return;
            }

            if (games.Exists(a => a.IsInstalling || a.IsRunning || a.IsLaunching || a.IsUninstalling))
            {
                Dialogs.ShowMessage(
                    "LOCGameRemoveRunningError",
                    "LOCGameError",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var addToExclusionList = false;
            if (games.All(a => a.IsCustomGame))
            {
                if (Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGamesRemoveAskMessage"), games.Count()),
                    "LOCGameRemoveAskTitle",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                var options = new List<MessageBoxOption>
                {
                    new MessageBoxOption("LOCRemoveAskAddToExlusionListYesResponse"),
                    new MessageBoxOption("LOCYesLabel", true),
                    new MessageBoxOption("LOCNoLabel", false, true)
                };
                var result = Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGamesRemoveAskMessageIgnoreOption"), games.Count()),
                    "LOCGameRemoveAskTitle",
                    MessageBoxImage.Question,
                    options);
                if (result == options[0])
                {
                    addToExclusionList = true;
                }
                else if (result == options[2])
                {
                    return;
                }
            }

            foreach (var game in games.ToList())
            {
                if (Database.Games[game.Id] == null)
                {
                    logger.Warn($"Failed to remove game {game.Name} {game.Id}, game doesn't exists anymore.");
                    games.Remove(game);
                }

                if (addToExclusionList && !game.IsCustomGame)
                {
                    var exclusion = new ImportExclusionItem(game.GameId, game.Name, game.PluginId, Extensions.GetLibraryPlugin(game.PluginId)?.Name);
                    if (Database.ImportExclusions[exclusion.Id] == null)
                    {
                        Database.ImportExclusions.Add(exclusion);
                    }
                }
            }

            Database.Games.Remove(games);
        }

        public void CreateDesktopShortcut(List<Game> games)
        {
            foreach (var game in games)
            {
                CreateDesktopShortcut(game);
            }
        }

        public void CreateDesktopShortcut(Game game)
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Paths.GetSafePathName(game.Name) + ".url"));
                string icon = string.Empty;

                if (!game.Icon.IsNullOrEmpty())
                {
                    icon = Database.GetFullFilePath(game.Icon);
                }
                else
                {
                    icon = GamesCollectionViewEntry.GetDefaultIconFile(game, AppSettings, Database, Extensions.GetLibraryPlugin(game.PluginId));
                    if (!File.Exists(icon))
                    {
                        icon = string.Empty;
                    }
                }

                if (File.Exists(icon))
                {
                    if (Path.GetExtension(icon) != ".ico")
                    {
                        FileSystem.CreateDirectory(PlaynitePaths.IconsCachePath);
                        var targetIconPath = Path.Combine(PlaynitePaths.IconsCachePath, game.Id + ".ico");
                        BitmapExtensions.ConvertToIcon(icon, targetIconPath);
                        if (File.Exists(targetIconPath))
                        {
                            icon = targetIconPath;
                        }
                    }
                }
                else
                {
                    icon = PlaynitePaths.DesktopExecutablePath;
                }

                var args = new CmdLineOptions() { Start = game.Id.ToString() }.ToString();
                Programs.CreateUrlShortcut($"playnite://playnite/start/{game.Id}", icon, path);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to create shortcut: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameShortcutError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenManual(Game game)
        {
            if (game.Manual.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                var manualPath = game.ExpandVariables(game.Manual, fixSeparators: true);
                if (!manualPath.IsUri() && !File.Exists(manualPath))
                {
                    manualPath = Path.Combine(Database.GetFileStoragePath(game.Id), manualPath);
                }

                if (manualPath.IsUri())
                {
                    ProcessStarter.StartUrl(manualPath);
                }
                else
                {
                    ProcessStarter.StartProcess(manualPath);
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to open manual.");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCManualOpenError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateDesktopShortcuts(List<Game> games)
        {
            foreach (var game in games)
            {
                CreateDesktopShortcut(game);
            }
        }

        public void InstallGame(Game game)
        {
            logger.Info($"Installing {game.GetIdentifierInfo()}");
            InstallController controller = null;
            try
            {
                var installControllers = GetInstallActions(game);
                if (!installControllers.HasItems())
                {
                    Dialogs.ShowErrorMessage(LOC.ErrorNoInstallAction, LOC.GameError);
                    return;
                }

                if (installControllers.Count > 1)
                {
                    controller = actionSelector.SelectInstallAction(installControllers);
                }
                else
                {
                    controller = installControllers[0];
                }

                if (controller == null)
                {
                    return;
                }

                controllers.RemoveInstallController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, true, null, null);
                controller.Install(new InstallActionArgs());
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot install game: ");
                Dialogs.ShowMessage(
                    resources.GetString(LOC.GameInstallError).Format(exc.Message), LOC.GameError,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (controller != null)
                {
                    controllers.RemoveController(controller);
                    UpdateGameState(game.Id, null, null, false, null, null);
                }
            }
        }

        public void UnInstallGame(Game game)
        {
            if (game.IsRunning || game.IsLaunching)
            {
                Dialogs.ShowMessage(LOC.GameUninstallRunningError, LOC.GameError,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            logger.Info($"Uninstalling {game.GetIdentifierInfo()}");
            UninstallController controller = null;

            try
            {
                var uninstallControllers = GetUninstallActions(game);
                if (!uninstallControllers.HasItems())
                {
                    Dialogs.ShowErrorMessage(LOC.ErrorNoInstallAction, LOC.GameError);
                    return;
                }

                if (uninstallControllers.Count > 1)
                {
                    controller = actionSelector.SelectUninstallAction(uninstallControllers);
                }
                else
                {
                    controller = uninstallControllers[0];
                }

                if (controller == null)
                {
                    return;
                }

                controllers.RemoveInstallController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, null, true, null);
                controller.Uninstall(new UninstallActionArgs());
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot un-install game: ");
                Dialogs.ShowMessage(
                    resources.GetString(LOC.GameUninstallError).Format(exc.Message), LOC.GameError,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (controller != null)
                {
                    controllers.RemoveController(controller);
                    UpdateGameState(game.Id, null, null, null, false, null);
                }
            }
        }

        public void UpdateJumpList()
        {
            try
            {
                var jumpList = new JumpList();
                jumpList.ShowFrequentCategory = false;
                jumpList.ShowRecentCategory = false;

                if (AppSettings.QuickLaunchItems > 0)
                {
                    foreach (var lastGame in QuickLaunchItems)
                    {
                        var args = new CmdLineOptions() { Start = lastGame.Id.ToString() }.ToString();
                        JumpTask task = new JumpTask
                        {
                            Title = lastGame.Name,
                            Arguments = args,
                            Description = string.Empty,
                            CustomCategory = "Recent",
                            ApplicationPath = PlaynitePaths.DesktopExecutablePath
                        };

                        if (lastGame.Icon?.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            task.IconResourcePath = Database.GetFullFilePath(lastGame.Icon);
                        }
                        else
                        {
                            task.IconResourcePath = lastGame.GetRawExecutablePath();
                        }

                        jumpList.JumpItems.Add(task);
                    }

                    JumpList.SetJumpList(System.Windows.Application.Current, jumpList);
                }
                else
                {
                    JumpList.SetJumpList(System.Windows.Application.Current, new JumpList());
                }

                jumpList.Apply();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to set jump list data.");
            }
        }

        public void CancelGameMonitoring(Game game)
        {
            var wasRunningOrLaunching = game.IsRunning || game.IsLaunching;
            controllers.RemoveInstallController(game.Id);
            controllers.RemoveUninstallController(game.Id);
            controllers.RemovePlayController(game.Id);
            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsRunning = false;
            dbGame.IsLaunching = false;
            dbGame.IsInstalling = false;
            dbGame.IsUninstalling = false;
            ulong ellapsedTime = 0;
            if (gameStartups.TryRemove(game.Id, out var startupTime))
            {
                // This shouldn't be a problem, but there was one crash report with startup type being more recent
                if (startupTime < DateTime.Now)
                {
                    ellapsedTime = Convert.ToUInt64((DateTime.Now - startupTime).TotalSeconds);
                    dbGame.Playtime += ellapsedTime;
                }
            }

            if (scriptRuntimes.TryRemove(game.Id, out var runtime))
            {
                runtime.Dispose();
            }

            Database.Games.Update(dbGame);
            if (wasRunningOrLaunching)
            {
                RunningGames.Remove(game.Id);
                Extensions.InvokeOnGameStopped(game, ellapsedTime, true);
            }

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.ClearPresence();
            }
        }

        private void UpdateGameState(Guid id, bool? installed, bool? running, bool? installing, bool? uninstalling, bool? launching)
        {
            if (running == true || launching == true)
            {
                RunningGames.AddMissing(id);
            }
            else if (running == false || launching == false)
            {
                RunningGames.Remove(id);
            }

            var game = Database.Games.Get(id);
            if (installed != null)
            {
                game.IsInstalled = installed.Value;
            }

            if (running != null)
            {
                game.IsRunning = running.Value;
            }

            if (installing != null)
            {
                game.IsInstalling = installing.Value;
            }

            if (uninstalling != null)
            {
                game.IsUninstalling = uninstalling.Value;
            }

            if (launching != null)
            {
                game.IsLaunching = launching.Value;
            }

            if (running == true)
            {
                game.LastActivity = DateTime.Now;
                game.PlayCount += 1;
                var comSettings = Database.GetCompletionStatusSettings();
                if (game.CompletionStatusId == Guid.Empty || game.CompletionStatusId == comSettings.DefaultStatus)
                {
                    game.CompletionStatusId = comSettings.PlayedStatus;
                }
            }

            Database.Games.Update(game);
        }

        private void Controllers_Started(object sender, GameStartedEventArgs args)
        {
            var game = args.Source.Game;
            logger.Info($"Started {game.Name} game.");
            UpdateGameState(game.Id, null, true, null, null, false);
            gameStartups.TryAdd(game.Id, DateTime.Now);

            var scriptVars = new Dictionary<string, object>
            {
                { "SourceAction", (args.Source as GenericPlayController)?.StartingArgs?.SourceAction?.GetClone() },
                { "SelectedRomFile", (args.Source as GenericPlayController)?.StartingArgs?.SelectedRomFile },
                { "StartedProcessId", args.StartedProcessId }
            };

            ExecuteScriptAction(scriptRuntimes[game.Id], game.GameStartedScript, game, true, false, GameScriptType.Started, scriptVars);
            ExecuteScriptAction(scriptRuntimes[game.Id], AppSettings.GameStartedScript, game, game.UseGlobalGameStartedScript, true, GameScriptType.Started, scriptVars);

            if (Application.Mode == ApplicationMode.Desktop)
            {
                if (AppSettings.AfterLaunch == AfterLaunchOptions.Close)
                {
                    Application.Quit();
                }
                else if (AppSettings.AfterLaunch == AfterLaunchOptions.Minimize)
                {
                    Application.Minimize();
                }
            }
            else
            {
                if (AppSettings.AfterLaunch == AfterLaunchOptions.Close)
                {
                    Application.Quit();
                    return;
                }

                AppSettings.Fullscreen.IsMusicMuted = true;
                if (AppSettings.Fullscreen.MinimizeAfterGameStartup)
                {
                    Application.Minimize();
                }

                PlayniteApplication.Current.IsActive = false;
            }

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.SetPresence(game.Name);
            }
        }

        private void Controllers_Stopped(object sender, GameStoppedEventArgs args)
        {
            var game = args.Source.Game;
            logger.Info($"Game {game.Name} stopped after {args.SessionLength} seconds.");

            RunningGames.Remove(game.Id);
            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsRunning = false;
            dbGame.IsLaunching = false;
            dbGame.Playtime += args.SessionLength;
            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Source);
            gameStartups.TryRemove(game.Id, out _);

            var restore = false;
            if (Application.Mode == ApplicationMode.Desktop && AppSettings.AfterGameClose == AfterGameCloseOptions.Restore)
            {
                restore = true;
            }
            else if (Application.Mode == ApplicationMode.Fullscreen)
            {
                restore = true;
                AppSettings.Fullscreen.IsMusicMuted = false;
            }

            if (restore)
            {
                // This delay apparently fixes issues with Playnite not restoring properly after game exits.
                // The window will restore, but application will not regain active state.
                // This was mainly reported to happen with some emulators, like RPCS3, no idea why.
                Thread.Sleep(1000);
                Application.Restore();
            }

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.ClearPresence();
            }

            ExecuteScriptAction(scriptRuntimes[game.Id], game.PostScript, game, true, false, GameScriptType.Exit);
            ExecuteScriptAction(scriptRuntimes[game.Id], AppSettings.PostScript, game, game.UseGlobalPostScript, true, GameScriptType.Exit);
            if (scriptRuntimes.TryRemove(game.Id, out var runtime))
            {
                runtime.Dispose();
            }

            Extensions.InvokeOnGameStopped(game, args.SessionLength, false);
            if (AppSettings.ClientAutoShutdown.ShutdownClients && !game.IsCustomGame)
            {
                if (args.SessionLength <= AppSettings.ClientAutoShutdown.MinimalSessionTime)
                {
                    logger.Debug("Game session was too short for client to be shutdown.");
                }
                else if (Database.Games.Any(x => x.IsRunning && x.PluginId == game.PluginId))
                {
                    logger.Debug("Shutdown process canceled because another game from library was detected as running.");
                }
                else
                {
                    var plugin = Extensions.GetLibraryPlugin(game.PluginId);
                    if (plugin?.Properties?.CanShutdownClient == true &&
                        AppSettings.ClientAutoShutdown.ShutdownPlugins.Contains(plugin.Id))
                    {
                        if (shutdownJobs.TryGetValue(game.PluginId, out var existingJob))
                        {
                            existingJob.CancelToken.Cancel();
                            shutdownJobs.TryRemove(game.PluginId, out var _);
                        }

                        var newJob = new ClientShutdownJob
                        {
                            PluginId = plugin.Id,
                            CancelToken = new CancellationTokenSource()
                        };

                        var task = new Task(async () =>
                        {
                            var ct = newJob.CancelToken;
                            var libPlugin = plugin;
                            var timeout = AppSettings.ClientAutoShutdown.GraceTimeout;
                            var curTime = 0;
                            logger.Info($"Scheduled {libPlugin.Name} to be closed after {timeout} seconds.");

                            while (curTime < timeout)
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    logger.Debug($"Client {libPlugin.Name} shutdown canceled.");
                                    return;
                                }

                                await Task.Delay(1000);
                                curTime++;
                            }

                            if (curTime >= timeout)
                            {
                                try
                                {
                                    shutdownJobs.TryRemove(libPlugin.Id, out var _);
                                    libPlugin.Client.Shutdown();
                                }
                                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                                {
                                    logger.Error(e, $"Failed to shutdown {libPlugin.Name} client.");
                                }
                            }
                        });

                        newJob.CancelTask = task;
                        shutdownJobs.TryAdd(plugin.Id, newJob);
                        newJob.CancelTask.Start();
                    }
                }
            }
        }

        private void Controllers_Installed(object sender, GameInstalledEventArgs args)
        {
            var game = args.Source.Game;
            logger.Info($"Game {game.Name} installed.");

            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsInstalling = false;
            dbGame.IsInstalled = true;
            if (args.InstalledInfo != null)
            {
                dbGame.InstallDirectory = args.InstalledInfo.InstallDirectory;
                if (args.InstalledInfo.Roms.HasItems())
                {
                    dbGame.Roms = args.InstalledInfo.Roms.ToObservable();
                }
            }

            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Source);
        }

        private void Controllers_Uninstalled(object sender, GameUninstalledEventArgs args)
        {
            var game = args.Source.Game;
            logger.Info($"Game {game.Name} uninstalled.");

            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsUninstalling = false;
            dbGame.IsInstalled = false;
            dbGame.InstallDirectory = string.Empty;
            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Source);
        }

        public bool ExecuteScriptAction(
            IPowerShellRuntime runtime,
            string script,
            Game game,
            bool execute,
            bool global,
            GameScriptType type,
            Dictionary<string, object> vars = null)
        {
            if (!execute || script.IsNullOrWhiteSpace())
            {
                return true;
            }

            try
            {
                if (runtime == null)
                {
                    throw new Exception("Cannot execute script, no runtime given.");
                }

                if (!PowerShellRuntime.IsInstalled)
                {
                    throw new Exception(resources.GetString("LOCErrorPowerShellNotInstalled"));
                }

                var scriptVars = new Dictionary<string, object>
                {
                    {  "PlayniteApi", Application.PlayniteApiGlobal },
                    {  "Game", game.GetClone() }
                };

                vars?.ForEach(a => scriptVars.AddOrUpdate(a.Key, a.Value));
                var expandedScript = game.ExpandVariables(script);
                var dir = game.ExpandVariables(game.InstallDirectory, true);
                if (!dir.IsNullOrEmpty() && Directory.Exists(dir))
                {
                    runtime.Execute(expandedScript, dir, scriptVars);
                }
                else
                {
                    runtime.Execute(expandedScript, variables: scriptVars);
                }

                return true;
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, global ? "Failed to execute global script." : "Failed to execute game script.");
                logger.Debug(script);
                var message = type.GetDescription() + Environment.NewLine + exc.Message;
                if (exc is ScriptRuntimeException scriptExc)
                {
                    message = message + Environment.NewLine + Environment.NewLine + scriptExc.ScriptStackTrace;
                }

                Dialogs.ShowMessage(
                    message,
                    resources.GetString(global ? LOC.ErrorGlobalScriptAction : LOC.ErrorGameScriptAction),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        public Tuple<List<PlayController>, List<GameAction>> GetPlayActions(Game game)
        {
            var controllers = new List<PlayController>();
            var actions = new List<GameAction>();
            foreach (var plugin in Extensions.Plugins.Values)
            {
                if (!game.IncludeLibraryPluginAction && plugin.Plugin.Id == game.PluginId)
                {
                    continue;
                }

                try
                {
                    var ctrls = plugin.Plugin.GetPlayActions(new GetPlayActionsArgs { Game = game });
                    if (ctrls.HasItems())
                    {
                        controllers.AddRange(ctrls);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get play actions from {plugin.Description.Name}");
                    continue;
                }
            }

            if (game.GameActions.HasItems())
            {
                var selectEmuAdded = false;
                var multipleRoms = game.Roms?.Count > 1;
                var romList = game.Roms.HasItems() ? game.Roms : new ObservableCollection<GameRom> { new GameRom() };

                void addAction(string name, EmulatorProfile profile, EmulationPlayAction baseData)
                {
                    foreach (var rom in romList)
                    {
                        var newAction = baseData.GetClone();
                        newAction.Name = multipleRoms ? $"{name}: {rom.Name}" : name;
                        newAction.SelectedEmulatorProfile = profile;
                        newAction.SelectedRomPath = rom.Path;
                        actions.Add(newAction);
                    }
                }

                foreach (var action in game.GameActions.Where(a => a.IsPlayAction))
                {
                    if (action.Type == GameActionType.Emulator)
                    {
                        if (action.EmulatorId == Guid.Empty)
                        {
                            if (selectEmuAdded)
                            {
                                continue;
                            }

                            var supportedEmus = game.GetCompatibleEmulators(Database);
                            if (!supportedEmus.HasItems())
                            {
                                continue;
                            }

                            selectEmuAdded = true;
                            foreach (var emu in supportedEmus.Keys)
                            {
                                var profCount = supportedEmus[emu].Count;
                                foreach (var profile in supportedEmus[emu])
                                {
                                    addAction(profCount == 1 ? emu.Name : $"{emu.Name}: {profile.Name}", profile, new EmulationPlayAction
                                    {
                                        EmulatorId = emu.Id,
                                        EmulatorProfileId = profile.Id
                                    });
                                }
                            }
                        }
                        else
                        {
                            var emu = Database.Emulators[action.EmulatorId];
                            if (emu == null)
                            {
                                continue;
                            }

                            if (action.EmulatorProfileId == null)
                            {
                                var profiles = game.GetCompatibleProfiles(emu);
                                profiles.ForEach(profile =>
                                    addAction($"{action.Name}: {profile.Name}", profile, action.GetClone<GameAction, EmulationPlayAction>()));
                            }
                            else
                            {
                                var prof = emu.AllProfiles.FirstOrDefault(a => a.Id == action.EmulatorProfileId);
                                if (prof == null)
                                {
                                    logger.Error($"Specified emulator config does't exists {action.EmulatorProfileId}");
                                }
                                else
                                {
                                    addAction(action.Name, prof, action.GetClone<GameAction, EmulationPlayAction>());
                                }
                            }
                        }
                    }
                    else
                    {
                        actions.Add(action);
                    }
                }
            }

            return new Tuple<List<PlayController>, List<GameAction>>(controllers, actions);
        }

        public List<InstallController> GetInstallActions(Game game)
        {
            var allActions = new List<InstallController>();
            foreach (var plugin in Extensions.Plugins.Values)
            {
                try
                {
                    var actions = plugin.Plugin.GetInstallActions(new GetInstallActionsArgs { Game = game });
                    if (actions.HasItems())
                    {
                        allActions.AddRange(actions);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get install actions from {plugin.Description.Name}");
                }
            }

            return allActions;
        }

        public List<UninstallController> GetUninstallActions(Game game)
        {
            var allActions = new List<UninstallController>();
            foreach (var plugin in Extensions.Plugins.Values)
            {
                try
                {
                    var actions = plugin.Plugin.GetUninstallActions(new GetUninstallActionsArgs { Game = game });
                    if (actions.HasItems())
                    {
                        allActions.AddRange(actions);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get install actions from {plugin.Description.Name}");
                }
            }

            return allActions;
        }
    }
}
