using System;
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
using System.Windows.Shapes;
using NLog;
using Playnite.MetaProviders;
using Playnite.Models;

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class MetadataLookupWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private string searchTerm
        {
            get; set;
        }

        public Game MetadataData
        {
            get; set;
        }

        public MetadataLookupWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var title = ((Wikipedia.SearchResult)ListSearch.SelectedItem).title;

            Task.Factory.StartNew(() =>
            {
                TextDownloading.Dispatcher.Invoke(() =>
                {
                    ListSearch.Visibility = Visibility.Hidden;
                    TextDownloading.Visibility = Visibility.Visible;
                    ButtonOK.IsEnabled = false;
                    ButtonCancel.IsEnabled = false;
                });

                try
                {
                    var wiki = new Wikipedia();
                    var page = wiki.GetPage(title);
                    MetadataData = wiki.ParseGamePage(page, searchTerm);
                }
                catch (Exception exc)
                {
#if DEBUG
                    logger.Error(exc, "Failed to download metadata from meta page.");
#else
                    logger.Warn(exc, "Failed to download metadata from meta page.");
#endif
                    MessageBox.Show("Didn't found any relevant information about game \"" + searchTerm + "\" on specified page.");

                    TextDownloading.Dispatcher.Invoke(() =>
                    {
                        ListSearch.Visibility = Visibility.Visible;
                        TextDownloading.Visibility = Visibility.Hidden;
                        ButtonOK.IsEnabled = true;
                        ButtonCancel.IsEnabled = true;
                    });

                    return;
                }
                
                Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                    Close();
                });
            });
        }

        private void ListSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null)
            {
                ButtonOK.IsEnabled = false;
            }
            else
            {
                ButtonOK.IsEnabled = true;
            }
        }

        private void ListSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListSearch.SelectedItem != null)
            {
                ButtonOK_Click(this, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Search_Executed(this, null);
        }

        private void Search_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            searchTerm = TextSearch.Text;
            ListSearch.Visibility = Visibility.Hidden;
            TextDownloading.Visibility = Visibility.Visible;

            Task.Factory.StartNew(() =>
            {
                var wiki = new Wikipedia();
                var search = wiki.Search(searchTerm);
                ListSearch.Dispatcher.Invoke(() =>
                {
                    ListSearch.ItemsSource = search;
                    TextDownloading.Visibility = Visibility.Hidden;
                    ListSearch.Visibility = Visibility.Visible;
                });
            });
        }

        public bool? LookupData(string gameName)
        {
            searchTerm = gameName;
            TextSearch.Text = gameName;
            return ShowDialog();
        }
    }
}
