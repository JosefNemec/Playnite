using Playnite.Commands;
using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions.Markup;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class MainMenu : ContextMenu
    {
        private readonly DesktopAppViewModel mainModel;
        private MenuItem extensionsItem;
        private MenuItem toolsItem;
        private Separator extensionsEndItem;
        private static readonly ILogger logger = LogManager.GetLogger();

        static MainMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainMenu), new FrameworkPropertyMetadata(typeof(MainMenu)));
        }

        public MainMenu() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public MainMenu(DesktopAppViewModel model)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (model != null)
            {
                mainModel = model;
                InitializeItems();
                Opened += MainMenu_Opened;
                Closed += MainMenu_Closed;
            }
        }

        public static MenuItem AddMenuChild(
            ItemCollection parent,
            string locString,
            RelayCommand command,
            object commandParameter = null,
            string icon = null)
        {
            var item = new MenuItem
            {
                Command = command,
                CommandParameter = commandParameter,
                InputGestureText = command?.GestureText
            };

            if (locString.StartsWith("LOC"))
            {
                item.SetResourceReference(MenuItem.HeaderProperty, locString);
            }
            else
            {
                item.Header = locString;
            }

            var iconObj = MenuHelpers.GetIcon(icon);
            if (iconObj != null)
            {
                item.Icon = iconObj;
            }

            parent.Add(item);
            return item;
        }

        public void InitializeItems()
        {
            // Add Game
            var addGameItem = AddMenuChild(Items, "LOCMenuAddGame", null, null, "AddGameIcon");
            AddMenuChild(addGameItem.Items, "LOCMenuAddGameManual", mainModel.AddCustomGameCommand);
            AddMenuChild(addGameItem.Items, "LOCMenuAddGameInstalled", mainModel.AddInstalledGamesCommand);
            AddMenuChild(addGameItem.Items, "LOCMenuAddGameEmulated", mainModel.AddEmulatedGamesCommand);
            if (Computer.WindowsVersion == WindowsVersion.Win10)
            {
                AddMenuChild(addGameItem.Items, "LOCMenuAddWindowsStore", mainModel.AddWindowsStoreGamesCommand);
            }

            Items.Add(new Separator());

            // Library
            var libraryItem = AddMenuChild(Items, "LOCLibrary", null);
            AddMenuChild(libraryItem.Items, "LOCMenuLibraryManagerTitle", mainModel.OpenDbFieldsManagerCommand);
            AddMenuChild(libraryItem.Items, "LOCMenuConfigureEmulatorsMenuTitle", mainModel.OpenEmulatorsCommand);
            AddMenuChild(libraryItem.Items, "LOCMenuDownloadMetadata", mainModel.DownloadMetadataCommand);
            AddMenuChild(libraryItem.Items, "LOCMenuSoftwareTools", mainModel.OpenSoftwareToolsCommand);

            // Update Library
            var updateItem = AddMenuChild(Items, "LOCMenuReloadLibrary", null, null, "UpdateDbIcon");
            AddMenuChild(updateItem.Items, "LOCUpdateAll", mainModel.UpdateGamesCommand);
            updateItem.Items.Add(new Separator());
            foreach (var plugin in mainModel.Extensions.LibraryPlugins)
            {
                var item = new MenuItem
                {
                    Header = plugin.Name,
                    Command = mainModel.UpdateLibraryCommand,
                    CommandParameter = plugin
                };

                updateItem.Items.Add(item);
            }

            // Random game select
            AddMenuChild(Items, "LOCMenuSelectRandomGame", mainModel.SelectRandomGameCommand, null, "DiceIcon");

            // Settings
            AddMenuChild(Items, "LOCMenuPlayniteSettingsTitle", mainModel.OpenSettingsCommand, null, "SettingsIcon");

            // View
            var viewItem = AddMenuChild(Items, LOC.MenuView, null, null, null);
            var sideBarItem = AddMenuChild(viewItem.Items, LOC.Sidebar, null, null, null);
            var sideBarEnableItem = AddMenuChild(sideBarItem.Items, LOC.EnabledTitle, null);
            sideBarEnableItem.IsCheckable = true;
            BindingOperations.SetBinding(sideBarEnableItem, MenuItem.IsCheckedProperty,
                new Binding
                {
                    Source = mainModel.AppSettings,
                    Path = new PropertyPath(nameof(PlayniteSettings.SidebarVisible))
                });

            sideBarItem.Items.Add(new Separator());
            MenuHelpers.PopulateEnumOptions<Dock>(sideBarItem.Items, nameof(PlayniteSettings.SidebarPosition), mainModel.AppSettings);
            viewItem.Items.Add(new Separator());

            var librarySideItem = MainMenu.AddMenuChild(viewItem.Items, LOC.Library, null);
            librarySideItem.IsCheckable = true;
            MenuHelpers.SetEnumBinding(librarySideItem, nameof(mainModel.AppSettings.CurrentApplicationView), mainModel.AppSettings, ApplicationView.Library);

            var statsSideItem = MainMenu.AddMenuChild(viewItem.Items, LOC.Statistics, null);
            statsSideItem.IsCheckable = true;
            MenuHelpers.SetEnumBinding(statsSideItem, nameof(mainModel.AppSettings.CurrentApplicationView), mainModel.AppSettings, ApplicationView.Statistics);

            Items.Add(new Separator());

            // Open Client
            var openClientItem = AddMenuChild(Items, "LOCMenuClients", null);
            foreach (var tool in mainModel.ThirdPartyTools)
            {
                var item = new MenuItem
                {
                    Header = tool.Name,
                    Command = mainModel.ThirdPartyToolOpenCommand,
                    CommandParameter = tool,
                    Icon = tool.Icon
                };

                openClientItem.Items.Add(item);
            }

            // Tools
            toolsItem = AddMenuChild(Items, "LOCMenuTools", null);

            // Extensions
            extensionsItem = AddMenuChild(Items, "LOCExtensions", null);

            // FullScreen
            extensionsEndItem = new Separator();
            Items.Add(extensionsEndItem);
            AddMenuChild(Items, "LOCMenuOpenFullscreen", mainModel.OpenFullScreenCommand, null, "FullscreenModeIcon");
            Items.Add(new Separator());

            // Links
            var linksItem = AddMenuChild(Items, "LOCMenuLinksTitle", null);
            AddMenuChild(linksItem.Items, "LOCCommonLinksForum", GlobalCommands.NavigateUrlCommand, UrlConstants.Forum, "Images/applogo.png");
            AddMenuChild(linksItem.Items, "Discord", GlobalCommands.NavigateUrlCommand, UrlConstants.Discord, "Images/discord.png");
            AddMenuChild(linksItem.Items, "Twitter", GlobalCommands.NavigateUrlCommand, UrlConstants.Twitter, "Images/twitter.png");
            AddMenuChild(linksItem.Items, "Reddit", GlobalCommands.NavigateUrlCommand, UrlConstants.Reddit, "Images/reddit.png");

            // Help
            var helpItem = AddMenuChild(Items, "LOCMenuHelpTitle", null);
            AddMenuChild(helpItem.Items, "Wiki / FAQ", GlobalCommands.NavigateUrlCommand, UrlConstants.Wiki);
            AddMenuChild(helpItem.Items, "LOCMenuIssues", mainModel.ReportIssueCommand);
            AddMenuChild(helpItem.Items, "LOCSDKDocumentation", GlobalCommands.NavigateUrlCommand, UrlConstants.SdkDocs);
            helpItem.Items.Add(new Separator());
            AddMenuChild(helpItem.Items, "LOCCheckForUpdates", mainModel.CheckForUpdateCommand);

            // Patreon
            AddMenuChild(Items, "LOCMenuPatreonSupport", GlobalCommands.NavigateUrlCommand, UrlConstants.Patreon, "Images/patreon.png");

            // About
            AddMenuChild(Items, "LOCMenuAbout", mainModel.OpenAboutCommand, null, "AboutPlayniteIcon");
            Items.Add(new Separator());

            // Exit
            AddMenuChild(Items, "LOCExitAppLabel", mainModel.ShutdownCommand, null, "ExitIcon");
        }

        private void MainMenu_Closed(object sender, RoutedEventArgs e)
        {
            ClearExtensionItems();
        }

        private void MainMenu_Opened(object sender, RoutedEventArgs e)
        {
            AddExtensionItems();
            AddToolsItems();
        }

        private void AddToolsItems()
        {
            toolsItem.Items.Clear();
            if (mainModel.Database.SoftwareApps.HasItems())
            {
                foreach (var tool in mainModel.Database.SoftwareApps.OrderBy(a => a.Name))
                {
                    AddMenuChild(
                        toolsItem.Items,
                        tool.Name,
                        mainModel.StartSoftwareToolCommand,
                        tool,
                        mainModel.Database.GetFullFilePath(tool.Icon));
                }

                toolsItem.Visibility = Visibility.Visible;
            }
            else
            {
                toolsItem.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearExtensionItems()
        {
            if (extensionsEndItem == null)
            {
                return;
            }

            var startIndex = Items.IndexOf(extensionsItem);
            var endIndex = Items.IndexOf(extensionsEndItem);
            if (endIndex > startIndex + 1)
            {
                for (int i = 0; i < endIndex - startIndex - 1; i++)
                {
                    Items.RemoveAt(startIndex + 1);
                }
            }
        }

        private void AddExtensionItems()
        {
            extensionsItem.Items.Clear();
            AddMenuChild(extensionsItem.Items, "LOCReloadScripts", mainModel.ReloadScriptsCommand);
            extensionsItem.Items.Add(new Separator());
            foreach (var function in mainModel.Extensions.ExportedFunctions)
            {
                var item = new MenuItem
                {
                    Header = function.Name,
                    Command = mainModel.InvokeExtensionFunctionCommand,
                    CommandParameter = function
                };

                extensionsItem.Items.Add(item);
            }

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

            if (toAdd.Count > 0)
            {
                var menuItems = new Dictionary<string, MenuItem>();
                var menuExtensionItems = new Dictionary<string, MenuItem>();
                foreach (var item in toAdd)
                {
                    var newItem = new MenuItem()
                    {
                        Header = item.Description,
                        Icon = MenuHelpers.GetIcon(item.Icon)
                    };

                    if (item.Action != null)
                    {
                        newItem.Click += (_, __) =>
                        {
                            try
                            {
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
                        };
                    }

                    var startIndex = Items.IndexOf(extensionsItem) + 1;
                    if (item.MenuSection.IsNullOrEmpty())
                    {
                        Items.Insert(startIndex, newItem);
                    }
                    else
                    {
                        if (item.MenuSection == "@")
                        {
                            extensionsItem.Items.Add(newItem);
                        }
                        else if (item.MenuSection.StartsWith("@"))
                        {
                            var parent = MenuHelpers.GenerateMenuParents(menuExtensionItems, item.MenuSection.Substring(1), extensionsItem.Items);
                            parent?.Items.Add(newItem);
                        }
                        else
                        {
                            var parent = MenuHelpers.GenerateMenuParents(menuItems, item.MenuSection, Items, startIndex);
                            parent?.Items.Add(newItem);
                        }
                    }
                }
            }
        }
    }
}
