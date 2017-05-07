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
using System.Collections.ObjectModel;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesListView.xaml
    /// </summary>
    public partial class GamesListView : UserControl
    {
        public IEnumerable ItemsSource
        {
            get
            {
                return ListGames.ItemsSource;
            }

            set
            {
                ListGames.ItemsSource = value;

                if (value is ObservableCollection<IGame>)
                {
                    ((ObservableCollection<IGame>)value).CollectionChanged -= GamesGridView_CollectionChanged;
                    ((ObservableCollection<IGame>)value).CollectionChanged += GamesGridView_CollectionChanged;
                }
            }
        }

        public GamesListView()
        {
            InitializeComponent();
        }

        private void GamesGridView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                return;
            }

            // Can be called from another thread if games are being loaded
            GameDetails.Dispatcher.Invoke(() =>
            {
                if (GameDetails.DataContext == null)
                {
                    return;
                }

                var game = (IGame)GameDetails.DataContext;
                foreach (IGame removedGame in e.OldItems)
                {
                    if (game.Id == removedGame.Id)
                    {
                        GameDetails.DataContext = null;
                        return;
                    }
                }
            });
        }

        private void GamesListList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListGames.SelectedItem == null)
            {
                return;
            }

            var game = (IGame)ListGames.SelectedItem;
            GameDetails.DataContext = game;
        }

        private void ListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var game = ListGames.SelectedItem as IGame;
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

        private void ListItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListGames.SelectedItems.Count > 1)
            {
                PopupGameMulti.DataContext = ListGames.SelectedItems.Cast<IGame>().ToList();
                PopupGameMulti.IsOpen = true;
            }
            else if (ListGames.SelectedItems.Count == 1)
            {
                var game = ListGames.SelectedItem as IGame;
                PopupGame.DataContext = game;
                PopupGame.IsOpen = true;
            }
        }
    }
}
