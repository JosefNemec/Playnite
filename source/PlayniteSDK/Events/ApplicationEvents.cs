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
        OnGameSelected
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

    public class OnGameStartingEventArgs
    {
        public Game Game { get; internal set; }
    }

    public class OnGameStartedEventArgs
    {
        public Game Game { get; internal set; }
    }

    public class OnGameStoppedEventArgs
    {
        public Game Game { get; internal set; }
        public ulong EllapsedSeconds { get; internal set; }
    }

    public class OnGameInstalledEventArgs
    {
        public Game Game { get; internal set; }
    }

    public class OnGameUninstalledEventArgs
    {
        public Game Game { get; internal set; }
    }

    public class OnApplicationStartedEventArgs
    {
    }

    public class OnApplicationStoppedEventArgs
    {
    }

    public class OnLibraryUpdatedEventArgs
    {
    }
}
