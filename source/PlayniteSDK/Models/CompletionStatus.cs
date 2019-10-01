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
        /// Represents "Not Played" completion status.
        /// </summary>
        [Description("LOCCompletionStatusNotPlayed")]
        NotPlayed = 0,
        /// <summary>
        /// Represents Played completion status.
        /// </summary>
        [Description("LOCCompletionStatusPlayed")]
        Played = 1,
        /// <summary>
        /// Represents Beaten completion status.
        /// </summary>
        [Description("LOCCompletionStatusBeaten")]
        Beaten = 2,
        /// <summary>
        /// Represents Completed completion status.
        /// </summary>
        [Description("LOCCompletionStatusCompleted")]
        Completed = 3,
        /// <summary>
        /// Represents Playing completion status.
        /// </summary>
        [Description("LOCCompletionStatusPlaying")]
        Playing = 4,
        /// <summary>
        /// Represents Abandoned completion status.
        /// </summary>
        [Description("LOCCompletionStatusAbandoned")]
        Abandoned = 5,
        /// <summary>
        /// Represents "On hold" completion status.
        /// </summary>
        [Description("LOCCompletionStatusOnHold")]
        OnHold = 6,
        /// <summary>
        /// Represents "Plan to Play" completion status.
        /// </summary>
        [Description("LOCCompletionStatusPlanToPlay")]
        PlanToPlay = 7
    }
}
