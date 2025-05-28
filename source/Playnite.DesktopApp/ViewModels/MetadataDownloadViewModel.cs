using Playnite.Metadata;
using Playnite.SDK;
using Playnite.Windows;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IResourceProvider resources;

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

        public MetadataDownloadViewModel(IWindowFactory window, IResourceProvider resources)
        {
            this.window = window;
            this.resources = resources;
        }

        public bool? OpenView(ViewMode mode, MetadataDownloaderSettings settings)
        {
            Mode = mode;
            Settings = settings;
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool success)
        {
            if (success && !CanDownloadData())
            {
                Dialogs.ShowErrorMessage(resources.GetString(LOC.MetaNoFieldsSelectedErrorMessage), resources.GetString(LOC.MetaNoFieldsSelectedErrorCaption));
                return;
            }

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

        private bool CanDownloadData()
        {
            return CanDownloadData(Settings.AgeRating)
                || CanDownloadData(Settings.BackgroundImage)
                || CanDownloadData(Settings.CommunityScore)
                || CanDownloadData(Settings.CoverImage)
                || CanDownloadData(Settings.CriticScore)
                || CanDownloadData(Settings.Description)
                || CanDownloadData(Settings.Developer)
                || CanDownloadData(Settings.Feature)
                || CanDownloadData(Settings.Genre)
                || CanDownloadData(Settings.Icon)
                || CanDownloadData(Settings.InstallSize)
                || CanDownloadData(Settings.Links)
                || CanDownloadData(Settings.Name)
                || CanDownloadData(Settings.Platform)
                || CanDownloadData(Settings.Publisher)
                || CanDownloadData(Settings.Region)
                || CanDownloadData(Settings.ReleaseDate)
                || CanDownloadData(Settings.Series)
                || CanDownloadData(Settings.Tag);
        }

        private static bool CanDownloadData(MetadataFieldSettings fieldSettings)
        {
            return fieldSettings.Import && fieldSettings.Sources.Any();
        }
    }
}
