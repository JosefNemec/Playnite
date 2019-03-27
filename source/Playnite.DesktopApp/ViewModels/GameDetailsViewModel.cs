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
        private IResourceProvider resources;
        private IDialogsFactory dialogs;
        private GamesEditor editor;
        private PlayniteSettings settings;

        public bool ShowInfoPanel
        {
            get
            {
                if (game == null)
                {
                    return false;
                }

                return
                    (game.GenreIds?.Any() == true) ||
                    (game.PublisherIds?.Any() == true) ||
                    (game.DeveloperIds?.Any() == true) ||
                    (game.CategoryIds?.Any() == true) ||
                    (game.TagIds?.Any() == true) ||
                    game.ReleaseDate != null ||
                    (game.Links?.Any() == true) ||
                    game.PlatformId != Guid.Empty;

            }
        }

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
                return Game != null && Game.IsUnistalling;
            }
        }

        public bool IsLaunching
        {
            get
            {
                return Game != null && Game.IsLaunching;
            }
        }

        public bool IsPlayAvailable
        {
            get
            {
                return Game != null && Game.IsInstalled && !IsRunning && !IsInstalling && !IsUninstalling && !IsLaunching;
            }
        }

        public bool IsInstallAvailable
        {
            get
            {
                return Game != null && !Game.IsInstalled && !IsRunning && !IsInstalling && !IsUninstalling && !IsLaunching && Game.PluginId != Guid.Empty;
            }
        }

        public Visibility PlayTimeVisibility
        {
            get => game.Playtime > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility PlatformVisibility
        {
            get => game.Platform.Id != Guid.Empty ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility GenreVisibility
        {
            get => game.GenreIds.HasItems() ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility DeveloperVisibility
        {
            get => game.DeveloperIds.HasItems() ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility PublisherVisibility
        {
            get => game.PublisherIds.HasItems() ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility ReleaseDateVisibility
        {
            get => game.ReleaseDate != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility CategoryVisibility
        {
            get => game.CategoryIds.HasItems() ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility TagVisibility
        {
            get => game.TagIds.HasItems() ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility LinkVisibility
        {
            get => game.Links.HasItems() ? Visibility.Visible : Visibility.Collapsed;
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

        public RelayCommand<DatabaseObject> SetPlatformFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Platform);
            });
        }

        public RelayCommand<DatabaseObject> SetPublisherFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Publishers);
            });
        }

        public RelayCommand<DatabaseObject> SetDeveloperFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Developers);
            });
        }

        public RelayCommand<DatabaseObject> SetGenreFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Genres);
            });
        }

        public RelayCommand<DateTime?> SetReleaseDateFilterCommand
        {
            get => new RelayCommand<DateTime?>((filter) =>
            {
                SetReleaseDateFilter(filter);
            });
        }

        public RelayCommand<DatabaseObject> SetCategoryFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Categories);
            });
        }

        public RelayCommand<DatabaseObject> SetTagFilterCommand
        {
            get => new RelayCommand<DatabaseObject>((filter) =>
            {
                SetFilter(filter, GameField.Tags);
            });
        }

        public RelayCommand<DatabaseObject> PlayCommand
        {
            get => new RelayCommand<DatabaseObject>((a) =>
            {
                Play();
            });
        }

        public RelayCommand<object> InstallCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Install();
            });
        }

        public RelayCommand<object> CheckSetupCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CheckSetup();
            });
        }

        public RelayCommand<object> CheckExecutionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CheckExecution();
            });
        }

        public GameDetailsViewModel(GamesCollectionViewEntry game, PlayniteSettings settings, GamesEditor editor, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.resources = resources;
            this.dialogs = dialogs;
            this.editor = editor;
            this.settings = settings;
            Game = game;
            if (game != null)
            {
                Game.PropertyChanged += Game_PropertyChanged;
            }
        }

        public void Dispose()
        {
            if (game != null)
            {
                Game.PropertyChanged -= Game_PropertyChanged;
            }
        }

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShowInfoPanel));
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(IsInstalling));
            OnPropertyChanged(nameof(IsUninstalling));
            OnPropertyChanged(nameof(IsLaunching));
            OnPropertyChanged(nameof(IsPlayAvailable));
            OnPropertyChanged(nameof(IsInstallAvailable));

            foreach (var prop in GetType().GetProperties().Where(a => a.Name.EndsWith("Visibility")))
            {
                OnPropertyChanged(prop.Name);
            }
        }

        public void SetFilter(DatabaseObject value, GameField filterField)
        {
            var filter = new FilterItemProperites() { Ids = new List<Guid> { value.Id } };
            switch (filterField)
            {
                case GameField.Platform:
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
                default:
                    break;
            }
        }

        public void SetReleaseDateFilter(DateTime? date)
        {
            if (date != null)
            {
                settings.FilterSettings.ReleaseDate = date.Value.ToString(Common.Constants.DateUiFormat);
            }
        }

        public void Play()
        {
            editor.PlayGame(game.Game);
        }

        public void Install()
        {
            editor.InstallGame(game.Game);
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

