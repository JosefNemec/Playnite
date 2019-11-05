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
            mainModel.GamesEditor.PropertyChanged += GamesEditor_PropertyChanged;
            InitializeItems();
        }

        private void GamesEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GamesEditor.LastGames))
            {
                try
                {
                    InitializeItems();
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to reinitialize tray menu items.");
                }
            }
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
            foreach (var game in mainModel.GamesEditor.LastGames)
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

            Items.Add(new Separator());
            AddMenuChild(Items, "LOCExitAppLabel", mainModel.ShutdownCommand);
        }
    }
}
