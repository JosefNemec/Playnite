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
using Playnite;

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
                var list = value as ListCollectionView;

                if (list.SourceCollection is RangeObservableCollection<GameViewEntry>)
                {
                    ((RangeObservableCollection<GameViewEntry>)list.SourceCollection).CollectionChanged -= GamesGridView_CollectionChanged;
                    ((RangeObservableCollection<GameViewEntry>)list.SourceCollection).CollectionChanged += GamesGridView_CollectionChanged;
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
                foreach (GameViewEntry entry in e.OldItems)
                {
                    if (game.Id == entry.Game.Id)
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

            var entry = ListGames.SelectedItem as GameViewEntry;
            GameDetails.DataContext = entry.Game;
        }

        private void ListItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListGames.SelectedItem == null)
            {
                return;
            }

            var game = (ListGames.SelectedItem as GameViewEntry).Game;
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
                PopupGameMulti.DataContext = ListGames.SelectedItems.Cast<GameViewEntry>().Select(a => a.Game).ToList();
                PopupGameMulti.IsOpen = true;
            }
            else if (ListGames.SelectedItems.Count == 1)
            {
                var game = (ListGames.SelectedItem as GameViewEntry).Game;
                PopupGame.DataContext = game;
                PopupGame.IsOpen = true;
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
