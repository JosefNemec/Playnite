using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Playnite;
using Playnite.Database;
using Playnite.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using NLog;

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for InstalledGamesWindow.xaml
    /// </summary>
    public partial class InstalledGamesWindow : Window, INotifyPropertyChanged
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private List<InstalledGameMetadata> games = new List<InstalledGameMetadata>();
        public List<InstalledGameMetadata> Games
        {
            get
            {
                return games;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Programs"));
            }
        }

        public class ImportableProgram : Program
        {
            private Icon iconSource;
            public Icon IconSource
            {
                get
                {
                    if (iconSource != null)
                    {
                        return iconSource;
                    }

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
                    iconSource = IconExtension.ExtractIconFromExe(path, true);
                    if (iconSource == null)
                    {
                        return null;
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

            public ImportableProgram(Program program)
            {
                Name = program.Name;
                Icon = program.Icon;
                Path = program.Path;
                WorkDir = program.WorkDir;
            }
        }            

        public InstalledGamesWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            games = new List<InstalledGameMetadata>();
            foreach (var program in Programs)
            {
                if (!program.Import)
                {
                    continue;
                }

                var newGame = new Game()
                {
                    Name = program.Name,
                    Provider = Provider.Custom
                };

                newGame.PlayTask = new GameTask()
                {
                    Path = program.Path,
                    Type = GameTaskType.File,
                    WorkingDir = program.WorkDir,
                    Name = "Play"
                };

                InstalledGameMetadata.IconData icon = null;

                if (program.IconSource != null)
                {
                    icon = new InstalledGameMetadata.IconData()
                    {
                        Name = Guid.NewGuid().ToString() + ".png"
                    };

                    using (var stream = new MemoryStream())
                    {
                        program.IconSource.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        icon.Data = stream.ToArray();
                    }
                }

                var data = new InstalledGameMetadata()
                {
                    Game = newGame,
                    Icon = icon
                };

                games.Add(data);
            }

            DialogResult = true;
            Close();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Executable (.exe)|*.exe"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            var path = dialog.FileName;
            var game = new ImportableProgram()
            {
                Icon = path,
                Name = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(path)).Name,
                Path = path,
                WorkDir = System.IO.Path.GetDirectoryName(path),
                Import = true
            };
            
            Programs.Add(game);
            ListPrograms.SelectedItem = game;
            ListPrograms.ScrollIntoView(game);
        }

        private void ButtonScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select folder to scan for executables..."
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ListPrograms.Visibility = Visibility.Hidden;
                TextProgresssing.Visibility = Visibility.Visible;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Programs = new ObservableCollection<ImportableProgram>(Playnite.Programs.GetExecutablesFromFolder(dialog.FileName, SearchOption.AllDirectories).Select(a => new ImportableProgram(a)).OrderBy(a => a.Name));
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Failed to scan folder for executables: " + dialog.FileName);
                    }
                    finally
                    {
                        ListPrograms.Dispatcher.Invoke(() =>
                        {
                            ListPrograms.Visibility = Visibility.Visible;
                            TextProgresssing.Visibility = Visibility.Hidden;
                        });
                    }
                    
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListPrograms.Visibility = Visibility.Hidden;
            TextProgresssing.Visibility = Visibility.Visible;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Programs = new ObservableCollection<ImportableProgram>(Playnite.Programs.GetInstalledPrograms().Select(a => new ImportableProgram(a)).OrderBy(a => a.Name));
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Failed to load list of installed apps.");
                }
                finally
                {
                    ListPrograms.Dispatcher.Invoke(() =>
                    {
                        ListPrograms.Visibility = Visibility.Visible;
                        TextProgresssing.Visibility = Visibility.Hidden;
                    });
                }
            });            
        }
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
}
