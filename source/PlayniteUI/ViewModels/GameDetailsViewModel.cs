using Playnite;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI.ViewModels
{
    public class GameDetailsViewModel : ObservableObject
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
        private Settings settings;

        public bool ShowInfoPanel
        {
            get
            {
                if (game == null)
                {
                    return false;
                }

                return
                    (game.Genres != null && game.Genres.Count > 0) ||
                    (game.Publishers != null && game.Publishers.Count > 0) ||
                    (game.Developers != null && game.Developers.Count > 0) ||
                    (game.Categories != null && game.Categories.Count > 0) ||
                    (game.Tags != null && game.Tags.Count > 0) ||
                    game.ReleaseDate != null ||
                    (game.Links != null && game.Links.Count > 0) ||
                    !string.IsNullOrEmpty(game.Platform.Name);

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
                return Game != null && !Game.IsInstalled && !IsRunning && !IsInstalling && !IsUninstalling && !IsLaunching && Game.Provider != Provider.Custom;
            }
        }

        private GameViewEntry game;
        public GameViewEntry Game
        {
            get => game;
            set
            {
                game = value;
                OnPropertyChanged("Game");
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

        public GameDetailsViewModel(GameViewEntry game, Settings settings, GamesEditor editor, IDialogsFactory dialogs, IResourceProvider resources)
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

        private void Game_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("ShowInfoPanel");
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("IsInstalling");
            OnPropertyChanged("IsUninstalling");
            OnPropertyChanged("IsLaunching");
            OnPropertyChanged("IsPlayAvailable");
            OnPropertyChanged("IsInstallAvailable");
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
                    resources.FindString("URLFormatError"),
                    resources.FindString("InvalidURL"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetFilter(FilterProperty property, string value)
        {
            switch (property)
            {
                case FilterProperty.Genres:
                    settings.FilterSettings.Genres = new List<string>() { value };
                    break;
                case FilterProperty.Developers:
                    settings.FilterSettings.Developers = new List<string>() { value };
                    break;
                case FilterProperty.Publishers:
                    settings.FilterSettings.Publishers = new List<string>() { value };
                    break;
                case FilterProperty.ReleaseDate:
                    settings.FilterSettings.ReleaseDate = value;
                    break;
                case FilterProperty.Categories:
                    settings.FilterSettings.Categories = new List<string>() { value };
                    break;
                case FilterProperty.Tags:
                    settings.FilterSettings.Tags = new List<string>() { value };
                    break;
                case FilterProperty.Platform:
                    settings.FilterSettings.Platforms = new List<string>() { value };
                    break;
                default:
                    break;
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
                resources.FindString("CancelMonitoringSetupAsk"),
                resources.FindString("CancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }               
        }

        public void CheckExecution()
        {
            if (dialogs.ShowMessage(
                resources.FindString("CancelMonitoringExecutionAsk"),
                resources.FindString("CancelMonitoringAskTitle"),
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                editor.CancelGameMonitoring(game.Game);
            }
        }
    }
}

