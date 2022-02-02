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
using Playnite.Windows;
using Playnite.SDK.Plugins;

namespace Playnite.DesktopApp.ViewModels
{
    public class ItemSelectionWithSearchViewModel : ObservableObject
    {
        public string WindowTitle { get; set; }

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

        private ObservableCollection<GenericItemOption> searchResults = new ObservableCollection<GenericItemOption>();
        public ObservableCollection<GenericItemOption> SearchResults
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

        private GenericItemOption selectedResult;
        public GenericItemOption SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                OnPropertyChanged();
            }
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

        public RelayCommand<object> ItemDoubleClickCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand WindowOpenedCommand
        {
            get => new RelayCommand(() => Search());
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly Func<string, List<GenericItemOption>> searchFunction;

        public ItemSelectionWithSearchViewModel(
            IWindowFactory window,
            Func<string, List<GenericItemOption>> searchFunction,
            string defaultSearch = null,
            string caption = null)
        {
            this.window = window;
            this.searchFunction = searchFunction;
            SearchTerm = defaultSearch;
            if (caption.IsNullOrEmpty())
            {
                WindowTitle = ResourceProvider.GetString("LOCSelectItemTitle");
            }
            else
            {
                WindowTitle = caption;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            CloseView(true);
        }

        public void Search()
        {
            Dialogs.ActivateGlobalProgress(
                _ => SearchResults = SearchForResults(SearchTerm),
                new GlobalProgressOptions(LOC.LoadingLabel, false));
        }

        private ObservableCollection<GenericItemOption> SearchForResults(string keyword)
        {
            List<GenericItemOption> result = null;

            try
            {
                result = searchFunction(keyword);
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"searchFunction method failed.");
            }

            return result?.ToObservable();
        }
    }
}
