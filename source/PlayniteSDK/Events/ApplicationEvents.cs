using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Application wide events.
    /// </summary>
    public enum ApplicationEvent
    {
        /// <summary>
        ///
        /// </summary>
        OnApplicationStarted,
        /// <summary>
        ///
        /// </summary>
        OnApplicationStopped,
        /// <summary>
        ///
        /// </summary>
        OnLibraryUpdated,
        /// <summary>
        ///
        /// </summary>
        OnGameStarting,
        /// <summary>
        ///
        /// </summary>
        OnGameStarted,
        /// <summary>
        ///
        /// </summary>
        OnGameStopped,
        /// <summary>
        ///
        /// </summary>
        OnGameInstalled,
        /// <summary>
        ///
        /// </summary>
        OnGameUninstalled,
        /// <summary>
        ///
        /// </summary>
        OnGameSelected,
        /// <summary>
        ///
        /// </summary>
        OnGameStartupCancelled
    }

    /// <summary>
    /// Represents game selection change event.
    /// </summary>
    public class OnGameSelectedEventArgs
    {
        /// <summary>
        /// Gets previously selected games.
        /// </summary>
        public List<Game> OldValue { get; internal set; }

        /// <summary>
        /// Gets newly selected games.
        /// </summary>
        public List<Game> NewValue { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="OnGameSelectedEventArgs"/>.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public OnGameSelectedEventArgs(List<Game> oldValue, List<Game> newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Represents arguments for the event when a game is starting.
    /// </summary>
    public class OnGameStartingEventArgs
    {
        /// <summary>
        /// Gets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// Gets custom game action used to start the game.
        /// </summary>
        public GameAction SourceAction { get; internal set; }

        /// <summary>
        /// Gets ROM file selected when running a game with multiple ROMs assigned.
        /// </summary>
        public string SelectedRomFile { get; internal set; }

        /// <summary>
        /// Gets or sets value indicating whether game startup should be interrupted.
        /// </summary>
        public bool CancelStartup { get; set; }
    }

    /// <summary>
    /// Represents arguments for the event when a game starts running.
    /// </summary>
    public class OnGameStartedEventArgs
    {
        /// <summary>
        /// Gets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// Gets custom game action used to start the game.
        /// </summary>
        public GameAction SourceAction { get; internal set; }

        /// <summary>
        /// Gets ROM file selected when running a game with multiple ROMs assigned.
        /// </summary>
        public string SelectedRomFile { get; internal set; }

        /// <summary>
        /// Gets started process ID. Might not be valid for all started games depending on how the game was started.
        /// </summary>
        public int StartedProcessId { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for the event when a game is installed.
    /// </summary>
    public class OnGameStartupCancelledEventArgs
    {
        /// <summary>
        /// Gets or sets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for the event when a game stops running.
    /// </summary>
    public class OnGameStoppedEventArgs
    {
        /// <summary>
        /// Gets or sets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// Gets or sets length of the game session in seconds.
        /// </summary>
        public ulong ElapsedSeconds { get; internal set; }

        /// <summary>
        /// Gets value indicated whether game tracking was manually stopped by a user.
        /// </summary>
        public bool ManuallyStopped { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for the event when a game is installed.
    /// </summary>
    public class OnGameInstalledEventArgs
    {
        /// <summary>
        /// Gets or sets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for the event when a game is uninstalled.
    /// </summary>
    public class OnGameUninstalledEventArgs
    {
        /// <summary>
        /// Gets or sets game object initiating the event.
        /// </summary>
        public Game Game { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for the event when Playnite is started.
    /// </summary>
    public class OnApplicationStartedEventArgs
    {
    }

    /// <summary>
    /// Represents arguments for the event when Playnite is closing down.
    /// </summary>
    public class OnApplicationStoppedEventArgs
    {
    }

    /// <summary>
    /// Represents arguments for the event when the game library is updated.
    /// </summary>
    public class OnLibraryUpdatedEventArgs
    {
    }
}
