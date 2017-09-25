using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Windows;
using System.Windows.Shell;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using NLog;
using System.ComponentModel;
using LiteDB;

namespace PlayniteUI
{
    public class GamesEditor : INotifyPropertyChanged
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static GamesEditor instance;
        public static GamesEditor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GamesEditor();
                }

                return instance;
            }
        }       

        public IEnumerable<IGame> LastGames
        {
            get
            {
                return GameDatabase.Instance.GamesCollection?.Find(Query.All("LastActivity", Query.Descending))?.Where(a => a.LastActivity != null && a.IsInstalled)?.Take(10);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool? SetGameCategories(IGame game)
        {
            var window = new CategoryConfigWindow()
            {
                AutoUpdateGame = true,
                Game = game,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            return window.DialogResult;
        }

        public bool? SetGamesCategories(IEnumerable<IGame> games)
        {
            var window = new CategoryConfigWindow()
            {
                AutoUpdateGame = true,
                Games = games,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            return window.DialogResult;
        }

        public bool? EditGame(IGame game)
        {
            var window = new GameEditWindow()
            {
                Game = game,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            return window.DialogResult;
        }

        public bool? EditGames(IEnumerable<IGame> games)
        {
            var window = new GameEditWindow()
            {
                Games = games,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
            return window.DialogResult;
        }

        public void PlayGame(IGame game)
        {
            // Set parent for message boxes in this method
            // because this method can be invoked from tray icon which otherwise bugs the dialog
            if (GameDatabase.Instance.GamesCollection.FindOne(a => a.ProviderId == game.ProviderId) == null)
            {
                PlayniteMessageBox.Show(Application.Current.MainWindow, $"Cannot start game. '{game.Name}' was not found in database.", "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);                
                OnPropertyChanged("LastGames");
                UpdateJumpList();
                return;
            }

            try
            {
                if (game.IsInstalled)
                {
                    game.PlayGame(GameDatabase.Instance.EmulatorsCollection.FindAll().ToList());
                }
                else
                {
                    game.InstallGame();
                }

                GameDatabase.Instance.UpdateGameInDatabase(game);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot start game: ");
                PlayniteMessageBox.Show(Application.Current.MainWindow, "Cannot start game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                OnPropertyChanged("LastGames");
                UpdateJumpList();                
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to set jump list data: ");
            }

            if (Settings.Instance.MinimizeAfterLaunch)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        public void CreateShortcut(IGame game)
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(Path.Combine("%userprofile%", "Desktop", FileSystem.GetSafeFilename(game.Name) + ".lnk"));
                string icon = string.Empty;

                if (!string.IsNullOrEmpty(game.Icon) && Path.GetExtension(game.Icon) == ".ico")
                {
                    FileSystem.CreateFolder(Path.Combine(Paths.DataCachePath, "icons"));
                    icon = Path.Combine(Paths.DataCachePath, "icons", game.Id + ".ico");
                    GameDatabase.Instance.SaveFile(game.Icon, icon);
                }
                else if (game.PlayTask.Type == GameTaskType.File)
                {
                    icon = Path.Combine(game.PlayTask.WorkingDir, game.PlayTask.Path);
                }

                Programs.CreateShortcut(Paths.ExecutablePath, "-command launch:" + game.Id, icon, path);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to create shortcut: ");
                PlayniteMessageBox.Show("Failed to create shortcut: " + exc.Message, "Shortcut Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateShortcuts(IEnumerable<IGame> games)
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
                game.InstallGame();
                GameDatabase.Instance.UpdateGameInDatabase(game);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot install game: ");
                PlayniteMessageBox.Show("Cannot install game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UnInstallGame(IGame game)
        {
            try
            {
                game.UninstallGame();
                GameDatabase.Instance.UpdateGameInDatabase(game);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Cannot un-install game: ");
                PlayniteMessageBox.Show("Cannot un-install game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateJumpList()
        {
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

                if (lastGame.PlayTask != null && lastGame.PlayTask.Type == GameTaskType.File)
                {
                    if (string.IsNullOrEmpty(lastGame.PlayTask.WorkingDir))
                    {
                        task.IconResourcePath = lastGame.PlayTask.Path;
                    }
                    else
                    {
                        task.IconResourcePath = Path.Combine(lastGame.PlayTask.WorkingDir, lastGame.PlayTask.Path);
                    }
                }

                jumpList.JumpItems.Add(task);
                jumpList.ShowFrequentCategory = false;
                jumpList.ShowRecentCategory = false;
            }

            JumpList.SetJumpList(Application.Current, jumpList);
        }
    }
}
