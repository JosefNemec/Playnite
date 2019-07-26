using Playnite.Common;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
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

namespace Playnite.DesktopApp.Controls
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
            DependencyProperty.Register(
                nameof(ShowStartSection),
                typeof(bool),
                typeof(GameMenu),
                new PropertyMetadata(true, ShowStartSectionPropertyChangedCallback));

        private static void ShowStartSectionPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as GameMenu;
            obj.InitializeItems();
        }

        private IResourceProvider resources;
        private DesktopAppViewModel model;
        private readonly SynchronizationContext context;

        private static object startIcon;
        private static object removeIcon;
        private static object linksIcon;
        private static object favoriteIcon;
        private static object unFavoriteIcon;
        private static object hideIcon;
        private static object unHideIcon;
        private static object browseIcon;
        private static object shortcutIcon;
        private static object installIcon;
        private static object editIcon;

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
            startIcon = GetIcon("PlayIcon");
            removeIcon = GetIcon("RemoveGameIcon");
            linksIcon = GetIcon("LinksIcon");
            favoriteIcon = GetIcon("AddFavoritesIcon");
            unFavoriteIcon = GetIcon("RemoveFavoritesIcon");
            hideIcon = GetIcon("HideIcon");
            unHideIcon = GetIcon("UnHideIcon");
            browseIcon = GetIcon("OpenFolderIcon");
            shortcutIcon = GetIcon("DesktopShortcutIcon");
            installIcon = GetIcon("InstallIcon");
            editIcon = GetIcon("EditGameIcon");
        }

        public GameMenu() : this(DesktopApplication.Current?.MainModel)
        {            
        }

        public GameMenu(DesktopAppViewModel model)
        {
            if (model == null)
            {
                return;
            }

            context = SynchronizationContext.Current;
            this.model = model;
            resources = new ResourceProvider();
            Opened += GameMenu_Opened;
            DataContextChanged += GameMenu_DataContextChanged;
            InitializeItems();
        }

        private void GameMenu_Opened(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }

        private static object GetIcon(string iconName)
        {
            var resource = ResourceProvider.GetResource(iconName);
            if (resource != null)
            {
                if (resource is string stringIcon)
                {
                    return Images.GetImageFromFile(ThemeFile.GetFilePath(stringIcon));
                }
                else if (resource is BitmapImage bitmap)
                {
                    var image = new Image() { Source = bitmap };
                    RenderOptions.SetBitmapScalingMode(image, RenderOptions.GetBitmapScalingMode(bitmap));
                    return image;
                }
                else if (resource is TextBlock textIcon)
                {
                    return textIcon;
                }
            }

            return null;
        }

        private void GameMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is GamesCollectionViewEntry entry)
            {
                AssignGame(entry.Game);
            }
            else if (DataContext is IEnumerable<GamesCollectionViewEntry> entries)
            {
                if (entries.Count() > 0)
                {
                    AssignGame((entries.First() as GamesCollectionViewEntry).Game);
                }

                if (entries.Count() == 1)
                {
                    Games = null;
                }
                else
                {
                    Games = entries.Select(a => (a as GamesCollectionViewEntry).Game).ToList();
                }
            }
            else if (DataContext is IList<object> entries2)
            {
                if (entries2.Count() > 0)
                {
                    AssignGame((entries2.First() as GamesCollectionViewEntry).Game);
                }

                if (entries2.Count() == 1)
                {
                    Games = null;
                }
                else
                {
                    Games = entries2.Select(a => (a as GamesCollectionViewEntry).Game).ToList();
                }
            }
            else
            {
                AssignGame(null);
                Games = null;
            }
        }

        private void AssignGame(Game game)
        {
            Game = game;
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
                    Header = resources.GetString("LOCFavoriteGame"),
                    Icon = favoriteIcon,
                    Command = model.SetAsFavoritesCommand,
                    CommandParameter = Games
                };

                Items.Add(favoriteItem);

                var unFavoriteItem = new MenuItem()
                {
                    Header = resources.GetString("LOCRemoveFavoriteGame"),
                    Icon = unFavoriteIcon,
                    Command = model.RemoveAsFavoritesCommand,
                    CommandParameter = Games
                };

                Items.Add(unFavoriteItem);

                // Set Hide
                var hideItem = new MenuItem()
                {
                    Header = resources.GetString("LOCHideGame"),
                    Icon = hideIcon,
                    Command = model.SetAsHiddensCommand,
                    CommandParameter = Games
                };

                Items.Add(hideItem);

                var unHideItem = new MenuItem()
                {
                    Header = resources.GetString("LOCUnHideGame"),
                    Icon = unHideIcon,
                    Command = model.RemoveAsHiddensCommand,
                    CommandParameter = Games
                };

                Items.Add(unHideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.GetString("LOCEditGame"),
                    Icon = editIcon,
                    Command = model.EditGamesCommand,
                    CommandParameter = Games,
                    InputGestureText = model.EditSelectedGamesCommand.GestureText
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.GetString("LOCSetGameCategory"),
                    //Icon = Images.GetEmptyImage(),
                    Command = model.AssignGamesCategoryCommand,
                    CommandParameter = Games
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());

                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.GetString("LOCRemoveGame"),
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
                            Header = resources.GetString("LOCPlayGame"),
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
                            Header = resources.GetString("LOCInstallGame"),
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
                            //Icon = Images.GetEmptyImage()
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
                        Header = resources.GetString("LOCLinksLabel"),
                        Icon = linksIcon
                    };

                    foreach (var link in Game.Links)
                    {
                        linksItem.Items.Add(new MenuItem()
                        {
                            Header = link.Name,
                            Command = Commands.GlobalCommands.NavigateUrlCommand,
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
                        Header = resources.GetString("LOCOpenGameLocation"),
                        Icon = browseIcon,
                        Command = model.OpenGameLocationCommand,
                        CommandParameter = Game
                    };

                    Items.Add(locationItem);
                }

                // Create Desktop Shortcut
                var shortcutItem = new MenuItem()
                {
                    Header = resources.GetString("LOCCreateDesktopShortcut"),
                    Icon = shortcutIcon,
                    Command = model.CreateGameShortcutCommand,
                    CommandParameter = Game
                };

                Items.Add(shortcutItem);
                Items.Add(new Separator());

                // Toggle Favorites
                var favoriteItem = new MenuItem()
                {
                    Header = Game.Favorite ? resources.GetString("LOCRemoveFavoriteGame") : resources.GetString("LOCFavoriteGame"),
                    Icon = Game.Favorite ? unFavoriteIcon : favoriteIcon,
                    Command = model.ToggleFavoritesCommand,
                    CommandParameter = Game                    
                };

                Items.Add(favoriteItem);

                // Toggle Hide
                var hideItem = new MenuItem()
                {
                    Header = Game.Hidden ? resources.GetString("LOCUnHideGame") : resources.GetString("LOCHideGame"),
                    Icon = Game.Hidden ? unHideIcon : hideIcon,
                    Command = model.ToggleVisibilityCommand,
                    CommandParameter = Game
                };

                Items.Add(hideItem);

                // Edit
                var editItem = new MenuItem()
                {
                    Header = resources.GetString("LOCEditGame"),
                    Icon = editIcon,
                    Command = model.EditGameCommand,
                    CommandParameter = Game,
                    InputGestureText = model.EditSelectedGamesCommand.GestureText
                };

                Items.Add(editItem);

                // Set Category
                var categoryItem = new MenuItem()
                {
                    Header = resources.GetString("LOCSetGameCategory"),
                    //Icon = Images.GetEmptyImage(),
                    Command = model.AssignGameCategoryCommand,
                    CommandParameter = Game
                };

                Items.Add(categoryItem);
                Items.Add(new Separator());
                
                // Remove
                var removeItem = new MenuItem()
                {
                    Header = resources.GetString("LOCRemoveGame"),
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
                        Header = resources.GetString("LOCUninstallGame"),
                        //Icon = Images.GetEmptyImage(),
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
