using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game completion status.
    /// </summary>
    public enum CompletionStatus : int
    {
        /// <summary>
        /// Game has not been played yet.
        /// </summary>
        [Description("LOCNotPlayed")]
        NotPlayed = 0,
        /// <summary>
        /// Game has been played.
        /// </summary>
        [Description("LOCPlayed")]
        Played = 1,
        /// <summary>
        /// Main storyline has been beaten.
        /// </summary>
        [Description("LOCBeaten")]
        Beaten = 2,
        /// <summary>
        /// Game has been fully completed.
        /// </summary>
        [Description("LOCCompleted")]
        Completed = 3
    }
}
