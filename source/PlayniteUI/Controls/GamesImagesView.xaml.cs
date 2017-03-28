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
            }
        }

        public GamesImagesView()
        {
            InitializeComponent();
        }

        private void CloseDetailBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastDetailsWidht = ColumnDetails.Width;
            ColumnSplitter.Width = new GridLength(0, GridUnitType.Pixel);
            ColumnDetails.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void ImageGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowDetails(sender);
        }

        private void ShowDetails(object sender)
        {
            if (ColumnDetails.Width.Value == 0)
            {
                ColumnSplitter.Width = new GridLength(4, GridUnitType.Pixel);
                ColumnDetails.Width = lastDetailsWidht;
            }

            GameDetails.DataContext = (IGame)((FrameworkElement)sender).DataContext;
            ItemsView.ScrollIntoView(GameDetails.DataContext);
        }

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value += 10;
        }

        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SliderZoom.Value -= 10;
        }
    }
}
