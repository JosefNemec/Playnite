using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies past time segments.
    /// </summary>
    public enum PastTimeSegment : int
    {
        /// <summary>
        /// Indicates time occuring today.
        /// </summary>
        [Description("LOCToday")]
        Today = 0,

        /// <summary>
        /// Indicates time occuring yesterday.
        /// </summary>
        [Description("LOCYesterday")]
        Yesterday = 1,

        /// <summary>
        /// Indicates time occuring past week.
        /// </summary>
        [Description("LOCInPast7Days")]
        PastWeek = 2,

        /// <summary>
        /// Indicates time occuring past month.
        /// </summary>
        [Description("LOCInPast31Days")]
        PastMonth = 3,

        /// <summary>
        /// Indicates time occuring past year.
        /// </summary>
        [Description("LOCInPast365Days")]
        PastYear = 4,

        /// <summary>
        /// Indicates time occuring past year.
        /// </summary>
        [Description("LOCMoreThan365DaysAgo")]
        MoreThenYear = 5,

        /// <summary>
        /// Indicates time that never happened.
        /// </summary>
        [Description("LOCNever")]
        Never = 6
    }
}
