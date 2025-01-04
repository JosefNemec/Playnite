using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private int? searchWidth;
        public int? SearchWidth
        {
            get => searchWidth;
            set
            {
                searchWidth = value;
                OnPropertyChanged();
            }
        }

        private int? searchHeight;
        public int? SearchHeight
        {
            get => searchHeight;
            set
            {
                searchHeight = value;
                OnPropertyChanged();
            }
        }

        private SafeSearchSettings safeSearch;
        public SafeSearchSettings SafeSearch
        {
            get => safeSearch;
            set
            {
                safeSearch = value;
                OnPropertyChanged();
                Search();
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

        public RelayCommand<string> SetSearchResolutionCommand
        {
            get => new RelayCommand<string>((resolution) =>
            {
                SetSearchResolution(resolution);
            });
        }

        public RelayCommand<string> ClearSearchResolutionCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                ClearSearchResolution();
            });
        }

        public GoogleImageDownloadViewModel(
            IWindowFactory window,
            IResourceProvider resources,
            string initialSearch,
            SafeSearchSettings safeSearch,
            double itemWidth = 0,
            double itemHeigth = 0)
        {
            this.window = window;
            this.resources = resources;
            this.safeSearch = safeSearch;
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
            var query = SearchTerm;
            if (SearchWidth != null && SearchHeight != null && !query.Contains("imagesize:"))
            {
                query = $"{query} imagesize:{SearchWidth}x{SearchHeight}";
            }

            if (GlobalProgress.ActivateProgress((_) =>
            {
                AvailableImages = downloader.GetImages(query, SafeSearch, Transparent).GetAwaiter().GetResult();
            }, new GlobalProgressOptions("LOCDownloadingLabel")).Result == true)
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

        public void SetSearchResolution(string resolution)
        {
            var regex = Regex.Match(resolution, @"(\d+)x(\d+)");
            if (regex.Success)
            {
                SearchWidth = int.Parse(regex.Groups[1].Value);
                SearchHeight = int.Parse(regex.Groups[2].Value);
            }
        }

        public void ClearSearchResolution()
        {
            SearchWidth = null;
            SearchHeight = null;
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