using NLog;
using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Models;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static PlayniteUI.ViewModels.PlatformsViewModel;

namespace PlayniteUI.ViewModels
{
    public class EmulatorImportViewModel : ObservableObject
    {
        public enum DialogType
        {
            EmulatorImport,
            EmulatorDownload,
            GameImport,
            Wizard
        }

        public class ImportableEmulator
        {
            public bool Import
            {
                get; set;
            } = true;

            public ScannedEmulator Emulator
            {
                get; set;
            }

            public ImportableEmulator(ScannedEmulator emulator)
            {
                Emulator = emulator;
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

            public PlatformableEmulator Emulator
            {
                get; set;
            }

            public ImportableGame(IGame game, PlatformableEmulator emulator)
            {
                Game = game;
                Emulator = emulator;
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

        public List<PlatformableEmulator> AvailableEmulators
        {
            get
            {
                var platforms = DatabasePlatforms;
                return database.EmulatorsCollection.FindAll().Where(a => a.ImageExtensions != null && a.ImageExtensions.Count > 0)
                    .Select(a => PlatformableEmulator.FromEmulator(a, platforms.Where(b => a.Platforms != null && a.Platforms.Contains(b.Id)))).ToList();
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
            set
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

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseDialog(false);
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
                    // TODO
                    //ListEmulators.ScrollIntoView(Model.EmulatorList.Count == 0 ? null : Model.EmulatorList.Last());
                }
            });
        }

        public RelayCommand<PlatformableEmulator> ScanGamesCommand
        {
            get => new RelayCommand<PlatformableEmulator>((emu) =>
            {                
                var path = dialogs.SelectFolder();
                if (!string.IsNullOrEmpty(path))
                {
                    SearchGames(path, emu);
                    // TODO
                    //ListGames.ScrollIntoView(Model.GamesList.Count == 0 ? null : Model.GamesList.Last());
                }
            });
        }

        public RelayCommand<object> ScanGamesOpeningCommand
        {
            get => new RelayCommand<object>((args) =>
            {
                VerifyAvailableEmulators();
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddSelectedGamesToDB();
                CloseDialog(true);
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

                CloseDialog(true);
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

        public EmulatorImportViewModel(GameDatabase database, DialogType type, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;
            Type = type;
        }

        public bool? ShowDialog()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseDialog(bool? result)
        {
            window.Close(result);
        }

        public async void SearchEmulators(string path)
        {
            try
            {
                IsLoading = true;
                var emulators = await Task.Run(() =>
                {
                    return EmulatorFinder.SearchForEmulators(path, EmulatorDefinition.GetDefinitions());
                });

                if (EmulatorList == null)
                {
                    EmulatorList = new RangeObservableCollection<ImportableEmulator>();
                }

                EmulatorList.AddRange(emulators.Select(a => new ImportableEmulator(a)));
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void SearchGames(string path, PlatformableEmulator emulator)
        {
            try
            {
                IsLoading = true;
                var games = await Task.Run(() =>
                {
                    return EmulatorFinder.SearchForGames(path, emulator.ToEmulator());
                });

                if (GamesList == null)
                {
                    GamesList = new RangeObservableCollection<ImportableGame>();
                }

                GamesList.AddRange(games.Select(a =>
                {
                    a.PlatformId = emulator.Platforms?.FirstOrDefault();
                    return new ImportableGame(a, emulator);
                }));
            }
            finally
            {
                IsLoading = false;
            }
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
                    Type = GameTaskType.Emulator
                };

                database.AddGame(game.Game);
            }
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
                    foreach (var platform in emulator.Emulator.Definition.Platforms)
                    {
                        var existing = platforms.FirstOrDefault(a => string.Equals(a.Name, platform, StringComparison.InvariantCultureIgnoreCase));
                        if (existing == null)
                        {
                            var newPlatform = new Platform(platform) { Id = 0 };
                            database.AddPlatform(newPlatform);
                            existing = newPlatform;
                        }

                        if (emulator.Emulator.Emulator.Platforms == null)
                        {
                            emulator.Emulator.Emulator.Platforms = new List<int>();
                        }

                        emulator.Emulator.Emulator.Platforms.Add(existing.Id);
                    }

                    database.AddEmulator(emulator.Emulator.Emulator);
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
                        CloseDialog(false);
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

        public void VerifyAvailableEmulators()
        {
            if (AvailableEmulators == null || AvailableEmulators.Count == 0)
            {
                if (EmulatorList == null || EmulatorList.Count == 0 || EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (dialogs.ShowMessage(resources.FindString("EmuWizardNoEmulatorForGamesWarning")
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // TODO
                        OnPropertyChanged("AvailableEmulators");
                    }
                }
            }
        }
    }
}
