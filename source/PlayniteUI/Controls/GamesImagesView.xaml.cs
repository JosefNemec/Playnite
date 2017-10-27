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
    public class WidthToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var width = (double)value;
            return width / 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GamesImagesView));
        
        public GamesImagesView()
        {
            InitializeComponent();
            ClickTimer.Elapsed += ClickTimer_Elapsed;
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

        private void ItemsView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (r.VisualHit.GetType() != typeof(ListViewItem))
            {
                ItemsView.UnselectAll();
                return;
            }
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

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
                    //ShowDetails(ItemsView.SelectedItem as GameViewEntry);
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
