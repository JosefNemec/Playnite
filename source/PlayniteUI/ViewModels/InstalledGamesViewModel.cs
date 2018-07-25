using NLog;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PlayniteUI.ViewModels
{
    public class InstalledGamesViewModel : ObservableObject
    {
        public enum ProgramType
        {
            Win32,
            UWP
        }

        public class InstalledGameMetadata
        {
            public class IconData
            {
                public string Name
                {
                    get; set;
                }

                public byte[] Data
                {
                    get; set;
                }
            }

            public IconData Icon
            {
                get; set;
            }

            public Game Game
            {
                get; set;
            }
        }

        public class ImportableProgram : Program
        {
            public ProgramType Type
            {
                get; set;
            }

            public string DisplayPath
            {
                get; set;
            }

            private ImageSource iconSource;
            public ImageSource IconSource
            {
                get
                {
                    if (string.IsNullOrEmpty(Icon))
                    {
                        return null;
                    }

                    if (iconSource != null)
                    {
                        return iconSource;
                    }

                    if (Type == ProgramType.UWP)
                    {
                        iconSource = BitmapExtensions.CreateSourceFromURI(new Uri(Icon));
                    }
                    else
                    {
                        string path;
                        var match = Regex.Match(Icon, @"(.*),(\d+)");
                        if (match.Success)
                        {
                            path = match.Groups[1].Value;
                            if (string.IsNullOrEmpty(path))
                            {
                                path = Path;
                            }
                        }
                        else
                        {
                            path = Icon;
                        }

                        var index = match.Groups[2].Value;
                        if (!File.Exists(path))
                        {
                            return null;
                        }

                        var icon = IconExtension.ExtractIconFromExe(path, true);
                        if (icon != null)
                        {
                            iconSource = icon.ToImageSource();
                        }
                    }

                    return iconSource;
                }
            }

            public bool Import
            {
                get; set;
            }

            public ImportableProgram()
            {
            }

            public ImportableProgram(Program program, ProgramType type)
            {
                Type = type;
                Name = program.Name;
                Icon = program.Icon;
                Path = program.Path;
                DisplayPath = type == ProgramType.Win32 ? program.Path : "Windows Store";
                Arguments = program.Arguments;
                WorkDir = program.WorkDir;
            }
        }
        
        public List<InstalledGameMetadata> Games
        {
            get;
            private set;
        } = new List<InstalledGameMetadata>();

        private ObservableCollection<ImportableProgram> programs = new ObservableCollection<ImportableProgram>();
        public ObservableCollection<ImportableProgram> Programs
        {
            get
            {
                return programs;
            }

            set
            {
                programs = value;
                OnPropertyChanged("Programs");
            }
        }

        private ImportableProgram selectedProgram;
        public ImportableProgram SelectedProgram
        {
            get
            {
                return selectedProgram;
            }

            set
            {
                selectedProgram = value;
                OnPropertyChanged("SelectedProgram");
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

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private CancellationTokenSource cancelToken;

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

        public RelayCommand<object> SelectExecutableCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectExecutable();
            });
        }

        public RelayCommand<object> ScanFolderCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ScanFolder();
            });
        }

        public RelayCommand<object> DetectInstalledCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                DetectInstalled();
            });
        }

        public RelayCommand<object> CancelProgressCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CancelProgress();
            });
        }

        public InstalledGamesViewModel(IWindowFactory window, IDialogsFactory dialogs)
        {
            this.window = window;
            this.dialogs = dialogs;
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public bool? OpenView(string directory)
        {
            if (!string.IsNullOrEmpty(directory))
            {
#pragma warning disable CS4014
                ScanFolder(directory);
#pragma warning restore CS4014
            }

            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            Games = new List<InstalledGameMetadata>();
            foreach (var program in Programs)
            {
                if (!program.Import)
                {
                    continue;
                }

                var newGame = new Game()
                {
                    Name = program.Name,
                    Provider = Provider.Custom,
                    InstallDirectory = program.WorkDir,
                    Source = program.Type == ProgramType.UWP ? "Windows Store" : string.Empty
                };

                var path = program.Path;
                if (program.Type == ProgramType.Win32 && !string.IsNullOrEmpty(program.WorkDir))
                {
                    path = @"{InstallDir}\" + program.Path.Replace(program.WorkDir, string.Empty).TrimStart('\\');
                }

                newGame.PlayTask = new GameTask()
                {
                    Path = path,
                    Arguments = program.Arguments,
                    Type = GameTaskType.File,
                    WorkingDir = program.Type == ProgramType.Win32 ? "{InstallDir}" : string.Empty,
                    Name = "Play"                    
                };

                newGame.State = new GameState() { Installed = true };

                InstalledGameMetadata.IconData icon = null;

                if (program.IconSource != null)
                {
                    icon = new InstalledGameMetadata.IconData()
                    {
                        Name = Guid.NewGuid().ToString() + ".png"
                    };

                    var bitmap = (BitmapSource)program.IconSource;
                    icon.Data = bitmap.ToPngArray();
                }

                var data = new InstalledGameMetadata()
                {
                    Game = newGame,
                    Icon = icon
                };

                Games.Add(data);
            }

            CloseView(true);
        }

        public void SelectExecutable()
        {
            var path = dialogs.SelectFile("Executable (.exe)|*.exe");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var program = new ImportableProgram()
            {
                Icon = path,
                Name = new DirectoryInfo(Path.GetDirectoryName(path)).Name,
                Path = path,
                WorkDir = Path.GetDirectoryName(path),
                Import = true
            };

            Programs.Add(program);
            SelectedProgram = program;
        }

        public async void DetectInstalled()
        {
            IsLoading = true;
            cancelToken = new CancellationTokenSource();
       
            try
            {
                var allApps = new List<ImportableProgram>();
                var installed = await Playnite.Programs.GetInstalledPrograms(cancelToken);
                if (installed != null)
                {
                    allApps.AddRange(installed.Select(a => new ImportableProgram(a, ProgramType.Win32)));

                    if (Environment.OSVersion.Version.Major == 10)
                    {
                        allApps.AddRange(Playnite.Programs.GetUWPApps().Select(a => new ImportableProgram(a, ProgramType.UWP)));
                    }

                    Programs = new ObservableCollection<ImportableProgram>(allApps.OrderBy(a => a.Name));
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to load list of installed apps.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void ScanFolder()
        {
            var path = dialogs.SelectFolder();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await ScanFolder(path);
        }

        public async Task ScanFolder(string path)
        {
            IsLoading = true;
            cancelToken = new CancellationTokenSource();

            try
            {
                var executables = await Playnite.Programs.GetExecutablesFromFolder(path, SearchOption.AllDirectories, cancelToken);
                if (executables != null)
                {
                    var apps = executables.Select(a => new ImportableProgram(a, ProgramType.Win32)).OrderBy(a => a.Name);
                    Programs = new ObservableCollection<ImportableProgram>(apps);
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to scan folder for executables: " + path);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void CancelProgress()
        {
            cancelToken?.Cancel();
        }

        public static List<Game> AddImportableGamesToDb(List<InstalledGameMetadata> games, GameDatabase database)
        {
            foreach (var game in games)
            {
                if (game.Icon != null)
                {
                    var iconId = "images/custom/" + game.Icon.Name;
                    game.Game.Icon = database.AddFileNoDuplicate(iconId, game.Icon.Name, game.Icon.Data);
                }
            }

            var insertGames = games.Select(a => a.Game).ToList();
            database.AddGames(insertGames);
            database.AssignPcPlatform(insertGames);
            return insertGames;
        }
    }
}
