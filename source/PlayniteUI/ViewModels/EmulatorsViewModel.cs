using LiteDB;
using Newtonsoft.Json;
using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUI.Commands;
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
using Playnite.Common.System;
using Playnite.Settings;

namespace PlayniteUI.ViewModels
{
    public class EmulatorsViewModel : ObservableObject
    {
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
                AvailablePlatforms.SetSelection(value?.Platforms);
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

        public List<EmulatorDefinition> EmulatorDefinitions
        {
            get => EmulatorDefinition.GetDefinitions();
        }

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

        public RelayCommand<object> SelectEmulatorExecutableCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectEmulatorExecutable(SelectedEmulator);
            }, (a) => SelectedEmulator != null);
        }

        public RelayCommand<object> AddEmulatorCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddEmulator();
            });
        }

        public RelayCommand<object> RemoveEmulatorCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveEmulator(SelectedEmulator);
            }, (a) => SelectedEmulator != null);
        }

        public RelayCommand<object> CopyEmulatorCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CopyEmulator(SelectedEmulator);
            }, (a) => SelectedEmulator != null);
        }

        public RelayCommand<object> AddEmulatorProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddEmulatorProfile();
            }, (a) => SelectedEmulator != null);
        }

        public RelayCommand<object> RemoveEmulatorProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveEmulatorProfile(SelectedProfile);
            }, (a) => SelectedProfile != null);
        }

        public RelayCommand<object> CopyEmulatorProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CopyEmulatorProfile(SelectedProfile);
            }, (a) => SelectedProfile != null);
        }

        public RelayCommand<object> ImportEmulatorsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var model = new EmulatorImportViewModel(
                    database,
                    EmulatorImportViewModel.DialogType.EmulatorImport,
                    EmulatorImportWindowFactory.Instance,
                    new DialogsFactory(),
                    new DefaultResourceProvider());
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
                    EmulatorImportWindowFactory.Instance,
                    new DialogsFactory(),
                    new DefaultResourceProvider());
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
            AvailablePlatforms = new SelectableDbItemList(database.Platforms.OrderBy(a => a.Name));
            AvailablePlatforms.SelectionChanged += (s, e) =>
            {
                if (selectedProfile != null)
                {
                    selectedProfile.Platforms = AvailablePlatforms.GetSelectedIds().ToList();
                }
            };

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
                    string.Format(resources.FindString("LOCEmuRemovalConfirmation"), emulator.Name, games.Count()),
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
            var copy = emulator.CloneJson();
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

        public void AddEmulatorProfile()
        {
            var profile = new EmulatorProfile() { Name = "New Profile" };
            if (SelectedEmulator.Profiles == null)
            {
                SelectedEmulator.Profiles = new ObservableCollection<EmulatorProfile>();
            }

            SelectedEmulator.Profiles.Add(profile);
            SelectedProfile = profile;
        }

        public void RemoveEmulatorProfile(EmulatorProfile profile)
        {
            SelectedEmulator.Profiles.Remove(profile);
        }

        public void CopyEmulatorProfile(EmulatorProfile profile)
        {
            var copy = profile.CloneJson();
            copy.Id = Guid.NewGuid();
            copy.Name += " Copy";
            SelectedEmulator.Profiles.Add(copy);
            SelectedProfile = copy;
        }

        public void SelectEmulatorExecutable(Emulator emulator)
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedProfile.Executable = path;
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
                    resources.FindString("LOCConfirmUnsavedEmulatorsTitle"),
                    resources.FindString("LOCSaveChangesAskTitle"),
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
