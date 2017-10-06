using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Playnite.Database;
using NLog;
using PlayniteUI.Controls;
using Playnite;
using Playnite.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Newtonsoft.Json;
using PlayniteUI.Windows;
using Playnite.Emulators;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class PlatformsWindow : WindowBase, INotifyPropertyChanged
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

        private bool isPlatformsSelected;
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
                        var askResult = PlayniteMessageBox.Show(FindResource("ConfirmUnsavedEmulatorsTitle") as string, FindResource("SaveChangesAskTitle") as string,
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
                        var askResult = PlayniteMessageBox.Show(FindResource("ConfirmUnsavedPlatformsTitle") as string, FindResource("SaveChangesAskTitle") as string,
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
                }

                isEmulatorsSelected = value;
                OnPropertyChanged("IsEmulatorsSelected");
            }
        }        

        private GameDatabase database;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public PlatformsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            using (database.BufferedUpdate())
            {
                if (TabMainTabControl.SelectedIndex == 0)
                {
                    UpdatePlatformsToDB();
                }
                else
                {
                    UpdateEmulatorsToDB();
                }
            }

            DialogResult = true;
            Close();
        }

        public bool? ConfigurePlatforms(GameDatabase database)
        {
            this.database = database;
            IsPlatformsSelected = true;
            return ShowDialog();
        }

        private void ButtonAddPlatform_Click(object sender, RoutedEventArgs e)
        {
            var platform = new Platform("New Platform") { Id = 0 };
            Platforms.Add(platform);
            ListPlatforms.SelectedItem = platform;            
            TextPlatformName.Focus();
            TextPlatformName.SelectAll();
        }

        private void ButtonRemovePlatform_Click(object sender, RoutedEventArgs e)
        {
            if (ListPlatforms.SelectedItem != null)
            {
                var platform = ListPlatforms.SelectedItem as Platform;
                Platforms.Remove(platform);
                if (Platforms.Count > 0)
                {
                    ListPlatforms.SelectedItem = Platforms[0];
                }
            }
        }

        private void ButtonSelectIcon_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectIconFile(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var platform = ListPlatforms.SelectedItem as Platform;
            platform.Icon = path;
        }

        private void ButtonSelectCover_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectIconFile(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var platform = ListPlatforms.SelectedItem as Platform;
            platform.Cover = path;
        }

        private void ButtonSelectExe_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectFile(this, "*.*|*.*");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            TextExecutable.Text = path;
        }

        private void ButtonAddEmulator_Click(object sender, RoutedEventArgs e)
        {
            var emulator = PlatformableEmulator.FromEmulator(new Emulator("New Emulator") { Id = 0 }, Platforms);
            Emulators.Add(emulator);
            ListEmulators.SelectedItem = emulator;
            TextEmulatorName.Focus();
            TextEmulatorName.SelectAll();
        }

        private void ButtonRemoveEmulator_Click(object sender, RoutedEventArgs e)
        {
            if (ListEmulators.SelectedItem != null)
            {
                var emulator = ListEmulators.SelectedItem as PlatformableEmulator;
                Emulators.Remove(emulator);
                if (Emulators.Count > 0)
                {
                    ListEmulators.SelectedItem = Emulators[0];
                }
            }
        }

        private void ButtonCopyEmulator_Click(object sender, RoutedEventArgs e)
        {
            if (ListEmulators.SelectedItem != null)
            {
                var emulator = ListEmulators.SelectedItem as PlatformableEmulator;
                var copy = emulator.CloneJson();
                copy.Id = 0;
                copy.Name += " Copy";
                copy.PlatformsList = emulator.PlatformsList.CloneJson();
                Emulators.Add(copy);
                ListEmulators.SelectedItem = copy;
            }
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

        private void ButtonImportEmulators_Click(object sender, RoutedEventArgs e)
        {
            var window = new EmulatorImportWindow(DialogType.EmulatorImport)
            {
                Owner = this
            };
            
            var result = window.ShowDialog();
            if (result == false)
            {
                return;
            }

            foreach (var emulator in window.Model.EmulatorList)
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

        private void ButtonDownloadEmulators_Click(object sender, RoutedEventArgs e)
        {
            var window = new EmulatorImportWindow(DialogType.EmulatorDownload)
            {
                Owner = this
            };

            var result = window.ShowDialog();
            if (result == false)
            {
                return;
            }
        }

        private void ButtonArgumentPresets_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.ItemsSource = EmulatorDefinitions;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            button.ContextMenu.IsOpen = true;
        }

        private void ArgumentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ListEmulators.SelectedItem == null)
            {
                return;
            }

            var definiton = ((MenuItem)sender).DataContext as EmulatorDefinition;
            var emulator = ListEmulators.SelectedItem as PlatformableEmulator;
            emulator.Arguments = definiton.DefaultArguments;
        }
    }
}