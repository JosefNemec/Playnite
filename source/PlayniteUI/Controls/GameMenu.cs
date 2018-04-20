using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
                return (bool)GetValue(ShowStartSectionProperty);
            }

            set
            {
                SetValue(ShowStartSectionProperty, value);
            }
        }

        public static readonly DependencyProperty ShowStartSectionProperty =
            DependencyProperty.Register("ShowStartSection", typeof(bool), typeof(GameMenu), new PropertyMetadata(true, ShowStartSectionPropertyChangedCallback));

        private static void ShowStartSectionPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as GameMenu;
            obj.InitializeItems();
        }

        private IResourceProvider resources;
        private GamesEditor editor;
        private readonly SynchronizationContext context;

        public Game Game
        {
            get; set;
        }

        public List<Game> Games
        {
            get;  set;
        }

        static GameMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
        }

        public GameMenu() : this(App.GamesEditor)
        {            
        }

        public GameMenu(GamesEditor editor)
        {
            context = SynchronizationContext.Current;
            this.editor = editor;
            resources = new ResourceProvider();
            DataContextChanged += GameMenu_DataContextChanged;
            InitializeItems();
        }

        private void GameMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is GameViewEntry entry)
            {
                AssignGame(entry.Game);
            }
            else if (DataContext is IEnumerable<GameViewEntry> entries)
            {
                if (entries.Count() > 0)
                {
                    AssignGame((entries.First() as GameViewEntry).Game);
                }

                if (entries.Count() == 1)
                {
                    Games = null;
                }
                else
                {
                    Games = entries.Select(a => (a as GameViewEntry).Game).ToList();
                }
            }
            else if (DataContext is IList<object> entries2)
            {
                if (entries2.Count() > 0)
                {
                    AssignGame((entries2.First() as GameViewEntry).Game);
                }

                if (entries2.Count() == 1)
                {
                    Games = null;
                }
                else
                {
                    Games = entries2.Select(a => (a as GameViewEntry).Game).ToList();
                }
            }
            else
            {
                AssignGame(null);
                Games = null;
            }

            InitializeItems();
        }

        private void AssignGame(Game game)
        {
            if (Game != null)
            {
                Game.PropertyChanged -= Game_PropertyChanged;
            }

            Game = game;

            if (Game != null)
            {
                Game.PropertyChanged += Game_PropertyChanged;
            }
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[]
            {
                "State", "OtherTasks", "Links", "Favorite", "Hidden"
            }).Contains(e.PropertyName))
            {
                //Can be called from different threads when game database update is done
                context.Send((a) => InitializeItems(), null);
            }
        }

        public void InitializeItems()
        {
            Items.Clear();

            if (Games != null)
            {
                // Set Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = resources.FindString("LOCFavoriteGame")
                };

                favoriteItem.Click += (s, e) =>
                {
                    editor.SetFavoriteGames(Games, true);
                };

                Items.Add(favoriteItem);

                var unFavoriteItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveFavoriteGame")
                };

                unFavoriteItem.Click += (s, e) =>
                {
                    editor.SetFavoriteGames(Games, false);
                };

                Items.Add(unFavoriteItem);

                // Set Hide
                var hideItem = new MenuItem()
                {
                    Header = resources.FindString("LOCHideGame")
                };

                hideItem.Click += (s, e) =>
                {
                    editor.SetHideGames(Games, true);
                };

                Items.Add(hideItem);

                var unHideItem = new MenuItem()
                {
                    Header = resources.FindString("LOCUnHideGame")
                };

                unHideItem.Click += (s, e) =>
                {
                    editor.SetHideGames(Games, false);
                };

                Items.Add(unHideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("LOCEditGame")
                };

                editItem.Click += (s, e) =>
                {
                    editor.EditGames(Games);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("LOCSetGameCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    editor.SetGamesCategories(Games);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveGame")
                };

                removeItem.Click += (s, e) =>
                {
                    editor.RemoveGames(Games);
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
                            Header = resources.FindString("LOCPlayGame"),
                            FontWeight = FontWeights.Bold
                        };

                        playItem.Click += (s, e) =>
                        {
                            editor.PlayGame(Game);
                        };

                        Items.Add(playItem);
                        added = true;
                    }
                    else if (Game.Provider != Provider.Custom)
                    {
                        var installItem = new MenuItem()
                        {
                            Header = resources.FindString("LOCInstallGame"),
                            FontWeight = FontWeights.Bold
                        };

                        installItem.Click += (s, e) =>
                        {
                            editor.InstallGame(Game);
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
                            editor.ActivateAction(Game, task);
                        };

                        Items.Add(taskItem);
                    }

                    Items.Add(new Separator());
                }

                // Links
                if (Game.Links?.Any() == true)
                {
                    var linksItem = new MenuItem()
                    {
                        Header = resources.FindString("LOCLinksLabel")
                    };

                    foreach (var link in Game.Links)
                    {
                        linksItem.Items.Add(new MenuItem()
                        {
                            Header = link.Name,
                            Command = Commands.GeneralCommands.NavigateUrlCommand,
                            CommandParameter = link.Url
                        });
                    }     
                    
                    Items.Add(linksItem);
                    Items.Add(new Separator());
                }

                // Open Game Location
                if (Game.IsInstalled)
                {
                    var locationItem = new MenuItem()
                    {
                        Header = resources.FindString("LOCOpenGameLocation")
                    };

                    locationItem.Click += (s, e) =>
                    {
                        editor.OpenGameLocation(Game);
                    };

                    Items.Add(locationItem);
                }

                // Create Desktop Shortcut
                var shortcutItem = new MenuItem()
                {
                    Header = resources.FindString("LOCCreateDesktopShortcut")
                };

                shortcutItem.Click += (s, e) =>
                {
                    editor.CreateShortcut(Game);
                };

                Items.Add(shortcutItem);
                Items.Add(new Separator());

                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("LOCRemoveFavoriteGame") : resources.FindString("FavoriteGame")
                };

                favoriteItem.Click += (s, e) =>
                {
                    editor.ToggleFavoriteGame(Game);
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Hidden ? resources.FindString("LOCUnHideGame") : resources.FindString("HideGame")
                };

                hideItem.Click += (s, e) =>
                {
                    editor.ToggleHideGame(Game);
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("LOCEditGame")
                };

                editItem.Click += (s, e) =>
                {
                    editor.EditGame(Game);
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("LOCSetGameCategory")
                };

                categoryItem.Click += (s, e) =>
                {
                    editor.SetGameCategories(Game);
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveGame")
                };

                removeItem.Click += (s, e) =>
                {
                    editor.RemoveGame(Game);
                };

                Items.Add(removeItem);

                // Uninstall
                if (Game.Provider != Provider.Custom && Game.IsInstalled)
                {
                    var uninstallItem = new MenuItem()
                    {
                        Header = resources.FindString("LOCUninstallGame")
                    };

                    uninstallItem.Click += (s, e) =>
                    {
                        editor.UnInstallGame(Game);
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
