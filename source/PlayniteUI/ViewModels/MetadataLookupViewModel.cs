using Playnite;
using Playnite.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Metadata.Providers;

namespace PlayniteUI.ViewModels
{
    public enum MetadataProvider
    {
        Wiki,
        IGDB
    }

    public class MetadataLookupViewModel : ObservableObject
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

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                searchTerm = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SearchResult> searchResults = new ObservableCollection<SearchResult>();
        public ObservableCollection<SearchResult> SearchResults
        {
            get
            {
                return searchResults;
            }

            set
            {
                searchResults = value;
                OnPropertyChanged();
            }
        }

        private SearchResult selectedResult;
        public SearchResult SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                OnPropertyChanged();
            }
        }        

        public Game MetadataData
        {
            get; set;
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            }, (a) => SelectedResult != null);
        }

        public RelayCommand<object> SearchCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Search();
            }, (a) => !string.IsNullOrEmpty(SearchTerm));
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private MetadataProvider provider;
        private IResourceProvider resources;

        public MetadataLookupViewModel(MetadataProvider provider, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.provider = provider;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
        }

        public bool? OpenView()
        {
            Search();
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public async void ConfirmDialog()
        {
            var id = SelectedResult.Id;
            var success = false;
            IsLoading = true;

            await Task.Run(() =>
            {
                try
                {
                    switch (provider)
                    {
                        case MetadataProvider.Wiki:
                            var wiki = new WikipediaMetadataProvider();
                            var page = wiki.GetPage(id);
                            MetadataData = wiki.ParseGamePage(page, searchTerm);
                            break;
                        case MetadataProvider.IGDB:
                            var igdb = new IGDBMetadataProvider();
                            MetadataData = igdb.GetMetadata(id).GameData;
                            break;
                    }

                    success = true;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to download metadata from meta page.");
                }
                finally
                {
                    IsLoading = false;
                }
            });

            if (!success)
            {
                dialogs.ShowMessage(string.Format(resources.FindString("LOCMetadownloadNoResultsMessage"), searchTerm));
            }
            else
            {
                CloseView(true);
            }
        }

        public async void Search()
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                try
                {
                    SearchResults = SearchForResults(SearchTerm, provider);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, "Failed to search for metadata.");
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private ObservableCollection<SearchResult> SearchForResults(string keyword, MetadataProvider provider)
        {
            var searchList = new ObservableCollection<SearchResult>();

            switch (provider)
            {
                case MetadataProvider.Wiki:
                    var wiki = new WikipediaMetadataProvider();
                    foreach (var page in wiki.Search(keyword))
                    {
                        searchList.Add(new SearchResult(page.title, page.title, page.snippet));
                    }

                    break;

                case MetadataProvider.IGDB:
                    var igdb = new IGDBMetadataProvider();
                    foreach (var page in igdb.SearchMetadata(new Game(keyword)))
                    {
                        searchList.Add(new SearchResult(
                            page.Id, 
                            page.Name + (page.ReleaseDate == null ? "" : $" ({page.ReleaseDate.Value.Year})"),
                            string.Empty));
                    }

                    break;
            }

            return searchList;
        }
    }
}
