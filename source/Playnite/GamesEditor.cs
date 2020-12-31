using Playnite;
using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.Controllers;
using Playnite.SDK;
using Playnite.SDK.Events;
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

namespace Playnite
{
    public class ClientShutdownJob
    {
        public Guid PluginId { get; set; }
        public CancellationTokenSource CancelToken { get; set; }
        public Task CancelTask { get; set; }
    }

    public class GamesEditor : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private IResourceProvider resources = new ResourceProvider();
        private GameControllerFactory controllers;
        private readonly ConcurrentDictionary<Guid, ClientShutdownJob> shutdownJobs = new ConcurrentDictionary<Guid, ClientShutdownJob>();
        private readonly ConcurrentDictionary<Guid, DateTime> gameStartups = new ConcurrentDictionary<Guid, DateTime>();

        public PlayniteApplication Application;
        public ExtensionFactory Extensions { get; private set; }
        public GameDatabase Database { get; private set; }
        public IDialogsFactory Dialogs { get; private set; }
        public PlayniteSettings AppSettings { get; private set; }

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
            PlayniteApplication app)
        {
            this.Dialogs = dialogs;
            this.Database = database;
            this.AppSettings = appSettings;
            this.Extensions = extensions;
            this.Application = app;
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
            foreach (var controller in controllers.Controllers)
            {
                UpdateGameState(controller.Game.Id, null, false, false, false, false);
            }

            controllers.Installed -= Controllers_Installed;
            controllers.Uninstalled -= Controllers_Uninstalled;
            controllers.Started -= Controllers_Started;
            controllers.Stopped -= Controllers_Stopped;
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

            IGameController controller = null;

            try
            {
                if (game.IsRunning || game.IsLaunching)
                {
                    logger.Warn("Failed to start the game, game is already running/launching.");
                    return;
                }

                if (game.PlayAction == null)
                {
                    Dialogs.ShowErrorMessage(
                        resources.GetString("LOCErrorNoPlayAction"),
                        resources.GetString("LOCGameError"));
                    return;
                }

                if (game.PlayAction.IsHandledByPlugin)
                {
                    logger.Info("Using library plugin to start the game.");
                    controller = controllers.GetGameBasedController(game, Extensions);
                }
                else
                {
                    logger.Info("Using generic controller start the game.");
                    controller = controllers.GetGenericGameController(game);
                }

                if (controller == null)
                {
                    Dialogs.ShowErrorMessage(
                        resources.GetString("LOCErrorLibraryPluginNotFound"),
                        resources.GetString("LOCGameError"));
                    return;
                }

                controllers.RemoveController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, null, null, true);

                if (!game.IsCustomGame && shutdownJobs.TryGetValue(game.PluginId, out var existingJob))
                {
                    logger.Debug($"Starting game with existing client shutdown job, canceling job {game.PluginId}.");
                    existingJob.CancelToken.Cancel();
                    shutdownJobs.TryRemove(game.PluginId, out var _);
                }

                if (!AppSettings.PreScript.IsNullOrWhiteSpace() && game.UseGlobalPreScript)
                {
                    try
                    {
                        var expanded = game.ExpandVariables(AppSettings.PreScript);
                        ExecuteScriptAction(AppSettings.ActionsScriptLanguage, expanded, game);
                    }
                    catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        var message = exc.Message;
                        if (exc is ScriptRuntimeException err)
                        {
                            message = err.Message + "\n\n" + err.ScriptStackTrace;
                        }

                        logger.Error(exc, "Failed to execute global pre-script action.");
                        logger.Error(AppSettings.PreScript);
                        Dialogs.ShowMessage(
                            message,
                            resources.GetString("LOCErrorGlobalScriptAction"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        controllers.RemoveController(game.Id);
                        UpdateGameState(game.Id, null, null, null, null, false);
                        return;
                    }
                }

                if (!game.PreScript.IsNullOrWhiteSpace())
                {
                    try
                    {
                        var expanded = game.ExpandVariables(game.PreScript);
                        ExecuteScriptAction(game.ActionsScriptLanguage, expanded, game);
                    }
                    catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        var message = exc.Message;
                        if (exc is ScriptRuntimeException err)
                        {
                            message = err.Message + "\n\n" + err.ScriptStackTrace;
                        }

                        logger.Error(exc, "Failed to execute game's pre-script action.");
                        logger.Error(game.PreScript);
                        Dialogs.ShowMessage(
                            message,
                            resources.GetString("LOCErrorGameScriptAction"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        controllers.RemoveController(game.Id);
                        UpdateGameState(game.Id, null, null, null, null, false);
                        return;
                    }
                }

                controller.Play();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot start game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameStartError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
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
                var emulators = Database.Emulators.ToList();
                var profile = GameActionActivator.GetGameActionEmulatorConfig(action, emulators)?.ExpandVariables(game);
                GameActionActivator.ActivateAction(action.ExpandVariables(game), profile);
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
                if (!Directory.Exists(installDirectory))
                {
                    var newInstallDirectory = FileSystem.LookupAlternativeDirectoryPath(installDirectory);
                    if (!string.IsNullOrWhiteSpace(newInstallDirectory))
                    {
                        logger.Warn($"InstallDirectory \"{installDirectory}\" does not exist for game \"{game.Name}\"" +
                        $" and is temporarily changed to {newInstallDirectory}");
                        installDirectory = newInstallDirectory;
                    }
                }

                installDirectory = game.ExpandVariables(installDirectory);
                if (AppSettings.DirectoryOpenCommand.IsNullOrWhiteSpace())
                {
                    Process.Start(installDirectory);
                }
                else
                {
                    try
                    {
                        ProcessStarter.ShellExecute(AppSettings.DirectoryOpenCommand.Replace("{Dir}", installDirectory));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to open directory using custom command.");
                        Process.Start(installDirectory);
                    }
                }
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

        public void SetHideGame(Game game, bool state)
        {
            game.Hidden = state;
            Database.Games.Update(game);
        }

        public void SetHideGames(List<Game> games, bool state)
        {
            foreach (var game in games)
            {
                SetHideGame(game, state);
            }
        }

        public void ToggleHideGame(Game game)
        {
            game.Hidden = !game.Hidden;
            Database.Games.Update(game);
        }

        public void ToggleHideGames(List<Game> games)
        {
            foreach (var game in games)
            {
                ToggleHideGame(game);
            }
        }

        public void SetFavoriteGame(Game game, bool state)
        {
            game.Favorite = state;
            Database.Games.Update(game);
        }

        public void SetFavoriteGames(List<Game> games, bool state)
        {
            foreach (var game in games)
            {
                SetFavoriteGame(game, state);
            }
        }

        public void ToggleFavoriteGame(Game game)
        {
            game.Favorite = !game.Favorite;
            Database.Games.Update(game);
        }

        public void ToggleFavoriteGame(List<Game> games)
        {
            foreach (var game in games)
            {
                ToggleFavoriteGame(game);
            }
        }

        public void SetCompletionStatus(Game game, CompletionStatus status)
        {
            if (game.CompletionStatus != status)
            {
                game.CompletionStatus = status;
                Database.Games.Update(game);
            }
        }

        public void SetCompletionStatus(List<Game> games, CompletionStatus status)
        {
            foreach(var game in games)
            {
                SetCompletionStatus(game, status);
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
                    AppSettings.ImportExclusionList.Add(game.GameId, game.Name, game.PluginId, Extensions.GetLibraryPlugin(game.PluginId)?.Name);
                }
            }
        }

        public void RemoveGames(List<Game> games)
        {
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
                    AppSettings.ImportExclusionList.Add(game.GameId, game.Name, game.PluginId, Extensions.GetLibraryPlugin(game.PluginId)?.Name);
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
                    icon = game.GetDefaultIcon(AppSettings, Database, Extensions.GetLibraryPlugin(game.PluginId));
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
                var manualPath = game.Manual;
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
            IGameController controller = null;
            try
            {
                controller = controllers.GetGameBasedController(game, Extensions);
                if (controller == null)
                {
                    logger.Error("Game installation failed, library plugin not found.");
                    Dialogs.ShowErrorMessage(
                        resources.GetString("LOCErrorLibraryPluginNotFound"),
                        resources.GetString("LOCGameError"));
                    return;
                }

                if (controller is GenericGameController)
                {
                    logger.Error("Game installation failed, library plugin doesn't provide game controller.");
                    return;
                }

                controllers.RemoveController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, true, null, null);
                controller.Install();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot install game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameInstallError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
                    UpdateGameState(game.Id, null, null, false, null, null);
                }
            }
        }

        public void UnInstallGame(Game game)
        {
            if (game.IsRunning || game.IsLaunching)
            {
                Dialogs.ShowMessage(
                    resources.GetString("LOCGameUninstallRunningError"),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            logger.Info($"Uninstalling {game.GetIdentifierInfo()}");
            IGameController controller = null;

            try
            {
                controller = controllers.GetGameBasedController(game, Extensions);
                if (controller == null)
                {
                    logger.Error("Game uninstallation failed, library plugin not found.");
                    Dialogs.ShowErrorMessage(
                        resources.GetString("LOCErrorLibraryPluginNotFound"),
                        resources.GetString("LOCGameError"));
                    return;
                }

                if (controller is GenericGameController)
                {
                    logger.Error("Game uninstallation failed, library plugin doesn't provide game controller.");
                    return;
                }

                controllers.RemoveController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, null, true, null);
                controller.Uninstall();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot un-install game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameUninstallError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
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
                        else if (lastGame.PlayAction?.Type == GameActionType.File)
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
            controllers.RemoveController(game.Id);
            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsRunning = false;
            dbGame.IsLaunching = false;
            dbGame.IsInstalling = false;
            dbGame.IsUninstalling = false;
            if (gameStartups.TryRemove(game.Id, out var startupTime))
            {
                dbGame.Playtime += Convert.ToInt64((DateTime.Now - startupTime).TotalSeconds);
            }

            Database.Games.Update(dbGame);

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.ClearPresence();
            }
        }

        private void UpdateGameState(Guid id, bool? installed, bool? running, bool? installing, bool? uninstalling, bool? launching)
        {
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

            if (launching == true)
            {
                game.LastActivity = DateTime.Now;
                game.PlayCount += 1;
                if (game.CompletionStatus == CompletionStatus.NotPlayed)
                {
                    game.CompletionStatus = CompletionStatus.Played;
                }
            }

            Database.Games.Update(game);
        }

        private void Controllers_Started(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Started {game.Name} game.");
            UpdateGameState(game.Id, null, true, null, null, false);
            gameStartups.TryAdd(game.Id, DateTime.Now);

            if (!AppSettings.GameStartedScript.IsNullOrWhiteSpace() && game.UseGlobalGameStartedScript)
            {
                try
                {
                    var expanded = game.ExpandVariables(AppSettings.GameStartedScript);
                    ExecuteScriptAction(AppSettings.ActionsScriptLanguage, expanded, game);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    var message = exc.Message;
                    if (exc is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    logger.Error(exc, "Failed to execute global game-started action.");
                    logger.Error(AppSettings.GameStartedScript);
                    Dialogs.ShowMessage(
                        message,
                        resources.GetString("LOCErrorGlobalScriptAction"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            if (!game.GameStartedScript.IsNullOrWhiteSpace())
            {
                try
                {
                    var expanded = game.ExpandVariables(game.GameStartedScript);
                    ExecuteScriptAction(game.ActionsScriptLanguage, expanded, game);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    var message = exc.Message;
                    if (exc is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    logger.Error(exc, "Failed to execute game's game-started action.");
                    logger.Error(game.GameStartedScript);
                    Dialogs.ShowMessage(
                        message,
                        resources.GetString("LOCErrorGameScriptAction"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

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
                }
                else
                {
                    Application.Minimize();
                }
            }

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.SetPresence(game.Name);
            }
        }

        private void Controllers_Stopped(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} stopped after {args.EllapsedTime} seconds.");

            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsRunning = false;
            dbGame.IsLaunching = false;
            dbGame.Playtime += args.EllapsedTime;
            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);
            gameStartups.TryRemove(game.Id, out _);
            if (Application.Mode == ApplicationMode.Desktop)
            {
                if (AppSettings.AfterGameClose == AfterGameCloseOptions.Restore)
                {
                    Application.Restore();
                }
            }
            else
            {
                Application.Restore();
            }

            if (AppSettings.DiscordPresenceEnabled)
            {
                Application.Discord?.ClearPresence();
            }

            if (!game.PostScript.IsNullOrWhiteSpace())
            {
                try
                {
                    var expanded = game.ExpandVariables(game.PostScript);
                    ExecuteScriptAction(game.ActionsScriptLanguage, expanded, game);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    var message = exc.Message;
                    if (exc is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    logger.Error(exc, "Failed to execute game's post-script action.");
                    logger.Error(game.PostScript);
                    Dialogs.ShowMessage(
                        message,
                        resources.GetString("LOCErrorGameScriptAction"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            if (!AppSettings.PostScript.IsNullOrWhiteSpace() && game.UseGlobalPostScript)
            {
                try
                {
                    var expanded = game.ExpandVariables(AppSettings.PostScript);
                    ExecuteScriptAction(AppSettings.ActionsScriptLanguage, expanded, game);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    var message = exc.Message;
                    if (exc is ScriptRuntimeException err)
                    {
                        message = err.Message + "\n\n" + err.ScriptStackTrace;
                    }

                    logger.Error(exc, "Failed to execute global post-script action.");
                    logger.Error(AppSettings.PostScript);
                    Dialogs.ShowMessage(
                        message,
                        resources.GetString("LOCErrorGlobalScriptAction"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }

            if (AppSettings.ClientAutoShutdown.ShutdownClients && !game.IsCustomGame)
            {
                if (args.EllapsedTime <= AppSettings.ClientAutoShutdown.MinimalSessionTime)
                {
                    logger.Debug("Game session was too short for client to be shutdown.");
                }
                else
                {
                    var plugin = Extensions.GetLibraryPlugin(game.PluginId);
                    if (plugin?.Capabilities?.CanShutdownClient == true &&
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
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} installed after {args.EllapsedTime} seconds.");

            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsInstalling = false;
            dbGame.IsInstalled = true;
            dbGame.InstallDirectory = args.InstalledInfo.InstallDirectory;

            if (args.InstalledInfo.PlayAction != null)
            {
                dbGame.PlayAction = args.InstalledInfo.PlayAction;
            }

            if (args.InstalledInfo.OtherActions != null)
            {
                dbGame.OtherActions = args.InstalledInfo.OtherActions.ToObservable();
            }

            if (!args.InstalledInfo.GameImagePath.IsNullOrEmpty())
            {
                dbGame.GameImagePath = args.InstalledInfo.GameImagePath;
            }

            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);
        }

        private void Controllers_Uninstalled(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} uninstalled after {args.EllapsedTime} seconds.");

            var dbGame = Database.Games.Get(game.Id);
            dbGame.IsUninstalling = false;
            dbGame.IsInstalled = false;
            dbGame.InstallDirectory = string.Empty;
            Database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);
        }

        internal void ExecuteScriptAction(ScriptLanguage language, string script, Game game)
        {
            if (language == ScriptLanguage.PowerShell && !Scripting.PowerShell.PowerShellRuntime.IsInstalled)
            {
                throw new Exception(resources.GetString("LOCErrorPowerShellNotInstalled"));
            }

            logger.Info($"Executing script action in {language} runtime.");
            IScriptRuntime runtime = null;
            switch (language)
            {
                case ScriptLanguage.PowerShell:
                    runtime = new Scripting.PowerShell.PowerShellRuntime($"PS script: {game.Name}");
                    break;
                case ScriptLanguage.IronPython:
                    runtime = new Scripting.IronPython.IronPythonRuntime($"IP script: {game.Name}");
                    break;
                case ScriptLanguage.Batch:
                    runtime = new Scripting.Batch.BatchRuntime();
                    break;
            }

            using (runtime)
            {
                var dir = game.ExpandVariables(game.InstallDirectory, true);
                if (!dir.IsNullOrEmpty() && Directory.Exists(dir))
                {
                    runtime.Execute(script, dir);
                }
                else
                {
                    runtime.Execute(script);
                }
            }
        }
    }
}
