using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Providers.BattleNet;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using Playnite.Providers.Uplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Events;

namespace Playnite.Providers
{
    public class GameControllerFactory : IDisposable
    {
        private GameDatabase database;
        public List<IGameController> Controllers
        {
            get; private set;
        }

        public event GameControllerEventHandler Starting;
        public event GameControllerEventHandler Started;
        public event GameControllerEventHandler Stopped;
        public event GameControllerEventHandler Uninstalled;
        public event GameControllerEventHandler Installed;

        public GameControllerFactory()
        {
            Controllers = new List<IGameController>();
        }

        public GameControllerFactory(GameDatabase database) : this()
        {
            this.database = database;
        }
        
        public void Dispose()
        {
            foreach (var controller in Controllers.ToList())
            {
                RemoveController(controller);
            }
        }

        public void AddController(IGameController controller)
        {
            controller.Installed += Controller_Installed;
            controller.Uninstalled += Controller_Uninstalled;
            controller.Started += Controller_Started;
            controller.Starting += Controller_Starting;
            controller.Stopped += Controller_Stopped;
            Controllers.Add(controller);
            database?.AddActiveController(controller);
        }

        public void RemoveController(int gameId)
        {
            var controller = Controllers.FirstOrDefault(a => a.Game.Id == gameId);
            if (controller != null)
            {
                RemoveController(controller);
            }
        }

        public void RemoveController(IGameController controller)
        {
            DisposeController(controller);
            Controllers.Remove(controller);
            database?.RemoveActiveController(controller.Game.Id);
        }

        public void DisposeController(IGameController controller)
        {
            controller.Installed -= Controller_Installed;
            controller.Uninstalled -= Controller_Uninstalled;
            controller.Started -= Controller_Started;
            controller.Starting -= Controller_Starting;
            controller.Stopped -= Controller_Stopped;
            controller.Dispose();
        }

        public IGameController GetController(int gameId)
        {
            return Controllers.FirstOrDefault(a => a.Game.Id == gameId);
        }

        private void Controller_Stopped(object sender, GameControllerEventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        private void Controller_Starting(object sender, GameControllerEventArgs e)
        {
            Starting?.Invoke(this, e);
        }

        private void Controller_Started(object sender, GameControllerEventArgs e)
        {
            Started?.Invoke(this, e);
        }

        private void Controller_Uninstalled(object sender, GameControllerEventArgs e)
        {
            Uninstalled?.Invoke(this, e);
        }

        private void Controller_Installed(object sender, GameControllerEventArgs e)
        {
            Installed?.Invoke(this, e);
        }

        public static IGameController GetGameBasedController(Game game, Settings settings)
        {
            //switch (game.Provider)
            //{
            //    case Provider.Custom:
            //        return new GenericGameController(game);
            //    case Provider.GOG:
            //        return new GogGameController(game, settings);
            //    case Provider.Origin:
            //        return new OriginGameController(game);
            //    case Provider.Steam:
            //        return new SteamGameController(game);
            //    case Provider.Uplay:
            //        return new UplayGameController(game);
            //    case Provider.BattleNet:
            //        return new BattleNetGameController(game);
            //}

            return null;
        }
    }
}
