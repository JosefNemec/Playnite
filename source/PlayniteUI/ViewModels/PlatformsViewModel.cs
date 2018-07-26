using LiteDB;
using Newtonsoft.Json;
using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Converters;
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
                OnPropertyChanged("Selected");
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
                OnPropertyChanged("IsPlatformsSelected");
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
                OnPropertyChanged("IsEmulatorsSelected");
            }
        }

        private Platform selectedPlatform;
        public Platform SelectedPlatform
        {
            get => selectedPlatform;
            set
            {
                selectedPlatform = value;
                OnPropertyChanged("SelectedPlatform");
            }
        }

        private Emulator selectedEmulator;
        public Emulator SelectedEmulator
        {
            get => selectedEmulator;
            set
            {
                selectedEmulator = value;
                OnPropertyChanged("SelectedEmulator");
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
                OnPropertyChanged("SelectedEmulatorProfile");
            }
        }

        private ObservableCollection<Platform> platforms;
        public ObservableCollection<Platform> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
                OnPropertyChanged("Platforms");
            }
        }

        private ObservableCollection<SelectablePlatform> selectablePlatforms;
        public ObservableCollection<SelectablePlatform> SelectablePlatforms
        {
            get => selectablePlatforms;
            set
            {
                selectablePlatforms = value;
                OnPropertyChanged("SelectablePlatforms");
            }
        }

        private ObservableCollection<Emulator> emulators;
        public ObservableCollection<Emulator> Emulators
        {
            get => emulators;
            set
            {
                emulators = value;
                OnPropertyChanged("Emulators");
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
                    new ResourceProvider());
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
                    new ResourceProvider());
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
            return new ObservableCollection<Platform>(database.PlatformsCollection.FindAll().OrderBy(a => a.Name));
        }

        private void UpdatePlatformsToDB()
        {
            // Remove deleted platforms from database
            var dbPlatforms = database.PlatformsCollection.FindAll();
            var removedPlatforms = dbPlatforms.Where(a => Platforms.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.RemovePlatform(removedPlatforms?.ToList());

            // Add new platforms to database
            var addedPlatforms = Platforms.Where(a => a.Id == null).ToList();
            database.AddPlatform(addedPlatforms?.ToList());

            // Remove files from deleted platforms
            foreach (var platform in removedPlatforms)
            {
                if (!string.IsNullOrEmpty(platform.Icon))
                {
                    database.DeleteFile(platform.Icon);
                }

                if (!string.IsNullOrEmpty(platform.Cover))
                {
                    database.DeleteFile(platform.Cover);
                }
            }

            // Save files from modified platforms
            var fileIdMask = "images/platforms/{0}/{1}";
            foreach (var platform in Platforms)
            {
                var dbPlatform = database.PlatformsCollection.FindById(platform.Id);

                if (!string.IsNullOrEmpty(platform.Icon) && !platform.Icon.StartsWith("images") && File.Exists(platform.Icon))
                {
                    if (!string.IsNullOrEmpty(dbPlatform.Icon))
                    {
                        database.DeleteFile(dbPlatform.Icon);
                    }

                    var extension = Path.GetExtension(platform.Icon);
                    var name = Guid.NewGuid() + extension;
                    var id = string.Format(fileIdMask, platform.Id, name);
                    database.AddFile(id, name, File.ReadAllBytes(platform.Icon));

                    if (Paths.AreEqual(Path.GetDirectoryName(platform.Icon), PlaynitePaths.TempPath))
                    {
                        File.Delete(platform.Icon);
                    }

                    platform.Icon = id;
                }

                if (!string.IsNullOrEmpty(platform.Cover) && !platform.Cover.StartsWith("images") && File.Exists(platform.Cover))
                {
                    if (!string.IsNullOrEmpty(dbPlatform.Cover))
                    {
                        database.DeleteFile(dbPlatform.Cover);
                    }

                    var extension = Path.GetExtension(platform.Cover);
                    var name = Guid.NewGuid() + extension;
                    var id = string.Format(fileIdMask, platform.Id, name);
                    database.AddFile(id, name, File.ReadAllBytes(platform.Cover));
                    platform.Cover = id;
                }
            }

            // Update modified platforms in database
            foreach (var platform in Platforms)
            {
                var dbPlatform = database.PlatformsCollection.FindById(platform.Id);
                if (dbPlatform != null && !platform.IsEqualJson(dbPlatform))
                {
                    database.UpdatePlatform(platform);
                }
            }
        }

        private ObservableCollection<Emulator> GetEmulatorsFromDB()
        {
            return new ObservableCollection<Emulator>(database.EmulatorsCollection.FindAll().OrderBy(a => a.Name));
        }

        private void UpdateEmulatorsToDB()
        {
            // Remove deleted emulators from database
            var dbEmulators = database.EmulatorsCollection.FindAll();
            var removedEmulators = dbEmulators.Where(a => Emulators.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.RemoveEmulator(removedEmulators?.ToList());

            // Add new platforms to database
            var addedEmulators = Emulators.Where(a => a.Id == null).ToList();
            database.AddEmulator(addedEmulators?.ToList());

            // Update modified platforms in database
            foreach (var emulator in Emulators)
            {
                if (emulator.Id == null)
                {
                    continue;
                }

                var dbEmulator = database.EmulatorsCollection.FindById(emulator.Id);
                if (dbEmulator != null && !emulator.IsEqualJson(dbEmulator))
                {
                    database.UpdateEmulator(emulator);
                }
            }
        }

        public void AddPlatform()
        {
            var platform = new Platform("New Platform") { Id = null };
            Platforms.Add(platform);
            SelectedPlatform = platform;
        }

        public void RemovePlatform(Platform platform)
        {
            var games = database.GamesCollection.Find(a => a.PlatformId == platform.Id);
            var emus = database.EmulatorsCollection.FindAll().Where(a => a.Profiles != null && a.Profiles.FirstOrDefault(b => b.Platforms.Contains(platform.Id)) != null);
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

            if (path.EndsWith("exe", StringComparison.CurrentCultureIgnoreCase))
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
            var emulator = new Emulator("New Emulator") { Id = null };
            Emulators.Add(emulator);
            SelectedEmulator = emulator;
        }

        public void RemoveEmulator(Emulator emulator)
        {
            var games = database.GamesCollection.Find(a => a.PlayAction != null && a.PlayAction.Type == GameActionType.Emulator && a.PlayAction.EmulatorId == emulator.Id);
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
            var copy = emulator.CloneJson(new JsonSerializerSettings() { ContractResolver = new ObjectIdContractResolver() });
            copy.Id = null;
            copy.Name += " Copy";
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
            var copy = profile.CloneJson(new JsonSerializerSettings() { ContractResolver = new ObjectIdContractResolver() });
            copy.Name += " Copy";
            copy.Id = ObjectId.NewObjectId();
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
                            OnPropertyChanged("SelectablePlatforms");                            
                            selectedEmulatorProfile.Platforms = SelectablePlatforms?.Where(b => b.Selected)?.Select(c => c.Id).ToList();
                        }
                    };

                    return platform;
                }));
        }
    }

}
