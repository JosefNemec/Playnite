using Playnite.Database;
using Playnite.Models;
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

namespace Playnite.Providers
{
    public class GameState
    {
        public bool Installed
        {
            get; set;
        }

        public bool Launching
        {
            get; set;
        }

        public bool Running
        {
            get; set;
        }

        public bool Installing
        {
            get; set;
        }

        public bool Uninstalling
        {
            get; set;
        }

        public void SetState(bool? installed, bool? running, bool? installing, bool? uninstalling, bool? launching)
        {
            if (installed != null)
            {
                Installed = installed.Value;
            }

            if (running != null)
            {
                Running = running.Value;
            }

            if (installing != null)
            {
                Installing = installing.Value;
            }

            if (uninstalling != null)
            {
                Uninstalling = uninstalling.Value;
            }

            if (launching != null)
            {
                Launching = launching.Value;
            }
        }

        public override string ToString()
        {
            var inst = Installed ? 1 : 0;
            var run = Running ? 1 : 0;
            var installing = Installing ? 1 : 0;
            var uninstalling = Uninstalling ? 1 : 0;
            var launch = Launching ? 1 : 0;
            return $"Inst:{inst}; Run:{run}; Instl:{installing}; Uninst:{uninstalling}; Lnch:{launch}";
        }
    }

    public class GameControllerFactory : IDisposable
    {
        private GameDatabase database;
        public List<IGameController> Controllers
        {
            get; private set;
        }
                
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
            foreach (var controller in Controllers)
            {
                DisposeController(controller);
                database?.RemoveActiveController(controller.Game.Id);
            }

            Controllers = null;
        }

        public void AddController(IGameController controller)
        {
            controller.Installed += Controller_Installed;
            controller.Uninstalled += Controller_Uninstalled;
            controller.Started += Controller_Started;
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

        public static IGameController GetGameBasedController(IGame game, Settings settings)
        {
            switch (game.Provider)
            {
                case Provider.Custom:
                    return new GenericGameController(game as Game);
                case Provider.GOG:
                    return new GogGameController(game as Game, settings);
                case Provider.Origin:
                    return new OriginGameController(game as Game);
                case Provider.Steam:
                    return new SteamGameController(game as Game);
                case Provider.Uplay:
                    return new UplayGameController(game as Game);
                case Provider.BattleNet:
                    return new BattleNetGameController(game as Game);
            }

            return null;
        }
    }
}
