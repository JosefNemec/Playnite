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
using System.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class DesignMainViewModel : DesktopAppViewModel
    {
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
                return new NotificationMessage("1", "Some testing notification message.", NotificationType.Info);
            }
        }

        public DesignMainViewModel() : base(new InMemoryGameDatabase(), null, null, new ResourceProvider(), null)
        {
            ProgressStatus = "Status example in progress...";
            ProgressValue = 50;
            ProgressTotal = 100;
            ProgressActive = true;

            Game.DatabaseReference = Database;
            GameDatabase.GenerateSampleData(Database);
            var designGame = Database.Games.First();
            designGame.CoverImage = "pack://application:,,,/Playnite;component/Resources/Images/DesignCover.jpg";
            designGame.BackgroundImage = "pack://application:,,,/Playnite;component/Resources/Images/DesignBackground.jpg";
            designGame.Icon = "pack://application:,,,/Playnite;component/Resources/Images/DesignIcon.png";

            AppSettings = new PlayniteSettings();
            AppSettings.ExplorerPanelVisible = true;
            AppSettings.GridViewSideBarVisible = true;
            AppSettings.ShowNamesUnderCovers = true;
            AppSettings.ShowNameEmptyCover = true;
            AppSettings.ViewSettings.SelectedExplorerField = ExplorerField.LastActivity;

            Extensions = new ExtensionFactory(Database, new GameControllerFactory(), null);
            GamesView = new DesktopCollectionView(Database, AppSettings, Extensions);
            SelectedGames = new List<GamesCollectionViewEntry>() { GamesView.Items[0] };
        }
    }
}
