using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers
{
    public delegate void GameInstalledEventHandler(object sender, GameInstalledEventArgs args);

    public class GameInstalledEventArgs : EventArgs
    {
        public IGame NewGame
        {
            get; set;
        }

        public GameInstalledEventArgs()
        {
        }

        public GameInstalledEventArgs(IGame game)
        {
            NewGame = game;
        }
    }
}
