using Playnite.Common;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.FullscreenApp.ViewModels
{
    public class ExtensionsMenuViewModels : ObservableObject
    {
        public class MainMenuItemWrapper
        {
            private readonly ICommand switchListCommand;

            public string Header { get; set; }
            public object Icon { get; set; }
            public List<MainMenuItemWrapper> Children { get; set; }
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }

            public MainMenuItemWrapper(List<MenuItem> items, ICommand switchListCommand)
            {
                this.switchListCommand = switchListCommand;
                if (items.Count == 0)
                    return;

                Children = new List<MainMenuItemWrapper>();
                foreach (var item in items)
                {
                    if (item is MenuItem mI)
                        Children.Add(new MainMenuItemWrapper(mI, switchListCommand));
                }
            }

            public MainMenuItemWrapper(MenuItem menuItem, ICommand switchListCommand)
            {
                this.switchListCommand = switchListCommand;
                Command = menuItem.Command;
                Header = menuItem.Header.ToString();
                Icon = menuItem.Icon;
                if (menuItem.Items.Count > 0)
                {
                    Command = switchListCommand;
                    CommandParameter = this;
                    PopulateChildren(this, menuItem);
                }
            }

            private void PopulateChildren(MainMenuItemWrapper wrapper, MenuItem items)
            {
                wrapper.Children = new List<MainMenuItemWrapper>();
                foreach (var item in items.Items)
                {
                    if (item is MenuItem mI)
                        wrapper.Children.Add(new MainMenuItemWrapper(mI, switchListCommand));
                }
            }
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly MainMenuItemWrapper rootItem;
        private readonly Stack<MainMenuItemWrapper> itemsStack = new Stack<MainMenuItemWrapper>();
        public FullscreenAppViewModel MainModel { get; }

        private List<MainMenuItemWrapper> items = new List<MainMenuItemWrapper>();
        public List<MainMenuItemWrapper> Items { get => items; set => SetValue(ref items, value); }

        public RelayCommand CloseCommand { get; }
        public RelayCommand<MainMenuItemWrapper> SwitchListCommand { get; }


        public ExtensionsMenuViewModels(
            IWindowFactory window,
            FullscreenAppViewModel mainModel,
            Game game)
        {
            this.window = window;
            this.MainModel = mainModel;
            CloseCommand = new RelayCommand(Close);
            SwitchListCommand = new RelayCommand<MainMenuItemWrapper>(SwitchList);

            var args = new GetGameMenuItemsArgs() { Games = new List<Game>() { game } };
            var toAdd = new List<GameMenuItem>();
            var rootMenu = new MenuItem();

            foreach (var plugin in mainModel.Extensions.Plugins.Values)
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

            foreach (var script in mainModel.Extensions.Scripts)
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

            if (toAdd.Count == 0)
                toAdd.Add(new GameMenuItem()
                {
                    Description = LOC.NoItemsFound.GetLocalized(),
                    Action = (_) => { }
                });

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
                    newItem = new MenuItem()                        {
                        Header = item.Description,
                        Icon = MenuHelpers.GetIcon(item.Icon)
                    };

                    if (item.Action != null)
                    {
                        ((MenuItem)newItem).Command = new RelayCommand(() =>
                        {
                            try
                            {
                                window.Close(true);
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
                        });
                    }
                }

                if (item.MenuSection.IsNullOrEmpty())
                {
                    rootMenu.Items.Add(newItem);
                }
                else
                {
                    var parent = MenuHelpers.GenerateMenuParents(menuItems, item.MenuSection, rootMenu.Items);
                    parent?.Items.Add(newItem);
                }
            }

            // The above monstrosity and insanity is copied from Desktop mode's menu
            // We'll just wrap that shit in here to something digastable by Fullscreen view

            var temp = new List<MenuItem>();
            foreach (var item in rootMenu.Items)
            {
                if (item is MenuItem menuItem)
                    temp.Add(menuItem);
            }

            rootItem = new MainMenuItemWrapper(temp, SwitchListCommand);
            itemsStack.Push(rootItem);
            Items = rootItem.Children;
        }


        public ExtensionsMenuViewModels(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;
            this.MainModel = mainModel;
            CloseCommand = new RelayCommand(Close);
            SwitchListCommand = new RelayCommand<MainMenuItemWrapper>(SwitchList);

            var args = new GetMainMenuItemsArgs();
            var toAdd = new List<MainMenuItem>();
            foreach (var plugin in mainModel.Extensions.Plugins.Values)
            {
                try
                {
                    var items = plugin.Plugin.GetMainMenuItems(args);
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

            foreach (var script in mainModel.Extensions.Scripts)
            {
                if (script.SupportedMenus.Contains(Scripting.SupportedMenuMethods.MainMenu))
                {
                    try
                    {
                        var items = script.GetMainMenuItems(args);
                        if (items.HasItems())
                        {
                            foreach (var item in items)
                            {
                                var newItem = MainMenuItem.FromScriptMainMenuItem(item);
                                newItem.Action = (a) =>
                                {
                                    script.InvokeFunction(item.FunctionName, new List<object>
                                    {
                                        new ScriptMainMenuItemActionArgs()
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

            if (toAdd.Count == 0)
                toAdd.Add(new MainMenuItem()
                {
                    Description = LOC.NoItemsFound.GetLocalized(),
                    Action = (_) => { }
                });

            var rootMenu = new MenuItem();
            var extensionsMenu = new MenuItem();
            var menuItems = new Dictionary<string, MenuItem>();
            var menuExtensionItems = new Dictionary<string, MenuItem>();
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
                        ((MenuItem)newItem).Command = new RelayCommand(() =>
                        {
                            try
                            {
                                window.Close(true);
                                item.Action(new MainMenuItemActionArgs());
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                logger.Error(e, "Main menu extension action failed.");
                                Dialogs.ShowErrorMessage(
                                    ResourceProvider.GetString("LOCMenuActionExecError") +
                                    Environment.NewLine + Environment.NewLine +
                                    e.Message, "");
                            }
                        });
                    }
                }

                if (item.MenuSection.IsNullOrEmpty())
                {
                    rootMenu.Items.Insert(0, newItem);
                }
                else
                {
                    if (item.MenuSection == "@")
                    {
                        extensionsMenu.Items.Add(newItem);
                    }
                    else if (item.MenuSection.StartsWith("@"))
                    {
                        var parent = MenuHelpers.GenerateMenuParents(menuExtensionItems, item.MenuSection.Substring(1), extensionsMenu.Items);
                        parent?.Items.Add(newItem);
                    }
                    else
                    {
                        var parent = MenuHelpers.GenerateMenuParents(menuItems, item.MenuSection, rootMenu.Items, 0);
                        parent?.Items.Add(newItem);
                    }
                }
            }

            // The above monstrosity and insanity is copied from Desktop mode's menu
            // We'll just wrap that shit in here to something digastable by Fullscreen view

            var temp = new List<MenuItem>();
            foreach (var item in rootMenu.Items)
            {
                if (item is MenuItem menuItem)
                    temp.Add(menuItem);
            }

            foreach (var item in extensionsMenu.Items)
            {
                if (item is MenuItem menuItem)
                    temp.Add(menuItem);
            }

            rootItem = new MainMenuItemWrapper(temp, SwitchListCommand);
            itemsStack.Push(rootItem);
            Items = rootItem.Children;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            if (itemsStack.Count > 0)
                itemsStack.Pop();

            if (itemsStack.Count == 0)
            {
                window.Close(true);
                return;
            }

            Items = itemsStack.Peek().Children;
        }

        public void SwitchList(MainMenuItemWrapper item)
        {
            Items = item.Children;
            itemsStack.Push(item);
        }
    }
}
