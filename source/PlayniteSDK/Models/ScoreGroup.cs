using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies game score rating.
    /// </summary>
    public enum ScoreRating : int
    {
        /// <summary>
        /// No score.
        /// </summary>
        None = 0,
        /// <summary>
        /// Negative rating.
        /// </summary>
        Negative = 1,
        /// <summary>
        /// Positive rating.
        /// </summary>
        Positive = 2,
        /// <summary>
        /// Mixed rating.
        /// </summary>
        Mixed = 3
    }

    /// <summary>
    /// Specifies rating score groups.
    /// </summary>
    public enum ScoreGroup : int
    {
        /// <summary>
        /// Score range from 0 to 10.
        /// </summary>
        [Description("0x")]
        O0x = 0,

        /// <summary>
        /// Score range from 10 to 20.
        /// </summary>
        [Description("1x")]
        O1x = 1,

        /// <summary>
        /// Score range from 20 to 30.
        /// </summary>
        [Description("2x")]
        O2x = 2,

        /// <summary>
        /// Score range from 30 to 40.
        /// </summary>
        [Description("3x")]
        O3x = 3,

        /// <summary>
        /// Score range from 40 to 50.
        /// </summary>
        [Description("4x")]
        O4x = 4,

        /// <summary>
        /// Score range from 50 to 60.
        /// </summary>
        [Description("5x")]
        O5x = 5,

        /// <summary>
        /// Score range from 60 to 70.
        /// </summary>
        [Description("6x")]
        O6x = 6,

        /// <summary>
        /// Score range from 70 to 80.
        /// </summary>
        [Description("7x")]
        O7x = 7,

        /// <summary>
        /// Score range from 80 to 90.
        /// </summary>
        [Description("8x")]
        O8x = 8,

        /// <summary>
        /// Score range from 90 to 100.
        /// </summary>
        [Description("9x")]
        O9x = 9,

        /// <summary>
        /// No score.
        /// </summary>
        [Description("LOCNone")]
        None = 10
    }
}
