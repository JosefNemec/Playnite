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
        public Game Game { get; }

        /// <summary>
        ///
        /// </summary>
        public InstallController(Game game)
        {
            execContext = SynchronizationContext.Current;
            Game = game;
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
        /// <param name="args"></param>
        protected void InvokeOnUninstalled(GameUninstalledEventArgs args)
        {
            args.Controller = this;
            execContext.Send((a) => Uninstalled?.Invoke(this, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnInstalled(GameInstalledEventArgs args)
        {
            args.Controller = this;
            execContext.Send((a) => Installed?.Invoke(this, args), null);
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
        public Game Game { get; }

        /// <summary>
        ///
        /// </summary>
        public PlayController(Game game)
        {
            execContext = SynchronizationContext.Current;
            Game = game;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        public abstract void Play(PlayAction action);

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStarting(GameStartingEventArgs args)
        {
            args.Controller = this;
            execContext.Send((a) => Starting?.Invoke(this, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStarted(GameStartedEventArgs args)
        {
            args.Controller = this;
            execContext.Send((a) => Started?.Invoke(this, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStopped(GameStoppedEventArgs args)
        {
            args.Controller = this;
            execContext.Send((a) => Stopped?.Invoke(this, args), null);
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
        internal InstallController Controller { get; set; }

        /// <summary>
        ///
        /// </summary>
        public InstallEventArgs()
        {
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
        internal PlayController Controller { get; set; }

        /// <summary>
        ///
        /// </summary>
        public PlayEventArgs()
        {
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
        public GameStartingEventArgs() : base()
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
        public GameStartedEventArgs() : base()
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
        public GameStoppedEventArgs() : base()
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
        public GameUninstalledEventArgs() : base()
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
        public GameInstalledEventArgs() : base()
        {
        }
    }
}
