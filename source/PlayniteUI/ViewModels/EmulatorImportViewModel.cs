using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Models;
using PlayniteUI.Commands;
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
using static PlayniteUI.ViewModels.PlatformsViewModel;

namespace PlayniteUI.ViewModels
{
    public class SelectedPlatformsToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var platforms = (IEnumerable<LiteDB.ObjectId>)values[0];
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
            var platforms = (IEnumerable<LiteDB.ObjectId>)values[0];
            var allPlatforms = (IEnumerable<Platform>)values[1];
            return allPlatforms.Where(a => platforms.Contains(a.Id));
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

            public IGame Game
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

            public ImportableGame(IGame game, Emulator emulator, EmulatorProfile emulatorProfile)
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
                OnPropertyChanged("ViewTabIndex");
                OnPropertyChanged("ShowNextButton");
                OnPropertyChanged("ShowBackButton");
            }
        }

        private RangeObservableCollection<ImportableEmulator> emulatorList;
        public RangeObservableCollection<ImportableEmulator> EmulatorList
        {
            get => emulatorList;
            set
            {
                emulatorList = value;
                OnPropertyChanged("EmulatorList");
            }
        }

        private RangeObservableCollection<ImportableGame> gamesList;
        public RangeObservableCollection<ImportableGame> GamesList
        {
            get => gamesList;
            set
            {
                gamesList = value;
                OnPropertyChanged("GamesList");
            }
        }

        public List<Emulator> AvailableEmulators
        {
            get
            {
                var platforms = DatabasePlatforms;
                return database.EmulatorsCollection.FindAll()
                    .Where(a => a.Profiles != null && a.Profiles.Any(b => b.ImageExtensions != null && b.ImageExtensions.Count > 0))
                    .OrderBy(a => a.Name).ToList();
            }
        }

        public List<Platform> DatabasePlatforms
        {
            get
            {
                return database.PlatformsCollection.FindAll().ToList();
            }
        }

        public List<EmulatorDefinition> EmulatorDefinitions
        {
            get
            {
                return EmulatorDefinition.GetDefinitions();
            }
        }

        private DialogType type;
        public DialogType Type
        {
            get => type;
            private set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
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
                        resources.FindString("ScanEmulatorGamesEmptyProfileError"),
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

        public RelayCommand<object> SelectGameFilesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var files = dialogs.SelectFiles("All files|*.*");
                if (files != null)
                {
                    ImportGamesFiles(files);
                }
            });
        }

        public RelayCommand<object> ScanGamesOpeningCommand
        {
            get => new RelayCommand<object>((args) =>
            {
                VerifyAvailableEmulators(new PlatformsViewModel(database, new PlatformsWindowFactory(), dialogs, resources));
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

        public RelayCommand<Uri> NavigateUrlCommand
        {
            get => new RelayCommand<Uri>((url) =>
            {
                NavigateUrl(url.AbsoluteUri);
            });
        }

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
            try
            {
                IsLoading = true;
                cancelToken = new CancellationTokenSource();
                var emulators = await EmulatorFinder.SearchForEmulators(path, EmulatorDefinition.GetDefinitions(), cancelToken);
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
            try
            {
                IsLoading = true;
                cancelToken = new CancellationTokenSource();
                var games = await  EmulatorFinder.SearchForGames(path, profile, cancelToken);
                if (games != null)
                {

                    if (GamesList == null)
                    {
                        GamesList = new RangeObservableCollection<ImportableGame>();
                    }

                    var dbGames = database.GamesCollection.FindAll();
                    var emulator = AvailableEmulators.First(a => a.Profiles.Any(b => b.Id == profile.Id));
                    GamesList.AddRange(games
                        .Where(a =>
                        {
                            return dbGames.FirstOrDefault(b => Paths.AreEqual(a.IsoPath, b.IsoPath)) == null;
                        })
                        .Select(a =>
                        {
                            a.PlatformId = profile.Platforms?.FirstOrDefault();
                            return new ImportableGame(a, emulator, profile);
                        }));
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ImportGamesFiles(List<string> files)
        {

        }

        public void AddSelectedGamesToDB()
        {
            if (GamesList == null || GamesList.Count == 0)
            {
                return;
            }

            foreach (var game in GamesList)
            {
                if (!game.Import)
                {
                    continue;
                }

                game.Game.PlayTask = new GameTask()
                {
                    EmulatorId = game.Emulator.Id,
                    EmulatorProfileId = game.EmulatorProfile.Id,
                    Type = GameTaskType.Emulator
                };               
            }

            database.AddGames(GamesList.Where(a => a.Import)?.Select(a => a.Game).ToList());
        }

        public void AddSelectedEmulatorsToDB()
        {
            if (EmulatorList == null || EmulatorList.Count == 0)
            {
                return;
            }

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
                                var newPlatform = new Platform(platform) { Id = null };
                                database.AddPlatform(newPlatform);
                                platforms = DatabasePlatforms;
                                existing = newPlatform;
                            }

                            if (profile.Platforms == null)
                            {
                                profile.Platforms = new List<LiteDB.ObjectId>();
                            }

                            profile.Platforms.Add(existing.Id);                            
                        }
                    }

                    database.AddEmulator(new Emulator(emulator.Name)
                    {
                        Profiles = new ObservableCollection<EmulatorProfile>(emulator.Profiles.Select(a => (EmulatorProfile)a))
                    });
                }
            }

            OnPropertyChanged("DatabasePlatforms");
            OnPropertyChanged("AvailableEmulators");
        }

        public void GoNextScreen()
        {
            if (ViewTabIndex == 2)
            {
                if (EmulatorList == null || EmulatorList.Count == 0 || EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (dialogs.ShowMessage(resources.FindString("EmuWizardNoEmulatorWarning"),
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

        public void NavigateUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public void VerifyAvailableEmulators(PlatformsViewModel platforms)
        {
            if (AvailableEmulators == null || AvailableEmulators.Count == 0)
            {
                if (EmulatorList == null || EmulatorList.Count == 0 || EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (dialogs.ShowMessage(resources.FindString("EmuWizardNoEmulatorForGamesWarning")
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {                       
                        if (platforms.OpenView() == true)
                        {
                            OnPropertyChanged("AvailableEmulators");
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
