using NLog;
using Playnite;
using Playnite.MetaProviders;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                OnPropertyChanged("IsLoading");
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                searchTerm = value;
                OnPropertyChanged("SearchTerm");
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
                OnPropertyChanged("SearchResults");
            }
        }

        private SearchResult selectedResult;
        public SearchResult SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                OnPropertyChanged("SelectedResult");
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

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private MetadataProvider provider;
        private IResourceProvider resources;
        private string igdbApiKey;

        public MetadataLookupViewModel(MetadataProvider provider, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources, string igdbApiKey)
        {
            this.provider = provider;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.igdbApiKey = igdbApiKey;
        }

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

            await Task.Factory.StartNew(() =>
            {
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
                            var igdb = new IGDBMetadataProvider(igdbApiKey);
                            MetadataData = igdb.GetParsedGame(UInt64.Parse(id));
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
                dialogs.ShowMessage(string.Format(resources.FindString("MetadownloadNoResultsMessage"), searchTerm));
            }
            else
            {
                CloseView(true);
            }
        }

        public async void Search()
        {
            IsLoading = true;
            await Task.Factory.StartNew(() =>
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
                    var wiki = new Wikipedia();
                    foreach (var page in wiki.Search(keyword))
                    {
                        searchList.Add(new SearchResult(page.title, page.title, page.snippet));
                    }

                    break;

                case MetadataProvider.IGDB:
                    var igdb = new IGDBMetadataProvider(igdbApiKey);
                    foreach (var page in igdb.Search(keyword))
                    {
                        searchList.Add(new SearchResult(
                            page.id.ToString(), 
                            page.name + (page.first_release_date == 0 ? "" : $" ({DateTimeOffset.FromUnixTimeMilliseconds(page.first_release_date).DateTime.Year.ToString()})"),
                            string.Empty));
                    }

                    break;
            }

            return searchList;
        }
    }
}
