using Playnite;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI.ViewModels
{
    public class GameDetailsViewModel : ObservableObject, IDisposable
    {
        public enum FilterProperty
        {
            Genres,
            Developers,
            Publishers,
            ReleaseDate,
            Categories,
            Tags,
            Platform
        };
        
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

        private GameViewEntry game;
        public GameViewEntry Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Uri> NavigateUrlCommand
        {
            get => new RelayCommand<Uri>((url) =>
            {
                NavigateUrl(url.AbsoluteUri);
            });
        }

        public RelayCommand<object> SetGenresFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Genres, filter.ToString());
            });
        }

        public RelayCommand<object> SetDevelopersFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Developers, filter.ToString());
            });
        }

        public RelayCommand<object> SetPublishersFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Publishers, filter.ToString());
            });
        }

        public RelayCommand<object> SetReleaseDateFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.ReleaseDate, filter.ToString());
            });
        }

        public RelayCommand<object> SetCategoriesFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Categories, filter.ToString());
            });
        }

        public RelayCommand<object> SetTagsFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Tags, filter.ToString());
            });
        }

        public RelayCommand<object> SetPlatformFilterCommand
        {
            get => new RelayCommand<object>((filter) =>
            {
                SetFilter(FilterProperty.Platform, filter.ToString());
            });
        }

        public RelayCommand<object> PlayCommand
        {
            get => new RelayCommand<object>((a) =>
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

        public GameDetailsViewModel(GameViewEntry game, PlayniteSettings settings, GamesEditor editor, IDialogsFactory dialogs, IResourceProvider resources)
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
        }

        public void NavigateUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                dialogs.ShowMessage(
                    resources.FindString("LOCURLFormatError"),
                    resources.FindString("LOCInvalidURL"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetFilter(FilterProperty property, string value)
        {
            // TODO
            //switch (property)
            //{
            //    case FilterProperty.Genres:
            //        settings.FilterSettings.GenreString = value;
            //        break;
            //    case FilterProperty.Developers:
            //        settings.FilterSettings.Developers = value;
            //        break;
            //    case FilterProperty.Publishers:
            //        settings.FilterSettings.Publishers = value;
            //        break;
            //    case FilterProperty.ReleaseDate:
            //        settings.FilterSettings.ReleaseDate = value;
            //        break;
            //    case FilterProperty.Categories:
            //        settings.FilterSettings.Categories = value;
            //        break;
            //    case FilterProperty.Tags:
            //        settings.FilterSettings.Tags = value;
            //        break;
            //    case FilterProperty.Platform:
            //        settings.FilterSettings.Platforms = value;
            //        break;
            //    default:
            //        break;
            //}
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
                resources.FindString("LOCCancelMonitoringSetupAsk"),
                resources.FindString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }               
        }

        public void CheckExecution()
        {
            if (dialogs.ShowMessage(
                resources.FindString("LOCCancelMonitoringExecutionAsk"),
                resources.FindString("LOCCancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }
        }
    }
}

