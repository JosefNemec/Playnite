using Playnite.FullscreenApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.API
{
    public class MainViewAPI : IMainViewAPI
    {
        private FullscreenAppViewModel mainModel;

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

        public MainViewAPI(FullscreenAppViewModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public bool OpenPluginSettings(Guid pluginId)
        {
            throw new NotSupportedException("Cannot open plugin settings in Fullscreen mode.");
        }
    }
}
