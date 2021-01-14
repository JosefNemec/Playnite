using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes game controller.
    /// </summary>
    [Obsolete("Not used anymore in Playnite 9.")]
    public interface IGameController : IDisposable
    {
        /// <summary>
        /// Gets value indicating wheter the game is running.
        /// </summary>
        bool IsGameRunning { get; }

        /// <summary>
        /// Gets game being handled.
        /// </summary>
        Game Game
        {
            get;
        }

        /// <summary>
        /// Installs game.
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstalls game.
        /// </summary>
        void Uninstall();

        /// <summary>
        /// Starts game.
        /// </summary>
        void Play();

        /// <summary>
        /// Occurs when game is being started.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Starting;

        /// <summary>
        /// Occurs when game is started.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Started;

        /// <summary>
        /// Occurs when game stops running.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Stopped;

        /// <summary>
        /// Occurs when game is finished uninstalling.
        /// </summary>
        event EventHandler<Events.GameControllerEventArgs> Uninstalled;

        /// <summary>
        /// Occurs when game is finished installing.
        /// </summary>
        event EventHandler<Events.GameInstalledEventArgs> Installed;
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class InstallController : IDisposable
    {
        private readonly SynchronizationContext execContext;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<GameUninstalledEventArgs> Uninstalled;
        /// <summary>
        ///
        /// </summary>
        public event EventHandler<GameInstalledEventArgs> Installed;
        /// <summary>
        ///
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        ///
        /// </summary>
        public InstallController()
        {
            execContext = SynchronizationContext.Current;
        }

        /// <summary>
        ///
        /// </summary>
        public abstract void Install();
        /// <summary>
        ///
        /// </summary>
        public abstract void Uninstall();

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InvokeOnUninstalled(object sender, GameUninstalledEventArgs args)
        {
            execContext.Send((a) => Uninstalled?.Invoke(sender, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InvokeOnInstalled(object sender, GameInstalledEventArgs args)
        {
            execContext.Send((a) => Installed?.Invoke(sender, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Dispose()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class PlayController : IDisposable
    {
        private readonly SynchronizationContext execContext;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler<GameStartingEventArgs> Starting;
        /// <summary>
        ///
        /// </summary>
        public event EventHandler<GameStartedEventArgs> Started;
        /// <summary>
        ///
        /// </summary>
        public event EventHandler<GameStoppedEventArgs> Stopped;

        /// <summary>
        ///
        /// </summary>
        public bool IsRunning { get; set; }
        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        ///
        /// </summary>
        public PlayController()
        {
            execContext = SynchronizationContext.Current;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        public abstract void Play(PlayAction action);

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InvokeOnStarting(object sender, GameStartingEventArgs args)
        {
            execContext.Send((a) => Starting?.Invoke(sender, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InvokeOnStarted(object sender, GameStartedEventArgs args)
        {
            execContext.Send((a) => Started?.Invoke(sender, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void InvokeOnStopped(object sender, GameStoppedEventArgs args)
        {
            execContext.Send((a) => Stopped?.Invoke(sender, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Dispose()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class InstallEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        public InstallController Controller { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public InstallEventArgs(InstallController controller)
        {
            Controller = controller;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PlayEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        public PlayController Controller { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public PlayEventArgs(PlayController controller)
        {
            Controller = controller;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStartingEventArgs : PlayEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public GameStartingEventArgs(PlayController controller) : base(controller)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStartedEventArgs : PlayEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public GameStartedEventArgs(PlayController controller) : base(controller)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStoppedEventArgs : PlayEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        public long SessionLength { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public GameStoppedEventArgs(PlayController controller) : base(controller)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameUninstalledEventArgs : InstallEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public GameUninstalledEventArgs(InstallController controller) : base(controller)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameInstalledEventArgs : InstallEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        public GameInfo InstalledInfo { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="controller"></param>
        public GameInstalledEventArgs(InstallController controller) : base(controller)
        {
        }
    }
}
