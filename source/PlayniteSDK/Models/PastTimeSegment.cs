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
        /// Idicates time occurig today.
        /// </summary>
        [Description("LOCToday")]
        Today = 0,

        /// <summary>
        /// Idicates time occurig yesterday.
        /// </summary>
        [Description("LOCYesterday")]
        Yesterday = 1,

        /// <summary>
        /// Idicates time occurig past week.
        /// </summary>
        [Description("LOCPastWeek")]
        PastWeek = 2,

        /// <summary>
        /// Idicates time occurig past month.
        /// </summary>
        [Description("LOCPastMonth")]
        PastMonth = 3,

        /// <summary>
        /// Idicates time occurig past year.
        /// </summary>
        [Description("LOCPastYear")]
        PastYear = 4,

        /// <summary>
        /// Idicates time occurig past year.
        /// </summary>
        [Description("LOCMoreThenYear")]
        MoreThenYear = 5,

        /// <summary>
        /// Idicates time that never happened.
        /// </summary>
        [Description("LOCNever")]
        Never = 6
    }
}
