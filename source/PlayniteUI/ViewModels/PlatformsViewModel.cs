using Newtonsoft.Json;
using Playnite;
using Playnite.Database;
using Playnite.Emulators;
using Playnite.Models;
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

namespace PlayniteUI.ViewModels
{
    public class PlatformsViewModel : ObservableObject
    {
        public class SelectablePlatform : INotifyPropertyChanged
        {
            public int Id
            {
                get; set;
            }

            public string Name
            {
                get; set;
            }

            private bool selected;
            public bool Selected
            {
                get => selected;
                set
                {
                    selected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
                }
            }

            public SelectablePlatform()
            {
            }

            public SelectablePlatform(Platform platform)
            {
                Id = platform.Id;
                Name = platform.Name;
                Selected = false;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public class PlatformableEmulator : Emulator
        {
            public new List<int> Platforms
            {
                get
                {
                    return PlatformsList?.Where(a => a.Selected).Select(a => a.Id).OrderBy(a => a).ToList();
                }
            }

            [JsonIgnore]
            public List<SelectablePlatform> PlatformsList
            {
                get; set;
            }

            [JsonIgnore]
            public string PlatformsString
            {
                get => PlatformsList == null ? string.Empty : string.Join(", ", PlatformsList.Where(a => a.Selected).Select(a => a.Name));
            }

            public Emulator ToEmulator()
            {
                return JsonConvert.DeserializeObject<Emulator>(this.ToJson());
            }

            public static PlatformableEmulator FromEmulator(Emulator emulator, IEnumerable<Platform> platforms)
            {
                var newObj = JsonConvert.DeserializeObject<PlatformableEmulator>(emulator.ToJson());
                var newPlatforms = platforms?.Select(a => new SelectablePlatform(a) { Selected = emulator.Platforms == null ? false : emulator.Platforms.Contains(a.Id) });
                newObj.PlatformsList = new List<SelectablePlatform>(newPlatforms);
                foreach (var platform in newObj.PlatformsList)
                {
                    platform.PropertyChanged += (s, e) => { newObj.OnPropertyChanged("PlatformsString"); };
                }
                return newObj;
            }
        }

        private bool isPlatformsSelected = true;
        public bool IsPlatformsSelected
        {
            get => isPlatformsSelected;
            set
            {
                if (value == true)
                {
                    var dbEmulators = GetEmulatorsFromDB();
                    if (Emulators != null && !Emulators.Select(a => a.ToEmulator()).IsEqualJson(dbEmulators))
                    {
                        var askResult = dialogs.ShowMessage(
                            resources.FindString("ConfirmUnsavedEmulatorsTitle"),
                            resources.FindString("SaveChangesAskTitle"),
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

                    Platforms = GetPlatformsFromDB();
                    SelectedPlatform = Platforms.Count > 0 ? Platforms[0] : null;
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
                            resources.FindString("ConfirmUnsavedPlatformsTitle"),
                            resources.FindString("SaveChangesAskTitle") as string,
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

                    Emulators = new ObservableCollection<PlatformableEmulator>(GetEmulatorsFromDB().Select(a => PlatformableEmulator.FromEmulator(a, Platforms)));
                    SelectedEmulator = Emulators.Count > 0 ? Emulators[0] : null;
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

        private PlatformableEmulator selectedEmulator;
        public PlatformableEmulator SelectedEmulator
        {
            get => selectedEmulator;
            set
            {
                selectedEmulator = value;
                OnPropertyChanged("SelectedEmulator");
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

        private ObservableCollection<PlatformableEmulator> emulators;
        public ObservableCollection<PlatformableEmulator> Emulators
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
                CloseDialog(false);
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

        public RelayCommand<EmulatorDefinition> SetEmulatorArgumentsCommand
        {
            get => new RelayCommand<EmulatorDefinition>((emu) =>
            {
                SetEmulatorArguments(SelectedEmulator, emu.DefaultArguments);
            }, (a) => SelectedEmulator != null);
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

        public bool? ShowDialog()
        {
            IsPlatformsSelected = true;
            return window.CreateAndOpenDialog(this);
        }

        public void CloseDialog(bool? result)
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

            CloseDialog(true);
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
            var addedPlatforms = Platforms.Where(a => a.Id == 0).ToList();
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

                    var extension = System.IO.Path.GetExtension(platform.Icon);
                    var name = Guid.NewGuid() + extension;
                    var id = string.Format(fileIdMask, platform.Id, name);
                    database.AddImage(id, name, File.ReadAllBytes(platform.Icon));

                    if (Paths.AreEqual(System.IO.Path.GetDirectoryName(platform.Icon), Paths.TempPath))
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

                    var extension = System.IO.Path.GetExtension(platform.Cover);
                    var name = Guid.NewGuid() + extension;
                    var id = string.Format(fileIdMask, platform.Id, name);
                    database.AddImage(id, name, File.ReadAllBytes(platform.Cover));
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
            var addedEmulators = Emulators.Select(a => a.ToEmulator()).Where(a => a.Id == 0).ToList();
            database.AddEmulator(addedEmulators?.ToList());

            // Update modified platforms in database
            foreach (var emulator in Emulators)
            {
                var dbEmulator = database.EmulatorsCollection.FindById(emulator.Id);
                if (dbEmulator != null && !emulator.ToEmulator().IsEqualJson(dbEmulator))
                {
                    database.UpdateEmulator(emulator.ToEmulator());
                }
            }
        }

        public void AddPlatform()
        {
            var platform = new Platform("New Platform") { Id = 0 };
            Platforms.Add(platform);
            SelectedPlatform = platform;
        }

        public void RemovePlatform(Platform platform)
        {
            var games = database.GamesCollection.Find(a => a.PlatformId == platform.Id);
            var emus = database.EmulatorsCollection.FindAll().Where(a => a.Platforms != null && a.Platforms.Contains(platform.Id));
            if (games.Count() > 0 || emus.Count() > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.FindString("PlatformRemovalConfirmation"), platform.Name, games.Count(), emus.Count()),
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

                path = System.IO.Path.Combine(Paths.TempPath, Guid.NewGuid() + ".png");
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
            var emulator = PlatformableEmulator.FromEmulator(new Emulator("New Emulator") { Id = 0 }, Platforms);
            Emulators.Add(emulator);
            SelectedEmulator = emulator;
        }

        public void RemoveEmulator(PlatformableEmulator emulator)
        {
            var games = database.GamesCollection.Find(a => a.PlayTask != null && a.PlayTask.Type == GameTaskType.Emulator && a.PlayTask.EmulatorId == emulator.Id);
            if (games.Count() > 0)
            {
                if (dialogs.ShowMessage(
                    string.Format(resources.FindString("EmuRemovalConfirmation"), emulator.Name, games.Count()),
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

        public void CopyEmulator(PlatformableEmulator emulator)
        {
            var copy = emulator.CloneJson();
            copy.Id = 0;
            copy.Name += " Copy";
            copy.PlatformsList = emulator.PlatformsList.CloneJson();
            Emulators.Add(copy);
            SelectedEmulator = copy;
        }

        public void SelectEmulatorExecutable(PlatformableEmulator emulator)
        {
            var path = dialogs.SelectFile("*.*|*.*");
            if (!string.IsNullOrEmpty(path))
            {
                emulator.Executable = path;
            }
        }

        public void SetEmulatorArguments(PlatformableEmulator emulator, string arguments)
        {
            emulator.Arguments = arguments;
        }

        public void DownloadEmulators(EmulatorImportViewModel model)
        {
            model.ShowDialog();
        }

        public void ImportEmulators(EmulatorImportViewModel model)
        {
            var result = model.ShowDialog();
            if (result != true)
            {
                return;
            }

            foreach (var emulator in model.EmulatorList)
            {
                if (emulator.Import)
                {
                    foreach (var platform in emulator.Emulator.Definition.Platforms)
                    {
                        var existing = Platforms.FirstOrDefault(a => string.Equals(a.Name, platform, StringComparison.InvariantCultureIgnoreCase));
                        if (existing == null)
                        {
                            var newPlatform = new Platform(platform) { Id = 0 };
                            Platforms.Add(newPlatform);
                            UpdatePlatformsToDB();
                            existing = newPlatform;
                        }

                        if (emulator.Emulator.Emulator.Platforms == null)
                        {
                            emulator.Emulator.Emulator.Platforms = new List<int>();
                        }

                        emulator.Emulator.Emulator.Platforms.Add(existing.Id);
                    }

                    Emulators.Add(PlatformableEmulator.FromEmulator(emulator.Emulator.Emulator, Platforms));
                }
            }
        }
    }

}
