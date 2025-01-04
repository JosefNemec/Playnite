using Playnite.Commands;
using Playnite.Common;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
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
        private static readonly ILogger logger = LogManager.GetLogger();

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
                typeof(GameMenu));

        private DesktopAppViewModel model;

        private static object startIcon;
        private static object removeIcon;
        private static object linksIcon;
        private static object favoriteIcon;
        private static object unFavoriteIcon;
        private static object hideIcon;
        private static object unHideIcon;
        private static object browseIcon;
        private static object installSizeIcon;
        private static object shortcutIcon;
        private static object installIcon;
        private static object editIcon;
        private static object manualIcon;
        private static bool iconsLoaded = false;

        static GameMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
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

            this.model = model;
            Opened += GameMenu_Opened;
            Closed += GameMenu_Closed;
        }

        private void GameMenu_Closed(object sender, RoutedEventArgs e)
        {
            Deinitialize();
        }

        private void GameMenu_Opened(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }

        public void Deinitialize()
        {
            Items.Clear();
        }

        private void InitializeItems(Game game)
        {
            // Play / Install
            if (ShowStartSection)
            {
                bool added = false;
                if (game.IsInstalled)
                {
                    var playItem = new MenuItem()
                    {
                        Header = ResourceProvider.GetString(LOC.PlayGame),
                        Icon = startIcon,
                        FontWeight = FontWeights.Bold,
                        Command = model.StartGameCommand,
                        CommandParameter = game,
                        InputGestureText = model.StartSelectedGameCommand.GestureText
                    };

                    Items.Add(playItem);
                    added = true;
                }
                else if (!game.IsCustomGame)
                {
                    var installItem = new MenuItem()
                    {
                        Header = ResourceProvider.GetString(LOC.InstallGame),
                        Icon = installIcon,
                        FontWeight = FontWeights.Bold,
                        Command = model.InstallGameCommand,
                        CommandParameter = game
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
            var customAdded = false;
            foreach (var task in game.GameActions?.Where(a => !a.IsPlayAction) ?? Enumerable.Empty<GameAction>())
            {
                var taskItem = new MenuItem { Header = task.Name };
                taskItem.Click += (s, e) => model.GamesEditor.ActivateAction(game, task);
                Items.Add(taskItem);
                customAdded = true;
            }

            if (customAdded)
            {
                Items.Add(new Separator());
            }

            // Links
            if (game.Links?.Any() == true)
            {
                var linksItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.LinksLabel),
                    Icon = linksIcon
                };

                foreach (var link in game.Links)
                {
                    if (link != null)
                    {
                        linksItem.Items.Add(new MenuItem()
                        {
                            Header = link.Name,
                            Command = new RelayCommand<Link>((_) =>
                            {
                                try
                                {
                                    GlobalCommands.NavigateUrl(game.ExpandVariables(link.Url));
                                }
                                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                                {
                                    logger.Error(e, "Failed to open url.");
                                }
                            })
                        });
                    }
                }

                Items.Add(linksItem);
                Items.Add(new Separator());
            }

            // Open Game Location
            if (game.IsInstalled)
            {
                var locationItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.OpenGameLocation),
                    Icon = browseIcon,
                    Command = model.OpenGameLocationCommand,
                    CommandParameter = game
                };

                Items.Add(locationItem);
            }

            // Create Desktop Shortcut
            var shortcutItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.CreateDesktopShortcut),
                Icon = shortcutIcon,
                Command = model.CreateDesktopShortcutCommand,
                CommandParameter = game
            };

            Items.Add(shortcutItem);

            // InstallSize
            if (game.IsInstalled)
            {
                Items.Add(new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.CalculateInstallSize),
                    Icon = installSizeIcon,
                    Command = model.UpdateGameInstallSizeWithDialogCommand,
                    CommandParameter = game
                });
            }

            // Manual
            if (!game.Manual.IsNullOrEmpty())
            {
                Items.Add(new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.OpenGameManual),
                    Icon = manualIcon,
                    Command = model.OpenManualCommand,
                    CommandParameter = game
                });
            }

            Items.Add(new Separator());

            // Toggle Favorites
            var favoriteItem = new MenuItem()
            {
                Header = game.Favorite ? ResourceProvider.GetString(LOC.RemoveFavoriteGame) : ResourceProvider.GetString(LOC.FavoriteGame),
                Icon = game.Favorite ? unFavoriteIcon : favoriteIcon,
                Command = model.ToggleFavoritesCommand,
                CommandParameter = game
            };

            Items.Add(favoriteItem);

            // Toggle Hide
            var hideItem = new MenuItem()
            {
                Header = game.Hidden ? ResourceProvider.GetString(LOC.UnHideGame) : ResourceProvider.GetString(LOC.HideGame),
                Icon = game.Hidden ? unHideIcon : hideIcon,
                Command = model.ToggleVisibilityCommand,
                CommandParameter = game
            };

            Items.Add(hideItem);

            // Edit
            var editItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.EditGame),
                Icon = editIcon,
                Command = model.EditGameCommand,
                CommandParameter = game,
                InputGestureText = model.EditSelectedGamesCommand.GestureText
            };

            Items.Add(editItem);

            // Set Category
            var categoryItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.SetGameCategory),
                //Icon = Images.GetEmptyImage(),
                Command = model.AssignGameCategoryCommand,
                CommandParameter = game
            };

            Items.Add(categoryItem);

            // Set Completion Status
            Items.Add(LoadCompletionStatusItem(game));

            // Extensions items
            AddExtensionItems(new List<Game>(1) { game });
            Items.Add(new Separator());

            // Remove
            var removeItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.RemoveGame),
                Icon = removeIcon,
                Command = model.RemoveGameCommand,
                CommandParameter = game,
                InputGestureText = model.RemoveGameCommand.GestureText
            };

            Items.Add(removeItem);

            // Uninstall
            if (!game.IsCustomGame && game.IsInstalled)
            {
                var uninstallItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.UninstallGame),
                    //Icon = Images.GetEmptyImage(),
                    Command = model.UninstallGameCommand,
                    CommandParameter = game
                };

                Items.Add(uninstallItem);
            }
        }

        private void InitializeItems(List<Game> games)
        {
            // Create Desktop Shortcut
            var shortcutItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.CreateDesktopShortcut),
                Icon = shortcutIcon,
                Command = model.CreateDesktopShortcutsCommand,
                CommandParameter = games
            };

            Items.Add(shortcutItem);

            if (!games.All(a => a.Favorite))
            {
                var favoriteItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.FavoriteGame),
                    Icon = favoriteIcon,
                    Command = model.SetAsFavoritesCommand,
                    CommandParameter = games
                };

                Items.Add(favoriteItem);
            }

            if (!games.All(a => !a.Favorite))
            {
                var unFavoriteItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.RemoveFavoriteGame),
                    Icon = unFavoriteIcon,
                    Command = model.RemoveAsFavoritesCommand,
                    CommandParameter = games
                };

                Items.Add(unFavoriteItem);
            }

            if (!games.All(a => a.Hidden))
            {
                var hideItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.HideGame),
                    Icon = hideIcon,
                    Command = model.SetAsHiddensCommand,
                    CommandParameter = games
                };

                Items.Add(hideItem);
            }

            if (!games.All(a => !a.Hidden))
            {
                var unHideItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.UnHideGame),
                    Icon = unHideIcon,
                    Command = model.RemoveAsHiddensCommand,
                    CommandParameter = games
                };

                Items.Add(unHideItem);
            }

            // InstallSize
            if (games.Any(x => x.IsInstalled))
            {
                var installSizeItem = new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.InstallSizeMenuLabel),
                    Icon = installSizeIcon
                };

                installSizeItem.Items.Add(new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.CalculateGamesAllInstallSize),
                    Command = model.UpdateGamesAllInstallSizeWithDialogCommand,
                    CommandParameter = games
                });

                installSizeItem.Items.Add(new MenuItem()
                {
                    Header = ResourceProvider.GetString(LOC.CalculateGamesMissingInstallSize),
                    Command = model.UpdateGamesMissingInstallSizeWithDialogCommand,
                    CommandParameter = games
                });

                Items.Add(installSizeItem);
            }

            // Edit
            var editItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.EditGame),
                Icon = editIcon,
                Command = model.EditGamesCommand,
                CommandParameter = games,
                InputGestureText = model.EditSelectedGamesCommand.GestureText
            };

            Items.Add(editItem);

            // Set Category
            var categoryItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.SetGameCategory),
                //Icon = Images.GetEmptyImage(),
                Command = model.AssignGamesCategoryCommand,
                CommandParameter = games
            };

            Items.Add(categoryItem);

            // Set Completion Status
            Items.Add(LoadCompletionStatusItem(games));

            // Extensions items
            AddExtensionItems(games);
            Items.Add(new Separator());

            // Remove
            var removeItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.RemoveGame),
                Icon = removeIcon,
                Command = model.RemoveGamesCommand,
                CommandParameter = games,
                InputGestureText = model.RemoveSelectedGamesCommand.GestureText
            };

            Items.Add(removeItem);
        }

        public void InitializeItems()
        {
            // Have to load icons as late as possible to make sure ovewritten theme resources are loaded.
            if (!iconsLoaded)
            {
                startIcon = MenuHelpers.GetIcon("PlayIcon");
                removeIcon = MenuHelpers.GetIcon("RemoveGameIcon");
                linksIcon = MenuHelpers.GetIcon("LinksIcon");
                favoriteIcon = MenuHelpers.GetIcon("AddFavoritesIcon");
                unFavoriteIcon = MenuHelpers.GetIcon("RemoveFavoritesIcon");
                hideIcon = MenuHelpers.GetIcon("HideIcon");
                unHideIcon = MenuHelpers.GetIcon("UnHideIcon");
                browseIcon = MenuHelpers.GetIcon("OpenFolderIcon");
                installSizeIcon = MenuHelpers.GetIcon("InstallSizeIcon");
                shortcutIcon = MenuHelpers.GetIcon("DesktopShortcutIcon");
                installIcon = MenuHelpers.GetIcon("InstallIcon");
                editIcon = MenuHelpers.GetIcon("EditGameIcon");
                manualIcon = MenuHelpers.GetIcon("ManualIcon");
                iconsLoaded = true;
            }

            Items.Clear();

            Game game = null;
            List<Game> games = null;

            if (DataContext is GamesCollectionViewEntry entry)
            {
                game = entry.Game;
            }
            else if (DataContext is IEnumerable<GamesCollectionViewEntry> entries)
            {
                if (entries.Count() > 0)
                {
                    game = (entries.First() as GamesCollectionViewEntry).Game;
                }

                if (entries.Count() == 1)
                {
                    games = null;
                }
                else
                {
                    games = entries.Select(a => (a as GamesCollectionViewEntry).Game).ToList();
                }
            }
            else if (DataContext is IList<object> entries2)
            {
                if (entries2.Count() > 0)
                {
                    game = (entries2.First() as GamesCollectionViewEntry).Game;
                }

                if (entries2.Count() == 1)
                {
                    games = null;
                }
                else
                {
                    games = entries2.Select(a => (a as GamesCollectionViewEntry).Game).ToList();
                }
            }

            if (games?.Count == 0 && game == null)
            {
                return;
            }

            if (games != null)
            {
                InitializeItems(games);
            }
            else if (game != null)
            {
                InitializeItems(game);
            }
        }

        private MenuItem LoadCompletionStatusItem(List<Game> games)
        {
            var completionItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.SetCompletionStatus)
            };

            foreach (var status in model.Database.CompletionStatuses.OrderBy(a => a.Name))
            {
                completionItem.Items.Add(new MenuItem
                {
                    Header = status.Name,
                    Command = model.SetGamesCompletionStatusCommand,
                    CommandParameter = new Tuple<IEnumerable<Game>, CompletionStatus>(games, status)
                });
            }

            return completionItem;
        }

        private MenuItem LoadCompletionStatusItem(Game game)
        {
            var completionItem = new MenuItem()
            {
                Header = ResourceProvider.GetString(LOC.SetCompletionStatus)
            };

            foreach (var status in model.Database.CompletionStatuses.OrderBy(a => a.Name))
            {
                completionItem.Items.Add(new MenuItem
                {
                    Header = status.Name,
                    Command = model.SetGameCompletionStatusCommand,
                    CommandParameter = new Tuple<Game, CompletionStatus>(game, status),
                    IsChecked = game.CompletionStatusId == status.Id
                });
            }

            return completionItem;
        }

        private void AddExtensionItems(List<Game> games)
        {
            var args = new GetGameMenuItemsArgs();
            var toAdd = new List<GameMenuItem>();
            args.Games = games;

            foreach (var plugin in model.Extensions.Plugins.Values)
            {
                try
                {
                    var items = plugin.Plugin.GetGameMenuItems(args);
                    if (items.HasItems())
                    {
                        toAdd.AddRange(items);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get menu items from plugin {plugin.Description.Name}");
                }
            }

            foreach (var script in model.Extensions.Scripts)
            {
                if (script.SupportedMenus.Contains(Scripting.SupportedMenuMethods.GameMenu))
                {
                    try
                    {
                        var items = script.GetGameMenuItems(args);
                        if (items.HasItems())
                        {
                            foreach (var item in items)
                            {
                                var newItem = GameMenuItem.FromScriptGameMenuItem(item);
                                newItem.Action = (a) =>
                                {
                                    script.InvokeFunction(item.FunctionName, new List<object>
                                    {
                                        new ScriptGameMenuItemActionArgs
                                        {
                                            Games = a.Games,
                                            SourceItem = item
                                        }
                                    });
                                };

                                toAdd.Add(newItem);
                            }
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to get menu items from script {script.Name}");
                    }
                }
            }

            if (toAdd.Count > 0)
            {
                Items.Add(new Separator());
                var menuItems = new Dictionary<string, MenuItem>();
                foreach (var item in toAdd)
                {
                    object newItem = null;
                    if (item.Description == "-")
                    {
                        newItem = new Separator();
                    }
                    else
                    {
                        newItem = new MenuItem()
                        {
                            Header = item.Description,
                            Icon = MenuHelpers.GetIcon(item.Icon)
                        };

                        if (item.Action != null)
                        {
                            ((MenuItem)newItem).Click += (_, __) =>
                            {
                                try
                                {
                                    item.Action(new GameMenuItemActionArgs
                                    {
                                        Games = args.Games,
                                        SourceItem = item
                                    });
                                }
                                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                                {
                                    logger.Error(e, "Game menu extension action failed.");
                                    Dialogs.ShowErrorMessage(
                                        ResourceProvider.GetString(LOC.MenuActionExecError) +
                                        Environment.NewLine + Environment.NewLine +
                                        e.Message, "");
                                }
                            };
                        }
                    }

                    if (item.MenuSection.IsNullOrEmpty())
                    {
                        Items.Add(newItem);
                    }
                    else
                    {
                        var parent = MenuHelpers.GenerateMenuParents(menuItems, item.MenuSection, Items);
                        parent?.Items.Add(newItem);
                    }
                }
            }
        }
    }
}
