using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.ViewModels
{
    public interface IMainViewModelBase
    {
        string ProgressStatus { get; set; }
        double ProgressValue { get; set; }
        double ProgressTotal { get; set; }
        bool ProgressVisible { get; set; }
        BaseCollectionView GamesView { get; set; }
        GamesCollectionViewEntry SelectedGame { get; set; }
        IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
    }

    public class MainViewModelBase : ObservableObject, IMainViewModelBase
    {
        private string progressStatus;
        public string ProgressStatus
        {
            get => progressStatus;
            set
            {
                progressStatus = value;
                OnPropertyChanged();
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private double progressTotal;
        public double ProgressTotal
        {
            get => progressTotal;
            set
            {
                progressTotal = value;
                OnPropertyChanged();
            }
        }

        private bool progressVisible;
        public bool ProgressVisible
        {
            get => progressVisible;
            set
            {
                progressVisible = value;
                OnPropertyChanged();
            }
        }

        private BaseCollectionView gamesView;
        public BaseCollectionView GamesView
        {
            get => gamesView;
            set
            {
                gamesView = value;
                OnPropertyChanged();
            }
        }

        public List<FilterPreset> SortedFilterPresets
        {
            get => Database.FilterPresets.OrderBy(a => a.Name).ToList();
        }

        public List<FilterPreset> SortedFilterFullscreenPresets
        {
            get => Database.FilterPresets.Where(a => a.ShowInFullscreeQuickSelection).OrderBy(a => a.Name).ToList();
        }

        public bool IsDisposing { get; set; } = false;
        public GamesCollectionViewEntry SelectedGame { get; set; }
        public IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }
        public RelayCommand<object> AddFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RenameFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> RemoveFilterPresetCommand { get; private set; }
        public RelayCommand<FilterPreset> ApplyFilterPresetCommand { get; private set; }
        public ApplicationMode Mode { get; }
        public GameDatabase Database { get; }

        public MainViewModelBase(ApplicationMode mode, GameDatabase database)
        {
            Mode = mode;
            Database = database;

            ApplyFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                ApplyFilterPreset(a);
            });

            RemoveFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                RemoveFilterPreset(a);
            }, (a) => a != null);

            RenameFilterPresetCommand = new RelayCommand<FilterPreset>((a) =>
            {
                RenameFilterPreset(a);
            }, (a) => a != null);

            AddFilterPresetCommand = new RelayCommand<object>((a) =>
            {
                AddFilterPreset();
            });
        }

        private PlayniteSettings appSettings;
        public PlayniteSettings AppSettings
        {
            get => appSettings;
            set
            {
                appSettings = value;
                OnPropertyChanged();
            }
        }

        private FilterPreset activeFilterPreset;
        public FilterPreset ActiveFilterPreset
        {
            get => activeFilterPreset;
            set
            {
                activeFilterPreset = value;
                if (Mode == ApplicationMode.Desktop)
                {
                    AppSettings.SelectedFilterPreset = value?.Id ?? Guid.Empty;
                }
                else
                {
                    AppSettings.Fullscreen.SelectedFilterPreset = value?.Id ?? Guid.Empty;
                }

                ApplyFilterPreset(value);
                OnPropertyChanged();
            }
        }

        private void ApplyFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            if (ActiveFilterPreset != preset)
            {
                ActiveFilterPreset = preset;
                return;
            }

            if (GamesView != null)
            {
                GamesView.IgnoreViewConfigChanges = true;
            }

            var filter = Mode == ApplicationMode.Desktop ? AppSettings.FilterSettings : AppSettings.Fullscreen.FilterSettings;
            var view = Mode == ApplicationMode.Desktop ? AppSettings.ViewSettings : (ViewSettingsBase)AppSettings.Fullscreen.ViewSettings;
            filter.ApplyFilter(preset.Settings);
            if (preset.SortingOrder != null)
            {
                view.SortingOrder = preset.SortingOrder.Value;
            }

            if (preset.SortingOrderDirection != null)
            {
                view.SortingOrderDirection = preset.SortingOrderDirection.Value;
            }

            if (Mode == ApplicationMode.Desktop && preset.GroupingOrder != null)
            {
                AppSettings.ViewSettings.GroupingOrder = preset.GroupingOrder.Value;
            }

            if (GamesView != null)
            {
                GamesView.IgnoreViewConfigChanges = false;
                GamesView.RefreshView();
            }
        }

        private void RenameFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            var options = new List<MessageBoxToggle>
            {
                new MessageBoxToggle(LOC.FilterPresetShowOnFSTopPanel, preset.ShowInFullscreeQuickSelection)
            };

            var res = Dialogs.SelectString(LOC.EnterName, string.Empty, preset.Name, options);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                preset.Name = res.SelectedString;
                preset.ShowInFullscreeQuickSelection = options[0].Selected;
                Database.FilterPresets.Update(preset);
            }

            OnPropertyChanged(nameof(SortedFilterPresets));
            OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
        }

        private void RemoveFilterPreset(FilterPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            if (Dialogs.ShowMessage(LOC.AskRemoveItemMessage, LOC.AskRemoveItemTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                Database.FilterPresets.Remove(preset);
                if (ActiveFilterPreset == preset)
                {
                    ActiveFilterPreset = null;
                }

                OnPropertyChanged(nameof(SortedFilterPresets));
                OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            }
        }

        private void AddFilterPreset()
        {
            var options = new List<MessageBoxToggle>
            {
                new MessageBoxToggle(LOC.FilterPresetSaveViewOptions, true),
                new MessageBoxToggle(LOC.FilterPresetShowOnFSTopPanel, false)
            };

            var res = Dialogs.SelectString(LOC.EnterName, string.Empty, string.Empty, options);
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                var filter = Mode == ApplicationMode.Desktop ? AppSettings.FilterSettings : AppSettings.Fullscreen.FilterSettings;
                var preset = new FilterPreset
                {
                    Name = res.SelectedString,
                    Settings = filter.GetClone(),
                    ShowInFullscreeQuickSelection = options[1].Selected
                };

                if (options[0].Selected)
                {
                    var view = Mode == ApplicationMode.Desktop ? AppSettings.ViewSettings : (ViewSettingsBase)AppSettings.Fullscreen.ViewSettings;
                    preset.SortingOrder = view.SortingOrder;
                    preset.SortingOrderDirection = view.SortingOrderDirection;
                    if (Mode == ApplicationMode.Desktop)
                    {
                        preset.GroupingOrder = AppSettings.ViewSettings.GroupingOrder;
                    }
                }

                Database.FilterPresets.Add(preset);
                ActiveFilterPreset = preset;
                OnPropertyChanged(nameof(SortedFilterPresets));
                OnPropertyChanged(nameof(SortedFilterFullscreenPresets));
            }
        }
    }
}
