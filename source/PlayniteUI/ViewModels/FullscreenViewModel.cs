using NLog;
using Playnite;
using Playnite.Database;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlayniteUI.ViewModels
{
    public class FullscreenViewModel : MainViewModel
    {
        public bool IsFullScreen
        {
            get;
            private set;
        } = true;

        private double viewLeft = 0;
        public double ViewLeft
        {
            get => viewLeft;
            set
            {
                viewLeft = value;
                OnPropertyChanged("ViewLeft");
            }
        }

        private double viewTop = 0;
        public double ViewTop
        {
            get => viewTop;
            set
            {
                viewTop = value;
                OnPropertyChanged("ViewTop");
            }
        }

        private double viewWidth = Screen.PrimaryScreen.Bounds.Width;
        public double ViewWidth
        {
            get => viewWidth;
            set
            {
                viewWidth = value;
                OnPropertyChanged("ViewWidth");
            }
        }

        private double viewHeight = Screen.PrimaryScreen.Bounds.Height;
        public double ViewHeight
        {
            get => viewHeight;
            set
            {
                viewHeight = value;
                OnPropertyChanged("ViewHeight");
            }
        }

        private bool showFilter = false;
        public bool ShowFilter
        {
            get => showFilter;
            set
            {
                showFilter = value;
                OnPropertyChanged("ShowFilter");
            }
        }

        private bool showGameDetails = false;
        public bool ShowGameDetails
        {
            get => showGameDetails;
            set
            {
                showGameDetails = value;
                OnPropertyChanged("ShowGameDetails");
                OnPropertyChanged("ShowBackOption");
                OnPropertyChanged("ShowDetailsOption");
            }
        }

        public bool ShowInstallOption
        {
            get => SelectedGame?.IsInstalled == false;
        }

        public bool ShowPlayOption
        {
            get => SelectedGame?.IsInstalled == true;
        }
        
        public bool ShowBackOption
        {
            get => SelectedGame != null && ShowGameDetails;
        }

        public bool ShowDetailsOption
        {
            get => SelectedGame != null && !ShowGameDetails;
        }

        private bool showExitMenu = false;
        public bool ShowExitMenu
        {
            get => showExitMenu;
            set
            {
                showExitMenu = value;
                OnPropertyChanged("ShowExitMenu");
            }
        }

        public new RelayCommand<object> OpenSearchCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                OpenSearch();
            });
        }

        public RelayCommand<object> SwitchToDesktopModeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SwitchToDesktopMode(true);
            });
        }

        public RelayCommand<object> ToggleExitMenuCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleExitMenu();
            });
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {                
                ToggleFilter();
            });
        }

        public RelayCommand<object> ToggleFilterCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleFilter();
            });
        }

        public RelayCommand<object> ToggleGameDetailsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleGameDetails();
            });
        }

        public RelayCommand<object> ToggleFullscreenCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleFullscreen();
            });
        }

        public RelayCommand<object> ToggleSortingOrderCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleSortingOrder();
            });
        }

        public RelayCommand<object> ToggleInstallFilterCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleInstallFilter();
            });
        }

        public RelayCommand<object> ClearSearchCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ClearSearch();
            });
        }

        public FullscreenViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            Settings settings,
            GamesEditor gamesEditor) : base(database, window, dialogs, resources, settings, gamesEditor)
        {
            IsFullscreenView = true;
            PropertyChanged += FullscreenViewModel_PropertyChanged;
        }

        private void FullscreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedGame")
            {
                OnPropertyChanged("ShowInstallOption");
                OnPropertyChanged("ShowPlayOption");
                OnPropertyChanged("ShowBackOption");
                OnPropertyChanged("ShowDetailsOption");
            }
        }

        public void OpenView(bool fullscreen)
        {
            Window.Show(this);
            Window.BringToForeground();
            if (!fullscreen)
            {
                LeaveFullscreen();
            }

            InitializeView();
            AppSettings.FullScreenFilterSettings.FilterChanged += FullScreenFilterSettings_FilterChanged;
            AppSettings.FullscreenViewSettings.PropertyChanged += FullscreenViewSettings_PropertyChanged;
        }

        public void ToggleFullscreen()
        {
            if (IsFullScreen)
            {
                LeaveFullscreen();
            }
            else
            {
                GoFullscreen();
            }
        }

        public void GoFullscreen()
        {
            IsFullScreen = true;
            ViewLeft = 0;
            ViewTop = 0;
            ViewWidth = Screen.PrimaryScreen.Bounds.Width;
            ViewHeight = Screen.PrimaryScreen.Bounds.Height;
        }

        public void LeaveFullscreen()
        {
            IsFullScreen = false;
            ViewWidth = Screen.PrimaryScreen.Bounds.Width / 1.5;
            ViewHeight = Screen.PrimaryScreen.Bounds.Height / 1.5;
            ViewLeft = (Screen.PrimaryScreen.Bounds.Width - ViewWidth) / 2;
            ViewTop = (Screen.PrimaryScreen.Bounds.Height - ViewHeight) / 2;
        }

        public void ToggleFilter()
        {
            ShowFilter = !ShowFilter;
        }

        public void ToggleGameDetails()
        {
            ShowGameDetails = !ShowGameDetails;
        }

        public void ToggleExitMenu()
        {
            ShowExitMenu = !ShowExitMenu;
        }

        public void ToggleSortingOrder()
        {
            if (AppSettings.FullscreenViewSettings.SortingOrder == Playnite.SortOrder.Name)
            {
                AppSettings.FullscreenViewSettings.SortingOrder = Playnite.SortOrder.LastActivity;
            }
            else if (AppSettings.FullscreenViewSettings.SortingOrder == Playnite.SortOrder.LastActivity)
            {
                AppSettings.FullscreenViewSettings.SortingOrder = Playnite.SortOrder.Playtime;
            }
            else if (AppSettings.FullscreenViewSettings.SortingOrder == Playnite.SortOrder.Playtime)
            {
                AppSettings.FullscreenViewSettings.SortingOrder = Playnite.SortOrder.Name;
            }
        }

        public void ToggleInstallFilter()
        {
            if (!AppSettings.FullScreenFilterSettings.IsInstalled && !AppSettings.FullScreenFilterSettings.IsUnInstalled)
            {                
                AppSettings.FullScreenFilterSettings.IsInstalled = true;
                AppSettings.FullScreenFilterSettings.IsUnInstalled = false;
            }
            else if (AppSettings.FullScreenFilterSettings.IsInstalled)
            {
                AppSettings.FullScreenFilterSettings.IsInstalled = false;
                AppSettings.FullScreenFilterSettings.IsUnInstalled = true;
            }
            else if (AppSettings.FullScreenFilterSettings.IsUnInstalled)
            {
                AppSettings.FullScreenFilterSettings.IsInstalled = false;
                AppSettings.FullScreenFilterSettings.IsUnInstalled = false;
            }

            // TODO: Handle this properly inside of Settings class.
            AppSettings.OnPropertyChanged("FullScreenFilterSettings");
        }

        public void ClearSearch()
        {
            AppSettings.FullScreenFilterSettings.Name = null;
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            if (ignoreCloseActions)
            {
                return;
            }

            if (AppSettings.StartInFullscreen)
            {
                Dispose();
                App.CurrentApp.Quit();
            }
            else
            {
                SwitchToDesktopMode(false);
            }
        }

        public void SwitchToDesktopMode(bool closeView)
        {
            if (GlobalTaskHandler.IsActive)
            {
                ProgressViewViewModel.ActivateProgress(() => GlobalTaskHandler.CancelAndWait(), Resources.FindString("LOCOpeningDesktopModeMessage"));
            }

            if (closeView)
            {
                CloseView();
            }
            else
            {
                Dispose();
            }

            App.CurrentApp.OpenNormalView(0, false);
        }

        public void OpenSearch()
        {
            var result = Dialogs.SelectString("", "", AppSettings.FullScreenFilterSettings.Name);
            if (result.Result)
            {
                AppSettings.FullScreenFilterSettings.Name = result.SelectedString;                
            }
        }

        public override void ClearFilters()
        {
            AppSettings.FullScreenFilterSettings.ClearFilters();
        }

        private void FullScreenFilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GameViewEntry).ProviderId);
            }
            else
            {
                SelectedGame = null;
            }
        }

        private void FullscreenViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
            if (GamesView.CollectionView.Count > 0)
            {
                SelectGame((GamesView.CollectionView.GetItemAt(0) as GameViewEntry).ProviderId);
            }
            else
            {
                SelectedGame = null;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            AppSettings.FullScreenFilterSettings.FilterChanged -= FullScreenFilterSettings_FilterChanged;
            AppSettings.FullscreenViewSettings.PropertyChanged -= FullscreenViewSettings_PropertyChanged;
        }
    }
}
