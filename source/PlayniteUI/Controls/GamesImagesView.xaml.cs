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
    /// Interaction logic for GamesImagesView.xaml
    /// </summary>
    public partial class GamesImagesView : UserControl
    {
        private GridLength lastDetailsWidht = new GridLength(400, GridUnitType.Pixel);

        public IEnumerable ItemsSource
        {
            get
            {
                return ItemsView.ItemsSource;
            }

            set
            {
                ItemsView.ItemsSource = value;
                var list = value as ListCollectionView;

                if (list.SourceCollection is RangeObservableCollection<GameViewEntry>)
                {
                    ((RangeObservableCollection<GameViewEntry>)list.SourceCollection).CollectionChanged -= GamesGridView_CollectionChanged;
                    ((RangeObservableCollection<GameViewEntry>)list.SourceCollection).CollectionChanged += GamesGridView_CollectionChanged;
                }
            }
        }

        public GamesImagesView()
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
                        CloseDetailBorder_MouseLeftButtonDown(this, null);
                        return;
                    }
                }
            });
        }

        private void CloseDetailBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastDetailsWidht = ColumnDetails.Width;
            ColumnSplitter.Width = new GridLength(0, GridUnitType.Pixel);
            ColumnDetails.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void ShowDetails(object sender)
        {
            if (ColumnDetails.Width.Value == 0)
            {
                ColumnSplitter.Width = new GridLength(4, GridUnitType.Pixel);
                ColumnDetails.Width = lastDetailsWidht;
            }

            var entry = (GameViewEntry)((FrameworkElement)sender).DataContext;
            var game = entry.Game;

            GameDetails.DataContext = game;
            ItemsView.ScrollIntoView(entry);
        }

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value += 10;
        }

        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value -= 10;
        }

        private void PlayGame_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var entry = (GameViewEntry)((FrameworkElement)e.OriginalSource).DataContext;
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

        private void ShowDetails_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowDetails(e.OriginalSource);
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
