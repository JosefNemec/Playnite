using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.API
{
    public class MainViewAPI : IMainViewAPI
    {
        private MainViewModel mainModel;

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

        public MainViewAPI(MainViewModel mainModel)
        {
            this.mainModel = mainModel;
        }
    }
}
