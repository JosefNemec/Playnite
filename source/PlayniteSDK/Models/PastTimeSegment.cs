using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public enum PastTimeSegment : int
    {
        [Description("LOCToday")]
        Today = 0,
        [Description("LOCYesterday")]
        Yesterday = 1,
        [Description("LOCPastWeek")]
        PastWeek = 2,
        [Description("LOCPastMonth")]
        PastMonth = 3,
        [Description("LOCPastYear")]
        PastYear = 4,
        [Description("LOCNever")]
        Never = 5
    }
}
