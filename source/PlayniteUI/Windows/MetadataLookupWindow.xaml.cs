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
using PlayniteUI.Controls;

namespace PlayniteUI.Windows
{
    public enum MetadataProvider
    {
        Wiki,
        IGDB
    }

    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class MetadataLookupWindow : WindowBase
    {
        public class SearchResult
        {
            public string Id
            {
                get; set;
            }

            public string Name
            {
                get; set;
            }

            public string Description
            {
                get; set;
            }

            public SearchResult(string id, string name, string description)
            {
                Id = id;
                Name = name;
                Description = description;
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private string searchTerm
        {
            get; set;
        }

        private MetadataProvider provider
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
            var id = ((SearchResult)ListSearch.SelectedItem).Id;

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
                    switch (provider)
                    {
                        case MetadataProvider.Wiki:
                            var wiki = new Wikipedia();
                            var page = wiki.GetPage(id);
                            MetadataData = wiki.ParseGamePage(page, searchTerm);
                            break;
                        case MetadataProvider.IGDB:
                            var igdb = new IGDB();
                            MetadataData = igdb.GetParsedGame(UInt64.Parse(id));
                            break;
                    }
                }
                catch (Exception exc)
                {
#if DEBUG
                    logger.Error(exc, "Failed to download metadata from meta page.");
#else
                    logger.Warn(exc, "Failed to download metadata from meta page.");
#endif

                    Dispatcher.Invoke(() =>
                    {
                        PlayniteMessageBox.Show("Didn't found any relevant information about game \"" + searchTerm + "\" on specified page.");
                    });

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
                var search = SearchGames(searchTerm, provider);

                ListSearch.Dispatcher.Invoke(() =>
                {
                    ListSearch.ItemsSource = search;
                    TextDownloading.Visibility = Visibility.Hidden;
                    ListSearch.Visibility = Visibility.Visible;
                });
            });
        }

        private List<SearchResult> SearchGames(string keyword, MetadataProvider provider)
        {
            var searchList = new List<SearchResult>();

            switch (provider)
            {
                case MetadataProvider.Wiki:
                    var wiki = new Wikipedia();
                    foreach (var page in wiki.Search(keyword))
                    {
                        searchList.Add(new SearchResult(page.title, page.title, page.snippet));
                    }

                    break;

                case MetadataProvider.IGDB:
                    var igdb = new IGDB();
                    foreach (var page in igdb.Search(keyword))
                    {
                        searchList.Add(new SearchResult(page.id.ToString(), page.name, string.Empty));
                    }

                    break;
            }

            return searchList;
        }

        public bool? LookupData(string gameName, MetadataProvider provider)
        {
            this.provider = provider;
            this.searchTerm = gameName;
            TextSearch.Text = gameName;
            return ShowDialog();
        }
    }
}
