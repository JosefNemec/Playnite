using Playnite.Commands;
using Playnite.Common;
using Playnite.DesktopApp.Markup;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class TrayContextMenu : ContextMenu
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private DesktopAppViewModel mainModel;

        static TrayContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TrayContextMenu), new FrameworkPropertyMetadata(typeof(TrayContextMenu)));
        }

        public TrayContextMenu()
        {
        }

        public TrayContextMenu(DesktopAppViewModel model)
        {
            mainModel = model;
            Opened += TrayContextMenu_Opened;
        }

        private void TrayContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }

        private MenuItem AddMenuChild(
            ItemCollection parent,
            string locString,
            RelayCommand command,
            object commandParameter = null,
            object icon = null)
        {
            var item = new MenuItem
            {
                Command = command,
                CommandParameter = commandParameter
            };

            if (locString.IsNullOrEmpty())
            {
                item.Header = "<NO_STRING>";
            }
            else if (locString.StartsWith("LOC"))
            {
                item.SetResourceReference(MenuItem.HeaderProperty, locString);
            }
            else
            {
                item.Header = locString;
            }

            if (icon != null)
            {
                item.Icon = icon;
            }

            parent.Add(item);
            return item;
        }

        private void InitializeItems()
        {
            Items.Clear();

            foreach (var game in mainModel.GamesEditor.QuickLaunchItems)
            {
                object icon = null;
                if (!game.Icon.IsNullOrEmpty())
                {
                    var path = mainModel.Database.GetFullFilePath(game.Icon);
                    if (File.Exists(path))
                    {
                        icon = Images.GetImageFromFile(path);
                    }
                }

                if (icon == null)
                {
                    var resourceIcon = ResourceProvider.GetResource("DefaultGameIcon") as BitmapImage;
                    var image = new Image() { Source = resourceIcon };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
                    icon = image;
                }

                AddMenuChild(Items, game.Name, mainModel.StartGameCommand, game, icon);
            }

            Items.Add(new Separator());
            AddMenuChild(Items, "LOCOpenPlaynite", mainModel.ShowWindowCommand);

            var openClientItem = AddMenuChild(Items, "LOCMenuClients", null);
            foreach (var tool in mainModel.ThirdPartyTools)
            {
                openClientItem.Items.Add(new MenuItem
                {
                    Header = tool.Name,
                    Command = mainModel.ThirdPartyToolOpenCommand,
                    CommandParameter = tool,
                    Icon = tool.Icon
                });
            }

            if (mainModel.Database.SoftwareApps.HasItems())
            {
                var toolsItem = AddMenuChild(Items, "LOCMenuTools", null);
                foreach (var tool in mainModel.Database.SoftwareApps.OrderBy(a => a.Name))
                {
                    object icon = null;
                    if (!tool.Icon.IsNullOrEmpty())
                    {
                        var path = mainModel.Database.GetFullFilePath(tool.Icon);
                        if (File.Exists(path))
                        {
                            icon = Images.GetImageFromFile(path);
                        }
                    }

                    AddMenuChild(toolsItem.Items, tool.Name, mainModel.StartSoftwareToolCommand, tool, icon);
                }
            }

            Items.Add(new Separator());
            AddMenuChild(Items, "LOCExitAppLabel", mainModel.ShutdownCommand);
        }
    }
}
