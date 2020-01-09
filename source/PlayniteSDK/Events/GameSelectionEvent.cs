using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Represents game selection change event.
    /// </summary>
    public class GameSelectionEventArgs
    {
        /// <summary>
        /// Gets previously selected games.
        /// </summary>
        public List<Game> OldValue { get; }

        /// <summary>
        /// Gets newly selected games.
        /// </summary>
        public List<Game> NewValue { get; }

        /// <summary>
        /// Creates new instance of <see cref="GameSelectionEventArgs"/>.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public GameSelectionEventArgs(List<Game> oldValue, List<Game> newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
