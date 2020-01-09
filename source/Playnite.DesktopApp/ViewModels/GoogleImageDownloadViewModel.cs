using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class GoogleImageDownloadViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IResourceProvider resources;
        private bool closingHanled = false;
        private readonly GoogleImageDownloader downloader;

        public double ItemWidth { get; set; } = 240;
        public double ItemHeight { get; set; } = 180;

        private bool showLoadMore = false;
        public bool ShowLoadMore
        {
            get => showLoadMore;
            set
            {
                showLoadMore = value;
                OnPropertyChanged();
            }
        }

        private bool transparent = false;
        public bool Transparent
        {
            get => transparent;
            set
            {
                var old = transparent;
                transparent = value;                
                OnPropertyChanged();
                if (old != transparent)
                {
                    Search();
                }
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

        private List<GoogleImage> images = new List<GoogleImage>();
        public List<GoogleImage> AvailableImages
        {
            get
            {
                return images;
            }

            set
            {
                images = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GoogleImage> DisplayImages
        {
            get;
        } = new ObservableCollection<GoogleImage>();

        private GoogleImage selectedImage;
        public GoogleImage SelectedImage
        {
            get => selectedImage;
            set
            {
                selectedImage = value;
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
            }, (a) => SelectedImage != null);
        }

        public RelayCommand<object> ItemDoubleClickCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                WindowClosing();
            });
        }

        public RelayCommand<object> LoadMoreCommand
        {

            get => new RelayCommand<object>((a) =>
            {
                LoadMore();
            });
        }

        public RelayCommand<object> SearchCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Search();
            }, (a) => !string.IsNullOrEmpty(SearchTerm));
        }

        public GoogleImageDownloadViewModel(
            IWindowFactory window,
            IResourceProvider resources,
            string initialSearch,
            double itemWidth = 0,
            double itemHeigth = 0)
        {
            this.window = window;
            this.resources = resources;
            if (itemWidth != 0)
            {
                ItemWidth = itemWidth;
            }

            if (itemHeigth != 0)
            {
                ItemHeight = itemHeigth;
            }

            downloader = new GoogleImageDownloader();
            SearchTerm = initialSearch;
            if (!initialSearch.IsNullOrEmpty())
            {
                Search();
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            downloader.Dispose();
            closingHanled = true;
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            downloader.Dispose();
            closingHanled = true;
            CloseView(true);
        }

        public void Search()
        {
            AvailableImages = new List<GoogleImage>();
            if (ProgressViewViewModel.ActivateProgress(() =>
            {
                AvailableImages = downloader.GetImages(SearchTerm, Transparent).GetAwaiter().GetResult();
            }, resources.GetString("LOCDownloadingLabel")) == true)
            {
                if (!AvailableImages.HasItems())
                {
                    return;
                }

                DisplayImages.Clear();
                if (AvailableImages.Count > 20)
                {
                    DisplayImages.AddRange(AvailableImages.Take(20));
                    AvailableImages.RemoveRange(0, 20);
                    ShowLoadMore = true;
                }
                else if (AvailableImages.Count > 0)
                {
                    DisplayImages.AddRange(AvailableImages);
                    AvailableImages.Clear();
                    ShowLoadMore = false;
                }
            }
        }

        public void LoadMore()
        {
            if (AvailableImages.Count > 20)
            {
                DisplayImages.AddRange(AvailableImages.Take(20));
                AvailableImages.RemoveRange(0, 20);
                ShowLoadMore = true;
            }
            else if (AvailableImages.Count > 0)
            {
                DisplayImages.AddRange(AvailableImages);
                AvailableImages.Clear();
                ShowLoadMore = false;
            }
        }

        public void WindowClosing()
        {
            if (!closingHanled)
            {
                downloader.Dispose();
            }
        }
    }
}
