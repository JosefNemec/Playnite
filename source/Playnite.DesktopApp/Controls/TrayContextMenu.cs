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

namespace Playnite.DesktopApp.Controls
{
    public class TrayContextMenu : ContextMenu
    {
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
                    icon = ResourceProvider.GetResource("DefaultGameIcon");
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

                openClientItem.Items.Add(item);
            }

            Items.Add(new Separator());
            AddMenuChild(Items, "LOCExitAppLabel", mainModel.ShutdownCommand);
        }
    }
}
