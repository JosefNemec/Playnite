using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    ///
    /// </summary>
    public enum GenericPlayActionType
    {
        /// <summary>
        ///
        /// </summary>
        File,
        /// <summary>
        ///
        /// </summary>
        Url
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class AutomaticPlayController : PlayController
    {
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public GenericPlayActionType Type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TrackingMode TrackingMode { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string TrackingPath { get; set; }

        /// <summary>
        /// Gets or sets executable arguments for File type tasks.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets executable path for File action type or URL for URL action type.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets working directory for File action type executable.
        /// </summary>
        public string WorkingDir { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        public AutomaticPlayController(Game game) : base(game)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public override void Play(PlayActionArgs args)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class ControllerBase : IDisposable
    {
        internal SynchronizationContext execContext;

        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        ///
        /// </summary>
        public ControllerBase(Game game)
        {
            execContext = SynchronizationContext.Current;
            Game = game;
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
    public abstract class InstallController : ControllerBase
    {
        /// <summary>
        ///
        /// </summary>
        internal event EventHandler<GameInstalledEventArgs> Installed;

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        public InstallController(Game game) : base(game)
        {
        }

        /// <summary>
        ///
        /// </summary>
        public abstract void Install(InstallActionArgs args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnInstalled(GameInstalledEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Installed?.Invoke(this, args), null);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class UninstallController : ControllerBase
    {
        /// <summary>
        ///
        /// </summary>
        internal event EventHandler<GameUninstalledEventArgs> Uninstalled;

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        public UninstallController(Game game) : base(game)
        {
        }

        /// <summary>
        ///
        /// </summary>
        public abstract void Uninstall(UninstallActionArgs args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnUninstalled(GameUninstalledEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Uninstalled?.Invoke(this, args), null);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class PlayController : ControllerBase
    {
        /// <summary>
        ///
        /// </summary>
        internal event EventHandler<GameStartingEventArgs> Starting;
        /// <summary>
        ///
        /// </summary>
        internal event EventHandler<GameStartedEventArgs> Started;
        /// <summary>
        ///
        /// </summary>
        internal event EventHandler<GameStoppedEventArgs> Stopped;

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        public PlayController(Game game) : base(game)
        {
        }

        /// <summary>
        ///
        /// </summary>
        public abstract void Play(PlayActionArgs args);

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStarting(GameStartingEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Starting?.Invoke(this, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStarted(GameStartedEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Started?.Invoke(this, args), null);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStopped(GameStoppedEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Stopped?.Invoke(this, args), null);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class InstallActionArgs
    {
        /// <summary>
        ///
        /// </summary>
        public InstallActionArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class UninstallActionArgs
    {
        /// <summary>
        ///
        /// </summary>
        public UninstallActionArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PlayActionArgs
    {
        /// <summary>
        ///
        /// </summary>
        public PlayActionArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStartingEventArgs
    {
        internal PlayController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameStartingEventArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStartedEventArgs
    {
        internal PlayController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameStartedEventArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameStoppedEventArgs
    {
        internal PlayController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public long SessionLength { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameStoppedEventArgs()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionLength"></param>
        public GameStoppedEventArgs(long sessionLength)
        {
            SessionLength = sessionLength;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameUninstalledEventArgs
    {
        internal UninstallController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameUninstalledEventArgs()
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class GameInstalledEventArgs
    {
        internal InstallController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameInfo InstalledInfo { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameInstalledEventArgs()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="installedInfo"></param>
        public GameInstalledEventArgs(GameInfo installedInfo)
        {
            InstalledInfo = installedInfo;
        }
    }
}
