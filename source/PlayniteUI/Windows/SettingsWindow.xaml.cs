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
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using Playnite.Providers.Steam;
using Playnite.Database;
using NLog;
using PlayniteUI.Controls;
using Playnite;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class SettingsWindow : WindowBase, INotifyPropertyChanged
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

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

        private bool providerIntegrationChanged = false;
        public bool ProviderIntegrationChanged
        {
            get
            {
                return providerIntegrationChanged;
            }
        }

        private bool databaseLocationChanged = false;
        public bool DatabaseLocationChanged
        {
            get
            {
                return databaseLocationChanged;
            }
        }

        public List<LocalSteamUser> SteamUsers
        {
            get
            {
                return new SteamLibrary().GetSteamUsers();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if ((RadioLibrarySteam.IsChecked == true && RadioSteamLibName.IsChecked == true) && string.IsNullOrEmpty(TextSteamAccountName.Text))
            {
                PlayniteMessageBox.Show(FindResource("SettingsInvalidSteamAccountName") as string, FindResource("InvalidDataTitle") as string, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (RadioLibrarySteam.IsChecked == true && RadioSteamLibAccount.IsChecked == true && ((ulong)ComboSteamAccount.SelectedValue) == 0)
            {
                PlayniteMessageBox.Show(FindResource("SettingsInvalidSteamAccountLibImport") as string, FindResource("InvalidDataTitle") as string, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Paths.GetValidFilePath(TextDatabase.Text))
            {
                PlayniteMessageBox.Show(FindResource("SettingsInvalidDBLocation") as string, FindResource("InvalidDataTitle") as string, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var databasePath = TextDatabase.GetBindingExpression(TextBox.TextProperty);
            if (databasePath.IsDirty)
            {
                databaseLocationChanged = true;
                databasePath.UpdateSource();
            }

            var steamEnabled = CheckSteamEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (steamEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                steamEnabled.UpdateSource();
            }

            var steamDownloadLib = RadioLibrarySteam.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (steamDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                steamDownloadLib.UpdateSource();
            }

            RadioButton radioSteam = RadioSteamLibName.IsChecked == true ? RadioSteamLibName : RadioSteamLibAccount;
            var steamIdSource = radioSteam.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (steamIdSource.IsDirty)
            {
                providerIntegrationChanged = true;
                steamIdSource.UpdateSource();
            }

            var steamAccount = ComboSteamAccount.GetBindingExpression(ComboBox.SelectedValueProperty);
            if (steamAccount.IsDirty)
            {
                providerIntegrationChanged = true;
                steamAccount.UpdateSource();
            }

            var steamAccountName = TextSteamAccountName.GetBindingExpression(TextBox.TextProperty);
            if (steamAccountName.IsDirty)
            {
                providerIntegrationChanged = true;
                steamAccountName.UpdateSource();
            }

            var gogEnabled = CheckGogEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                gogEnabled.UpdateSource();
            }

            var gogRunGalaxy = CheckGogRunGalaxy.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogRunGalaxy.IsDirty)
            {
                gogRunGalaxy.UpdateSource();
            }

            var gogIcons = CheckGogIcons.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (gogIcons.IsDirty)
            {
                gogIcons.UpdateSource();
            }

            var gogDownloadLib = RadioLibraryGOG.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (gogDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                gogDownloadLib.UpdateSource();
            }

            var originEnabled = CheckOriginEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (originEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                originEnabled.UpdateSource();
            }

            var originDownloadLib = RadioLibraryOrigin.GetBindingExpression(RadioButton.IsCheckedProperty);
            if (originDownloadLib.IsDirty)
            {
                providerIntegrationChanged = true;
                originDownloadLib.UpdateSource();
            }

            var uplayEnabled = CheckUplayEnabled.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (uplayEnabled.IsDirty)
            {
                providerIntegrationChanged = true;
                uplayEnabled.UpdateSource();
            }

            var trayMinimize = CheckMinimizeToTray.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (trayMinimize.IsDirty)
            {
                trayMinimize.UpdateSource();
            }

            var trayClose = CheckCloseToTray.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (trayClose.IsDirty)
            {
                trayClose.UpdateSource();
            }

            var trayEnable = CheckEnableTray.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (trayEnable.IsDirty)
            {
                trayEnable.UpdateSource();
            }

            var language = ComboLanguage.GetBindingExpression(ComboBox.SelectedValueProperty);
            if (language.IsDirty)
            {
                language.UpdateSource();
            }

            var minimizeLaunch = CheckMinimizeLaunch.GetBindingExpression(CheckBox.IsCheckedProperty);
            if (minimizeLaunch.IsDirty)
            {
                minimizeLaunch.UpdateSource();
            }

            Playnite.Settings.Instance.SaveSettings();
            Close();
        }

        private void ButtonGogAuth_Click(object sender, RoutedEventArgs e)
        {
            if (gogApiClient.Login(this))
            {
                providerIntegrationChanged = true;
            }

            OnPropertyChanged("GogLoginStatus");
        }

        private void ButtonOriginAuth_Click(object sender, RoutedEventArgs e)
        {
            if (originApiClient.Login(this))
            {
                providerIntegrationChanged = true;
            }

            OnPropertyChanged("OriginLoginStatus");
        }

        private void ButtonBrowserDbFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Database file (*.db)|*.db"
            };

            if (dialog.ShowDialog(this) == true)
            {
                TextDatabase.Text = dialog.FileName;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ButtonImportSteamCategories_Click(object sender, RoutedEventArgs e)
        {
            if (PlayniteMessageBox.Show("This will overwrite current categories on all Steam games. Do you want to continue?",
                "Import Categories?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            if (ComboSteamCatImport.SelectedValue == null)
            {
                PlayniteMessageBox.Show("Cannot import categories, account for import is not selected.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var userId = (ulong)ComboSteamCatImport.SelectedValue;
                var steamLib = new SteamLibrary();
                var games = steamLib.GetCategorizedGames(userId);

                if (GameDatabase.Instance.GamesCollection == null)
                {
                    throw new Exception("Playnite database is not opened.");
                }

                GameDatabase.Instance.ImportCategories(games);
                PlayniteMessageBox.Show("Import finished.", "Import Successful");
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Failed to import Steam categories.");
                PlayniteMessageBox.Show("Failed to import Steam categories: " + exc.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
