using LiteDB;
using Playnite;
using Playnite.API;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.Controllers;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.Settings;
using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shell;

namespace PlayniteUI
{
    public class GamesEditor : ObservableObject, IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private IResourceProvider resources = new DefaultResourceProvider();
        private GameDatabase database;
        private IDialogsFactory dialogs;
        private PlayniteSettings appSettings;
        private ExtensionFactory extensions;

        public bool IsFullscreen
        {
            get; set;
        }

        private readonly GameControllerFactory controllers;

        public List<Game> LastGames
        {
            get
            {
                return database.Games.Where(a => a.LastActivity != null && a.IsInstalled).OrderByDescending(a => a.LastActivity).Take(10).ToList();
            }
        }

        public GamesEditor(
            GameDatabase database,
            GameControllerFactory controllerFactory,
            PlayniteSettings appSettings,
            IDialogsFactory dialogs,
            ExtensionFactory extensions)
        {
            this.dialogs = dialogs;
            this.database = database;
            this.appSettings = appSettings;
            this.extensions = extensions;
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

        public bool? SetGameCategories(Game game)
        {
            var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, game, true);
            return model.OpenView();
        }

        public bool? SetGamesCategories(List<Game> games)
        {
            var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, games, true);
            return model.OpenView();
        }

        public bool? EditGame(Game game)
        {
            var model = new GameEditViewModel(
                            game,
                            database,
                            GameEditWindowFactory.Instance,
                            new DialogsFactory(),
                            new DefaultResourceProvider(),
                            extensions);
            return model.OpenView();
        }

        public bool? EditGames(List<Game> games)
        {
            var model = new GameEditViewModel(
                            games,
                            database,
                            GameEditWindowFactory.Instance,
                            new DialogsFactory(),
                            new DefaultResourceProvider(),
                            extensions);
            return model.OpenView();
        }

        public void PlayGame(Game game)
        {
            if (!game.IsInstalled)
            {
                InstallGame(game);
                return;
            }

            logger.Info($"Starting {game.GetIdentifierInfo()}");
            var dbGame = database.Games.Get(game.Id);
            if (dbGame == null)
            {
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameStartErrorNoGame"), game.Name),
                    resources.FindString("LOCGameError"),
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
                    controller = controllers.GetGameBasedController(game, extensions.LibraryPlugins.Select(a => a.Value.Plugin));
                }
                else
                {
                    logger.Info("Using generic controller start the game.");
                    controller = controllers.GetGenericGameController(game);
                }

                if (controller == null)
                {
                    dialogs.ShowErrorMessage(
                        resources.FindString("LOCErrorLibraryPluginNotFound"),
                        resources.FindString("LOCGameError"));
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
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameStartError"), exc.Message),
                    resources.FindString("LOCGameError"),
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
                var emulators = database.Emulators.ToList();
                var profile = GameActionActivator.GetGameActionEmulatorConfig(action, emulators)?.ExpandVariables(game);
                GameActionActivator.ActivateAction(action.ExpandVariables(game), profile);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameStartActionError"), exc.Message),
                    resources.FindString("LOCGameError"),
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
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameOpenLocationError"), exc.Message),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetHideGame(Game game, bool state)
        {
            game.Hidden = state;
            database.Games.Update(game);
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
            database.Games.Update(game);
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
            database.Games.Update(game);
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
            database.Games.Update(game);
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
                dialogs.ShowMessage(
                    resources.FindString("LOCGameRemoveRunningError"),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (dialogs.ShowMessage(
                resources.FindString("LOCGameRemoveAskMessage"),
                resources.FindString("LOCGameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            database.Games.Remove(game);
        }

        public void RemoveGames(List<Game> games)
        {
            if (games.Exists(a => a.IsInstalling || a.IsRunning || a.IsLaunching || a.IsUninstalling))
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCGameRemoveRunningError"),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (dialogs.ShowMessage(
                string.Format(resources.FindString("LOCGamesRemoveAskMessage"), games.Count()),
                resources.FindString("LOCGameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            database.Games.Remove(games);
        }

        public void CreateShortcut(Game game)
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Paths.GetSafeFilename(game.Name) + ".lnk"));
                string icon = string.Empty;

                if (!string.IsNullOrEmpty(game.Icon) && Path.GetExtension(game.Icon) == ".ico")
                {
                    icon = database.GetFullFilePath(game.Icon);
                }
                else if (game.PlayAction?.Type == GameActionType.File)
                {
                    icon = game.GetRawExecutablePath();
                }

                Programs.CreateShortcut(PlaynitePaths.ExecutablePath, "-command launch:" + game.Id, icon, path);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to create shortcut: ");
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameShortcutError"), exc.Message),
                    resources.FindString("LOCGameError"),
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
                controller = controllers.GetGameBasedController(game, extensions.LibraryPlugins.Select(a => a.Value.Plugin));
                if (controller == null)
                {
                    logger.Error("Game installation failed, library plugin not found.");
                    dialogs.ShowErrorMessage(
                        resources.FindString("LOCErrorLibraryPluginNotFound"),
                        resources.FindString("LOCGameError"));
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
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameInstallError"), exc.Message),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UnInstallGame(Game game)
        {
            if (game.IsRunning || game.IsLaunching)
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCGameUninstallRunningError"),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            logger.Info($"Uninstalling {game.GetIdentifierInfo()}");
            IGameController controller = null;

            try
            {
                controller = controllers.GetGameBasedController(game, extensions.LibraryPlugins.Select(a => a.Value.Plugin));
                if (controller == null)
                {
                    logger.Error("Game uninstallation failed, library plugin not found.");
                    dialogs.ShowErrorMessage(
                        resources.FindString("LOCErrorLibraryPluginNotFound"),
                        resources.FindString("LOCGameError"));
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
                dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCGameUninstallError"), exc.Message),
                    resources.FindString("LOCGameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateJumpList()
        {           
            OnPropertyChanged(nameof(LastGames));
            var jumpList = new JumpList();
            foreach (var lastGame in LastGames)
            {
                JumpTask task = new JumpTask
                {
                    Title = lastGame.Name,
                    Arguments = "-command launch:" + lastGame.Id,
                    Description = string.Empty,
                    CustomCategory = "Recent",
                    ApplicationPath = PlaynitePaths.ExecutablePath
                };

                if (lastGame.Icon?.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) == true)
                {
                    task.IconResourcePath = database.GetFullFilePath(lastGame.Icon);
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
            var game = database.Games.Get(id);
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

            database.Games.Update(game);
        }

        private void Controllers_Started(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Started {game.Name} game.");
            UpdateGameState(game.Id, null, true, null, null, false);
  
            if (appSettings.AfterLaunch == AfterLaunchOptions.Close)
            {
                App.CurrentApp.Quit();
            }
            else if (appSettings.AfterLaunch == AfterLaunchOptions.Minimize)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void Controllers_Stopped(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} stopped after {args.EllapsedTime} seconds.");

            var dbGame = database.Games.Get(game.Id);
            dbGame.IsRunning = false;
            dbGame.IsLaunching = false;
            dbGame.Playtime += args.EllapsedTime;
            database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);

            if (appSettings.AfterGameClose == AfterGameCloseOptions.Restore)
            {
                App.CurrentApp.MainViewWindow.RestoreWindow();
            }
        }

        private void Controllers_Installed(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} installed after {args.EllapsedTime} seconds.");

            var dbGame = database.Games.Get(game.Id);
            dbGame.IsInstalling = false;
            dbGame.IsInstalled = true;
            dbGame.InstallDirectory = args.Controller.Game.InstallDirectory;

            if (dbGame.PlayAction == null)
            {
                dbGame.PlayAction = args.Controller.Game.PlayAction;
            }

            if (dbGame.OtherActions == null)
            {
                dbGame.OtherActions = args.Controller.Game.OtherActions;
            }

            database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);
        }

        private void Controllers_Uninstalled(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} uninstalled after {args.EllapsedTime} seconds.");

            var dbGame = database.Games.Get(game.Id);
            dbGame.IsUninstalling = false;
            dbGame.IsInstalled = false;
            dbGame.InstallDirectory = string.Empty;
            database.Games.Update(dbGame);
            controllers.RemoveController(args.Controller);
        }
    }
}
