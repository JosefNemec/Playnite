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

namespace PlayniteUI
{
    public class GamesEditor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
            try
            {
                game.PlayGame();
                var lastGames = GameDatabase.Instance.Games.OrderByDescending(a => a.LastActivity).Where(a => a.LastActivity != null).Take(10);

                var jumpList = new JumpList();
                foreach (var lastGame in lastGames)
                {
                    JumpTask task = new JumpTask
                    {
                        Title = lastGame.Name,
                        Arguments = "-command launch:" + lastGame.Id,
                        Description = string.Empty,
                        CustomCategory = "Recent",
                        ApplicationPath = Paths.ExecutablePath
                    };

                    if (lastGame.PlayTask.Type == GameTaskType.File)
                    {
                        task.IconResourcePath = Path.Combine(lastGame.PlayTask.WorkingDir, lastGame.PlayTask.Path);
                    }

                    jumpList.JumpItems.Add(task);
                    jumpList.ShowFrequentCategory = false;
                    jumpList.ShowRecentCategory = false;
                }

                JumpList.SetJumpList(Application.Current, jumpList);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Cannot start game: ");
                MessageBox.Show("Cannot start game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GameDatabase.Instance.UpdateGameInDatabase(game);
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
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to create shortcut: ");
                MessageBox.Show("Failed to create shortcut: " + exc.Message, "Shortcut Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Cannot install game: ");
                MessageBox.Show("Cannot install game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GameDatabase.Instance.UpdateGameInDatabase(game);
            }
        }

        public void UnInstallGame(IGame game)
        {
            try
            {
                game.UninstallGame();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Cannot un-install game: ");
                MessageBox.Show("Cannot un-install game: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GameDatabase.Instance.UpdateGameInDatabase(game);
            }
        }

        public static Game GetMultiGameEditObject(IEnumerable<IGame> games)
        {
            var dummyGame = new Game();
            var firstGame = games.First();

            var firstName = firstGame.Name;
            if (games.All(a => a.Name == firstName) == true)
            {
                dummyGame.Name = firstName;
            }

            var firstGenres = firstGame.Genres;
            if (games.All(a => a.Genres.IsListEqual(firstGenres) == true))
            {
                dummyGame.Genres = firstGenres;
            }

            var firstReleaseDate = firstGame.ReleaseDate;
            if (games.All(a => a.ReleaseDate == firstReleaseDate) == true)
            {
                dummyGame.ReleaseDate = firstReleaseDate;
            }

            var firstDeveloper = firstGame.Developers;
            if (games.All(a => a.Developers.IsListEqual(firstDeveloper) == true))
            {
                dummyGame.Developers = firstDeveloper;
            }

            var firstPublisher = firstGame.Publishers;
            if (games.All(a => a.Publishers.IsListEqual(firstPublisher) == true))
            {
                dummyGame.Publishers = firstPublisher;
            }

            var firstTag = firstGame.Categories;
            if (games.All(a => a.Categories.IsListEqual(firstTag) == true))
            {
                dummyGame.Categories = firstTag;
            }

            var firstStoreUrl = firstGame.StoreUrl;
            if (games.All(a => a.StoreUrl == firstStoreUrl) == true)
            {
                dummyGame.StoreUrl = firstStoreUrl;
            }

            var firstWikiUrl = firstGame.WikiUrl;
            if (games.All(a => a.WikiUrl == firstWikiUrl) == true)
            {
                dummyGame.WikiUrl = firstWikiUrl;
            }

            var firstForumsUrl = firstGame.CommunityHubUrl;
            if (games.All(a => a.CommunityHubUrl == firstForumsUrl) == true)
            {
                dummyGame.CommunityHubUrl = firstForumsUrl;
            }

            var firstDescription = firstGame.Description;
            if (games.All(a => a.Description == firstDescription) == true)
            {
                dummyGame.Description = firstDescription;
            }

            return dummyGame;
        }
    }
}
