using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Playnite.FullscreenApp.ViewModels
{
    public partial class FullscreenAppViewModel
    {
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; private set; }
        public RelayCommand<EventArgs> WindowGotFocusCommand { get; private set; }
        public RelayCommand<object> ToggleFullscreenCommand { get; private set; }
        public RelayCommand<object> OpenMainMenuCommand { get; private set; }
        public RelayCommand<object> OpenNotificationsMenuCommand { get; private set; }
        public RelayCommand OpenGameMenuCommand { get; private set; }
        public RelayCommand<object> ToggleGameDetailsCommand { get; private set; }
        public RelayCommand<object> ToggleFiltersCommand { get; private set; }
        public RelayCommand<GameField> LoadSubFilterCommand { get; private set; }
        public RelayCommand<object> CloseSubFilterCommand { get; private set; }
        public RelayCommand<object> CloseAdditionalFilterCommand { get; private set; }
        public RelayCommand<object> ClearFiltersCommand { get; private set; }
        public RelayCommand<object> OpenAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> CloseAdditionalFiltersCommand { get; private set; }
        public RelayCommand<object> ActivateSelectedCommand { get; private set; }
        public RelayCommand<object> OpenSearchCommand { get; private set; }
        public RelayCommand<object> NextFilterViewCommand { get; private set; }
        public RelayCommand<object> PrevFilterViewCommand { get; private set; }
        public RelayCommand<object> SelectPrevGameCommand { get; private set; }
        public RelayCommand<object> SelectNextGameCommand { get; private set; }
        public RelayCommand<DragEventArgs> FileDroppedCommand { get; private set; }
        public RelayCommand<object> CloseGameStatusCommand { get; private set; }
        public RelayCommand SwitchToDesktopCommand { get; private set; }
        public RelayCommand SelectFilterPresetCommand { get; private set; }
        public RelayCommand MinimizeCommand { get; private set; }

        private void InitializeCommands()
        {
            WindowClosingCommand = new RelayCommand<CancelEventArgs>((a) =>
            {
                if (!ignoreCloseActions)
                {
                    Dispose();
                    App.Quit();
                }
            });

            WindowGotFocusCommand = new RelayCommand<EventArgs>((a) =>
            {
                if (Keyboard.FocusedElement == Window.Window && isInitialized && !ignoreCloseActions)
                {
                    Logger.Warn("Lost keyboard focus from known controls, trying to focus something.");
                    foreach (var child in ElementTreeHelper.FindVisualChildren<FrameworkElement>(Window.Window))
                    {
                        if (child.Focusable && child.IsVisible)
                        {
                            Logger.Debug($"Focusing {child}");
                            child.Focus();
                            return;
                        }
                    }
                }
            });

            ToggleFullscreenCommand = new RelayCommand<object>((a) =>
            {
                ToggleFullscreen();
            });

            OpenGameMenuCommand = new RelayCommand(() =>
            {
                OpenGameMenu();
            }, () => SelectedGameDetails != null);

            ToggleGameDetailsCommand = new RelayCommand<object>((a) =>
            {
                GameDetailsVisible = !GameDetailsVisible;
                GameListVisible = !GameListVisible;

                if (!GameDetailsVisible)
                {
                    GameDetailsFocused = false;
                    GameListFocused = true;
                }
                else
                {
                    GameDetailsFocused = true;
                    GameListFocused = false;
                }
            }, (a) => SelectedGame != null);

            LoadSubFilterCommand = new RelayCommand<GameField>((gameField) =>
            {
                switch (gameField)
                {
                    case GameField.PluginId:
                        OpenSubFilter("LOCLibrary", nameof(DatabaseFilter.Libraries), nameof(FilterSettings.Library), true);
                        break;
                    case GameField.Categories:
                        OpenSubFilter("LOCCategoryLabel", nameof(DatabaseFilter.Categories), nameof(FilterSettings.Category), true);
                        break;
                    case GameField.Platforms:
                        OpenSubFilter("LOCPlatformTitle", nameof(DatabaseFilter.Platforms), nameof(FilterSettings.Platform), true);
                        break;
                    case GameField.CompletionStatus:
                        OpenSubFilter("LOCCompletionStatus", nameof(DatabaseFilter.CompletionStatuses), nameof(FilterSettings.CompletionStatuses));
                        break;
                    case GameField.ReleaseYear:
                        OpenSubStringFilter("LOCGameReleaseYearTitle", nameof(DatabaseFilter.ReleaseYears), nameof(FilterSettings.ReleaseYear));
                        break;
                    case GameField.Genres:
                        OpenSubFilter("LOCGenreLabel", nameof(DatabaseFilter.Genres), nameof(FilterSettings.Genre));
                        break;
                    case GameField.Developers:
                        OpenSubFilter("LOCDeveloperLabel", nameof(DatabaseFilter.Developers), nameof(FilterSettings.Developer));
                        break;
                    case GameField.Publishers:
                        OpenSubFilter("LOCPublisherLabel", nameof(DatabaseFilter.Publishers), nameof(FilterSettings.Publisher));
                        break;
                    case GameField.Features:
                        OpenSubFilter("LOCFeatureLabel", nameof(DatabaseFilter.Features), nameof(FilterSettings.Feature));
                        break;
                    case GameField.Tags:
                        OpenSubFilter("LOCTagLabel", nameof(DatabaseFilter.Tags), nameof(FilterSettings.Tag));
                        break;
                    case GameField.Playtime:
                        OpenSubEnumFilter("LOCTimePlayed", typeof(PlaytimeCategory), nameof(FilterSettings.PlayTime));
                        break;
                    case GameField.InstallSize:
                        OpenSubEnumFilter("LOCInstallSizeLabel", typeof(InstallSizeGroup), nameof(FilterSettings.InstallSize));
                        break;
                    case GameField.Series:
                        OpenSubFilter("LOCSeriesLabel", nameof(DatabaseFilter.Series), nameof(FilterSettings.Series));
                        break;
                    case GameField.Regions:
                        OpenSubFilter("LOCRegionLabel", nameof(DatabaseFilter.Regions), nameof(FilterSettings.Region));
                        break;
                    case GameField.Source:
                        OpenSubFilter("LOCSourceLabel", nameof(DatabaseFilter.Sources), nameof(FilterSettings.Source));
                        break;
                    case GameField.AgeRatings:
                        OpenSubFilter("LOCAgeRatingLabel", nameof(DatabaseFilter.AgeRatings), nameof(FilterSettings.AgeRating));
                        break;
                    case GameField.UserScore:
                        OpenSubEnumFilter("LOCUserScore", typeof(ScoreGroup), nameof(FilterSettings.UserScore));
                        break;
                    case GameField.CommunityScore:
                        OpenSubEnumFilter("LOCCommunityScore", typeof(ScoreGroup), nameof(FilterSettings.CommunityScore));
                        break;
                    case GameField.CriticScore:
                        OpenSubEnumFilter("LOCCriticScore", typeof(ScoreGroup), nameof(FilterSettings.CriticScore));
                        break;
                    case GameField.LastActivity:
                        OpenSubEnumFilter("LOCGameLastActivityTitle", typeof(PastTimeSegment), nameof(FilterSettings.LastActivity));
                        break;
                    case GameField.RecentActivity:
                        OpenSubEnumFilter("LOCRecentActivityLabel", typeof(PastTimeSegment), nameof(FilterSettings.RecentActivity));
                        break;
                    case GameField.Added:
                        OpenSubEnumFilter("LOCAddedLabel", typeof(PastTimeSegment), nameof(FilterSettings.Added));
                        break;
                    case GameField.Modified:
                        OpenSubEnumFilter("LOCModifiedLabel", typeof(PastTimeSegment), nameof(FilterSettings.Modified));
                        break;
                }
            });

            OpenAdditionalFiltersCommand = new RelayCommand<object>((a) =>
            {
                FilterPanelVisible = false;
                FilterAdditionalPanelVisible = true;
            });

            CloseAdditionalFiltersCommand = new RelayCommand<object>((a) =>
            {
                FilterAdditionalPanelVisible = false;
                FilterPanelVisible = true;
            });

            CloseAdditionalFilterCommand = new RelayCommand<object>((a) =>
            {
                ((IDisposable)SubFilterControl).Dispose();
                SubFilterControl = null;
                FilterAdditionalPanelVisible = true;
            });

            CloseSubFilterCommand = new RelayCommand<object>((a) =>
            {
                if (SubFilterControl != null)
                {
                    FilterPanelVisible = true;
                    ((IDisposable)SubFilterControl).Dispose();
                    SubFilterControl = null;
                }
            });

            ToggleFiltersCommand = new RelayCommand<object>((a) =>
            {
                if (SubFilterVisible)
                {
                    ((IDisposable)SubFilterControl).Dispose();
                    SubFilterControl = null;
                    FilterPanelVisible = false;
                }
                else if (FilterAdditionalPanelVisible)
                {
                    FilterAdditionalPanelVisible = false;
                }
                else
                {
                    FilterPanelVisible = !FilterPanelVisible;
                }

                if (FilterPanelVisible)
                {
                    GameListFocused = false;
                }
                else
                {
                    GameListFocused = true;
                }
            });

            ClearFiltersCommand = new RelayCommand<object>((a) =>
            {
                AppSettings.Fullscreen.FilterSettings.ClearFilters();
                ActiveFilterPreset = null;
            });

            ActivateSelectedCommand = new RelayCommand<object>((a) =>
            {
                if (SelectedGame?.IsInstalled == true)
                {
                    GamesEditor.PlayGame(SelectedGame.Game);
                }
                else if (SelectedGame?.IsInstalled == false)
                {
                    GamesEditor.InstallGame(SelectedGame.Game);
                }
            }, (a) => Database?.IsOpen == true);

            OpenSearchCommand = new RelayCommand<object>((a) =>
            {
                GameListFocused = false;
                var oldSearch = AppSettings.Fullscreen.FilterSettings.Name;
                var input = new Windows.TextInputWindow();
                input.PropertyChanged += SearchText_PropertyChanged;
                var res = input.ShowInput(WindowManager.CurrentWindow, "", "", AppSettings.Fullscreen.FilterSettings.Name);
                input.PropertyChanged -= SearchText_PropertyChanged;
                if (res.Result != true)
                {
                    AppSettings.Fullscreen.FilterSettings.Name = oldSearch;
                }

                GameListFocused = true;
            });

            NextFilterViewCommand = new RelayCommand<object>((a) =>
            {
                var presets = SortedFilterFullscreenPresets;
                if (!presets.HasItems())
                {
                    return;
                }

                if (ActiveFilterPreset == null)
                {
                    ActiveFilterPreset = presets[0];
                }
                else
                {
                    var curIndex = presets.IndexOf(ActiveFilterPreset);
                    if (curIndex < (presets.Count - 1))
                    {
                        ActiveFilterPreset = presets[curIndex + 1];
                    }
                }
            }, (a) => Database?.IsOpen == true);

            PrevFilterViewCommand = new RelayCommand<object>((a) =>
            {
                var presets = SortedFilterFullscreenPresets;
                if (!presets.HasItems())
                {
                    return;
                }

                if (ActiveFilterPreset == null)
                {
                    ActiveFilterPreset = presets[0];
                }
                else
                {
                    var curIndex = presets.IndexOf(ActiveFilterPreset);
                    if (curIndex > 0)
                    {
                        ActiveFilterPreset = presets[curIndex - 1];
                    }
                }
            }, (a) => Database?.IsOpen == true);

            SelectPrevGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(SelectedGame);
                var prevIndex = currIndex - 1;
                if (prevIndex >= 0)
                {
                    SelectedGame = GamesView.CollectionView.GetItemAt(prevIndex) as GamesCollectionViewEntry;
                }
            }, (a) => Database?.IsOpen == true);

            SelectNextGameCommand = new RelayCommand<object>((a) =>
            {
                var currIndex = GamesView.CollectionView.IndexOf(SelectedGame);
                var nextIndex = currIndex + 1;
                if (nextIndex < GamesView.CollectionView.Count)
                {
                    SelectedGame = GamesView.CollectionView.GetItemAt(nextIndex) as GamesCollectionViewEntry;
                }
            }, (a) => Database?.IsOpen == true);

            FileDroppedCommand = new RelayCommand<DragEventArgs>((args) =>
            {
                OnFileDropped(args);
            });

            OpenMainMenuCommand = new RelayCommand<object>((_) => OpenMainMenu());
            OpenNotificationsMenuCommand = new RelayCommand<object>((_) => OpenNotificationsMenu());

            CloseGameStatusCommand = new RelayCommand<object>((_) =>
            {
                GameStatusVisible = false;
                GameListFocused = true;
            });

            SwitchToDesktopCommand = new RelayCommand(() => SwitchToDesktopMode());
            SelectFilterPresetCommand = new RelayCommand(() => SelectFilterPreset());
            MinimizeCommand = new RelayCommand(() => MinimizeWindow());
        }
    }
}
