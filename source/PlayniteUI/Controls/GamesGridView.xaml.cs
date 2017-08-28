using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Playnite.Database;
using Playnite.Models;
using Playnite;
using System.ComponentModel;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesGridView.xaml
    /// </summary>
    public partial class GamesGridView : UserControl, INotifyPropertyChanged
    {
        public IEnumerable ItemsSource
        {
            get
            {
                return GridGames.ItemsSource;
            }

            set
            {
                GridGames.ItemsSource = value;
            }
        }

        private Settings appSettings;
        public Settings AppSettings
        {
            get
            {
                return appSettings;
            }

            set
            {
                GridGames.DataContext = value;
                HeaderMenu.DataContext = value;
                appSettings = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AppSettings"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GamesGridView()
        {
            InitializeComponent();
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GridGames.SelectedItems.Count > 1)
            {
                PopupGameMulti.DataContext = GridGames.SelectedItems.Cast<GameViewEntry>().Select(a => a.Game).ToList();
                PopupGameMulti.IsOpen = true;
            }
            else if (GridGames.SelectedItems.Count == 1)
            {
                var game = (GridGames.SelectedItem as GameViewEntry).Game;
                PopupGame.DataContext = game;
                PopupGame.IsOpen = true;
            }
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridGames.SelectedItem == null)
            {
                return;
            }

            var entry = (GameViewEntry)GridGames.SelectedItem;
            var game = entry.Game;
            if (game.IsInstalled)
            {
                GamesEditor.Instance.PlayGame(game);
            }
            else
            {
                if (game.Provider == Provider.Custom)
                {
                    GamesEditor.Instance.EditGame(game);
                }
                else
                {
                    GamesEditor.Instance.InstallGame(game);
                }
            }
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridViewColumnHeader_Loaded(object sender, RoutedEventArgs e)
        {
            var header = sender as GridViewColumnHeader;
            if (header.Tag == null)
            {
                return;
            }

            BindingOperations.SetBinding(header, GridViewColumnHeader.VisibilityProperty, new Binding(string.Format("GridViewHeaders[{0}]", header.Tag.ToString()))
            {
                Source = AppSettings,
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            });
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var header = sender as GridViewColumnHeader;            
            if (header == null || header.Tag == null)
            {
                return;
            }

            var sortOrder = (SortOrder)header.Tag;
            if (sortOrder == SortOrder.Icon)
            {
                return;
            }

            if (AppSettings.SortingOrder == sortOrder)
            {
                AppSettings.SortingOrderDirection = AppSettings.SortingOrderDirection == SortOrderDirection.Ascending ? SortOrderDirection.Descending : SortOrderDirection.Ascending;
            }
            else
            {
                AppSettings.SortingOrder = sortOrder;
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var group = (CollectionViewGroup)((Expander)sender).DataContext;
            if (group.Name is CategoryView category)
            {
                if (!Settings.Instance.CollapsedCategories.Contains(category.Category))
                {
                    Settings.Instance.CollapsedCategories.Add(category.Category);
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var group = (CollectionViewGroup)((Expander)sender).DataContext;
            if (group.Name is CategoryView category)
            {
                if (Settings.Instance.CollapsedCategories.Contains(category.Category))
                {
                    Settings.Instance.CollapsedCategories.Remove(category.Category);
                }
            }
        }
    }
}
