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
    public class SelectablePlatform : Platform
    {
        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        public SelectablePlatform(Platform platform)
        {
            Id = platform.Id;
            Name = platform.Name;
            Selected = false;
        }
    }

    public class PlatformsViewModel : ObservableObject
    {
        private bool isPlatformsSelected = true;
        public bool IsPlatformsSelected
        {
            get => isPlatformsSelected;
            set
            {
                if (value == true)
                {
                    var dbEmulators = GetEmulatorsFromDB();
                    if (Emulators != null && !Emulators.IsEqualJson(dbEmulators))
                    {
                        var askResult = dialogs.ShowMessage(
                            resources.FindString("LOCConfirmUnsavedEmulatorsTitle"),
                            resources.FindString("LOCSaveChangesAskTitle"),
                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (askResult == MessageBoxResult.Cancel)
                        {
                            IsPlatformsSelected = false;
                            return;
                        }
                        else if (askResult == MessageBoxResult.Yes)
                        {
                            UpdateEmulatorsToDB();
                        }
                    }

                    ReloadPlatforms();
                }

                isPlatformsSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isEmulatorsSelected;
        public bool IsEmulatorsSelected
        {
            get => isEmulatorsSelected;
            set
            {
                if (value == true)
                {
                    var dbPlatforms = GetPlatformsFromDB();
                    if (Platforms != null && !Platforms.IsEqualJson(dbPlatforms))
                    {
                        var askResult = dialogs.ShowMessage(
                            resources.FindString("LOCConfirmUnsavedPlatformsTitle"),
                            resources.FindString("LOCSaveChangesAskTitle") as string,
                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (askResult == MessageBoxResult.Cancel)
                        {
                            IsEmulatorsSelected = false;
                            return;
                        }
                        else if (askResult == MessageBoxResult.Yes)
                        {
                            UpdatePlatformsToDB();
                        }
                    }

                    ReloadEmulators();
                }

                isEmulatorsSelected = value;
                OnPropertyChanged();
            }
        }

        private Platform selectedPlatform;
        public Platform SelectedPlatform
        {
            get => selectedPlatform;
            set
            {
                selectedPlatform = value;
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
                SelectedEmulatorProfile = SelectedEmulator?.Profiles?.FirstOrDefault();
            }
        }

        private EmulatorProfile selectedEmulatorProfile;
        public EmulatorProfile SelectedEmulatorProfile
        {
            get => selectedEmulatorProfile;
            set
            {
                selectedEmulatorProfile = value;
                ReloadSelectablePlatforms(value);
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Platform> platforms;
        public ObservableCollection<Platform> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SelectablePlatform> selectablePlatforms;
        public ObservableCollection<SelectablePlatform> SelectablePlatforms
        {
            get => selectablePlatforms;
            set
            {
                selectablePlatforms = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Emulator> emulators;
        public ObservableCollection<Emulator> Emulators
        {
            get => emulators;
            set
            {
                emulators = value;
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

        public RelayCommand<object> AddPlatformCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddPlatform();
            });
        }

        public RelayCommand<object> RemovePlatformCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                RemovePlatform(SelectedPlatform);
            }, (a) => SelectedPlatform != null);
        }

        public RelayCommand<object> SelectPlatformIconCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectPlatformIcon(SelectedPlatform);
            }, (a) => SelectedPlatform != null);
        }

        public RelayCommand<object> SelectPlatformCoverCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectPlatformCover(SelectedPlatform);
            }, (a) => SelectedPlatform != null);
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
                RemoveEmulatorProfile(SelectedEmulatorProfile);
            }, (a) => SelectedEmulatorProfile != null);
        }

        public RelayCommand<object> CopyEmulatorProfileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CopyEmulatorProfile(SelectedEmulatorProfile);
            }, (a) => SelectedEmulatorProfile != null);
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

        public PlatformsViewModel(GameDatabase database, IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;
        }

        public bool? OpenView()
        {
            IsPlatformsSelected = true;
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            using (database.BufferedUpdate())
            {
                if (IsPlatformsSelected)
                {
                    UpdatePlatformsToDB();
                }
                else
                {
                    UpdateEmulatorsToDB();
                }
            }

            CloseView(true);
        }

        private ObservableCollection<Platform> GetPlatformsFromDB()
        {
            return new ObservableCollection<Platform>(database.Platforms.Select(a => a.CloneJson()).OrderBy(a => a.Name));
        }

        private void UpdatePlatformsToDB()
        {
            // Update modified platforms in database
            foreach (var platform in Platforms.Where(a => a.Id != Guid.Empty))
            {
                var dbPlatform = database.Platforms.Get(platform.Id);

                // Save files from modified platforms
                if (!string.IsNullOrEmpty(platform.Icon) && File.Exists(platform.Icon))
                {
                    database.RemoveFile(dbPlatform.Icon);
                    var id = database.AddFile(platform.Icon, dbPlatform.Id);
                    // In case Icon was extracted from EXE
                    if (Paths.AreEqual(Path.GetDirectoryName(platform.Icon), PlaynitePaths.TempPath))
                    {
                        File.Delete(platform.Icon);
                    }

                    platform.Icon = id;
                }

                if (!string.IsNullOrEmpty(platform.Cover) && File.Exists(platform.Cover))
                {
                    database.RemoveFile(dbPlatform.Cover);
                    platform.Cover = database.AddFile(platform.Cover, dbPlatform.Id);
                }

                if (dbPlatform != null && !platform.IsEqualJson(dbPlatform))
                {
                    database.Platforms.Update(platform);
                }
            }

            // Remove deleted platforms from database
            var removedPlatforms = database.Platforms.Where(a => Platforms.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.Platforms.Remove(removedPlatforms?.ToList());      

            // Add new platforms to database
            var addedPlatforms = Platforms.Where(a => a.Id == Guid.Empty).ToList();
            foreach (var addedPlatform in addedPlatforms)
            {
                addedPlatform.Id = Guid.NewGuid();

                if (!string.IsNullOrEmpty(addedPlatform.Icon))
                {
                    var id = database.AddFile(addedPlatform.Icon, addedPlatform.Id);
                    // In case Icon was extracted from EXE
                    if (Paths.AreEqual(Path.GetDirectoryName(addedPlatform.Icon), PlaynitePaths.TempPath))
                    {
                        File.Delete(addedPlatform.Icon);
                    }

                    addedPlatform.Icon = id;
                }

                if (!string.IsNullOrEmpty(addedPlatform.Cover))
                {
                    addedPlatform.Cover = database.AddFile(addedPlatform.Cover, addedPlatform.Id);
                }
                
                database.Platforms.Add(addedPlatform);
            }
        }

        private ObservableCollection<Emulator> GetEmulatorsFromDB()
        {
            return new ObservableCollection<Emulator>(database.Emulators.Select(a => a.CloneJson()).OrderBy(a => a.Name));
        }

        private void UpdateEmulatorsToDB()
        {
            // Remove deleted emulators from database
            var removedEmulators = database.Emulators.Where(a => Emulators.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.Emulators.Remove(removedEmulators?.ToList());

            // Add new platforms to database
            var addedEmulators = Emulators.Where(a => a.Id == Guid.Empty).ToList();
            addedEmulators.ForEach(a => a.Id = Guid.NewGuid());
            database.Emulators.Add(addedEmulators?.ToList());

            // Update modified platforms in database
            foreach (var emulator in Emulators)
            {
                var dbEmulator = database.Emulators.Get(emulator.Id);
                if (dbEmulator != null && !emulator.IsEqualJson(dbEmulator))
                {
                    database.Emulators.Update(emulator);
                }
            }
        }

        public void AddPlatform()
        {
            var platform = new Platform("New Platform")
            {
                Id = Guid.Empty
            };
            Platforms.Add(platform);
            SelectedPlatform = platform;
        }

        public void RemovePlatform(Platform platform)
        {
            var games = database.Games.Where(a => a.PlatformId == platform.Id);
            var emus = database.Emulators.Where(a => a.Profiles != null && a.Profiles.FirstOrDefault(b => b.Platforms.Contains(platform.Id)) != null);
            if (games.Count() > 0 || emus.Count() > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.FindString("LOCPlatformRemovalConfirmation"), platform.Name, games.Count(), emus.Count()),
                    "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Platforms.Remove(platform);
            if (Platforms.Count > 0)
            {
                SelectedPlatform = Platforms[0];
            }
            else
            {
                SelectedPlatform = null;
            }
        }

        public void SelectPlatformIcon(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path.EndsWith("exe", StringComparison.OrdinalIgnoreCase))
            {
                var ico = IconExtension.ExtractIconFromExe(path, true);
                if (ico == null)
                {
                    return;
                }

                path = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".png");
                FileSystem.PrepareSaveFile(path);
                ico.ToBitmap().Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }

            platform.Icon = path;
        }

        public void SelectPlatformCover(Platform platform)
        {
            var path = dialogs.SelectIconFile();
            if (!string.IsNullOrEmpty(path))
            {
                platform.Cover = path;
            } 
        }

        public void AddEmulator()
        {
            var emulator = new Emulator("New Emulator")
            {
                Id = Guid.Empty
            };
            Emulators.Add(emulator);
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

            Emulators.Remove(emulator);
            if (Emulators.Count > 0)
            {
                SelectedEmulator = Emulators[0];
            }
            else
            {
                SelectedEmulator = null;
            }
        }

        public void CopyEmulator(Emulator emulator)
        {
            var copy = emulator.CloneJson();
            copy.Id = Guid.Empty;
            copy.Name += " Copy";
            if (copy.Profiles?.Any() == true)
            {
                foreach (var profile in copy.Profiles)
                {
                    profile.Id = Guid.NewGuid();
                }
            }

            Emulators.Add(copy);
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
            SelectedEmulatorProfile = profile;
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
            SelectedEmulatorProfile = copy;
        }

        public void SelectEmulatorExecutable(Emulator emulator)
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedEmulatorProfile.Executable = path;
            }
        }

        public void DownloadEmulators(EmulatorImportViewModel model)
        {
            model.OpenView();
        }

        public void ImportEmulators(EmulatorImportViewModel model)
        {
            var dbEmulators = GetEmulatorsFromDB();
            if (Emulators != null && !Emulators.IsEqualJson(dbEmulators))
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
                ReloadPlatforms();
                ReloadEmulators();
            }
        }

        public void ReloadEmulators()
        {
            Emulators = GetEmulatorsFromDB();
            SelectedEmulator = Emulators.Count > 0 ? Emulators[0] : null;
        }

        public void ReloadPlatforms()
        {
            Platforms = GetPlatformsFromDB();
            ReloadSelectablePlatforms(SelectedEmulatorProfile);
            SelectedPlatform = Platforms.Count > 0 ? Platforms[0] : null;
        }

        public void ReloadSelectablePlatforms(EmulatorProfile selectedProfile)
        {
            if (selectedProfile == null)
            {
                SelectablePlatforms = null;
                return;
            }

            SelectablePlatforms = new ObservableCollection<SelectablePlatform>(
                Platforms.Select(a =>
                {
                    var platform = new SelectablePlatform(a)
                    {
                        Selected = selectedProfile.Platforms?.Contains(a.Id) == true
                    };

                    platform.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "Selected")
                        {
                            OnPropertyChanged(nameof(SelectablePlatforms));
                            selectedEmulatorProfile.Platforms = SelectablePlatforms?.Where(b => b.Selected)?.Select(c => c.Id).ToList();
                        }
                    };

                    return platform;
                }));
        }
    }

}
