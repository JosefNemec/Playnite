using Playnite;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Metadata.Providers;
using Playnite.SDK.Metadata;
using Playnite.Windows;
using Playnite.SDK.Plugins;

namespace Playnite.DesktopApp.ViewModels
{
    public class MetadataLookupViewModel : ObservableObject
    {
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

        private ObservableCollection<MetadataSearchResult> searchResults = new ObservableCollection<MetadataSearchResult>();
        public ObservableCollection<MetadataSearchResult> SearchResults
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

        private MetadataSearchResult selectedResult;
        public MetadataSearchResult SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                OnPropertyChanged();
            }
        }        

        public GameMetadata MetadataData
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

        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private MetadataPlugin plugin;
        private IResourceProvider resources;

        public MetadataLookupViewModel(MetadataPlugin plugin, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.plugin = plugin;
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
            var success = false;
            IsLoading = true;

            await Task.Run(() =>
            {
                try
                {
                    MetadataData = plugin.GetMetadata(SelectedResult);
                    success = true;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"GetMetadata method from plugin {plugin.Name} failed.");
                }
                finally
                {
                    IsLoading = false;
                }
            });

            if (!success)
            {
                dialogs.ShowMessage(string.Format(resources.GetString("LOCMetadownloadNoResultsMessage"), searchTerm));
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
                    SearchResults = SearchForResults(SearchTerm);
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private ObservableCollection<MetadataSearchResult> SearchForResults(string keyword)
        {
            var searchList = new ObservableCollection<MetadataSearchResult>();
            List<MetadataSearchResult> result = null;

            try
            {
                result = plugin.SearchMetadata(keyword);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"SearchMetadata method from plugin {plugin.Name} failed.");
            }

            if (result != null)
            {
                searchList.AddRange(result);
            }

            return searchList;
        }
    }
}
