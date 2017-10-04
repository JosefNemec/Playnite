using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Win32;
using Playnite.Models;
using System.Diagnostics;
using Playnite.Providers.Steam;
using PlayniteUI.Controls;
using System.IO;
using Playnite;
using Playnite.Emulators;
using System.Threading;
using Playnite.Database;
using static PlayniteUI.PlatformsWindow;

namespace PlayniteUI.Windows
{
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

    public enum DialogType
    {
        EmulatorImport,
        EmulatorDownload,
        GameImport,
        Wizard
    }

    public class EmulatorImportWindowModel : INotifyPropertyChanged
    {
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
                return GameDatabase.Instance.EmulatorsCollection.FindAll().Where(a => a.ImageExtensions != null && a.ImageExtensions.Count > 0)                
                    .Select(a => PlatformableEmulator.FromEmulator(a, platforms.Where(b => a.Platforms != null && a.Platforms.Contains(b.Id)))).ToList();
            }
        }

        public List<Platform> DatabasePlatforms
        {
            get
            {
                return GameDatabase.Instance.PlatformsCollection.FindAll().ToList();
            }
        }

        public List<EmulatorDefinition> EmulatorDefinitions
        {
            get
            {
                return EmulatorDefinition.GetDefinitions();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async Task SearchEmulators(string path)
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

        public async Task SearchGames(string path, PlatformableEmulator emulator)
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

                GameDatabase.Instance.AddGame(game.Game);
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
                            GameDatabase.Instance.AddPlatform(newPlatform);
                            existing = newPlatform;
                        }

                        if (emulator.Emulator.Emulator.Platforms == null)
                        {
                            emulator.Emulator.Emulator.Platforms = new List<int>();
                        }

                        emulator.Emulator.Emulator.Platforms.Add(existing.Id);
                    }

                    GameDatabase.Instance.AddEmulator(emulator.Emulator.Emulator);
                }
            }

            OnPropertyChanged("DatabasePlatforms");
            OnPropertyChanged("AvailableEmulators");
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatorImportWindow : WindowBase
    {
        public EmulatorImportWindowModel Model
        {
            get; private set;
        }

        public EmulatorImportWindow(DialogType type)
        {
            InitializeComponent();
            Model = new EmulatorImportWindowModel()
            {
                Type = type
            };
            DataContext = Model;
        }

        private async void ButtonScanEmulator_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.SelectFolder(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await Model.SearchEmulators(path);
            ListEmulators.ScrollIntoView(Model.EmulatorList.Count == 0 ? null : Model.EmulatorList.Last());
        }

        private void ButtonScanGames_Click(object sender, RoutedEventArgs e)
        {
            if (Model.AvailableEmulators == null || Model.AvailableEmulators.Count == 0)
            {
                if (Model.EmulatorList == null || Model.EmulatorList.Count == 0 || Model.EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (PlayniteMessageBox.Show(FindResource("EmuWizardNoEmulatorForGamesWarning") as string, "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                    else
                    {
                        ButtonConfigEmulator_Click(this, null);
                    }
                }
            }

            var button = (Button)sender;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            button.ContextMenu.IsOpen = true;
        }

        private async void ScanGamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var emulator = ((MenuItem)sender).DataContext as PlatformableEmulator;
            var path = Dialogs.SelectFolder(this);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await Model.SearchGames(path, emulator);
            ListGames.ScrollIntoView(Model.GamesList.Count == 0 ? null : Model.GamesList.Last());
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            Model.AddSelectedGamesToDB();
            DialogResult = true;
            Close();
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (Model.ViewTabIndex == 2)
            {
                if (Model.EmulatorList == null || Model.EmulatorList.Count == 0 || Model.EmulatorList.Where(a => a.Import).Count() == 0)
                {
                    if (PlayniteMessageBox.Show(FindResource("EmuWizardNoEmulatorWarning") as string, "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        return;
                    }
                    else
                    {
                        DialogResult = true;
                        Close();
                    }
                }
            }

            Model.ViewTabIndex++;                  

            if (Model.ViewTabIndex == 3)
            {
                Model.AddSelectedEmulatorsToDB();
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            Model.ViewTabIndex--;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        private void ButtonConfigEmulator_Click(object sender, RoutedEventArgs e)
        {
            var window = new PlatformsWindow()
            {
                Owner = this
            };

            window.ConfigurePlatforms(GameDatabase.Instance);                
            Model.OnPropertyChanged("AvailableEmulators");
        }
    }
}
