using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        private readonly IWindowFactory window;
        private readonly GameDatabase database;
        private readonly ExtensionFactory extensions;
        private readonly MainViewModelBase mainModel;
        private CancellationTokenSource currentSearchToken;
        private readonly Dictionary<string, Plugin> keywordPlugins = new Dictionary<string, Plugin>();
        private static readonly Regex searchRegex = new Regex("", RegexOptions.Compiled);

        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                searchTerm = value;
                OnPropertyChanged();
                currentSearchToken?.Cancel();
                currentSearchToken?.Dispose();
                currentSearchToken = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                PerformSearch(currentSearchToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private ObservableCollection<SearchItem> searchResults = new ObservableCollection<SearchItem>();
        private SearchItem selectedSearchItem;

        public ObservableCollection<SearchItem> SearchResults { get => searchResults; set => SetValue(ref searchResults, value); }
        public SearchItem SelectedSearchItem { get => selectedSearchItem; set => SetValue(ref selectedSearchItem, value); }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand<KeyEventArgs> TextBoxKeyDownCommand => new RelayCommand<KeyEventArgs>((keyArgs) => TextBoxKeyDown(keyArgs));
        public RelayCommand<KeyEventArgs> TextBoxKeyUpCommand => new RelayCommand<KeyEventArgs>((keyArgs) => TextBoxKeyUp(keyArgs));

        public SearchViewModel(
            IWindowFactory window,
            GameDatabase database,
            ExtensionFactory extensions,
            MainViewModelBase mainModel)
        {
            this.window = window;
            this.database = database;
            this.extensions = extensions;
            this.mainModel = mainModel;

            foreach (var plugin in extensions.GenericPlugins.Where(a => a.Properties?.Searches.HasItems() == true))
            {
                foreach (var search in plugin.Properties.Searches)
                {
                    keywordPlugins.AddOrUpdate(search.Keyword, plugin);
                }
            }

            foreach (var plugin in extensions.LibraryPlugins.Where(a => a.Properties?.Searches.HasItems() == true))
            {
                foreach (var search in plugin.Properties.Searches)
                {
                    keywordPlugins.AddOrUpdate(search.Keyword, plugin);
                }
            }

            foreach (var plugin in extensions.MetadataPlugins.Where(a => a.Properties?.Searches.HasItems() == true))
            {
                foreach (var search in plugin.Properties.Searches)
                {
                    keywordPlugins.AddOrUpdate(search.Keyword, plugin);
                }
            }
        }

        public void OpenSearch()
        {
            window.Show(this);
        }

        public void Close()
        {
            window.Close();
        }

        internal IEnumerable<SearchItem> GetDefaultSearchItems(GetSearchResultsArgs args)
        {
            if (args.Keyword.IsNullOrWhiteSpace())
            {
                yield break;
            }
        }

        public async Task PerformSearch(CancellationToken cancelToken)
        {
            //  TOOD: prdelat na collection view source?

            var searchArgs = new GetSearchResultsArgs
            {
                CancelToken = cancelToken,
                Keyword = "",
                SearchTerm = SearchTerm,
            };

            var results = await Task.Run(() =>
            {
                return database.Games.Where(a => a.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)).Select(a => new SearchItem
                {
                    Name = a.Name,
                    Icon = a.Icon,
                    PrimaryAction = new SearchItemAction
                    {
                        Name = "Play",
                        Action = () => mainModel.SelectGame(a.Id)
                    },
                    SecondaryAction = new SearchItemAction
                    {
                        Name = "Go to details",
                        Action = () => mainModel.SelectGame(a.Id)
                    }
                }).ToList();
            });

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            SearchResults.Clear();
            SearchResults.AddRange(results);
            if (results.HasItems())
            {
                SelectedSearchItem = results[0];
            }
        }

        private void TextBoxKeyDown(KeyEventArgs keyArgs)
        {
            if (!SearchResults.HasItems())
            {
                return;
            }

            if (SelectedSearchItem == null)
            {
                SelectedSearchItem = SearchResults[0];
                return;
            }

            var curIndex = SearchResults.IndexOf(SelectedSearchItem);
            if (keyArgs.Key == Key.Up)
            {
                if (curIndex > 0)
                {
                    SelectedSearchItem = SearchResults[curIndex - 1];
                }
            }
            else if (keyArgs.Key == Key.Down)
            {
                if (curIndex < SearchResults.Count - 1)
                {
                    SelectedSearchItem = SearchResults[curIndex + 1];
                }
            }
            else if (keyArgs.Key == Key.PageUp)
            {
            }
            else if (keyArgs.Key == Key.PageDown)
            {
            }
        }

        private void TextBoxKeyUp(KeyEventArgs keyArgs)
        {
            if (SelectedSearchItem == null)
            {
                return;
            }

            if (keyArgs.Key == Key.Return || keyArgs.Key == Key.Enter)
            {
                SelectedSearchItem.PrimaryAction?.Action.Invoke();
                Close();
            }
        }
    }
}
