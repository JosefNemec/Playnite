using Playnite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayniteUI.Controls
{
    public class GameMenu : ContextMenu
    {
        public bool ShowStartSection
        {
            get
            {
                return (bool)GetValue(GameProperty);
            }

            set
            {
                SetValue(GameProperty, value);
            }
        }

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("ShowStartSection", typeof(bool), typeof(GameMenu), new PropertyMetadata(true, ShowStartSectionPropertyChangedCallback));

        private static void ShowStartSectionPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as GameMenu;
            obj.InitializeItems();
        }

        private IResourceProvider resources;

        public IGame Game
        {
            get
            {
                if (DataContext is GameViewEntry entry)
                {
                    return entry.Game;
                }
                else if (DataContext is IEnumerable<object> entries)
                {
                    return (entries.First() as GameViewEntry).Game;
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<IGame> Games
        {
            get
            {
                if (DataContext is  IEnumerable<object> entries)
                {
                    if (entries.Count() == 1)
                    {
                        return null;
                    }

                    return entries.Select(a => (a as GameViewEntry).Game);
                }
                else
                {
                    return null;
                }
            }
        }

        static GameMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
        }

        public GameMenu()
        {
            resources = new ResourceProvider();
            InitializeItems();
            DataContextChanged += GameMenu_DataContextChanged;
        }        

        private void GameMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InitializeItems();
        }

        public void InitializeItems()
        {
            Items.Clear();

            if (Games != null)
            {
                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("MenuUnFavorite") : resources.FindString("MenuFavorite")
                };

                favoriteItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.ToggleFavoriteGameg(Games);
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("MenuUnHide") : resources.FindString("MenuHide")
                };

                hideItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.ToggleHideGames(Games);
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("MenuEdit")
                };

                editItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.EditGames(Games);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("MenuSetCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.SetGamesCategories(Games);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("MenuRemove")
                };

                removeItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.RemoveGames(Games);
                };

                Items.Add(removeItem);
            }
            else if (Game != null)
            {
                // Play / Install
                if (ShowStartSection)
                {
                    bool added = false;
                    if (Game.IsInstalled)
                    {
                        var playItem = new MenuItem()
                        {
                            Header = resources.FindString("MenuPlay"),
                            FontWeight = FontWeights.Bold
                        };

                        playItem.Click += (s, e) =>
                        {
                            GamesEditor.Instance.PlayGame(Game);
                        };

                        Items.Add(playItem);
                        added = true;
                    }
                    else if (Game.Provider != Provider.Custom)
                    {
                        var installItem = new MenuItem()
                        {
                            Header = resources.FindString("MenuInstall"),
                            FontWeight = FontWeights.Bold
                        };

                        installItem.Click += (s, e) =>
                        {
                            GamesEditor.Instance.InstallGame(Game);
                        };

                        Items.Add(installItem);
                        added = true;
                    }

                    if (added)
                    {
                        Items.Add(new Separator());
                    }
                }

                // Custom Actions
                if (Game.OtherTasks != null && Game.OtherTasks.Count > 0)
                {
                    foreach (var task in Game.OtherTasks)
                    {
                        var taskItem = new MenuItem()
                        {
                            Header = task.Name
                        };

                        taskItem.Click += (s, e) =>
                        {
                            GamesEditor.Instance.ActivateAction(Game, task);
                        };

                        Items.Add(taskItem);
                    }

                    Items.Add(new Separator());
                }

                // Open Game Location
                if (Game.IsInstalled)
                {
                    var locationItem = new MenuItem()
                    {
                        Header = resources.FindString("MenuOpenLocation")
                    };

                    locationItem.Click += (s, e) =>
                    {
                        GamesEditor.Instance.OpenGameLocation(Game);
                    };

                    Items.Add(locationItem);
                }

                // Create Desktop Shortcut
                var shortcutItem = new MenuItem()
                {
                    Header = resources.FindString("MenuShortcut")
                };

                shortcutItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.CreateShortcut(Game);
                };

                Items.Add(shortcutItem);
                Items.Add(new Separator());

                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("MenuUnFavorite") : resources.FindString("MenuFavorite")
                };

                favoriteItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.ToggleFavoriteGame(Game);
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("MenuUnHide") : resources.FindString("MenuHide")
                };

                hideItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.ToggleHideGame(Game);
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("MenuEdit")
                };

                editItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.EditGame(Game);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("MenuSetCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.SetGameCategories(Game);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("MenuRemove")
                };

                removeItem.Click += (s, e) =>
                {
                    GamesEditor.Instance.RemoveGame(Game);
                };

                Items.Add(removeItem);

                // Uninstall
                if (Game.Provider != Provider.Custom)
                {
                    var uninstallItem = new MenuItem()
                    {
                        Header = resources.FindString("MenuUninstall")
                    };

                    uninstallItem.Click += (s, e) =>
                    {
                        GamesEditor.Instance.UnInstallGame(Game);
                    };

                    Items.Add(uninstallItem);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
