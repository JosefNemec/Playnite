using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.API
{
    public class MainViewAPI : IMainViewAPI
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private DesktopAppViewModel mainModel;

        public IEnumerable<Game> SelectedGames
        {
            get
            {
                if (mainModel.SelectedGames == null && mainModel.SelectedGame != null)
                {
                    return new List<Game>() { mainModel.SelectedGame.Game };
                }
                else
                {
                    return mainModel.SelectedGames?.Select(a => a.Game).ToList();
                }
            }
        }

        public DesktopView ActiveDesktopView => (DesktopView)mainModel.AppSettings.ViewSettings.GamesViewType;

        public List<Game> FilteredGames => mainModel.GamesView.CollectionView.Cast<GamesCollectionViewEntry>().Select(a => a.Game).Distinct().ToList();

        public MainViewAPI(DesktopAppViewModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public bool OpenPluginSettings(Guid pluginId)
        {
            return mainModel.OpenPluginSettings(pluginId);
        }

        public void SwitchToLibraryView()
        {
            mainModel.SwitchToLibraryView();
        }

        public void SelectGame(Guid gameId)
        {
            var game = mainModel.Database.Games.Get(gameId);
            if (game == null)
            {
                logger.Error($"Can't select game, game ID {gameId} not found.");
            }
            else
            {
                mainModel.SelectGame(game.Id);
            }
        }

        public void SelectGames(IEnumerable<Guid> gameIds)
        {
            mainModel.SelectGames(gameIds);
        }
    }
}
