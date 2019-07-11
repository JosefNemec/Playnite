using Playnite.API;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class DesignMainViewModel : DesktopAppViewModel
    {
        public new GamesCollectionViewEntry SelectedGame { get; set; }
        public new IEnumerable<GamesCollectionViewEntry> SelectedGames { get; set; }

        public DesignMainViewModel()
        {
            ProgressStatus = "Status example in progress...";
            ProgressValue = 50;
            ProgressTotal = 100;
            ProgressVisible = true;

            var database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            var winPlatform = database.Platforms.Add("Windows");
            var designGame = new Game($"Star Wars: Knights of the Old Republic")
            {
                ReleaseDate = new DateTime(2009, 9, 5),
                PlatformId = winPlatform.Id,
                PlayCount = 20,
                Playtime = 115200,
                LastActivity = DateTime.Today,
                IsInstalled = true,
                Description = "Star Wars: Knights of the Old Republic (often abbreviated as KotOR) is the first installment in the Knights of the Old Republic series. KotOR is the first computer role-playing game set in the Star Wars universe."
            };

            database.Games.Add(designGame);
            designGame.CoverImage = "pack://application:,,,/Playnite;component/Resources/Images/DesignCover.jpg";
            designGame.BackgroundImage = "pack://application:,,,/Playnite;component/Resources/Images/DesignBackground.jpg";
            designGame.Icon = "pack://application:,,,/Playnite;component/Resources/Images/DesignIcon.png";

            AppSettings = new PlayniteSettings();
            AppSettings.ExplorerPanelVisible = true;
            AppSettings.GridViewSideBarVisible = true;
            AppSettings.ShowNamesUnderCovers = true;
            AppSettings.ShowNameEmptyCover = true;
            AppSettings.ViewSettings.SelectedExplorerField = GroupableField.LastActivity;

            Extensions = new ExtensionFactory(database, new GameControllerFactory());
            GamesView = new DesktopCollectionView(database, AppSettings, Extensions);

            SelectedGame = GamesView.Items[0];
            SelectedGames = new List<GamesCollectionViewEntry>() { SelectedGame };
            SelectedGameDetails = new GameDetailsViewModel(GamesView.Items[0], AppSettings);
            DatabaseExplorer = new DatabaseExplorer(database, Extensions, AppSettings);
            
            PlayniteApi = new PlayniteAPI(null, null, null, null, null, null, null, new NotificationsAPI());
            PlayniteApi.Notifications.Add(new NotificationMessage("1", "Some testing notification message.", NotificationType.Info));
            PlayniteApi.Notifications.Add(new NotificationMessage("2", "Some really long testing notification message that should be on more lines of text.", NotificationType.Error));
        }
    }
}
