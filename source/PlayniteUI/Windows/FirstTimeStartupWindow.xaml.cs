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

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for FirstTimeStartupWindow.xaml
    /// </summary>
    public partial class FirstTimeStartupWindow : Window, INotifyPropertyChanged
    {
        #region Database
        public enum DbLocation
        {
            ProgramData,
            Custom
        }

        private DbLocation databaseLocation = DbLocation.ProgramData;
        public DbLocation DatabaseLocation
        {
            get
            {
                return databaseLocation;
            }

            set
            {
                databaseLocation = value;
                OnPropertyChanged("DatabaseLocation");
            }
        }

        private string databasePath;
        public string DatabasePath
        {
            get
            {
                return databasePath;
            }

            set
            {
                databasePath = value;
                OnPropertyChanged("DatabasePath");
            }
        }
        #endregion Database

        #region General
        private bool steamEnabled;
        public bool SteamEnabled
        {
            get
            {
                return steamEnabled;
            }

            set
            {
                steamEnabled = value;
                OnPropertyChanged("SteamEnabled");
            }
        }

        private bool gogEnabled;
        public bool GOGEnabled
        {
            get
            {
                return gogEnabled;
            }

            set
            {
                gogEnabled = value;
                OnPropertyChanged("GOGEnabled");
            }
        }

        private bool originEnabled;
        public bool OriginEnabled
        {
            get
            {
                return originEnabled;
            }

            set
            {
                originEnabled = value;
                OnPropertyChanged("OriginEnabled");
            }
        }
        #endregion General

        #region Steam
        private bool steamImportLibrary;
        public bool SteamImportLibrary
        {
            get
            {
                return steamImportLibrary;
            }

            set
            {
                steamImportLibrary = value;
                OnPropertyChanged("SteamImportLibrary");
            }
        }

        private string steamAccountName;
        public string SteamAccountName
        {
            get
            {
                return steamAccountName;
            }

            set
            {
                steamAccountName = value;
                OnPropertyChanged("SteamAccountName");
            }
        }
        #endregion Steam

        #region GOG
        private bool gogImportLibrary;
        public bool GogImportLibrary
        {
            get
            {
                return gogImportLibrary;
            }

            set
            {
                gogImportLibrary = value;
                OnPropertyChanged("GogImportLibrary");
            }
        }
        #endregion GOG

        #region Origin
        private bool originImportLibrary;
        public bool OriginImportLibrary
        {
            get
            {
                return originImportLibrary;
            }

            set
            {
                originImportLibrary = value;
                OnPropertyChanged("OriginImportLibrary");
            }
        }
        #endregion Origin

        private List<InstalledGameMetadata> importedGames = new List<InstalledGameMetadata>();
        public List<InstalledGameMetadata> ImportedGames
        {
            get
            {
                return importedGames;
            }
        }

        public FirstTimeStartupWindow()
        {
            InitializeComponent();
            SteamEnabled = true;
            GOGEnabled = true;
            OriginEnabled = true;
            SteamImportLibrary = false;
            SteamAccountName = string.Empty;
            GogImportLibrary = false;
            OriginImportLibrary = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            
            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "Steam" && !SteamEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }

            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "GOG" && !GOGEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }

            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "Origin" && !OriginEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            TabMain.SelectedIndex = TabMain.SelectedIndex - 1;

            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "Origin" && !OriginEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex - 1;
            }

            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "GOG" && !GOGEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex - 1;
            }

            if ((TabMain.SelectedItem as TabItem).Header.ToString() == "Steam" && !SteamEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex - 1;
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                ButtonBack.IsEnabled = TabMain.SelectedIndex != 0;

                if (TabMain.SelectedIndex == TabMain.Items.Count - 1)
                {
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonFinish.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonFinish.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ButtonImportGames_Click(object sender, RoutedEventArgs e)
        {
            var window = new InstalledGamesWindow()
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
            {
                importedGames = window.Games;
            }
        }

        private void ButtonGogAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            var api = new Playnite.Providers.GOG.WebApiClient();
            api.Login();
        }

        private void ButtonOriginAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            var api = new Playnite.Providers.Origin.WebApiClient();
            api.Login();
        }

        private void ButtonBrowserDbFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "Database file (*.db)|*.db",
                OverwritePrompt = false
            };

            if (dialog.ShowDialog(this) == true)
            {
                DatabasePath = dialog.FileName;
            }
        }
    }
}
