using Playnite.Commands;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class MenuItems
    {
        private static ILogger logger = LogManager.GetLogger();

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
        private static object manualIcon;
        private static bool iconsCached = false;

        private static void CacheIcons()
        {
            if (!iconsCached)
            {
                PlayniteApplication.CurrentNative.Dispatcher.Invoke(() =>
                {
                    startIcon = MenuHelpers.GetIcon("PlayIcon");
                    removeIcon = MenuHelpers.GetIcon("RemoveGameIcon");
                    linksIcon = MenuHelpers.GetIcon("LinksIcon");
                    favoriteIcon = MenuHelpers.GetIcon("AddFavoritesIcon");
                    unFavoriteIcon = MenuHelpers.GetIcon("RemoveFavoritesIcon");
                    hideIcon = MenuHelpers.GetIcon("HideIcon");
                    unHideIcon = MenuHelpers.GetIcon("UnHideIcon");
                    browseIcon = MenuHelpers.GetIcon("OpenFolderIcon");
                    shortcutIcon = MenuHelpers.GetIcon("DesktopShortcutIcon");
                    installIcon = MenuHelpers.GetIcon("InstallIcon");
                    editIcon = MenuHelpers.GetIcon("EditGameIcon");
                    manualIcon = MenuHelpers.GetIcon("ManualIcon");
                    iconsCached = true;
                });
            }
        }

        public static List<SearchItem> GetGlobalPluginCommands(MainViewModelBase model)
        {
            var items = new List<SearchItem>();
            var args = new GetMainMenuItemsArgs()
            {
                IsGlobalSearchRequest = true
            };

            foreach (var plugin in model.Extensions.Plugins.Values)
            {
                try
                {
                    var plugItems = plugin.Plugin.GetSearchGlobalCommands()?.ToList();
                    if (plugItems.HasItems())
                    {
                        items.AddRange(plugItems);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get menu items from plugin {plugin.Description.Name}");
                }
            }

            return items;
        }

        public static List<SearchItem> GetSearchExtensionsMainMenuItem(MainViewModelBase model)
        {
            var items = new List<SearchItem>();
            var args = new GetMainMenuItemsArgs()
            {
                IsGlobalSearchRequest = true
            };

            foreach (var plugin in model.Extensions.Plugins.Values)
            {
                try
                {
                    var plugItems = plugin.Plugin.GetMainMenuItems(args);
                    foreach (var item in plugItems ?? new List<MainMenuItem>())
                    {
                        if (item.Description == "-")
                        {
                            continue;
                        }

                        item.MenuSection = item.MenuSection?.TrimStart('@');
                        var description = item.MenuSection.IsNullOrEmpty() ? item.Description : $"{item.MenuSection.Replace("|", " > ")} > {item.Description}";
                        items.Add(new SearchItem(description, LOC.Activate, () => item.Action(new MainMenuItemActionArgs
                        {
                            SourceItem = item
                        }), item.Icon));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get menu items from plugin {plugin.Description.Name}");
                }
            }

            foreach (var script in model.Extensions.Scripts)
            {
                if (script.SupportedMenus.Contains(Scripting.SupportedMenuMethods.MainMenu))
                {
                    try
                    {
                        var plugItems = script.GetMainMenuItems(args);
                        foreach (var item in plugItems ?? new List<ScriptMainMenuItem>())
                        {
                            if (item.Description == "-")
                            {
                                continue;
                            }

                            item.MenuSection = item.MenuSection?.TrimStart('@');
                            var newItem = MainMenuItem.FromScriptMainMenuItem(item);
                            newItem.Action = (a) =>
                            {
                                script.InvokeFunction(item.FunctionName, new List<object>
                                {
                                    new ScriptMainMenuItemActionArgs()
                                });
                            };

                            var description = item.MenuSection.IsNullOrEmpty() ? item.Description : $"{item.MenuSection.Replace("|", " > ")} > {item.Description}";
                            items.Add(new SearchItem(description, LOC.Activate, () =>
                            {
                                script.InvokeFunction(item.FunctionName, new List<object>
                                {
                                    new ScriptMainMenuItemActionArgs
                                    {
                                        SourceItem = item
                                    }
                                });
                            }, item.Icon));
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to get menu items from script {script.Name}");
                    }
                }
            }

            return items;
        }

        public static List<SearchItem> GetSearchGameMenuItems(Game game, MainViewModelBase model)
        {
            CacheIcons();
            var items = new List<SearchItem>();

            // Play/Install
            if (game.IsInstalled)
            {
                items.Add(new SearchItem(LOC.PlayGame, LOC.Activate, () => model.StartGame(game), startIcon));
            }
            else if (!game.IsCustomGame)
            {
                items.Add(new SearchItem(LOC.InstallGame, LOC.Activate, () => model.InstallGame(game), installIcon));
            }

            // Custom Actions
            foreach (var task in game.GameActions?.Where(a => !a.IsPlayAction) ?? Enumerable.Empty<GameAction>())
            {
                items.Add(new SearchItem(task.Name, LOC.Activate, () => model.App.GamesEditor.ActivateAction(game, task)));
            }

            // Links
            if (game.Links.HasItems())
            {
                var links = new List<SearchItem>();
                foreach (var link in game.Links.Where(a => a != null))
                {
                    links.Add(new SearchItem(link.Name, LOC.Activate, () =>
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
                    {
                        Description = link.Url
                    });
                }

                items.Add(new SearchItem(LOC.LinksLabel, new ContextSwitchSearchItemAction(LOC.Activate, new GenericListSearchContext(links, LOC.LinksLabel)), linksIcon));
            }

            // Open Game Location
            if (game.IsInstalled)
            {
                items.Add(new SearchItem(LOC.OpenGameLocation, LOC.Activate, () => model.App.GamesEditor.OpenGameLocation(game), browseIcon));
            }

            // Create Desktop Shortcut
            items.Add(new SearchItem(LOC.CreateDesktopShortcut, LOC.Activate, () => model.App.GamesEditor.CreateDesktopShortcut(game), shortcutIcon));

            // Manual
            if (!game.Manual.IsNullOrEmpty())
            {
                items.Add(new SearchItem(LOC.OpenGameManual, LOC.Activate, () => model.App.GamesEditor.OpenManual(game), manualIcon));
            }

            // Toggle Favorites
            items.Add(new SearchItem(
                game.Favorite ? LOC.RemoveFavoriteGame : LOC.FavoriteGame,
                LOC.Activate,
                () => model.App.GamesEditor.ToggleFavoriteGame(game))
                { Icon = game.Favorite ? unFavoriteIcon : favoriteIcon });

            // Toggle Hide
            items.Add(new SearchItem(
                game.Hidden ? LOC.UnHideGame : LOC.HideGame,
                LOC.Activate,
                () => model.App.GamesEditor.ToggleHideGame(game))
                { Icon = game.Hidden ? unHideIcon : hideIcon });

            // Edit
            items.Add(new SearchItem(LOC.EditGame, LOC.Activate, () => model.EditGame(game), editIcon));

            // Set Category
            items.Add(new SearchItem(LOC.SetGameCategory, LOC.Activate, () => model.AssignCategories(game)));

            // Set Completion Status
            var complStats = new List<SearchItem>();
            foreach (var status in model.Database.CompletionStatuses)
            {
                complStats.Add(new SearchItem(status.Name, LOC.Assign, () => model.App.GamesEditor.SetCompletionStatus(game, status)));
            }

            if (complStats.HasItems())
            {
                items.Add(new SearchItem(LOC.SetCompletionStatus, new ContextSwitchSearchItemAction(LOC.Activate, new GenericListSearchContext(complStats, LOC.CompletionStatus))));
            }

            // Extensions items
            var args = new GetGameMenuItemsArgs()
            {
                Games = new List<Game>(1) { game },
                IsGlobalSearchRequest = true
            };

            foreach (var plugin in model.Extensions.Plugins.Values)
            {
                try
                {
                    var plugItems = plugin.Plugin.GetGameMenuItems(args);
                    foreach (var item in plugItems ?? new List<GameMenuItem>())
                    {
                        if (item.Description == "-")
                        {
                            continue;
                        }

                        var description = item.MenuSection.IsNullOrEmpty() ? item.Description : $"{item.MenuSection.Replace("|", " > ")} > {item.Description}";
                        items.Add(new SearchItem(description, LOC.Activate, () => item.Action(new GameMenuItemActionArgs
                        {
                            Games = args.Games,
                            SourceItem = item
                        }), item.Icon));
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
                        var plugItems = script.GetGameMenuItems(args);
                        foreach (var item in plugItems ?? new List<ScriptGameMenuItem>())
                        {
                            if (item.Description == "-")
                            {
                                continue;
                            }

                            var description = item.MenuSection.IsNullOrEmpty() ? item.Description : $"{item.MenuSection.Replace("|", " > ")} > {item.Description}";
                            items.Add(new SearchItem(description, LOC.Activate, () =>
                            {
                                script.InvokeFunction(item.FunctionName, new List<object>
                                {
                                    new ScriptGameMenuItemActionArgs
                                    {
                                        Games = args.Games,
                                        SourceItem = item
                                    }
                                });
                            }, item.Icon));
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to get menu items from script {script.Name}");
                    }
                }
            }

            // Remove
            items.Add(new SearchItem(LOC.RemoveGame, LOC.Activate, () => model.App.GamesEditor.RemoveGame(game), removeIcon));

            // Uninstall
            if (!game.IsCustomGame && game.IsInstalled)
            {
                items.Add(new SearchItem(LOC.UninstallGame, LOC.Activate, () => model.App.GamesEditor.UnInstallGame(game)));
            }

            return items;
        }
    }
}
