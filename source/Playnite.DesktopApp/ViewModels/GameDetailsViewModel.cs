using Playnite;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Converters;

namespace Playnite.DesktopApp.ViewModels
{
    public class GameDetailsViewModel : ObservableObject, IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IResourceProvider resources;
        private IDialogsFactory dialogs;
        private DesktopGamesEditor editor;
        private PlayniteSettings settings;

        public bool IsRunning
        {
            get
            {
                return Game != null && Game.IsRunning;
            }
        }

        public bool IsInstalling
        {
            get
            {
                return Game != null && Game.IsInstalling;
            }
        }

        public bool IsUninstalling
        {
            get
            {
                return Game != null && Game.IsUninstalling;
            }
        }

        public bool IsLaunching
        {
            get
            {
                return Game != null && Game.IsLaunching;
            }
        }

        public bool IsInstalled
        {
            get
            {
                return Game != null && Game.IsInstalled;
            }
        }

        public bool IsPlayAvailable
        {
            get
            {
                return Game != null && Game.IsInstalled && !IsRunning && !IsInstalling && !IsUninstalling && !IsLaunching;
            }
        }

        public bool IsContextAvailable
        {
            get
            {
                return Game != null && (IsRunning || IsInstalling || IsUninstalling || IsLaunching || !IsInstalled);
            }
        }

        public bool IsInstallAvailable
        {
            get
            {
                return Game != null && !Game.IsInstalled && !IsRunning && !IsInstalling && !IsUninstalling && !IsLaunching && Game.PluginId != Guid.Empty;
            }
        }

        public string ContextActionDescription
        {
            get
            {
                if (Game?.IsRunning == true)
                {
                    return resources.GetString("LOCGameRunning");
                }
                else if (Game?.IsLaunching == true)
                {
                    return resources.GetString("LOCGameLaunching");
                }
                else if (Game?.IsInstalling == true)
                {
                    return resources.GetString("LOCSetupRunning");
                }
                else if (Game?.IsUninstalling == true)
                {
                    return resources.GetString("LOCUninstalling");
                }
                else if (Game?.IsInstalled == false)
                {
                    return resources.GetString("LOCInstallGame");
                }
                else if (Game?.IsInstalled == true)
                {
                    return resources.GetString("LOCPlayGame");
                }

                return "<ErrorState>";
            }
        }

        public Visibility SourceLibraryVisibility
        {
            get => (settings.DetailsVisibility.Library & Game.LibraryPlugin != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility PlayTimeVisibility
        {
            get => (settings.DetailsVisibility.PlayTime) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility InstallSizeVisibility
        {
            get => (settings.DetailsVisibility.InstallSize && game.InstallSize != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility InstallDirectoryVisibility
        {
            get => (settings.DetailsVisibility.InstallDirectory && game.IsInstalled && !string.IsNullOrWhiteSpace(game.InstallDirectory)) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility LastPlayedVisibility
        {
            get => (settings.DetailsVisibility.LastPlayed) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility AddedVisibility
        {
            get => (settings.DetailsVisibility.Added && game.Added != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility RecentActivityVisibility
        {
            get => (settings.DetailsVisibility.RecentActivity && (game.LastActivity != null || game.Added != null)) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CompletionStatusVisibility
        {
            get => (settings.DetailsVisibility.CompletionStatus && game.CompletionStatus.Id != Guid.Empty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility PlatformVisibility
        {
            get => (settings.DetailsVisibility.Platform && game.Platforms.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility GenreVisibility
        {
            get => (settings.DetailsVisibility.Genres && game.GenreIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility DeveloperVisibility
        {
            get => (settings.DetailsVisibility.Developers && game.DeveloperIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility PublisherVisibility
        {
            get => (settings.DetailsVisibility.Publishers && game.PublisherIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility ReleaseDateVisibility
        {
            get => (settings.DetailsVisibility.ReleaseDate && game.ReleaseDate != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CategoryVisibility
        {
            get => (settings.DetailsVisibility.Categories && game.CategoryIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility TagVisibility
        {
            get => (settings.DetailsVisibility.Tags && game.TagIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility FeatureVisibility
        {
            get => (settings.DetailsVisibility.Features && game.FeatureIds.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility LinkVisibility
        {
            get => (settings.DetailsVisibility.Links && game.Links.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility DescriptionVisibility
        {
            get => (settings.DetailsVisibility.Description && !game.Description.IsNullOrEmpty()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility NotesVisibility
        {
            get => (settings.DetailsVisibility.Notes && !game.Notes.IsNullOrEmpty()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CoverVisibility
        {
            get => (settings.DetailsVisibility.CoverImage) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility BackgroundVisibility
        {
            get => (settings.DetailsVisibility.BackgroundImage) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility IconVisibility
        {
            get => (settings.DetailsVisibility.Icon) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility AgeRatingVisibility
        {
            get => (settings.DetailsVisibility.AgeRating && game.AgeRatings.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility SeriesVisibility
        {
            get => (settings.DetailsVisibility.Series && game.Series.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility SourceVisibility
        {
            get => (settings.DetailsVisibility.Source && game.Source.Id != Guid.Empty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility RegionVisibility
        {
            get => (settings.DetailsVisibility.Region && game.Regions.HasItems()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility VersionVisibility
        {
            get => (settings.DetailsVisibility.Version && !game.Version.IsNullOrEmpty()) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CommunityScoreVisibility
        {
            get => (settings.DetailsVisibility.CommunityScore && game.CommunityScore != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CriticScoreVisibility
        {
            get => (settings.DetailsVisibility.CriticScore && game.CriticScore != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility UserScoreVisibility
        {
            get => (settings.DetailsVisibility.UserScore && game.UserScore != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility NameVisibility
        {
            get => settings.DetailsVisibility.Name ? Visibility.Visible : Visibility.Collapsed;
        }

        private GamesCollectionViewEntry game;
        public GamesCollectionViewEntry Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Guid> SetLibraryFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetPlatformFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetPublisherFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetDeveloperFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetGenreFilterCommand { get; }
        public RelayCommand<ReleaseDate?> SetReleaseDateFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetCategoryFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetTagFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetFeatureFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetAgeRatingFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetSeriesFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetSourceFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetRegionFilterCommand { get; }
        public RelayCommand<DatabaseObject> SetCompletionStatusFilterCommand { get; }
        public RelayCommand<Game> OpenGameLocationCommand { get; }
        public RelayCommand<string> SetVersionFilterCommand { get; }
        public RelayCommand<Link> OpenLinkCommand { get; }
        public RelayCommand<DatabaseObject> PlayCommand { get; }
        public RelayCommand<object> InstallCommand { get; }
        public RelayCommand<object> CheckSetupCommand { get; }
        public RelayCommand<object> CheckExecutionCommand { get; }
        public RelayCommand<object> EditGameCommand { get; }
        public RelayCommand<object> ContextActionCommand { get; }

        public GameDetailsViewModel(GamesCollectionViewEntry game, PlayniteSettings settings)
        {
            this.resources = new ResourceProvider();
            this.settings = settings;
            Game = game;
        }

        public GameDetailsViewModel(GamesCollectionViewEntry game, PlayniteSettings settings, DesktopGamesEditor editor, IDialogsFactory dialogs, IResourceProvider resources)
        {
            OpenLinkCommand = new RelayCommand<Link>((a) =>
            {
                try
                {
                    GlobalCommands.NavigateUrl(Game.Game.ExpandVariables(a.Url));
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to open url.");
                }
            });

            SetLibraryFilterCommand = new RelayCommand<Guid>((a) => SetLibraryFilter(a));
            SetPlatformFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Platforms));
            SetPublisherFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Publishers));
            SetDeveloperFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Developers));
            SetGenreFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Genres));
            SetReleaseDateFilterCommand = new RelayCommand<ReleaseDate?>((a) => SetReleaseDateFilter(a));
            SetCategoryFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Categories));
            SetTagFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Tags));
            SetFeatureFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Features));
            SetAgeRatingFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.AgeRatings));
            SetSeriesFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Series));
            SetSourceFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Source));
            SetRegionFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.Regions));
            SetVersionFilterCommand = new RelayCommand<string>((filter) => SetVersionFilter(filter));
            SetCompletionStatusFilterCommand = new RelayCommand<DatabaseObject>((a) => SetFilter(a, GameField.CompletionStatus));
            PlayCommand = new RelayCommand<DatabaseObject>((a) => Play());
            InstallCommand = new RelayCommand<object>((a) => Install());
            CheckSetupCommand = new RelayCommand<object>((a) => CheckSetup());
            CheckExecutionCommand = new RelayCommand<object>((a) => CheckExecution());
            EditGameCommand = new RelayCommand<object>((a) => EditGame());
            ContextActionCommand = new RelayCommand<object>((a) =>
            {
                if (Game?.IsInstalling == true || Game?.IsUninstalling == true)
                {
                    CheckSetup();
                }
                else if (Game?.IsRunning == true || Game?.IsLaunching == true)
                {
                    CheckExecution();
                }
                else if (Game?.IsInstalled == false)
                {
                    Install();
                }
                else if (Game?.IsInstalled == true)
                {
                    Play();
                }
            });

            OpenGameLocationCommand = new RelayCommand<Game>((a) =>
            {
                editor.OpenGameLocation(a);
            });

            this.resources = resources;
            this.dialogs = dialogs;
            this.editor = editor;
            this.settings = settings;
            Game = game;
            if (game != null)
            {
                Game.PropertyChanged += Game_PropertyChanged;
            }

            settings.PropertyChanged += Settings_PropertyChanged;
        }

        public void Dispose()
        {
            if (game != null)
            {
                Game.PropertyChanged -= Game_PropertyChanged;
            }

            settings.PropertyChanged -= Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyVisibilityChange();
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsInstalling));
            OnPropertyChanged(nameof(IsUninstalling));
            OnPropertyChanged(nameof(IsLaunching));
            OnPropertyChanged(nameof(IsPlayAvailable));
            OnPropertyChanged(nameof(IsContextAvailable));
            OnPropertyChanged(nameof(IsInstallAvailable));
            OnPropertyChanged(nameof(ContextActionDescription));
            NotifyVisibilityChange();
        }

        private void NotifyVisibilityChange()
        {
            foreach (var prop in GetType().GetProperties().Where(a => a.Name.EndsWith("Visibility")))
            {
                OnPropertyChanged(prop.Name);
            }
        }

        public void SetFilter(DatabaseObject value, GameField filterField)
        {
            var filter = new IdItemFilterItemProperties() { Ids = new List<Guid> { value.Id } };
            switch (filterField)
            {
                case GameField.Platforms:
                    settings.FilterSettings.Platform = filter;
                    break;
                case GameField.Genres:
                    settings.FilterSettings.Genre = filter;
                    break;
                case GameField.Developers:
                    settings.FilterSettings.Developer = filter;
                    break;
                case GameField.Publishers:
                    settings.FilterSettings.Publisher = filter;
                    break;
                case GameField.Categories:
                    settings.FilterSettings.Category = filter;
                    break;
                case GameField.Tags:
                    settings.FilterSettings.Tag = filter;
                    break;
                case GameField.AgeRatings:
                    settings.FilterSettings.AgeRating = filter;
                    break;
                case GameField.Series:
                    settings.FilterSettings.Series = filter;
                    break;
                case GameField.Regions:
                    settings.FilterSettings.Region = filter;
                    break;
                case GameField.Source:
                    settings.FilterSettings.Source = filter;
                    break;
                case GameField.Features:
                    settings.FilterSettings.Feature = filter;
                    break;
                case GameField.CompletionStatus:
                    settings.FilterSettings.CompletionStatuses = filter;
                    break;
                default:
                    break;
            }

            settings.FilterPanelVisible = true;
        }

        public void SetReleaseDateFilter(ReleaseDate? date)
        {
            if (date != null)
            {
                settings.FilterSettings.ReleaseYear = new StringFilterItemProperties(date.Value.Year.ToString());
                settings.FilterPanelVisible = true;
            }
        }

        public void SetVersionFilter(string version)
        {
            if (!version.IsNullOrEmpty())
            {
                settings.FilterSettings.Version = version;
                settings.FilterPanelVisible = true;
            }
        }

        public void SetLibraryFilter(Guid LibraryId)
        {
            var filter = new IdItemFilterItemProperties() { Ids = new List<Guid> { LibraryId } };
            settings.FilterSettings.Library = filter;
            settings.FilterPanelVisible = true;
        }

        public void Play()
        {
            editor.PlayGame(game.Game, true);
        }

        public void Install()
        {
            editor.InstallGame(game.Game);
        }

        public void EditGame()
        {
            editor.EditGame(game.Game);
        }

        public void CheckSetup()
        {
            if (dialogs.ShowMessage(
                resources.GetString("LOCCancelMonitoringSetupAsk"),
                resources.GetString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }
        }

        public void CheckExecution()
        {
            if (dialogs.ShowMessage(
                resources.GetString("LOCCancelMonitoringExecutionAsk"),
                resources.GetString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }
        }
    }
}

