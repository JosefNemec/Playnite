using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies plat time categories.
    /// </summary>
    public enum PlaytimeCategory : int
    {
        /// <summary>
        /// Not playtime.
        /// </summary>
        [Description("LOCPlayedNone")]
        NotPlayed = 0,

        /// <summary>
        /// Less then an hour played.
        /// </summary>
        [Description("LOCPLaytimeLessThenAnHour")]
        LessThenHour = 1,

        /// <summary>
        /// Played 1 to 10 hours.
        /// </summary>
        [Description("LOCPLaytime1to10")]
        O1_10 = 2,

        /// <summary>
        /// Played 10 to 100 hours.
        /// </summary>
        [Description("LOCPLaytime10to100")]
        O10_100 = 3,

        /// <summary>
        /// Played 100 to 500 hours.
        /// </summary>
        [Description("LOCPLaytime100to500")]
        O100_500 = 4,

        /// <summary>
        /// Played 500 to 1000 hours.
        /// </summary>
        [Description("LOCPLaytime500to1000")]
        O500_1000 = 5,

        /// <summary>
        /// Played more then 1000 hours.
        /// </summary>
        [Description("LOCPLaytime1000plus")]
        O1000plus = 6
    }
}
