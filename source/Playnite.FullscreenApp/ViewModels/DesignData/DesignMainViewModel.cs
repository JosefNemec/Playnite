using Playnite.API;
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

namespace Playnite.FullscreenApp.ViewModels.DesignData
{
    public class DesignMainViewModel : FullscreenAppViewModel
    {
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
            for (int i = 0; i < 25; i++)
            {
                database.Games.Add(new Game($"Test Game {i}")
                {
                    //Icon = ThemeFile.GetFilePath("Images/custom_cover_background.png", ThemeFile.GetDesignTimeDefaultTheme())
                });
            }

            GamesView = new FullscreenCollectionView(
                database,
                new PlayniteSettings(),
                new ExtensionFactory(database, new GameControllerFactory()));

            MainMenuVisible = false;
            SettingsMenuVisible = false;
            AppSettings = new PlayniteSettings();
            AppSettings.Fullscreen.ShowBattery = true;
            AppSettings.Fullscreen.ShowBatteryPercentage = true;
            AppSettings.Fullscreen.ShowClock = true;
            PlayniteApi = new PlayniteAPI(null, null, null, null, null, null, null, new NotificationsAPI());
            PlayniteApi.Notifications.Add(new NotificationMessage("1", "Some testing notification message.", NotificationType.Info));
            PlayniteApi.Notifications.Add(new NotificationMessage("1", "Some really long testing notification message that should be on more lines of text.", NotificationType.Error));
        }
    }
}
