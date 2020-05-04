using Playnite.Commands;
using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions.Markup;
using Playnite.Plugins;
using Playnite.SDK;
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
                mainModel.Extensions.PropertyChanged += Extensions_PropertyChanged;
                InitializeItems();
            }
        }

        private void Extensions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExtensionFactory.ExportedFunctions))
            {
                InitializeExtensionsMenu();
            }
        }

        public static MenuItem AddMenuChild(
            ItemCollection parent,
            string locString,
            RelayCommand command,
            object commandParameter = null,
            object icon = null)
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

            if (icon != null)
            {
                if (icon is string stringIcon)
                {
                    item.Icon = Images.GetImageFromFile(ThemeFile.GetFilePath(stringIcon));
                }
                else if (icon is BitmapImage bitmap)
                {
                    var image = new Image() { Source = bitmap };
                    RenderOptions.SetBitmapScalingMode(image, RenderOptions.GetBitmapScalingMode(bitmap));
                    item.Icon = image;
                }
                else if (icon is TextBlock textIcon)
                {
                    item.Icon = textIcon;
                }
            }

            parent.Add(item);
            return item;
        }

        private void InitializeExtensionsMenu()
        {
            // Can be called from other thread via Extensions_PropertyChanged event hook
            Dispatcher.Invoke(() =>
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
            });
        }

        public void InitializeItems()
        {
            // Add Game
            var addGameItem = AddMenuChild(Items, "LOCMenuAddGame", null, null, ResourceProvider.GetResource("AddGameIcon"));
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

            // Update Library
            var updateItem = AddMenuChild(Items, "LOCMenuReloadLibrary", null, null, ResourceProvider.GetResource("UpdateDbIcon"));
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

            // Extensions
            extensionsItem = AddMenuChild(Items, "LOCExtensions", null);
            InitializeExtensionsMenu();

            // Open Client
            var openClientItem = AddMenuChild(Items, "LOCMenuOpenClient", null);
            foreach (var tool in mainModel.ThirdPartyTools)
            {
                var item = new MenuItem
                {
                    Header = tool.Name,
                    Command = mainModel.ThirdPartyToolOpenCommand,
                    CommandParameter = tool
                };

                if (tool.Client?.Icon != null && File.Exists(tool.Client.Icon))
                {
                    item.Icon = Images.GetImageFromFile(tool.Client.Icon);
                }

                openClientItem.Items.Add(item);
            }

            // Random game select
            AddMenuChild(Items, "LOCMenuSelectRandomGame", mainModel.SelectRandomGameCommand, null, ResourceProvider.GetResource("DiceIcon"));

            // Settings
            AddMenuChild(Items, "LOCMenuPlayniteSettingsTitle", mainModel.OpenSettingsCommand, null, ResourceProvider.GetResource("SettingsIcon"));

            // FullScreen
            Items.Add(new Separator());
            AddMenuChild(Items, "LOCMenuOpenFullscreen", mainModel.OpenFullScreenCommand, null, ResourceProvider.GetResource("FullscreenModeIcon"));
            Items.Add(new Separator());

            // Links
            var linksItem = AddMenuChild(Items, "LOCMenuLinksTitle", null);
            AddMenuChild(linksItem.Items, "LOCCommonLinksForum", GlobalCommands.NavigateUrlCommand, UrlConstants.Forum, "Images/applogo.png");
            AddMenuChild(linksItem.Items, "Discord", GlobalCommands.NavigateUrlCommand, UrlConstants.Discord, "Images/discord.png");
            AddMenuChild(linksItem.Items, "Twitter", GlobalCommands.NavigateUrlCommand, UrlConstants.Twitter, "Images/twitter.png");

            // Help
            var helpItem = AddMenuChild(Items, "LOCMenuHelpTitle", null);
            AddMenuChild(helpItem.Items, "Wiki / FAQ", GlobalCommands.NavigateUrlCommand, UrlConstants.Wiki);
            AddMenuChild(helpItem.Items, "LOCMenuIssues", mainModel.ReportIssueCommand);
            AddMenuChild(helpItem.Items, "LOCSDKDocumentation", GlobalCommands.NavigateUrlCommand, UrlConstants.SdkDocs);

            // Patreon
            AddMenuChild(Items, "LOCMenuPatreonSupport", GlobalCommands.NavigateUrlCommand, UrlConstants.Patreon, "Images/patreon.png");

            Items.Add(new Separator());

            // About
            AddMenuChild(Items, "LOCMenuAbout", mainModel.OpenAboutCommand, null, ResourceProvider.GetResource("AboutPlayniteIcon"));

            // Check for update
            AddMenuChild(Items, "LOCCheckForUpdates", mainModel.CheckForUpdateCommand);
            Items.Add(new Separator());

            // Exit
            AddMenuChild(Items, "LOCExitAppLabel", mainModel.ShutdownCommand, null, ResourceProvider.GetResource("ExitIcon"));
        }
    }
}
