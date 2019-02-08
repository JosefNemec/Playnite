using Playnite;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary
{
    class GameController : IGameController
    {
        public bool IsGameRunning => throw new NotImplementedException();

        public Game Game { get; private set; }

        public event GameControllerEventHandler Starting;
        public event GameControllerEventHandler Started;
        public event GameControllerEventHandler Stopped;
        public event GameControllerEventHandler Uninstalled;
        public event GameControllerEventHandler Installed;

        public GameController(Game game)
        {
            this.Game = game;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Install()
        {
            ProcessStarter.StartUrl(this.Game.GameImagePath);
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Uninstall()
        {
            throw new NotImplementedException();
        }
    }
}
