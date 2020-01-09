using Playnite.API;
using Playnite.Common;
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

        private static DesignMainViewModel designIntance;
        public static DesignMainViewModel DesignIntance
        {
            get
            {
                if (!DesignerTools.IsInDesignMode)
                {
                    return null;
                }
                else
                {
                    if (designIntance == null)
                    {
                        designIntance = new DesignMainViewModel();
                    }

                    return designIntance;
                }
            }
        }

        public static GameDetailsViewModel DesignSelectedGameDetailsIntance
        {
            get
            {
                return DesignIntance?.SelectedGameDetails;
            }
        }

        public static GamesCollectionViewEntry DesignSelectedGameIntance
        {
            get
            {
                return DesignIntance?.SelectedGame;
            }
        }

        public static NotificationMessage DesignNotificationIntance
        {
            get
            {
                return DesignIntance?.PlayniteApi.Notifications.Messages[0];
            }
        }

        public DesignMainViewModel()
        {
            ProgressStatus = "Status example in progress...";
            ProgressValue = 50;
            ProgressTotal = 100;
            ProgressVisible = true;

            var database = new InMemoryGameDatabase();
            Game.DatabaseReference = database;
            GameDatabase.GenerateSampleData(database);
            var designGame = database.Games.First();
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
            
            PlayniteApi = new PlayniteAPI(null, null, null, null, null, null, null, new NotificationsAPI(), null, null);
            PlayniteApi.Notifications.Add(new NotificationMessage("1", "Some testing notification message.", NotificationType.Info));
            PlayniteApi.Notifications.Add(new NotificationMessage("2", "Some really long testing notification message that should be on more lines of text.", NotificationType.Error));
        }
    }
}
