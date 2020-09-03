using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using Playnite.Plugins;

namespace Playnite.Controllers
{
    public class GameControllerFactory : IDisposable
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly GameDatabase database;

        public List<IGameController> Controllers
        {
            get; private set;
        }

        public event EventHandler<GameControllerEventArgs> Starting;
        public event EventHandler<GameControllerEventArgs> Started;
        public event EventHandler<GameControllerEventArgs> Stopped;
        public event EventHandler<GameControllerEventArgs> Uninstalled;
        public event EventHandler<GameInstalledEventArgs> Installed;

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
        }

        public void RemoveController(Guid gameId)
        {
            var controller = Controllers.FirstOrDefault(a => a.Game?.Id == gameId);
            if (controller != null)
            {
                RemoveController(controller);
            }
        }

        public void RemoveController(IGameController controller)
        {
            controller.Installed -= Controller_Installed;
            controller.Uninstalled -= Controller_Uninstalled;
            controller.Started -= Controller_Started;
            controller.Starting -= Controller_Starting;
            controller.Stopped -= Controller_Stopped;
            try
            {
                controller.Dispose();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to dispose game controller {controller.GetType()}");
            }

            Controllers.Remove(controller);
        }

        public IGameController GetController(Guid gameId)
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

        private void Controller_Installed(object sender, GameInstalledEventArgs e)
        {
            Installed?.Invoke(this, e);
        }

        public IGameController GetGameBasedController(Game game, ExtensionFactory extensions)
        {
            if (game.IsCustomGame)
            {
                return new GenericGameController(database, game);
            }
            else
            {
                if (extensions.Plugins.TryGetValue(game.PluginId, out var plugin))
                {
                    return ((LibraryPlugin)plugin.Plugin).GetGameController(game.GetClone()) ?? new GenericGameController(database, game);
                }
            }

            logger.Error($"Unable to find controller responsible for {game.Name} game.");
            return null;
        }

        public IGameController GetGenericGameController(Game game)
        {
            return new GenericGameController(database, game.GetClone());
        }
    }
}
