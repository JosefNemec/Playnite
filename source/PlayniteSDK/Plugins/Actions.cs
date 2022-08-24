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
    /// Type of automatic play action
    /// </summary>
    public enum AutomaticPlayActionType : int
    {
        /// <summary>
        ///
        /// </summary>
        File = 0,
        /// <summary>
        ///
        /// </summary>
        Url = 1
    }

    /// <summary>
    /// Represents controller for automatic handling of game startup.
    /// </summary>
    public sealed class AutomaticPlayController : PlayController
    {
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public AutomaticPlayActionType Type { get; set; }

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
        /// Gets or sets controller name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets game attached to a specific controller operation.
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

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }

    /// <summary>
    /// Represents installation controller.
    /// </summary>
    public abstract class InstallController : ControllerBase
    {
        internal event EventHandler<GameInstalledEventArgs> Installed;

        /// <summary>
        /// Creates new instance of <see cref="InstallController"/>.
        /// </summary>
        /// <param name="game"></param>
        public InstallController(Game game) : base(game)
        {
        }

        /// <summary>
        /// Start installation.
        /// </summary>
        public abstract void Install(InstallActionArgs args);

        /// <summary>
        /// Invoke to signal that installation completed.
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnInstalled(GameInstalledEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Installed?.Invoke(this, args), null);
        }
    }

    /// <summary>
    /// Represents uninstallation controller.
    /// </summary>
    public abstract class UninstallController : ControllerBase
    {
        internal event EventHandler<GameUninstalledEventArgs> Uninstalled;

        /// <summary>
        /// Creates new instance of <see cref="UninstallController"/>.
        /// </summary>
        /// <param name="game"></param>
        public UninstallController(Game game) : base(game)
        {
        }

        /// <summary>
        /// Start uninstallation.
        /// </summary>
        public abstract void Uninstall(UninstallActionArgs args);

        /// <summary>
        /// Invoke to signal that uninstallation completed.
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnUninstalled(GameUninstalledEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Uninstalled?.Invoke(this, args), null);
        }

        /// <summary>
        /// Invoke to signal that uninstallation completed.
        /// </summary>
        protected void InvokeOnUninstalled()
        {
            InvokeOnUninstalled(new GameUninstalledEventArgs());
        }
    }

    /// <summary>
    /// Represents play controller.
    /// </summary>
    public abstract class PlayController : ControllerBase
    {
        internal event EventHandler<GameStartedEventArgs> Started;
        internal event EventHandler<GameStoppedEventArgs> Stopped;

        /// <summary>
        /// Creates new instance of <see cref="PlayController"/>.
        /// </summary>
        /// <param name="game"></param>
        public PlayController(Game game) : base(game)
        {
        }

        /// <summary>
        /// Play game.
        /// </summary>
        public abstract void Play(PlayActionArgs args);

        /// <summary>
        /// Invoke to signal that game started running.
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStarted(GameStartedEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Started?.Invoke(this, args), null);
        }

        /// <summary>
        /// Invoke to signal that game started running.
        /// </summary>
        protected void InvokeOnStarted()
        {
            InvokeOnStarted(new GameStartedEventArgs());
        }

        /// <summary>
        /// Invoke to signal that game stopped running.
        /// </summary>
        /// <param name="args"></param>
        protected void InvokeOnStopped(GameStoppedEventArgs args)
        {
            args.Source = this;
            execContext.Send((a) => Stopped?.Invoke(this, args), null);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }

    /// <summary>
    /// Represents arguments for installation action.
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
    /// Represents arguments for uninstallation actions.
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
    /// Represents arguments for play action.
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
    /// Represents arguments for game started event.
    /// </summary>
    public class GameStartedEventArgs
    {
        internal PlayController Source { get; set; }

        /// <summary>
        /// Gets or sets started process ID.
        /// </summary>
        public int StartedProcessId { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameStartedEventArgs()
        {
        }
    }

    /// <summary>
    /// Represents arguments for game stopped event.
    /// </summary>
    public class GameStoppedEventArgs
    {
        internal PlayController Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        public ulong SessionLength { get; set; }

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
        public GameStoppedEventArgs(ulong sessionLength)
        {
            SessionLength = sessionLength;
        }
    }

    /// <summary>
    /// Represents arguments for game uninstalled event.
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
    /// Represents data for game installation.
    /// </summary>
    public class GameInstallationData
    {
        /// <summary>
        /// Gets or sets installation directory.
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Gets or sets Roms.
        /// </summary>
        public List<GameRom> Roms { get; set; }
    }

    /// <summary>
    /// Represents arguments for game installed event.
    /// </summary>
    public class GameInstalledEventArgs
    {
        internal InstallController Source { get; set; }

        /// <summary>
        /// Gets or sets data for newly installed game.
        /// </summary>
        public GameInstallationData InstalledInfo { get; set; }

        /// <summary>
        ///
        /// </summary>
        public GameInstalledEventArgs()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="installData"></param>
        public GameInstalledEventArgs(GameInstallationData installData)
        {
            InstalledInfo = installData;
        }
    }
}
