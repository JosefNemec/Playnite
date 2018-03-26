using LiteDB;
using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers;
using Playnite.Providers.Steam;
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
    public class GamesEditor : INotifyPropertyChanged, IDisposable
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private IResourceProvider resources = new ResourceProvider();
        private GameDatabase database;
        private Settings appSettings;
        private GameControllerFactory controllers;

        public List<IGame> LastGames
        {
            get
            {
                return database.GamesCollection?.Find(Query.And(Query.Not("LastActivity", null), Query.Not("PlayTask", null))).OrderByDescending(a => a.LastActivity).Take(10).ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GamesEditor(GameDatabase database, Settings appSettings)
        {
            this.database = database;
            this.appSettings = appSettings;
            controllers = new GameControllerFactory(database);
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

            controllers?.Dispose();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool? SetGameCategories(IGame game)
        {
            var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, game, true);
            return model.OpenView();
        }

        public bool? SetGamesCategories(List<IGame> games)
        {
            var model = new CategoryConfigViewModel(CategoryConfigWindowFactory.Instance, database, games, true);
            return model.OpenView();
        }

        public bool? EditGame(IGame game)
        {
            var model = new GameEditViewModel(
                            game,
                            database,
                            GameEditWindowFactory.Instance,
                            new DialogsFactory(),
                            new ResourceProvider(),
                            appSettings);
            return model.OpenView();
        }

        public bool? EditGames(List<IGame> games)
        {
            var model = new GameEditViewModel(
                            games,
                            database,
                            GameEditWindowFactory.Instance,
                            new DialogsFactory(),
                            new ResourceProvider(),
                            appSettings);
            return model.OpenView();
        }

        public void PlayGame(IGame game)
        {
            // Set parent for message boxes in this method
            // because this method can be invoked from tray icon which otherwise bugs the dialog
            var dbGame = database.GetGame(game.Id);
            if (dbGame == null)
            {
                PlayniteMessageBox.Show(
                    Application.Current.MainWindow,
                    string.Format(resources.FindString("GameStartErrorNoGame"), game.Name),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateJumpList();
                return;
            }

            try
            {
                var controller = GameControllerFactory.GetGameBasedController(game, appSettings);
                controllers.RemoveController(game.Id);
                controllers.AddController(controller);

                if (game.IsInstalled)
                {
                    UpdateGameState(game.Id, null, null, null, null, true);
                    controller.Play(database.GetEmulators());
                }
                else
                {
                    InstallGame(game);
                    return;
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot start game: ");
                PlayniteMessageBox.Show(
                    Application.Current.MainWindow,
                    string.Format(resources.FindString("GameStartError"), exc.Message),
                    resources.FindString("GameError"),
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

        public void ActivateAction(IGame game, GameTask action)
        {
            try
            {
                GameHandler.ActivateTask(action, game as Game, database.EmulatorsCollection.FindAll().ToList());
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                PlayniteMessageBox.Show(
                    string.Format(resources.FindString("GameStartActionError"), exc.Message),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenGameLocation(IGame game)
        {
            if (string.IsNullOrEmpty(game.InstallDirectory))
            {
                return;
            }

            try
            {
                Process.Start(game.ResolveVariables(game.InstallDirectory));
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                PlayniteMessageBox.Show(
                    string.Format(resources.FindString("GameOpenLocationError"), exc.Message),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetHideGame(IGame game, bool state)
        {
            game.Hidden = state;
            database.UpdateGameInDatabase(game);
        }

        public void SetHideGames(List<IGame> games, bool state)
        {
            foreach (var game in games)
            {
                SetHideGame(game, state);
            }
        }

        public void ToggleHideGame(IGame game)
        {
            game.Hidden = !game.Hidden;
            database.UpdateGameInDatabase(game);
        }

        public void ToggleHideGames(List<IGame> games)
        {
            foreach (var game in games)
            {
                ToggleHideGame(game);
            }
        }

        public void SetFavoriteGame(IGame game, bool state)
        {
            game.Favorite = state;
            database.UpdateGameInDatabase(game);
        }

        public void SetFavoriteGames(List<IGame> games, bool state)
        {
            foreach (var game in games)
            {
                SetFavoriteGame(game, state);
            }
        }

        public void ToggleFavoriteGame(IGame game)
        {
            game.Favorite = !game.Favorite;
            database.UpdateGameInDatabase(game);
        }

        public void ToggleFavoriteGame(List<IGame> games)
        {
            foreach (var game in games)
            {
                ToggleFavoriteGame(game);
            }
        }

        public void RemoveGame(IGame game)
        {
            if (game.State.Installing || game.State.Running || game.State.Launching || game.State.Uninstalling)
            {
                PlayniteMessageBox.Show(
                    resources.FindString("GameRemoveRunningError"),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (PlayniteMessageBox.Show(
                resources.FindString("GameRemoveAskMessage"),
                resources.FindString("GameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            database.DeleteGame(game);
        }

        public void RemoveGames(List<IGame> games)
        {
            if (games.Exists(a => a.State.Installing || a.State.Running || a.State.Launching || a.State.Uninstalling))
            {
                PlayniteMessageBox.Show(
                    resources.FindString("GameRemoveRunningError"),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (PlayniteMessageBox.Show(
                string.Format(resources.FindString("GamesRemoveAskMessage"), games.Count()),
                resources.FindString("GameRemoveAskTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            database.DeleteGames(games);
        }

        public void CreateShortcut(IGame game)
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(Path.Combine("%userprofile%", "Desktop", FileSystem.GetSafeFilename(game.Name) + ".lnk"));
                string icon = string.Empty;

                if (!string.IsNullOrEmpty(game.Icon) && Path.GetExtension(game.Icon) == ".ico")
                {
                    FileSystem.CreateDirectory(Path.Combine(Paths.DataCachePath, "icons"));
                    icon = Path.Combine(Paths.DataCachePath, "icons", game.Id + ".ico");
                    database.SaveFile(game.Icon, icon);
                }
                else if (game.PlayTask?.Type == GameTaskType.File)
                {
                    if (Path.IsPathRooted(game.ResolveVariables(game.PlayTask.Path)))
                    {
                        icon = game.ResolveVariables(game.PlayTask.Path);
                    }
                    else
                    {
                        icon = Path.Combine(game.ResolveVariables(game.PlayTask.WorkingDir), game.ResolveVariables(game.PlayTask.Path));
                    }
                }

                Programs.CreateShortcut(Paths.ExecutablePath, "-command launch:" + game.Id, icon, path);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to create shortcut: ");
                PlayniteMessageBox.Show(
                    string.Format(resources.FindString("GameShortcutError"), exc.Message),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateShortcuts(List<IGame> games)
        {
            foreach (var game in games)
            {
                CreateShortcut(game);
            }
        }

        public void InstallGame(IGame game)
        {
            try
            {
                var controller = GameControllerFactory.GetGameBasedController(game, appSettings);
                controllers.RemoveController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, true, null, null);
                controller.Install();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot install game: ");
                PlayniteMessageBox.Show(
                    string.Format(resources.FindString("GameInstallError"), exc.Message),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UnInstallGame(IGame game)
        {
            if (game.State.Running || game.State.Launching)
            {
                PlayniteMessageBox.Show(
                    resources.FindString("GameUninstallRunningError"),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                var controller = GameControllerFactory.GetGameBasedController(game, appSettings);
                controllers.RemoveController(game.Id);
                controllers.AddController(controller);
                UpdateGameState(game.Id, null, null, null, true, null);
                controller.Uninstall();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot un-install game: ");
                PlayniteMessageBox.Show(
                    string.Format(resources.FindString("GameUninstallError"), exc.Message),
                    resources.FindString("GameError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateJumpList()
        {           
            OnPropertyChanged("LastGames");
            var jumpList = new JumpList();
            foreach (var lastGame in LastGames)
            {
                JumpTask task = new JumpTask
                {
                    Title = lastGame.Name,
                    Arguments = "-command launch:" + lastGame.Id,
                    Description = string.Empty,
                    CustomCategory = "Recent",
                    ApplicationPath = Paths.ExecutablePath
                };

                if (lastGame.PlayTask?.Type == GameTaskType.File)
                {
                    if (Path.IsPathRooted(lastGame.ResolveVariables(lastGame.PlayTask.Path)))
                    {
                        task.IconResourcePath = lastGame.ResolveVariables(lastGame.PlayTask.Path);
                    }
                    else
                    {
                        var workDir = lastGame.ResolveVariables(lastGame.PlayTask.WorkingDir);
                        var path = lastGame.ResolveVariables(lastGame.PlayTask.Path);
                        if (string.IsNullOrEmpty(workDir))
                        {
                            task.IconResourcePath = path;
                        }
                        else
                        {
                            task.IconResourcePath = Path.Combine(workDir, path);
                        }
                    }
                }

                jumpList.JumpItems.Add(task);
                jumpList.ShowFrequentCategory = false;
                jumpList.ShowRecentCategory = false;
            }
            
            JumpList.SetJumpList(Application.Current, jumpList);
        }

        public void CancelGameMonitoring(IGame game)
        {
            controllers.RemoveController(game.Id);
            UpdateGameState(game.Id, null, false, false, false, false);
        }

        private void UpdateGameState(int id, bool? installed, bool? running, bool? installing, bool? uninstalling, bool? launching)
        {
            var game = database.GetGame(id);
            game.State.SetState(installed, running, installing, uninstalling, launching);
            if (launching == true)
            {
                game.LastActivity = DateTime.Now;
            }

            database.UpdateGameInDatabase(game);
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
            logger.Info($"Game {game.Name} stopped after {args.ElapsedTime} seconds.");

            var dbGame = database.GetGame(game.Id);
            dbGame.State.Running = false;
            dbGame.Playtime += args.ElapsedTime;
            database.UpdateGameInDatabase(dbGame);
            controllers.RemoveController(args.Controller);
        }

        private void Controllers_Installed(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} installed after {args.ElapsedTime} seconds.");

            var dbGame = database.GetGame(game.Id);
            dbGame.State.Installing = false;
            dbGame.State.Installed = true;
            dbGame.InstallDirectory = args.Controller.Game.InstallDirectory;

            if (dbGame.PlayTask == null)
            {
                dbGame.PlayTask = args.Controller.Game.PlayTask;
            }

            if (dbGame.OtherTasks == null)
            {
                dbGame.OtherTasks = args.Controller.Game.OtherTasks;
            }

            database.UpdateGameInDatabase(dbGame);
            controllers.RemoveController(args.Controller);
        }

        private void Controllers_Uninstalled(object sender, GameControllerEventArgs args)
        {
            var game = args.Controller.Game;
            logger.Info($"Game {game.Name} uninstalled after {args.ElapsedTime} seconds.");

            var dbGame = database.GetGame(game.Id);
            dbGame.State.Uninstalling = false;
            dbGame.State.Installed = false;
            dbGame.InstallDirectory = string.Empty;
            database.UpdateGameInDatabase(dbGame);
            controllers.RemoveController(args.Controller);
        }
    }
}
