using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.ViewModels
{
    public class GameStatusViewModel : ObservableObject
    {
        private string gameStatusText;
        public string GameStatusText
        {
            get => gameStatusText;
            set
            {
                gameStatusText = value;
                OnPropertyChanged();
            }
        }

        public GamesCollectionViewEntry Game { get; }

        public GameStatusViewModel(GamesCollectionViewEntry game)
        {
            Game = game;
        }
    }
}
