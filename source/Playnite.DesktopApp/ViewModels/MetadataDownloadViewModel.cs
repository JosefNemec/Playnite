using Playnite.SDK;
using Playnite.Metadata;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class MetadataDownloadViewModel : ObservableObject
    {
        public enum ViewMode
        {
            Wizard,
            Manual
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;

        public bool SaveAsDefault { get; set; }

        private int viewTabIndex = 0;
        public int ViewTabIndex
        {
            get => viewTabIndex;
            set
            {
                viewTabIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowDownloadButton));
                OnPropertyChanged(nameof(ShowNextButton));
                OnPropertyChanged(nameof(ShowBackButton));
                OnPropertyChanged(nameof(ShowFinishButton));
            }
        }

        public bool ShowDownloadButton
        {
            get => Mode == ViewMode.Manual && ViewTabIndex == 1;
        }

        public bool ShowFinishButton
        {
            get => Mode == ViewMode.Wizard && ViewTabIndex == 1;
        }

        public bool ShowNextButton
        {
            get => Mode == ViewMode.Manual && ViewTabIndex == 0;
        }

        public bool ShowBackButton
        {
            get => Mode == ViewMode.Manual && ViewTabIndex == 1;
        }

        private ViewMode mode;
        public ViewMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
                ViewTabIndex = mode == ViewMode.Manual ? 0 : 1;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowDownloadButton));
                OnPropertyChanged(nameof(ShowNextButton));
                OnPropertyChanged(nameof(ShowBackButton));
                OnPropertyChanged(nameof(ShowFinishButton));
            }
        }

        private MetadataDownloaderSettings settings = new MetadataDownloaderSettings();
        public MetadataDownloaderSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> DownloadCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(true);
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> NextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Next();
            });
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Back();
            });
        }

        public MetadataDownloadViewModel(IWindowFactory window)
        {
            this.window = window;            
        }

        public bool? OpenView(ViewMode mode, MetadataDownloaderSettings settings)
        {
            Mode = mode;
            Settings = settings;
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool success)
        {
            if (success && SaveAsDefault)
            {
                PlayniteApplication.Current.AppSettings.MetadataSettings = Settings;
                PlayniteApplication.Current.AppSettings.SaveSettings();
            }

            window.Close(success);
        }

        public void Next()
        {
            ViewTabIndex = 1;
        }

        public void Back()
        {
            ViewTabIndex = 0;
        }
    }
}
