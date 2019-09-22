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

namespace Playnite
{
    public class GamesEditor : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private IResourceProvider resources = new ResourceProvider();        
        private PlayniteSettings appSettings;
        private GameControllerFactory controllers;
        private PlayniteApplication application;

        public ExtensionFactory Extensions { get; private set; }
        public GameDatabase Database { get; private set; }
        public IDialogsFactory Dialogs { get; private set; }

        public List<Game> LastGames
        {
            get
            {
                return Database.Games.Where(a => a.LastActivity != null && a.IsInstalled).OrderByDescending(a => a.LastActivity).Take(10).ToList();
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
            this.appSettings = appSettings;
            this.Extensions = extensions;
            this.application = app;
            controllers = controllerFactory;
            controllers.Installed += Controllers_Installed;
            controllers.Uninstalled += Controllers_Uninstalled;
            controllers.Started += Controllers_Started;
            controllers.Stopped += Controllers_Stopped;            
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
                if (game.IsRunning)
                {
                    logger.Warn("Failed to start the game, game is already running.");
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
                controller.Play();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
                    UpdateGameState(game.Id, null, null, null, null, false);
                }

                logger.Error(exc, "Cannot start game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameStartError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                UpdateJumpList();                
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to set jump list data: ");
            }
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
                Process.Start(game.ExpandVariables(game.InstallDirectory));
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
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

        public void RemoveGame(Game game)
        {
            if (game.IsInstalling || game.IsRunning || game.IsLaunching || game.IsUninstalling)
            {
                Dialogs.ShowMessage(
                    resources.GetString("LOCGameRemoveRunningError"),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (Dialogs.ShowMessage(
                resources.GetString("LOCGameRemoveAskMessage"),
                resources.GetString("LOCGameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Database.Games.Remove(game);
        }

        public void RemoveGames(List<Game> games)
        {
            if (games.Exists(a => a.IsInstalling || a.IsRunning || a.IsLaunching || a.IsUninstalling))
            {
                Dialogs.ShowMessage(
                    resources.GetString("LOCGameRemoveRunningError"),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (Dialogs.ShowMessage(
                string.Format(resources.GetString("LOCGamesRemoveAskMessage"), games.Count()),
                resources.GetString("LOCGameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Database.Games.Remove(games);
        }

        public void CreateShortcut(Game game)
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Paths.GetSafeFilename(game.Name) + ".lnk"));
                string icon = string.Empty;

                if (!string.IsNullOrEmpty(game.Icon) && Path.GetExtension(game.Icon) == ".ico")
                {
                    icon = Database.GetFullFilePath(game.Icon);
                }
                else if (game.PlayAction?.Type == GameActionType.File)
                {
                    icon = game.GetRawExecutablePath();
                }

                var args = new CmdLineOptions() { Start = game.Id.ToString() }.ToString();
                Programs.CreateShortcut(PlaynitePaths.DesktopExecutablePath, args, icon, path);
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

        public void CreateShortcuts(List<Game> games)
        {
            foreach (var game in games)
            {
                CreateShortcut(game);
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
                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
                    UpdateGameState(game.Id, null, null, false, null, null);
                }

                logger.Error(exc, "Cannot install game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameInstallError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (controller != null)
                {
                    controllers.RemoveController(game.Id);
                    UpdateGameState(game.Id, null, null, null, false, null);
                }

                logger.Error(exc, "Cannot un-install game: ");
                Dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCGameUninstallError"), exc.Message),
                    resources.GetString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateJumpList()
        {           
            OnPropertyChanged(nameof(LastGames));
            var jumpList = new JumpList();
            foreach (var lastGame in LastGames)
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
                jumpList.ShowFrequentCategory = false;
                jumpList.ShowRecentCategory = false;
            }
            
            JumpList.SetJumpList(Application.Current, jumpList);
        }

        public void CancelGameMonitoring(Game game)
        {
            controllers.RemoveController(game.Id);
            UpdateGameState(game.Id, null, false, false, false, false);
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
            if (application.Mode == ApplicationMode.Desktop)
            {
                if (appSettings.AfterLaunch == AfterLaunchOptions.Close)
                {
                    application.Quit();
                }
                else if (appSettings.AfterLaunch == AfterLaunchOptions.Minimize)
                {
                    application.Minimize();
                }
            }
            else
            {
                if (appSettings.AfterLaunch == AfterLaunchOptions.Close)
                {
                    application.Quit();
                }
                else
                {
                    application.Minimize();
                }
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
            if (application.Mode == ApplicationMode.Desktop)
            {
                if (appSettings.AfterGameClose == AfterGameCloseOptions.Restore)
                {
                    application.Restore();
                }
            }
            else
            {
                application.Restore();
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
    }
}
