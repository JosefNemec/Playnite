using Playnite.API;
using Playnite.Common;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.FullscreenApp.Markup;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.ViewModels
{
    public class DesignMainViewModel : FullscreenAppViewModel
    {
        public new GamesCollectionViewEntry GameDetailsEntry { get; set; }
        public new GamesCollectionViewEntry SelectedGame { get; set; }
        public new bool GameDetailsButtonVisible { get; set; } = true;
        public new bool IsExtraFilterActive { get; set; } = true;

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
            MainMenuVisible = false;
            GameMenuVisible = false;
            SettingsMenuVisible  = false;
            GameListVisible = true;
            GameDetailsVisible  = false;
            FilterPanelVisible = true;
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

            GamesView = new FullscreenCollectionView(
                database,
                new PlayniteSettings(),
                new ExtensionFactory(database, new GameControllerFactory()));

            GameDetailsEntry = GamesView.Items[0];
            SelectedGame = GamesView.Items[0];
            SelectedGameDetails = new GameDetailsViewModel(GamesView.Items[0]);

            MainMenuVisible = false;
            SettingsMenuVisible = false;
            AppSettings = new PlayniteSettings();
            AppSettings.Fullscreen.ShowBattery = true;
            AppSettings.Fullscreen.ShowBatteryPercentage = true;
            AppSettings.Fullscreen.ShowClock = true;
            PlayniteApi = new PlayniteAPI(null, null, null, null, null, null, null, new NotificationsAPI(), null, null);
            PlayniteApi.Notifications.Add(new NotificationMessage("1", "Some testing notification message.", NotificationType.Info));
            PlayniteApi.Notifications.Add(new NotificationMessage("2", "Some really long testing notification message that should be on more lines of text.", NotificationType.Error));
        }
    }
}
