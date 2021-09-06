using Playnite;
using Playnite.Database;
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
using Playnite.Emulators;

namespace Playnite.DesktopApp.ViewModels
{
    public class EmulatedGamesImportViewModel : ObservableObject
    {
        public class ImportGameScannerConfig : GameScannerConfig
        {
            public bool Save { get; set; }
            public bool SavedConfig { get; set; }
        }

        public class MenuItem
        {
            public string Title { get; set; }
            public RelayCommand<object> Command { get; set; }
            public List<MenuItem> Items { get; set; }

            public override string ToString()
            {
                return Title;
            }
        }

        private readonly object listSyncLock = new object();
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IDialogsFactory dialogs;
        private readonly IResourceProvider resources;
        private readonly GameDatabase database;
        private readonly MenuItem menuSplitItem;
        private readonly MenuItem menuMergeItem;
        private readonly MenuItem menuPlatformsItem;
        private readonly MenuItem menuRegionsItem;
        private List<Platform> newPlatforms;
        private List<Region> newRegions;

        #region backing fields
        private List<Platform> platforms;
        private List<Region> regions;
        #endregion backing fields

        public List<Emulator> Emulators { get; set; }
        public List<Platform> Platforms { get => platforms; set => SetValue(ref platforms, value); }
        public List<Region> Regions     { get => regions; set => SetValue(ref regions, value); }
        public List<GameScannerConfig> SavedConfigs { get; set; }
        public List<Game> ImportedGames { get; } = new List<Game>();

        private List<MenuItem> menuItems;
        public List<MenuItem> MenuItems
        {
            get => menuItems;
            private set
            {
                menuItems = value;
                OnPropertyChanged();
            }
        }

        private ListCollectionView collectionView;
        public ListCollectionView CollectionView
        {
            get => collectionView;
            private set
            {
                collectionView = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ScannedGame> gameList = new ObservableCollection<ScannedGame>();
        public ObservableCollection<ScannedGame> GameList
        {
            get => gameList;
            set
            {
                gameList = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ImportGameScannerConfig> scannerConfigs = new ObservableCollection<ImportGameScannerConfig>();
        public ObservableCollection<ImportGameScannerConfig> ScannerConfigs
        {
            get => scannerConfigs;
            set
            {
                scannerConfigs = value;
                OnPropertyChanged();
            }
        }

        private bool isScanSetup = true;
        public bool IsScanSetup
        {
            get => isScanSetup;
            set
            {
                isScanSetup = value;
                OnPropertyChanged();
            }
        }

        private IList<object> selectedGames;
        public IList<object> SelectedGames
        {
            get => selectedGames;
            set
            {
                selectedGames = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SelectAllCommmand =>
            new RelayCommand(
                () => GameList.ForEach(e => e.Import = true),
                () => GameList.HasItems());

        public RelayCommand DeselectAllCommmand =>
            new RelayCommand(
                () => GameList.ForEach(e => e.Import = false),
                () => GameList.HasItems());

        public RelayCommand CancelCommand =>
            new RelayCommand(() => CloseView(false));

        public RelayCommand ScanCommmand =>
            new RelayCommand(() => StartScan());

        public RelayCommand ImportCommand =>
            new RelayCommand(() => ImportGames());

        public RelayCommand AddScanConfigCommand =>
            new RelayCommand(() => ScannerConfigs.Add(new ImportGameScannerConfig()));

        public RelayCommand<ImportGameScannerConfig> RemoveScanConfigCommand =>
            new RelayCommand<ImportGameScannerConfig>((a) => ScannerConfigs.Remove(a));

        public RelayCommand<GameScannerConfig> AddSavedScanConfigCommand =>
            new RelayCommand<GameScannerConfig>((a) =>
            {
                var config = a.GetClone<GameScannerConfig, ImportGameScannerConfig>();
                config.Save = false;
                config.SavedConfig = true;
                ScannerConfigs.Add(config);
            });

        public RelayCommand<ContextMenuEventArgs> ContextMenuOpening =>
            new RelayCommand<ContextMenuEventArgs>((a) =>
            {
                if (SelectedGames == null || SelectedGames.Count == 0)
                {
                    MenuItems = null;
                    return;
                }

                var items = new List<MenuItem>();
                if (SelectedGames?.Count == 1)
                {
                    items.Add(menuSplitItem);
                }
                else if (SelectedGames?.Count > 1)
                {
                    items.Add(menuMergeItem);
                }

                items.Add(menuPlatformsItem);
                items.Add(menuRegionsItem);
                MenuItems = items;
            });

        public EmulatedGamesImportViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;
            Emulators = database.Emulators.OrderBy(a => a.Name).ToList();
            Platforms = database.Platforms.OrderBy(a => a.Name).ToList();
            Regions = database.Regions.OrderBy(a => a.Name).ToList();
            SavedConfigs = database.GameScanners.OrderBy(a => a.Name).ToList();
            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(GameList);
            CollectionView.Filter = ListFilter;
            BindingOperations.EnableCollectionSynchronization(GameList, listSyncLock);

            menuSplitItem = new MenuItem
            {
                Title = resources.GetString(LOC.SplitEmuImportSplitGames),
                Command = new RelayCommand<object>((_) =>
                {
                    SplitSelectedGames();
                })
            };
            menuMergeItem = new MenuItem
            {
                Title = resources.GetString(LOC.SplitEmuImportMergeGames),
                Command = new RelayCommand<object>((_) =>
                {
                    MergeSelectedGames();
                })
            };

            menuPlatformsItem = new MenuItem
            {
                Title = resources.GetString(LOC.EmuImportAssignPlatform),
                Items = new List<MenuItem>()
            };

            Platforms.ForEach(a => menuPlatformsItem.Items.Add(new MenuItem
            {
                Title = a.Name,
                Command = new RelayCommand<object>((_) =>
                {
                    foreach (ScannedGame game in SelectedGames)
                    {
                        game.Platforms = new List<Platform> { a };
                    }
                })
            }));

            menuRegionsItem = new MenuItem
            {
                Title = resources.GetString(LOC.EmuImportAssignRegion),
                Items = new List<MenuItem>()
            };

            Regions.ForEach(a => menuRegionsItem.Items.Add(new MenuItem
            {
                Title = a.Name,
                Command = new RelayCommand<object>((_) =>
                {
                    foreach (ScannedGame game in SelectedGames)
                    {
                        game.Regions = new List<Region> { a };
                    }
                })
            }));
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        private bool ListFilter(object item)
        {
            return true;
        }

        private void SplitSelectedGames()
        {
            var game = SelectedGames[0] as ScannedGame;
            if (!game.Roms.HasItems() || game.Roms.Count == 1)
            {
                return;
            }

            var tmpList = game.Roms.ToList();
            for (int i = 1; i < tmpList.Count; i++)
            {
                var rom = tmpList[i];
                var newGame = new ScannedGame
                {
                    Roms = new ObservableCollection<ScannedRom> { rom },
                    Name = rom.Name.Name,
                    Platforms = game.Platforms?.ToList(),
                    Regions = game.Regions?.ToList(),
                    SourceConfig = game.SourceConfig
                };

                GameList.Insert(GameList.IndexOf(game) + 1, newGame);
                game.Roms.Remove(rom);
            }
        }

        private void MergeSelectedGames()
        {
            ScannedGame resGame = null;
            var tempList = SelectedGames.ToList();
            for (int i = 0; i < tempList.Count; i++)
            {
                var game = tempList[i] as ScannedGame;
                if (i == 0)
                {
                    resGame = game;
                }
                else
                {
                    resGame.Roms.AddRange(game.Roms);
                    GameList.Remove(game);
                }
            }
        }

        private void StartScan()
        {
            var validConfigs = ScannerConfigs.Where(a =>
                a.EmulatorId != Guid.Empty &&
                !a.EmulatorProfileId.IsNullOrEmpty() &&
                !a.Directory.IsNullOrEmpty());

            if (!validConfigs.HasItems())
            {
                Dialogs.ShowMessage(LOC.EmuNoValidConfigSet, "", MessageBoxButton.OK);
                return;
            }

            foreach (var config in validConfigs.Where(a => a.Save))
            {
                var exists = database.GameScanners.Any(c =>
                    string.Equals(c.Directory, config.Directory, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(c.EmulatorProfileId, config.EmulatorProfileId, StringComparison.Ordinal) &&
                    c.EmulatorId == config.EmulatorId);
                if (!exists)
                {
                    var emulator = database.Emulators[config.EmulatorId];
                    if (config.Name.IsNullOrEmpty())
                    {
                        config.Name = config.Directory + ": " + emulator.Name + ": " + emulator.GetProfile(config.EmulatorProfileId).Name;
                    }

                    database.GameScanners.Add(config.GetClone<GameScannerConfig>());
                }
            }

            newPlatforms = new List<Platform>();
            newRegions = new List<Region>();
            GameList.Clear();
            var scanRes = dialogs.ActivateGlobalProgress((args) =>
            {
                var existingGame = database.GetImportedRomFiles();
                foreach (GameScannerConfig config in validConfigs)
                {
                    args.Text = resources.GetString(LOC.EmuWizardScanningSpecific).Format(config.Directory);
                    GameList.AddRange(GameScanner.Scan(
                        config,
                        database,
                        existingGame,
                        args.CancelToken,
                        newPlatforms,
                        newRegions));
                }
            },
            new GlobalProgressOptions(LOC.EmuWizardScanning)
            {
                Cancelable = true,
                IsIndeterminate = true
            });

            if (scanRes.Error != null)
            {
                dialogs.ShowErrorMessage(resources.GetString(LOC.EmulatedGameScanFailed) + "\n" + scanRes.Error.Message, "");
                IsScanSetup = true;
            }
            else
            {
                if (newPlatforms.HasItems())
                {
                    // Control we use to bind this information doesn't support observable collections...
                    var pt = Platforms.GetClone();
                    newPlatforms.ForEach(platform =>
                    {
                        pt.Insert(0, platform);
                        menuPlatformsItem.Items.Insert(0, new MenuItem
                        {
                            Title = platform.Name,
                            Command = new RelayCommand<object>((_) =>
                            {
                                foreach (ScannedGame game in SelectedGames)
                                {
                                    game.Platforms = new List<Platform> { platform };
                                }
                            })
                        });
                    });
                    Platforms = pt;
                }

                if (newRegions.HasItems())
                {
                    // Control we use to bind this information doesn't support observable collections...
                    var rg = Regions.GetClone();
                    newRegions.ForEach(region =>
                    {
                        rg.Insert(0, region);
                        menuRegionsItem.Items.Insert(0, new MenuItem
                        {
                            Title = region.Name,
                            Command = new RelayCommand<object>((_) =>
                            {
                                foreach (ScannedGame game in SelectedGames)
                                {
                                    game.Regions = new List<Region> { region };
                                }
                            })
                        });
                    });
                    Regions = rg;
                }

                IsScanSetup = false;
            }
        }

        private void ImportGames()
        {
            using (database.BufferedUpdate())
            {
                foreach (var newPlat in newPlatforms)
                {
                    if (GameList.Any(a => a.Platforms?.FirstOrDefault(p => p.Id == newPlat.Id) != null))
                    {
                        database.Platforms.Add(newPlat);
                    }
                }

                foreach (var newReg in newRegions)
                {
                    if (GameList.Any(a => a.Regions?.FirstOrDefault(p => p.Id == newReg.Id) != null))
                    {
                        database.Regions.Add(newReg);
                    }
                }

                foreach (var scannedGame in GameList.Where(a => a.Import && a.Roms?.Any(r => r.Import) == true))
                {
                    var game = scannedGame.ToGame();
                    database.Games.Add(game);
                    ImportedGames.Add(game);
                }
            }

            CloseView(true);
        }
    }
}
