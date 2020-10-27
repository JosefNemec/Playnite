using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Playnite.DesktopApp.Controls
{
    public class GameGroupMenu : ContextMenu
    {
        private RelayCommand<object> CollapseCommand { get; }
        private RelayCommand<object> CollapseAllCommand { get; }
        private RelayCommand<object> ExpandCommand { get; }
        private RelayCommand<object> ExpandAllCommand { get; }

        private readonly PlayniteSettings settings;
        private readonly DesktopAppViewModel mainModel;

        static GameGroupMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameGroupMenu), new FrameworkPropertyMetadata(typeof(GameGroupMenu)));
        }

        public GameGroupMenu() : this(PlayniteApplication.Current?.AppSettings, DesktopApplication.Current?.MainModel)
        {
        }

        public GameGroupMenu(PlayniteSettings settings, DesktopAppViewModel mainModel)
        {
            this.settings = settings;
            this.mainModel = mainModel;
            CollapseCommand = new RelayCommand<object>((a) => Collapse());
            CollapseAllCommand = new RelayCommand<object>((a) => CollapseAll());
            ExpandCommand = new RelayCommand<object>((a) => Expand());
            ExpandAllCommand = new RelayCommand<object>((a) => ExpandAll());
            Opened += GameGroupMenu_Opened;
        }

        private void GameGroupMenu_Opened(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }

        public void InitializeItems()
        {
            if (settings == null)
            {
                return;
            }

            Items.Clear();
            if (DataContext is CollectionViewGroup group)
            {
                if (settings.ViewSettings.IsGroupCollapsed(settings.ViewSettings.GroupingOrder, group.Name?.ToString()))
                {
                    Items.Add(new MenuItem
                    {
                        Command = ExpandCommand,
                        Header = ResourceProvider.GetResource("LOCExpand")
                    });
                }
                else
                {
                    Items.Add(new MenuItem
                    {
                        Command = CollapseCommand,
                        Header = ResourceProvider.GetResource("LOCCollapse")
                    });
                }

                Items.Add(new Separator());

                Items.Add(new MenuItem
                {
                    Command = CollapseAllCommand,
                    Header = ResourceProvider.GetResource("LOCCollapseAll")
                });

                Items.Add(new MenuItem
                {
                    Command = ExpandAllCommand,
                    Header = ResourceProvider.GetResource("LOCExpandAll")
                });
            }
        }

        private void Collapse()
        {
            if (DataContext is CollectionViewGroup group)
            {
                settings.ViewSettings.SetGroupCollapseState(settings.ViewSettings.GroupingOrder, group.Name?.ToString(), true);
            }
        }

        private void Expand()
        {
            if (DataContext is CollectionViewGroup group)
            {
                settings.ViewSettings.SetGroupCollapseState(settings.ViewSettings.GroupingOrder, group.Name?.ToString(), false);
            }
        }

        private void CollapseAll()
        {
            if (mainModel?.GamesView != null)
            {
                var groups = mainModel.GamesView.CollectionView.Groups.Select(a => ((CollectionViewGroup)a).Name?.ToString()).ToList();
                settings.ViewSettings.CollapseGroups(settings.ViewSettings.GroupingOrder, groups);
            }
        }

        private void ExpandAll()
        {
            settings.ViewSettings.ExpandAllGroups(settings.ViewSettings.GroupingOrder);
        }
    }
}
