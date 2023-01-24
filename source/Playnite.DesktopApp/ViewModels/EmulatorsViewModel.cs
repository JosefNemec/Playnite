using Playnite;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Playnite.Common;
using Playnite.Settings;
using Playnite.Windows;
using Playnite.DesktopApp.Windows;
using Playnite.Emulators;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class EmulatorsViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        class DownloadPlatform : EmulatedPlatform
        {
            public new List<EmulatorDefinition> Emulators { get; set; }
        }

        public class DownloadEmu
        {
            public string Name { get; set; }
            public string Website { get; set; }
            public EmulatedPlatform Platform { get; set; }
        }

        public List<DownloadEmu> DownloadEmulatorsList { get; set; }
        public List<Platform> OverridePlatforms { get; set; }

        private SelectableDbItemList availablePlatforms;
        public SelectableDbItemList AvailablePlatforms
        {
            get => availablePlatforms;
            set
            {
                availablePlatforms = value;
                OnPropertyChanged();
            }
        }

        private List<string> selectedEmulatorBuiltInProfiles;
        public List<string> SelectedEmulatorBuiltInProfiles
        {
            get => selectedEmulatorBuiltInProfiles;
            set
            {
                selectedEmulatorBuiltInProfiles = value;
                OnPropertyChanged();
            }
        }

        private Emulator selectedEmulator;
        public Emulator SelectedEmulator
        {
            get => selectedEmulator;
            set
            {
                if (selectedEmulator != null)
                {
                    selectedEmulator.PropertyChanged -= SelectedEmulator_PropertyChanged;
                }

                selectedEmulator = value;
                OnPropertyChanged();
                SelectedProfile = SelectedEmulator?.CustomProfiles?.FirstOrDefault();
                if (selectedEmulator != null)
                {
                    selectedEmulator.PropertyChanged += SelectedEmulator_PropertyChanged;
                }
                UpdateSelectedEmulatorBuiltInProfiles();
            }
        }

        private CustomEmulatorProfile selectedCustomProfile;
        public CustomEmulatorProfile SelectedCustomProfile
        {
            get => selectedCustomProfile;
            set
            {
                selectedCustomProfile = value;
                OnPropertyChanged();
            }
        }

        private BuiltInEmulatorProfile selectedBuiltinProfile;
        public BuiltInEmulatorProfile SelectedBuiltinProfile
        {
            get => selectedBuiltinProfile;
            set
            {
                selectedBuiltinProfile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedBuiltInProfilePlatforms));
                OnPropertyChanged(nameof(SelectedBuiltInProfileDefinition));
            }
        }

        private object selectedProfile;
        public object SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                if (value is CustomEmulatorProfile cus)
                {
                    SelectedCustomProfile = cus;
                    SelectedBuiltinProfile = null;
                }
                else if (value is BuiltInEmulatorProfile blt)
                {
                    SelectedCustomProfile = null;
                    SelectedBuiltinProfile = blt;
                }
                else
                {
                    SelectedCustomProfile = null;
                    SelectedBuiltinProfile = null;
                }

                OnPropertyChanged();
            }
        }

        public EmulatorDefinitionProfile SelectedBuiltInProfileDefinition
        {
            get
            {
                if (SelectedEmulator == null || SelectedBuiltinProfile == null)
                {
                    return null;
                }

                return Emulation.GetProfile(SelectedEmulator.BuiltInConfigId, SelectedBuiltinProfile.BuiltInProfileName);
            }
        }

        public string SelectedBuiltInProfilePlatforms
        {
            get
            {
                if (SelectedBuiltinProfile == null)
                {
                    return string.Empty;
                }

                var profile = Emulation.GetProfile(SelectedEmulator.BuiltInConfigId, SelectedBuiltinProfile.BuiltInProfileName);
                if (profile == null)
                {
                    return string.Empty;
                }

                return string.Join(", ", profile.Platforms.Select(a => Emulation.GetPlatform(a)?.Name).Where(a => a != null));
            }
        }

        private ObservableCollection<Emulator> editingEmulators;
        public ObservableCollection<Emulator> EditingEmulators
        {
            get => editingEmulators;
            set
            {
                editingEmulators = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GameScannerConfig> editingScannerConfigs;
        public ObservableCollection<GameScannerConfig> EditingScanners
        {
            get => editingScannerConfigs;
            set
            {
                editingScannerConfigs = value;
                OnPropertyChanged();
            }
        }

        private GameScannerConfig selectedScanner;
        public GameScannerConfig SelectedScanner
        {
            get => selectedScanner;
            set
            {
                selectedScanner = value;
                OnPropertyChanged();
            }
        }

        public GameScannersSettings GlobalScanSettings
        {
            get;
        }

        public List<EmulatorDefinition> SelectableEmulatorDefinitions { get; set; }
        public IList<EmulatorDefinition> EmulatorDefinitions { get; set; }

        public RelayCommand CloseCommand =>
            new RelayCommand(() => CloseView(false));

        public RelayCommand ConfirmCommand =>
            new RelayCommand(() => ConfirmDialog());

        public RelayCommand<CustomEmulatorProfile> SelectEmulatorExecutableCommand =>
            new RelayCommand<CustomEmulatorProfile>(
                (a) => SelectEmulatorExecutable(a),
                (a) => a != null);

        public RelayCommand AddEmulatorCommand =>
            new RelayCommand(() => AddEmulator());

        public RelayCommand<Emulator> RemoveEmulatorCommand =>
            new RelayCommand<Emulator>(
                (a) => RemoveEmulator(a),
                (a) => a != null);

        public RelayCommand<Emulator> CopyEmulatorCommand =>
            new RelayCommand<Emulator>(
                (a) => CopyEmulator(a),
                (a) => a != null);

        public RelayCommand<Button> AddEmulatorProfileCommand
        {
            get => new RelayCommand<Button>((button) =>
            {
                if (!SelectedEmulator.BuiltInConfigId.IsNullOrEmpty())
                {
                    var def = Emulation.GetDefition(SelectedEmulator.BuiltInConfigId);
                    if (def == null)
                    {
                        logger.Error($"Trying to add built-in emulator profile to uknown emulator def {SelectedEmulator.BuiltInConfigId}");
                        return;
                    }

                    var menu = button.ContextMenu;
                    menu.Items.Clear();
                    foreach (var profile in def.Profiles.OrderBy(a => a.Name).Select(p => p.Name))
                    {
                        menu.Items.Add(new MenuItem
                        {
                            Header = $"{LOC.EmulatorBuiltInProfile.GetLocalized()}: {profile}",
                            Command = new RelayCommand<object>((_) => AddBuiltinEmulatorProfile(SelectedEmulator, profile))
                        });
                    }

                    menu.Items.Add(new MenuItem
                    {
                        Header = LOC.EmulatorCustomProfile.GetLocalized(),
                        Command = new RelayCommand<object>((_) => AddCustomEmulatorProfile(SelectedEmulator))
                    });

                    menu.PlacementTarget = button;
                    menu.IsOpen = true;
                }
                else
                {
                    AddCustomEmulatorProfile(SelectedEmulator);
                }
            }, (_) => SelectedEmulator != null);
        }

        public RelayCommand<object> RemoveEmulatorProfileCommand =>
            new RelayCommand<object>(
                (a) => RemoveEmulatorProfile(SelectedEmulator, a),
                (a) => a != null);

        public RelayCommand<object> CopyEmulatorProfileCommand =>
            new RelayCommand<object>(
                (a) => CopyEmulatorProfile(SelectedEmulator, (CustomEmulatorProfile)a),
                (a) => a is CustomEmulatorProfile);

        public RelayCommand ImportEmulatorsCommand =>
            new RelayCommand(() => ImportEmulators());

        public RelayCommand DownloadEmulatorsCommand =>
            new RelayCommand(() => DownloadEmulators());

        public RelayCommand AddScanConfigCommand =>
            new RelayCommand(() => AddNewScannerConfig());

        public RelayCommand<GameScannerConfig> RemoveScanConfigCommand =>
            new RelayCommand<GameScannerConfig>(
                (a) => RemoveScanConfig(a),
                (a) => a != null);

        public RelayCommand<GameScannerConfig> CopyScanConfigCommand =>
            new RelayCommand<GameScannerConfig>(
                (a) => CopyScanConfig(a),
                (a) => a != null);

        private IGameDatabaseMain database;
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        public EmulatorsViewModel(IGameDatabaseMain database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;

            EmulatorDefinitions = Emulation.Definitions;
            SelectableEmulatorDefinitions = Emulation.Definitions.ToList();
            SelectableEmulatorDefinitions.Insert(0, new EmulatorDefinition
            {
                Name = ResourceProvider.GetString(LOC.None),
                Id = null
            });

            AvailablePlatforms = new SelectableDbItemList(database.Platforms);
            OverridePlatforms = database.Platforms.OrderBy(a => a.Name).ToList();
            OverridePlatforms.Insert(0, new Platform(LOC.None.GetLocalized()) { Id = Guid.Empty });
            EditingEmulators = database.Emulators.ToList().GetClone().OrderBy(a => a.Name).ToObservable();
            EditingScanners = database.GameScanners.ToList().GetClone().OrderBy(a => a.Name).ToObservable();
            SelectedEmulator = EditingEmulators.Count > 0 ? EditingEmulators[0] : null;
            SelectedScanner = EditingScanners.Count > 0 ? EditingScanners[0] : null;
            GlobalScanSettings = database.GetGameScannersSettings();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            ConfirmDataChanges();
            CloseView(true);
        }

        private void ConfirmDataChanges()
        {
            using (database.BufferedUpdate())
            {
                // Remove deleted items
                var removedEmulators = database.Emulators.Where(a => EditingEmulators.FirstOrDefault(b => b.Id == a.Id) == null);
                if (removedEmulators.Any())
                {
                    database.Emulators.Remove(removedEmulators);
                }

                var removedScanners = database.GameScanners.Where(a => EditingScanners.FirstOrDefault(b => b.Id == a.Id) == null);
                if (removedScanners.Any())
                {
                    database.GameScanners.Remove(removedScanners);
                }

                // Add new items
                var addedEmulators = EditingEmulators.Where(a => database.Emulators[a.Id] == null);
                if (addedEmulators.Any())
                {
                    database.Emulators.Add(addedEmulators);
                }

                var addedScanners = EditingScanners.Where(a => database.GameScanners[a.Id] == null);
                if (addedScanners.Any())
                {
                    database.GameScanners.Add(addedScanners);
                }

                // Update modified items
                foreach (var item in EditingEmulators)
                {
                    var dbItem = database.Emulators[item.Id];
                    if (dbItem != null && !item.IsEqualJson(dbItem))
                    {
                        database.Emulators.Update(item);
                    }
                }

                foreach (var item in EditingScanners)
                {
                    var dbItem = database.GameScanners[item.Id];
                    if (dbItem != null && !item.IsEqualJson(dbItem))
                    {
                        database.GameScanners.Update(item);
                    }
                }
            }

            var dbSet = database.GetGameScannersSettings();
            if (!GlobalScanSettings.IsEqualJson(dbSet))
            {
                database.SetGameScannersSettings(GlobalScanSettings);
            }
        }

        public void AddEmulator()
        {
            var emulator = new Emulator("New Emulator");
            EditingEmulators.Add(emulator);
            SelectedEmulator = emulator;
        }

        public void RemoveEmulator(Emulator emulator)
        {
            var games = database.Games.Where(a => a.GameActions?.FirstOrDefault(action => action.Type == GameActionType.Emulator && action.EmulatorId == emulator.Id) != null);
            if (games.Count() > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.GetString("LOCEmuRemovalConfirmation"), emulator.Name, games.Count()),
                    "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            EditingEmulators.Remove(emulator);
            if (EditingEmulators.Count > 0)
            {
                SelectedEmulator = EditingEmulators[0];
            }
            else
            {
                SelectedEmulator = null;
            }
        }

        public void CopyEmulator(Emulator emulator)
        {
            var copy = emulator.GetClone();
            copy.Id = Guid.NewGuid();
            copy.Name += " Copy";
            if (copy.CustomProfiles?.Any() == true)
            {
                foreach (var profile in copy.CustomProfiles)
                {
                    profile.Id = $"{CustomEmulatorProfile.ProfilePrefix}{Guid.NewGuid()}";
                }
            }

            EditingEmulators.Add(copy);
            SelectedEmulator = copy;
        }

        public void AddCustomEmulatorProfile(Emulator emulator)
        {
            if (emulator.CustomProfiles == null)
            {
                emulator.CustomProfiles = new ObservableCollection<CustomEmulatorProfile>();
            }

            emulator.CustomProfiles.Add(new CustomEmulatorProfile
            {
                Name = "New Profile",
                WorkingDirectory = ExpandableVariables.EmulatorDirectory
            });
            SelectedProfile = emulator.CustomProfiles.Last();
        }

        private void AddBuiltinEmulatorProfile(Emulator emulator, string profileName)
        {
            if (emulator.BuiltinProfiles == null)
            {
                emulator.BuiltinProfiles = new ObservableCollection<BuiltInEmulatorProfile>();
            }

            emulator.BuiltinProfiles.Add(new BuiltInEmulatorProfile
            {
                Name = profileName,
                BuiltInProfileName = profileName
            });
            SelectedProfile = emulator.BuiltinProfiles.Last();
        }

        public void RemoveEmulatorProfile(Emulator parent, object profile)
        {
            if (profile is BuiltInEmulatorProfile biProf)
            {
                parent.BuiltinProfiles.Remove(biProf);
            }
            else if (profile is CustomEmulatorProfile csProf)
            {
                parent.CustomProfiles.Remove(csProf);
            }
        }

        public void CopyEmulatorProfile(Emulator emulator, CustomEmulatorProfile profile)
        {
            var copy = profile.GetClone();
            copy.Id = $"{CustomEmulatorProfile.ProfilePrefix}{Guid.NewGuid()}";
            copy.Name += " Copy";
            emulator.CustomProfiles.Add(copy);
            SelectedProfile = copy;
        }

        public void SelectEmulatorExecutable(CustomEmulatorProfile profile)
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                profile.Executable = path;
            }
        }

        public void DownloadEmulators()
        {
            // TODO rewrite to something more sane
            var plats = Emulation.Platforms.Where(a => a.Emulators.HasItems()).Select(a => new DownloadPlatform
            {
                Id = a.Id,
                Name = a.Name,
                Emulators = a.Emulators.Select(p => Emulation.GetDefition(p)).ToList()
            });
            DownloadEmulatorsList = plats.SelectMany(a => a.Emulators.Where(e => e != null).Select(b => new DownloadEmu
            {
                Name = b.Name,
                Website = b.Website,
                Platform = a
            })).ToList();
            new EmulatorDownloadWindowFactory().CreateAndOpenDialog(this);
        }

        public void ImportEmulators()
        {
            var model = new EmulatorImportViewModel(
                database,
                new EmulatorImportWindowFactory(),
                dialogs,
                resources);
            if (model.OpenView() != true)
            {
                return;
            }

            if (!model.SelectedEmulators.HasItems())
            {
                return;
            }

            foreach (var toImport in model.SelectedEmulators)
            {
                var importProfiles = toImport.Profiles.Where(a => a.Import);
                if (!importProfiles.HasItems())
                {
                    continue;
                }

                var newEmulator = new Emulator(toImport.Name)
                {
                    BuiltInConfigId = toImport.Id,
                    BuiltinProfiles = new ObservableCollection<BuiltInEmulatorProfile>(),
                    InstallDir = toImport.InstallDir
                };

                importProfiles.ForEach(a => newEmulator.BuiltinProfiles.Add(new BuiltInEmulatorProfile
                {
                    Name = a.Name,
                    BuiltInProfileName = a.ProfileName
                }));

                EditingEmulators.Add(newEmulator);
            }
        }

        private void SelectedEmulator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Emulator.BuiltInConfigId))
            {
                UpdateSelectedEmulatorBuiltInProfiles();
            }
        }

        private void UpdateSelectedEmulatorBuiltInProfiles()
        {
            if (SelectedEmulator == null)
            {
                SelectedEmulatorBuiltInProfiles = null;
                return;
            }

            SelectedEmulatorBuiltInProfiles =
                Emulation.GetDefition(SelectedEmulator.BuiltInConfigId)?.Profiles.Select(a => a.Name).ToList();
        }

        private void AddNewScannerConfig()
        {
            var newConfig = new GameScannerConfig { Name = "Config" };
            EditingScanners.Add(newConfig);
            SelectedScanner = newConfig;
        }

        private void CopyScanConfig(GameScannerConfig config)
        {
            var copy = config.GetClone();
            copy.Id = Guid.NewGuid();
            copy.Name += " Copy";
            EditingScanners.Add(copy);
            SelectedScanner = copy;
        }

        private void RemoveScanConfig(GameScannerConfig config)
        {
            EditingScanners.Remove(config);
            if (EditingScanners.Count > 0)
            {
                SelectedScanner = EditingScanners[0];
            }
            else
            {
                SelectedScanner = null;
            }
        }
    }
}
