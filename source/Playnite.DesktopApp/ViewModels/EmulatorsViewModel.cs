using Playnite;
using Playnite.Database;
using Playnite.Emulators;
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

namespace Playnite.DesktopApp.ViewModels
{
    public class EmulatorsViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();

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

        private Emulator selectedEmulator;
        public Emulator SelectedEmulator
        {
            get => selectedEmulator;
            set
            {
                selectedEmulator = value;
                OnPropertyChanged();
                SelectedProfile = SelectedEmulator?.Profiles?.FirstOrDefault();
            }
        }

        private EmulatorProfile selectedProfile;
        public EmulatorProfile SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                OnPropertyChanged();
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

        public List<EmulatorDefinition> EmulatorDefinitions { get; set; }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<EmulatorProfile> SelectEmulatorExecutableCommand
        {
            get => new RelayCommand<EmulatorProfile>((a) =>
            {
                SelectEmulatorExecutable(a);
            }, (a) => a != null);
        }

        public RelayCommand<object> AddEmulatorCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddEmulator();
            });
        }

        public RelayCommand<Emulator> RemoveEmulatorCommand
        {
            get => new RelayCommand<Emulator>((a) =>
            {
                RemoveEmulator(a);
            }, (a) => a != null);
        }

        public RelayCommand<Emulator> CopyEmulatorCommand
        {
            get => new RelayCommand<Emulator>((a) =>
            {
                CopyEmulator(a);
            }, (a) => a != null);
        }

        public RelayCommand<Emulator> AddEmulatorProfileCommand
        {
            get => new RelayCommand<Emulator>((a) =>
            {
                AddEmulatorProfile(a);
            }, (a) => a != null);
        }

        public RelayCommand<EmulatorProfile> RemoveEmulatorProfileCommand
        {
            get => new RelayCommand<EmulatorProfile>((a) =>
            {
                RemoveEmulatorProfile(SelectedEmulator, a);
            }, (a) => a != null);
        }

        public RelayCommand<EmulatorProfile> CopyEmulatorProfileCommand
        {
            get => new RelayCommand<EmulatorProfile>((a) =>
            {
                CopyEmulatorProfile(SelectedEmulator, a);
            }, (a) => a != null);
        }

        public RelayCommand<object> ImportEmulatorsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new EmulatorImportViewModel(
                    database,
                    EmulatorImportViewModel.DialogType.EmulatorImport,
                    new EmulatorImportWindowFactory(),
                    dialogs,
                    resources);
                ImportEmulators(model);
            });
        }

        public RelayCommand<object> DownloadEmulatorsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new EmulatorImportViewModel(
                    database,
                    EmulatorImportViewModel.DialogType.EmulatorDownload,
                    new EmulatorImportWindowFactory(),
                    dialogs,
                    resources);
                DownloadEmulators(model);
            });
        }

        private GameDatabase database;
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        public EmulatorsViewModel(GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;

            try
            {
                EmulatorDefinitions = EmulatorDefinition.GetDefinitions();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                EmulatorDefinitions = new List<EmulatorDefinition>();
                logger.Error(e, "Failed to load emulator definitions.");
            }

            AvailablePlatforms = new SelectableDbItemList(database.Platforms);
            ReloadEmulatorsFromDb();
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
            UpdateEmulatorsToDB();
            CloseView(true);
        }

        private void UpdateEmulatorsToDB()
        {
            using (database.BufferedUpdate())
            {
                // Remove deleted items
                var removedItems = database.Emulators.Where(a => EditingEmulators.FirstOrDefault(b => b.Id == a.Id) == null);
                if (removedItems.Any())
                {
                    database.Emulators.Remove(removedItems);
                }

                // Add new items
                var addedItems = EditingEmulators.Where(a => database.Emulators[a.Id] == null);
                if (addedItems.Any())
                {
                    database.Emulators.Add(addedItems);
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
            }
        }

        public void AddEmulator()
        {
            var emulator = new Emulator("New Emulator")
            {
                Id = Guid.NewGuid()
            };
            EditingEmulators.Add(emulator);
            SelectedEmulator = emulator;
        }

        public void RemoveEmulator(Emulator emulator)
        {
            var games = database.Games.Where(a => a.PlayAction != null && a.PlayAction.Type == GameActionType.Emulator && a.PlayAction.EmulatorId == emulator.Id);
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
            if (copy.Profiles?.Any() == true)
            {
                foreach (var profile in copy.Profiles)
                {
                    profile.Id = Guid.NewGuid();
                }
            }

            EditingEmulators.Add(copy);
            SelectedEmulator = copy;
        }

        public void AddEmulatorProfile(Emulator parent)
        {
            var profile = new EmulatorProfile() { Name = "New Profile" };
            if (parent.Profiles == null)
            {
                parent.Profiles = new ObservableCollection<EmulatorProfile>();
            }

            parent.Profiles.Add(profile);
            SelectedProfile = profile;
        }

        public void RemoveEmulatorProfile(Emulator parent, EmulatorProfile profile)
        {
            parent.Profiles.Remove(profile);
        }

        public void CopyEmulatorProfile(Emulator parent, EmulatorProfile profile)
        {
            var copy = profile.GetClone();
            copy.Id = Guid.NewGuid();
            copy.Name += " Copy";
            parent.Profiles.Add(copy);
            SelectedProfile = copy;
        }

        public void SelectEmulatorExecutable(EmulatorProfile profile)
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                profile.Executable = path;
            }
        }

        public void DownloadEmulators(EmulatorImportViewModel model)
        {
            model.OpenView();
        }

        public void ImportEmulators(EmulatorImportViewModel model)
        {
            var dbEmulators = database.Emulators.GetClone();
            if (EditingEmulators != null && !EditingEmulators.IsEqualJson(dbEmulators))
            {
                var askResult = dialogs.ShowMessage(
                    resources.GetString("LOCConfirmUnsavedEmulatorsTitle"),
                    resources.GetString("LOCSaveChangesAskTitle"),
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (askResult == MessageBoxResult.Yes)
                {
                    UpdateEmulatorsToDB();
                }
                else if (askResult == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var result = model.OpenView();
            if (result == true)
            {
                AvailablePlatforms = new SelectableDbItemList(database.Platforms);
                ReloadEmulatorsFromDb();
            }
        }

        public void ReloadEmulatorsFromDb()
        {
            EditingEmulators = database.Emulators.GetClone().OrderBy(a => a.Name).ToObservable();
            SelectedEmulator = EditingEmulators.Count > 0 ? EditingEmulators[0] : null;
        }
    }
}
