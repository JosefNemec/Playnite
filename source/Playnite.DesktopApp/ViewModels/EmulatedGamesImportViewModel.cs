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
using System.IO;

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
            public RelayCommandBase Command { get; set; }
            public List<MenuItem> Items { get; set; }

            public override string ToString()
            {
                return Title;
            }
        }

        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IDialogsFactory dialogs;
        private readonly IResourceProvider resources;
        private readonly IGameDatabaseMain database;
        private readonly MenuItem menuSplitItem;
        private readonly MenuItem menuMergeItem;
        private readonly MenuItem menuPlatformsItem;
        private readonly MenuItem menuRegionsItem;
        private readonly MenuItem menuAddFolderToExclusions;
        private readonly MenuItem menuAddFilesToExclusions;

        private List<Platform> newPlatforms;
        private List<Region> newRegions;

        #region backing fields
        private List<Platform> platforms;
        private List<Region> regions;
        #endregion backing fields

        public List<Emulator> Emulators { get; set; }
        public List<Platform> Platforms { get => platforms; set => SetValue(ref platforms, value); }
        public List<Platform> OverridePlatforms { get; set; }
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

        private ObservableCollection<ScannedGame> gameList;
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
                if (SelectedGames?.Count == 1 && SelectedGames.Cast<ScannedGame>().First().Roms.Count > 1)
                {
                    items.Add(menuSplitItem);
                }
                else if (SelectedGames?.Count > 1)
                {
                    items.Add(menuMergeItem);
                }

                items.Add(menuPlatformsItem);
                items.Add(menuRegionsItem);
                items.Add(menuAddFilesToExclusions);
                items.Add(menuAddFolderToExclusions);
                MenuItems = items;
            });

        public RelayCommand<ScannedRom> AddFileToExclusionListCommand => new RelayCommand<ScannedRom>((r) => AddFileToExclusionList(r));
        public RelayCommand<ScannedRom> AddDirToExclusionListCommand => new RelayCommand<ScannedRom>((r) => AddDirToExclusionList(r));
        public RelayCommand AddFilesToExclusionListCommand => new RelayCommand(() => AddFilesToExclusionList(SelectedGames?.Cast<ScannedGame>()));
        public RelayCommand AddDirsToExclusionListCommand => new RelayCommand(() => AddDirsToExclusionList(SelectedGames?.Cast<ScannedGame>()));

        public EmulatedGamesImportViewModel(
            IGameDatabaseMain database,
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
            OverridePlatforms = database.Platforms.OrderBy(a => a.Name).ToList();
            OverridePlatforms.Insert(0, new Platform(LOC.None.GetLocalized()) { Id = Guid.Empty });

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

            menuAddFilesToExclusions = new MenuItem
            {
                Title = resources.GetString(LOC.EmuImportAddROMExclusionList),
                Command = AddFilesToExclusionListCommand
            };

            menuAddFolderToExclusions = new MenuItem
            {
                Title = resources.GetString(LOC.EmuImportAddFolderExclusionList),
                Command = AddDirsToExclusionListCommand,
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
                    SourceConfig = game.SourceConfig,
                    SourceEmulator = game.SourceEmulator
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
            foreach (var config in ScannerConfigs)
            {
                if (config.Save && config.Name.IsNullOrWhiteSpace())
                {
                    Dialogs.ShowErrorMessage(resources.GetString(LOC.ScanConfigError) + "\n" + resources.GetString(LOC.ScanConfigNameError), "");
                    return;
                }

                if (config.EmulatorId == Guid.Empty || config.EmulatorProfileId.IsNullOrEmpty())
                {
                    Dialogs.ShowErrorMessage(resources.GetString(LOC.ScanConfigError) + "\n" + resources.GetString(LOC.ScanConfigNoEmulatorError), "");
                    return;
                }

                if (config.Directory.IsNullOrEmpty())
                {
                    Dialogs.ShowErrorMessage(resources.GetString(LOC.ScanConfigError) + "\n" + resources.GetString(LOC.ScanConfigDirectoryError), "");
                    return;
                }
                else
                {
                    var emulator = database.Emulators[config.EmulatorId];
                    if (emulator == null)
                    {
                        Dialogs.ShowErrorMessage(resources.GetString(LOC.ScanConfigError) + "\n" + resources.GetString(LOC.ScanConfigNoEmulatorError), "");
                        return;
                    }

                    var dirToScan = PlaynitePaths.ExpandVariables(config.Directory, emulator.InstallDir, true);
                    if (!Directory.Exists(dirToScan))
                    {
                        Dialogs.ShowErrorMessage(resources.GetString(LOC.ScanConfigError) + "\n" + resources.GetString(LOC.ScanConfigDirectoryError), "");
                        return;
                    }
                }
            }

            foreach (var config in ScannerConfigs.Where(a => a.Save))
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

            var tempList = new ObservableCollection<ScannedGame>();
            var scanString = resources.GetString(LOC.EmuWizardScanningSpecific);
            var scanRes = dialogs.ActivateGlobalProgress((args) =>
            {
                foreach (GameScannerConfig config in ScannerConfigs)
                {
                    args.Text = scanString.Format(config.Directory);
                    tempList.AddRange(new GameScanner(config, database).Scan(
                        args.CancelToken,
                        out newPlatforms,
                        out newRegions,
                        (path) => args.Text = scanString.Format(path)));
                }
            },
            new GlobalProgressOptions(LOC.EmuWizardScanning)
            {
                Cancelable = true,
                IsIndeterminate = true
            });

            if (scanRes.Error != null)
            {
                logger.Error(scanRes.Error, "Failed to scan emulated folder.");
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

            GameList = tempList;
        }

        private void ImportGames()
        {
            var statusSettings = database.GetCompletionStatusSettings();
            using (database.BufferedUpdate())
            {
                if (newPlatforms.HasItems())
                {
                    foreach (var newPlat in newPlatforms)
                    {
                        if (GameList.Any(a => a.Platforms?.FirstOrDefault(p => p.Id == newPlat.Id) != null))
                        {
                            database.Platforms.Add(newPlat);
                        }
                    }
                }

                if (newRegions.HasItems())
                {
                    foreach (var newReg in newRegions)
                    {
                        if (GameList.Any(a => a.Regions?.FirstOrDefault(p => p.Id == newReg.Id) != null))
                        {
                            database.Regions.Add(newReg);
                        }
                    }
                }

                foreach (var scannedGame in GameList.Where(a => a.Import && a.Roms?.Any(r => r.Import) == true))
                {
                    var game = scannedGame.ToGame();
                    if (statusSettings.DefaultStatus != Guid.Empty)
                    {
                        game.CompletionStatusId = statusSettings.DefaultStatus;
                    }

                    database.Games.Add(game);
                    ImportedGames.Add(game);
                }
            }

            CloseView(true);
        }

        private void AddFileToExclusionList(ScannedRom rom)
        {
            rom.Import = false;
            var parent = GameList.First(a => a.Roms.Contains(rom));
            ExcludeFiles(new List<string> { rom.Path }, parent.SourceConfig.Id);
        }

        private void AddDirToExclusionList(ScannedRom rom)
        {
            rom.Import = false;
            var parent = GameList.First(a => a.Roms.Contains(rom));
            ExcludeDirectories(new List<string> { rom.Path }, parent.SourceConfig.Id);
        }

        private void AddFilesToExclusionList(IEnumerable<ScannedGame> games)
        {
            games.ForEach(a => a.Import = false);
            foreach (var scanner in games.GroupBy(a => a.SourceConfig))
            {
                ExcludeFiles(scanner.SelectMany(a => a.Roms).Select(a => a.Path), scanner.Key.Id);
            }
        }

        private void AddDirsToExclusionList(IEnumerable<ScannedGame> games)
        {
            games.ForEach(a => a.Import = false);
            foreach (var scanner in games.GroupBy(a => a.SourceConfig))
            {
                ExcludeDirectories(scanner.SelectMany(a => a.Roms).Select(a => a.Path), scanner.Key.Id);
            }
        }

        private void ExcludeFiles(IEnumerable<string> romPaths, Guid scannerId)
        {
            var scanner = database.GameScanners[scannerId];
            if (scanner == null)
            {
                dialogs.ShowErrorMessage(LOC.EmuExclusionNoConfigError, "");
            }
            else
            {
                bool update = false;
                foreach (var file in romPaths)
                {
                    var exFile = file.Replace(scanner.Directory, "").TrimStart(Paths.DirectorySeparators);
                    if (scanner.ExcludedFiles == null)
                    {
                        scanner.ExcludedFiles = new List<string> { exFile };
                        update = true;
                    }
                    else
                    {
                        if (!scanner.ExcludedFiles.ContainsString(exFile, StringComparison.OrdinalIgnoreCase))
                        {
                            scanner.ExcludedFiles.Add(exFile);
                            update = true;
                        }
                    }
                }

                if (update)
                {
                    dialogs.ShowMessage(LOC.EmuExclusionAddedMessage.GetLocalized().Format(scanner.Name));
                    database.GameScanners.Update(scanner);
                }
            }
        }

        private void ExcludeDirectories(IEnumerable<string> romPaths, Guid scannerId)
        {
            var scanner = database.GameScanners[scannerId];
            if (scanner == null)
            {
                dialogs.ShowErrorMessage(LOC.EmuExclusionNoConfigError, "");
            }
            else
            {
                bool update = false;
                foreach (var file in romPaths)
                {
                    var dir = Path.GetDirectoryName(file);
                    var exDir = dir.Replace(scanner.Directory.TrimEnd(Paths.DirectorySeparators), "").TrimStart(Paths.DirectorySeparators);
                    if (exDir.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    if (scanner.ExcludedDirectories == null)
                    {
                        scanner.ExcludedDirectories = new List<string> { exDir };
                        update = true;
                    }
                    else
                    {
                        if (!scanner.ExcludedDirectories.ContainsString(exDir, StringComparison.OrdinalIgnoreCase))
                        {
                            scanner.ExcludedDirectories.Add(exDir);
                            update = true;
                        }
                    }
                }

                if (update)
                {
                    dialogs.ShowMessage(LOC.EmuExclusionAddedMessage.GetLocalized().Format(scanner.Name));
                    database.GameScanners.Update(scanner);
                }
            }
        }
    }
}
