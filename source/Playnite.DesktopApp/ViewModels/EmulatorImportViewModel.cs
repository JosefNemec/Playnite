using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Playnite.Common;
using Playnite.Windows;
using Playnite.DesktopApp.Windows;
using Playnite.ViewModels;

namespace Playnite.DesktopApp.ViewModels
{
    public class SelectedPlatformsToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var platforms = (IEnumerable<Guid>)values[0];
            var allPlatforms = (IEnumerable<Platform>)values[1];
            return string.Join(", ", allPlatforms.Where(a => platforms?.Contains(a.Id) == true)?.Select(a => a.Name));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SelectedPlatformsToListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var platforms = (IEnumerable<Guid>)values[0];
            var allPlatforms = (IEnumerable<Platform>)values[1];
            return allPlatforms.Where(a => platforms?.Contains(a.Id) == true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class EmulatorImportViewModel : ObservableObject
    {
        public enum DialogType
        {
            EmulatorImport,
            EmulatorDownload,
            GameImport,
            Wizard
        }

        public class ImportableEmulator : ScannedEmulator
        {
            public bool Import
            {
                get; set;
            } = true;

            public ImportableEmulator(ScannedEmulator emulator) : base(emulator.Name, emulator.Profiles)
            {
            }
        }

        public class ImportableGame
        {
            public bool Import
            {
                get; set;
            } = true;

            public Game Game
            {
                get; set;
            }

            public Emulator Emulator
            {
                get; set;
            }

            public EmulatorProfile EmulatorProfile
            {
                get; set;
            }

            public ImportableGame(Game game, Emulator emulator, EmulatorProfile emulatorProfile)
            {
                Game = game;
                Emulator = emulator;
                EmulatorProfile = emulatorProfile;
            }
        }

        public bool ShowCloseButton
        {
            get => Type != DialogType.Wizard;
        }

        public bool ShowNextButton
        {
            get => Type == DialogType.Wizard && ViewTabIndex != 3;
        }

        public bool ShowBackButton
        {
            get => Type == DialogType.Wizard;
        }

        public bool ShowFinishButton
        {
            get => Type == DialogType.Wizard;
        }

        public bool ShowImportButton
        {
            get => Type != DialogType.Wizard && Type != DialogType.EmulatorDownload;
        }

        public bool ShowConfigEmulatorButton
        {
            get => Type == DialogType.GameImport;
        }

        private int viewTabIndex = 0;
        public int ViewTabIndex
        {
            get
            {
                switch (Type)
                {
                    case DialogType.Wizard:
                        return viewTabIndex;
                    case DialogType.EmulatorDownload:
                        return 1;
                    case DialogType.EmulatorImport:
                        return 2;
                    case DialogType.GameImport:
                        return 3;
                }

                return 0;
            }

            set
            {
                viewTabIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowNextButton));
                OnPropertyChanged(nameof(ShowBackButton));
            }
        }

        private RangeObservableCollection<ImportableEmulator> emulatorList;
        public RangeObservableCollection<ImportableEmulator> EmulatorList
        {
            get => emulatorList;
            set
            {
                emulatorList = value;
                OnPropertyChanged();
            }
        }

        private RangeObservableCollection<ImportableGame> gamesList;
        public RangeObservableCollection<ImportableGame> GamesList
        {
            get => gamesList;
            set
            {
                gamesList = value;
                OnPropertyChanged();
            }
        }

        public List<Game> ImportedGames
        {
            get; private set;
        }

        public List<Emulator> AvailableEmulators
        {
            get
            {
                var platforms = DatabasePlatforms;
                return database.Emulators
                    .Where(a => a.Profiles != null && a.Profiles.Any(b => b.ImageExtensions != null && b.ImageExtensions.Count > 0))
                    .OrderBy(a => a.Name).ToList();
            }
        }

        public List<Platform> DatabasePlatforms
        {
            get
            {
                return database.Platforms.ToList();
            }
        }

        public List<EmulatorDefinition> EmulatorDefinitions { get; set; }

        private DialogType type;
        public DialogType Type
        {
            get => type;
            private set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged();
            }
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;
        private CancellationTokenSource cancelToken;

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> ScanEmulatorsCommmand
        {
            get => new RelayCommand<object>((a) =>
            {
                if (EmulatorDefinitions.Count == 0)
                {
                    dialogs.ShowErrorMessage("LOCEmulatorImportNoDefinitionsError", "");
                    return;
                }

                var path = dialogs.SelectFolder();
                if (!string.IsNullOrEmpty(path))
                {
                    SearchEmulators(path);
                }
            });
        }

        public RelayCommand<EmulatorProfile> ScanGamesCommand
        {
            get => new RelayCommand<EmulatorProfile>((profile) =>
            {
                if (profile.ImageExtensions?.Any() != true)
                {
                    dialogs.ShowMessage(
                        resources.GetString("LOCScanEmulatorGamesEmptyProfileError"),
                        "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var path = dialogs.SelectFolder();
                if (!string.IsNullOrEmpty(path))
                {
                    SearchGames(path, profile);
                }
            });
        }

        public RelayCommand<object> ScanGamesOpeningCommand
        {
            get => new RelayCommand<object>((args) =>
            {
                VerifyAvailableEmulators(new EmulatorsViewModel(database, new EmulatorsWindowFactory(), dialogs, resources));
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddSelectedGamesToDB();
                CloseView(true);
            });
        }

        public RelayCommand<object> ImportCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                if (Type == DialogType.GameImport)
                {
                    AddSelectedGamesToDB();
                }
                else if (Type == DialogType.EmulatorImport)
                {
                    AddSelectedEmulatorsToDB();
                }

                CloseView(true);
            });
        }

        public RelayCommand<object> NextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                GoNextScreen();
            });
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                GoPreviousScreen();
            });
        }

        public RelayCommand<object> NavigateUrlCommand => GlobalCommands.NavigateUrlCommand;

        public RelayCommand<object> CancelProgressCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CancelProgress();
            });
        }

        public EmulatorImportViewModel(GameDatabase database, DialogType type, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;
            Type = type;

            try
            {
                EmulatorDefinitions = EmulatorDefinition.GetDefinitions();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                EmulatorDefinitions = new List<EmulatorDefinition>();
                logger.Error(e, "Failed to load emulator definitions.");
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public async void SearchEmulators(string path)
        {
            logger.Info($"Scanning {path} for emulators.");

            try
            {
                IsLoading = true;
                cancelToken = new CancellationTokenSource();
                var emulators = await EmulatorFinder.SearchForEmulators(path, EmulatorDefinitions, cancelToken);
                if (emulators != null)
                {
                    if (EmulatorList == null)
                    {
                        EmulatorList = new RangeObservableCollection<ImportableEmulator>();
                    }

                    EmulatorList.AddRange(emulators.Select(a => new ImportableEmulator(a)));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void SearchGames(string path, EmulatorProfile profile)
        {
            logger.Info($"Scanning {path} for emulated games using {profile} profile.");

            try
            {
                IsLoading = true;
                cancelToken = new CancellationTokenSource();
                var games = await EmulatorFinder.SearchForGames(path, profile, cancelToken);
                if (games?.Any() == true)
                {
                    if (GamesList == null)
                    {
                        GamesList = new RangeObservableCollection<ImportableGame>();
                    }

                    var emulator = AvailableEmulators.First(a => a.Profiles.Any(b => b.Id == profile.Id));
                    var importedRoms = database.Games.Where(a => !a.GameImagePath.IsNullOrEmpty()).Select(a => a.GameImagePath).ToList();
                    GamesList.AddRange(games
                        .Where(a =>
                        {
                            return !importedRoms.ContainsString(a.GameImagePath, StringComparison.OrdinalIgnoreCase);
                        })
                        .Select(a =>
                        {
                            a.PlatformId = profile.Platforms?.FirstOrDefault() ?? Guid.Empty;
                            return new ImportableGame(a, emulator, profile);
                        }));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddSelectedGamesToDB()
        {
            if (GamesList == null || GamesList.Count == 0)
            {
                return;
            }

            logger.Info($"Adding {GamesList.Count} new emulated games to DB.");
            foreach (var game in GamesList)
            {
                if (!game.Import)
                {
                    continue;
                }

                game.Game.PlayAction = new GameAction()
                {
                    EmulatorId = game.Emulator.Id,
                    EmulatorProfileId = game.EmulatorProfile.Id,
                    Type = GameActionType.Emulator
                };

                game.Game.IsInstalled = true;
            }

            ImportedGames = GamesList.Where(a => a.Import)?.Select(a => a.Game).ToList();
            ProgressViewViewModel.ActivateProgress(
                () => database.Games.Add(ImportedGames),
                string.Format(resources.GetString("LOCProgressImportinGames"), ImportedGames.Count));
        }

        private void AddSelectedEmulatorsToDB()
        {
            if (EmulatorList == null || EmulatorList.Count == 0)
            {
                return;
            }

            logger.Info($"Adding {EmulatorList.Count} new emulators to DB.");
            foreach (var emulator in EmulatorList)
            {
                if (emulator.Import)
                {
                    var platforms = DatabasePlatforms;
                    foreach (var profile in emulator.Profiles)
                    {
                        foreach (var platform in profile.ProfileDefinition.Platforms)
                        {
                            var existing = platforms.FirstOrDefault(a => string.Equals(a.Name, platform, StringComparison.InvariantCultureIgnoreCase));
                            if (existing == null)
                            {
                                var newPlatform = new Platform(platform);
                                database.Platforms.Add(newPlatform);
                                platforms = DatabasePlatforms;
                                existing = newPlatform;
                            }

                            if (profile.Platforms == null)
                            {
                                profile.Platforms = new List<Guid>();
                            }

                            profile.Platforms.Add(existing.Id);
                        }
                    }

                    database.Emulators.Add(new Emulator(emulator.Name)
                    {
                        Profiles = new ObservableCollection<EmulatorProfile>(emulator.Profiles.Select(a => (EmulatorProfile)a))
                    });
                }
            }

            OnPropertyChanged(nameof(DatabasePlatforms));
            OnPropertyChanged(nameof(AvailableEmulators));
        }

        public void GoNextScreen()
        {
            if (ViewTabIndex == 2)
            {
                if (EmulatorList == null || EmulatorList.Count == 0 || EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (dialogs.ShowMessage(resources.GetString("LOCEmuWizardNoEmulatorWarning"),
                        "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        CloseView(false);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            ViewTabIndex++;

            if (ViewTabIndex == 3)
            {
                AddSelectedEmulatorsToDB();
            }
        }

        public void GoPreviousScreen()
        {
            ViewTabIndex--;
        }

        public void VerifyAvailableEmulators(EmulatorsViewModel platforms)
        {
            if (AvailableEmulators == null || AvailableEmulators.Count == 0)
            {
                if (EmulatorList == null || EmulatorList.Count == 0 || EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (dialogs.ShowMessage(resources.GetString("LOCEmuWizardNoEmulatorForGamesWarning")
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (platforms.OpenView() == true)
                        {
                            OnPropertyChanged(nameof(AvailableEmulators));
                        }
                    }
                }
            }
        }

        public void CancelProgress()
        {
            cancelToken?.Cancel();
        }
    }
}
