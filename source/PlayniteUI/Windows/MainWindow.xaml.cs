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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Threading;
using NLog;
using Playnite;
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Models;
using System.Collections.ObjectModel;
using PlayniteUI.Windows;
using Playnite.Database;
using Playnite.Providers.Origin;
using PlayniteUI.Controls;
using System.Globalization;
using Playnite.Services;
using Playnite.Providers.Uplay;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {                        
            //MenuMainMenu.DataContext = this;
            //MenuViewSettings.DataContext = Config;
            //FilterSelector.DataContext = new FilterSelectorConfig(gamesStats, Config.FilterSettings);
            //CheckFilterView.DataContext = Config.FilterSettings;
            //GridGamesView.AppSettings = Config;

            //if (!Config.FirstTimeWizardComplete)
            //{
            //    var window = new FirstTimeStartupWindow()
            //    {
            //        Owner = this
            //    };

            //    if (window.ShowDialog() == true)
            //    {
            //        config.SteamSettings.IdSource = window.SteamImportLibByName ? SteamIdSource.Name : SteamIdSource.LocalUser;
            //        Config.SteamSettings.AccountId = window.SteamIdLibImport;
            //        Config.SteamSettings.IntegrationEnabled = window.SteamEnabled;
            //        Config.SteamSettings.LibraryDownloadEnabled = window.SteamImportLibrary;
            //        Config.SteamSettings.AccountName = window.SteamAccountName;
            //        Config.GOGSettings.IntegrationEnabled = window.GOGEnabled;
            //        Config.GOGSettings.LibraryDownloadEnabled = window.GogImportLibrary;
            //        Config.OriginSettings.IntegrationEnabled = window.OriginEnabled;
            //        Config.OriginSettings.LibraryDownloadEnabled = window.OriginImportLibrary;
            //        Config.UplaySettings.IntegrationEnabled = window.UplayEnabled;
            //        Config.BattleNetSettings.IntegrationEnabled = window.BattleNetEnabled;
            //        Config.BattleNetSettings.LibraryDownloadEnabled = window.BattleNetImportLibrary;
            //        importSteamCatWizard = window.SteamImportCategories;
            //        importSteamCatWizardId = window.SteamIdCategoryImport;

            //        if (window.DatabaseLocation == FirstTimeStartupWindow.DbLocation.Custom)
            //        {
            //            Config.DatabasePath = window.DatabasePath;
            //        }
            //        else
            //        {
            //            FileSystem.CreateFolder(Paths.UserProgramDataPath);
            //            Config.DatabasePath = System.IO.Path.Combine(Paths.UserProgramDataPath, "games.db");
            //        }

            //        GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
            //        AddInstalledGames(window.ImportedGames);

            //        Config.FirstTimeWizardComplete = true;
            //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Config"));
            //    }
            //}            

            //if (!Config.EmulatorWizardComplete)
            //{
            //    GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
            //    var window = new EmulatorImportWindow(DialogType.Wizard)
            //    {
            //        Owner = this
            //    };

            //    if (window.ShowDialog() == true)
            //    {
            //        Config.EmulatorWizardComplete = true;
            //    }
            //}

            //if (!Config.MigrationV2PcPlatformAdded)
            //{
            //    GameDatabase.Instance.OpenDatabase(Config.DatabasePath);
            //    var db = GameDatabase.Instance;
            //    foreach (var game in db.GamesCollection.Find(a => a.Provider != Provider.Custom).ToList())
            //    {
            //        db.AssignPcPlatform(game);
            //        db.UpdateGameInDatabase(game);
            //    }

            //    Config.MigrationV2PcPlatformAdded = true;                
            //}

            //LoadGames(Config.UpdateLibStartup);
            //CheckUpdate();
            //SendUsageData();
            //Focus();
        }
    }

    public class MainWindowFactory : WindowFactory
    {
        public static MainWindowFactory Instance
        {
            get => new MainWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new MainWindow();
        }
    }
}
