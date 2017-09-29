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
using System.Timers;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesImagesView.xaml
    /// </summary>
    public partial class GamesImagesView : UserControl
    {
        private Timer ClickTimer = new Timer(300);
        private int ClickCount = 0;
        private GridLength LastDetailsWidht = new GridLength(400, GridUnitType.Pixel);

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
            ClickTimer.Elapsed += ClickTimer_Elapsed;
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
                                
                var game = (GameViewEntry)GameDetails.DataContext;
                foreach (GameViewEntry entry in e.OldItems)
                {
                    if (game.Id == entry.Game.Id)
                    {
                        HideDetails();
                        return;
                    }
                }
            });
        }

        private void CloseDetailBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LastDetailsWidht = ColumnDetails.Width;
            ColumnSplitter.Width = new GridLength(0, GridUnitType.Pixel);
            ColumnDetails.Width = new GridLength(0, GridUnitType.Pixel);

            if (ItemsView.SelectedItem != null)
            {
                ItemsView.ScrollIntoView(ItemsView.SelectedItem as GameViewEntry);
            }
        }

        private void HideDetails()
        {            
            CloseDetailBorder_MouseLeftButtonDown(this, null);            
        }

        private void ShowDetails(GameViewEntry viewEntry)
        {
            if (ColumnDetails.Width.Value == 0)
            {
                ColumnSplitter.Width = new GridLength(4, GridUnitType.Pixel);
                ColumnDetails.Width = LastDetailsWidht;
            }

            ItemsView.ScrollIntoView(viewEntry);
        }

        private void ChangeDetails(GameViewEntry viewEntry)
        {
            GameDetails.DataContext = viewEntry;
        }

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value += 10;
        }

        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value -= 10;
        }

        private void PlayGame(GameViewEntry viewEntry)
        {
            var game = viewEntry.Game;
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

        private void ItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsView.SelectedItems.Count == 1)
            {
                ChangeDetails(ItemsView.SelectedItem as GameViewEntry);
            }
            else if (ItemsView.SelectedItems.Count == 0)
            {
                HideDetails();
            }
        }

        private void ItemsView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListViewItem))
            {
                ItemsView.UnselectAll();
            }
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClickTimer.Stop();
            ClickCount++;
            ClickTimer.Start();
        }

        private void ClickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ClickTimer.Stop();

            Dispatcher.Invoke(() =>
            {
                if (ClickCount == 1)
                {
                    ShowDetails(ItemsView.SelectedItem as GameViewEntry);
                }
                else if (ClickCount == 2)
                {
                    PlayGame(ItemsView.SelectedItem as GameViewEntry);
                }
            });

            ClickCount = 0;
        }
    }
}
