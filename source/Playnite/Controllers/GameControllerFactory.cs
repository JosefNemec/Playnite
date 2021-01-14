using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Plugins;

namespace Playnite.Controllers
{
    public class GameControllerFactory : IDisposable
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly GameDatabase database;

        public List<PlayController> PlayControllers { get; } = new List<PlayController>();
        public List<InstallController> InstallControllers { get; } = new List<InstallController>();

        public event EventHandler<GameStartingEventArgs> Starting;
        public event EventHandler<GameStartedEventArgs> Started;
        public event EventHandler<GameStoppedEventArgs> Stopped;
        public event EventHandler<GameUninstalledEventArgs> Uninstalled;
        public event EventHandler<GameInstalledEventArgs> Installed;

        public GameControllerFactory()
        {
        }

        public GameControllerFactory(GameDatabase database) : this()
        {
            this.database = database;
        }

        public void Dispose()
        {
            foreach (var controller in PlayControllers.ToList())
            {
                RemoveController(controller);
            }

            foreach (var controller in InstallControllers.ToList())
            {
                RemoveController(controller);
            }
        }

        public void AddController(PlayController controller)
        {
            controller.Started += Controller_Started;
            controller.Starting += Controller_Starting;
            controller.Stopped += Controller_Stopped;
            PlayControllers.Add(controller);
        }

        public void AddController(InstallController controller)
        {
            controller.Installed += Controller_Installed;
            controller.Uninstalled += Controller_Uninstalled;
            InstallControllers.Add(controller);
        }

        public void RemovePlayController(Guid gameId)
        {
            var controller = PlayControllers.FirstOrDefault(a => a.Game?.Id == gameId);
            if (controller != null)
            {
                RemoveController(controller);
            }
        }

        public void RemoveInstallController(Guid gameId)
        {
            var controller = InstallControllers.FirstOrDefault(a => a.Game?.Id == gameId);
            if (controller != null)
            {
                RemoveController(controller);
            }
        }

        public void RemoveController(PlayController controller)
        {
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

            PlayControllers.Remove(controller);
        }

        public void RemoveController(InstallController controller)
        {
            controller.Installed -= Controller_Installed;
            controller.Uninstalled -= Controller_Uninstalled;
            try
            {
                controller.Dispose();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to dispose game controller {controller.GetType()}");
            }

            InstallControllers.Remove(controller);
        }

        public PlayController GetPlayController(Guid gameId)
        {
            return PlayControllers.FirstOrDefault(a => a.Game.Id == gameId);
        }

        public InstallController GetInstallController(Guid gameId)
        {
            return InstallControllers.FirstOrDefault(a => a.Game.Id == gameId);
        }

        private void Controller_Stopped(object sender, GameStoppedEventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        private void Controller_Starting(object sender, GameStartingEventArgs e)
        {
            Starting?.Invoke(this, e);
        }

        private void Controller_Started(object sender, GameStartedEventArgs e)
        {
            Started?.Invoke(this, e);
        }

        private void Controller_Uninstalled(object sender, GameUninstalledEventArgs e)
        {
            Uninstalled?.Invoke(this, e);
        }

        private void Controller_Installed(object sender, GameInstalledEventArgs e)
        {
            Installed?.Invoke(this, e);
        }

        public Tuple<List<PlayAction>, List<GameAction>> GetPlayActions(Game game, ExtensionFactory extensions)
        {
            var plugActions = new List<PlayAction>();
            foreach (var plugin in extensions.Plugins.Values)
            {
                List<PlayAction> actions = null;
                try
                {
                    actions = plugin.Plugin.GetPlayActions(new GetPlayActionsArgs { Game = game });
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get play actions from {plugin.Description.Name}");
                    continue;
                }

                if (actions.HasItems())
                {
                    plugActions.AddRange(actions);
                }
            }

            var customActions = game.GameActions?.Where(a => a.IsPlayAction).ToList();
            return new Tuple<List<PlayAction>, List<GameAction>>(
                plugActions ?? new List<PlayAction>(),
                customActions ?? new List<GameAction>());
        }

        public List<InstallAction> GetInstallActions(Game game, ExtensionFactory extensions, bool isUninstall)
        {
            List<InstallAction> plugActions = null;
            var plugin = extensions.GetLibraryPlugin(game.PluginId);
            if (plugin != null)
            {
                plugActions = plugin.GetInstallActions(new GetInstallActionsArgs { Game = game, IsUninstall = isUninstall });
            }

            return plugActions ?? new List<InstallAction>();
        }
    }
}
