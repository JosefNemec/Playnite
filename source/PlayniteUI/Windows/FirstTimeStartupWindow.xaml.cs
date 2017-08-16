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

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for FirstTimeStartupWindow.xaml
    /// </summary>
    public partial class FirstTimeStartupWindow : WindowBase, INotifyPropertyChanged
    {
        private string loginReuiredMessage = "Login Required";
        private string loginOKMessage = "OK";

        private Playnite.Providers.GOG.WebApiClient gogApiClient = new Playnite.Providers.GOG.WebApiClient();

        public string GogLoginStatus
        {
            get
            {
                if (gogApiClient.GetLoginRequired())
                {
                    return loginReuiredMessage;
                }
                else
                {
                    return loginOKMessage;
                }
            }
        }

        private Playnite.Providers.Origin.WebApiClient originApiClient = new Playnite.Providers.Origin.WebApiClient();

        public string OriginLoginStatus
        {
            get
            {
                if (originApiClient.GetLoginRequired())
                {
                    return loginReuiredMessage;
                }
                else
                {
                    return loginOKMessage;
                }
            }
        }

        public List<LocalSteamUser> SteamUsers
        {
            get
            {
                return new SteamLibrary().GetSteamUsers();
            }
        }

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

        public bool SteamImportCategories
        {
            get; set;
        } = true;

        public bool SteamImportLibByName
        {
            get; set;
        } = false;

        public ulong SteamIdLibImport
        {
            get; set;
        } = 0;

        public ulong SteamIdCategoryImport
        {
            get; set;
        } = 0;
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

        private string selectedPage
        {
            get
            {
                return (TabMain.SelectedItem as TabItem).Header.ToString();
            }
        }

        private Dictionary<string, Func<FirstTimeStartupWindow, bool>> pageValidators = new Dictionary<string, Func<FirstTimeStartupWindow,bool>>()
        {
            {
                "Steam", (window) =>
                {
                    if (window.SteamImportLibrary)
                    {
                        if (window.SteamImportLibByName && string.IsNullOrEmpty(window.SteamAccountName))
                        {
                            PlayniteMessageBox.Show("Steam account name cannot be empty.", "Wrong settings data", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        if (!window.SteamImportLibByName && window.SteamIdLibImport == 0)
                        {
                            PlayniteMessageBox.Show("No Steam account selected for library import.", "Wrong settings data", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }

                    if (window.SteamImportCategories && window.SteamIdCategoryImport == 0)
                    {
                        PlayniteMessageBox.Show("No Steam account selected for category import.", "Wrong settings data", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    return true;
                }
            }
        };

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
            if (pageValidators.ContainsKey(selectedPage))
            {
                if (pageValidators[selectedPage](this) == false)
                {
                    return;
                }
            }

            TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            
            if (selectedPage == "Steam" && !SteamEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }

            if (selectedPage == "GOG" && !GOGEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }

            if (selectedPage == "Origin" && !OriginEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex + 1;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            TabMain.SelectedIndex = TabMain.SelectedIndex - 1;

            if (selectedPage == "Origin" && !OriginEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex - 1;
            }

            if (selectedPage == "GOG" && !GOGEnabled)
            {
                TabMain.SelectedIndex = TabMain.SelectedIndex - 1;
            }

            if (selectedPage == "Steam" && !SteamEnabled)
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
            gogApiClient.Login();
            OnPropertyChanged("GogLoginStatus");
        }

        private void ButtonOriginAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            originApiClient.Login();
            OnPropertyChanged("OriginLoginStatus");
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
