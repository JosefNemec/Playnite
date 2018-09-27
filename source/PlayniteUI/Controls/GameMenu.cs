using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUI.ViewModels;
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
        private MainViewModel model;
        private readonly SynchronizationContext context;

        private Image startIcon;
        private Image removeIcon;
        private Image linksIcon;
        private Image favoriteIcon;
        private Image unFavoriteIcon;
        private Image hideIcon;
        private Image unHideIcon;
        private Image browseIcon;
        private Image shortcutIcon;
        private Image installIcon;
        private Image editIcon;


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

        public GameMenu() : this(App.MainModel)
        {            
        }

        public GameMenu(MainViewModel model)
        {
            startIcon = Images.GetImageFromResource("Images/MenuIcons/start.png");
            removeIcon = Images.GetImageFromResource("Images/MenuIcons/remove.png");
            linksIcon = Images.GetImageFromResource("Images/MenuIcons/link.png");
            favoriteIcon = Images.GetImageFromResource("Images/MenuIcons/favorite.png");
            unFavoriteIcon = Images.GetImageFromResource("Images/MenuIcons/favoriteEmpty.png");
            hideIcon = Images.GetImageFromResource("Images/MenuIcons/hideCrosed.png");
            unHideIcon = Images.GetImageFromResource("Images/MenuIcons/hide.png");
            browseIcon = Images.GetImageFromResource("Images/MenuIcons/folder.png");
            shortcutIcon = Images.GetImageFromResource("Images/MenuIcons/shortcut.png");
            installIcon = Images.GetImageFromResource("Images/MenuIcons/install.png");
            editIcon = Images.GetImageFromResource("Images/MenuIcons/edit.png");

            context = SynchronizationContext.Current;
            this.model = model;
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
                "State", "OtherActions", "Links", "Favorite", "Hidden"
            }).Contains(e.PropertyName))
            {
                //Can be called from different threads when game database update is done
                context.Post((a) => InitializeItems(), null);
            }
        }

        public void InitializeItems()
        {
            Items.Clear();

            if (Games?.Count == 0 && Game == null)
            {
                return;
            }

            if (Games != null)
            {
                // Set Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = resources.FindString("LOCFavoriteGame"),
                    Icon = favoriteIcon,
                    Command = model.SetAsFavoritesCommand,
                    CommandParameter = Games
                };

                Items.Add(favoriteItem);

                var unFavoriteItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveFavoriteGame"),
                    Icon = unFavoriteIcon,
                    Command = model.RemoveAsFavoritesCommand,
                    CommandParameter = Games
                };

                Items.Add(unFavoriteItem);

                // Set Hide
                var hideItem = new MenuItem()
                {
                    Header = resources.FindString("LOCHideGame"),
                    Icon = hideIcon,
                    Command = model.SetAsHiddensCommand,
                    CommandParameter = Games
                };

                Items.Add(hideItem);

                var unHideItem = new MenuItem()
                {
                    Header = resources.FindString("LOCUnHideGame"),
                    Icon = unHideIcon,
                    Command = model.RemoveAsHiddensCommand,
                    CommandParameter = Games
                };

                Items.Add(unHideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("LOCEditGame"),
                    Icon = editIcon,
                    Command = model.EditGamesCommand,
                    CommandParameter = Games,
                    InputGestureText = model.EditSelectedGamesCommand.GestureText
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("LOCSetGameCategory"),
                    Icon = Images.GetEmptyImage(),
                    Command = model.AssignGamesCategoryCommand,
                    CommandParameter = Games
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveGame"),
                    Icon = removeIcon,
                    Command = model.RemoveGamesCommand,
                    CommandParameter = Games,
                    InputGestureText = model.RemoveSelectedGamesCommand.GestureText
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
                            Icon = startIcon,
                            FontWeight = FontWeights.Bold,
                            Command = model.StartGameCommand,
                            CommandParameter = Game,
                            InputGestureText = model.StartSelectedGameCommand.GestureText
                        };

                        Items.Add(playItem);
                        added = true;
                    }
                    else if (!Game.IsCustomGame)
                    {
                        var installItem = new MenuItem()
                        {
                            Header = resources.FindString("LOCInstallGame"),
                            Icon = installIcon,
                            FontWeight = FontWeights.Bold,
                            Command = model.InstallGameCommand,
                            CommandParameter = Game
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
                if (Game.OtherActions != null && Game.OtherActions.Count > 0)
                {
                    foreach (var task in Game.OtherActions)
                    {
                        var taskItem = new MenuItem()
                        {
                            Header = task.Name,
                            Icon = Images.GetEmptyImage()
                        };

                        taskItem.Click += (s, e) =>
                        {
                            model.GamesEditor.ActivateAction(Game, task);
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
                        Header = resources.FindString("LOCLinksLabel"),
                        Icon = linksIcon
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
                        Header = resources.FindString("LOCOpenGameLocation"),
                        Icon = browseIcon,
                        Command = model.OpenGameLocationCommand,
                        CommandParameter = Game
                    };

                    Items.Add(locationItem);
                }

                // Create Desktop Shortcut
                var shortcutItem = new MenuItem()
                {
                    Header = resources.FindString("LOCCreateDesktopShortcut"),
                    Icon = shortcutIcon,
                    Command = model.CreateGameShortcutCommand,
                    CommandParameter = Game
                };

                Items.Add(shortcutItem);
                Items.Add(new Separator());

                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.FindString("LOCRemoveFavoriteGame") : resources.FindString("LOCFavoriteGame"),
                    Icon = Game.Favorite ? unFavoriteIcon : favoriteIcon,
                    Command = model.ToggleFavoritesCommand,
                    CommandParameter = Game                    
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Hidden ? resources.FindString("LOCUnHideGame") : resources.FindString("LOCHideGame"),
                    Icon = Game.Hidden ? unHideIcon : hideIcon,
                    Command = model.ToggleVisibilityCommand,
                    CommandParameter = Game
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.FindString("LOCEditGame"),
                    Icon = editIcon,
                    Command = model.EditGameCommand,
                    CommandParameter = Game,
                    InputGestureText = model.EditSelectedGamesCommand.GestureText
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.FindString("LOCSetGameCategory"),
                    Icon = Images.GetEmptyImage(),
                    Command = model.AssignGameCategoryCommand,
                    CommandParameter = Game
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());
                
                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.FindString("LOCRemoveGame"),
                    Icon = removeIcon,
                    Command = model.RemoveGameCommand,
                    CommandParameter = Game,
                    InputGestureText = model.RemoveGameCommand.GestureText
                };

                Items.Add(removeItem);

                // Uninstall
                if (!Game.IsCustomGame && Game.IsInstalled)
                {
                    var uninstallItem = new MenuItem()
                    {
                        Header = resources.FindString("LOCUninstallGame"),
                        Icon = Images.GetEmptyImage(),
                        Command = model.UninstallGameCommand,
                        CommandParameter = Game
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
