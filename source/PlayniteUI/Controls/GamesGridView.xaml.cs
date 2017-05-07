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
                PopupGameMulti.DataContext = GridGames.SelectedItems.Cast<IGame>().ToList();
                PopupGameMulti.IsOpen = true;
            }
            else if (GridGames.SelectedItems.Count == 1)
            {
                var game = GridGames.SelectedItem as IGame;
                PopupGame.DataContext = game;
                PopupGame.IsOpen = true;
            }
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridGames.SelectedItem != null)
            {
                var game = GridGames.SelectedItem as IGame;
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
    }
}
