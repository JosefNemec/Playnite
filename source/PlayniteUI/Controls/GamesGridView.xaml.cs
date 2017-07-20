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

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesGridView.xaml
    /// </summary>
    public partial class GamesGridView : UserControl
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
                Source = HeaderMenu.DataContext,
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            });
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var category = (CategoryView)((CollectionViewGroup)((Expander)sender).DataContext).Name;
            if (!Settings.Instance.CollapsedCategories.Contains(category.Category))
            {
                Settings.Instance.CollapsedCategories.Add(category.Category);
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var category = (CategoryView)((CollectionViewGroup)((Expander)sender).DataContext).Name;
            if (Settings.Instance.CollapsedCategories.Contains(category.Category))
            {
                Settings.Instance.CollapsedCategories.Remove(category.Category);
            }
        }
    }
}
