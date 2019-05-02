using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public enum PlaytimeCategory : int
    {
        [Description("LOCNotPlayed")]
        NotPlayed = 0,
        [Description("LOCPLaytimeLessThenAnHour")]
        LessThenHour = 1,
        [Description("LOCPLaytime1to10")]
        O1_10 = 2,
        [Description("LOCPLaytime10to100")]
        O10_100 = 3,
        [Description("LOCPLaytime100to500")]
        O100_500 = 4,
        [Description("LOCPLaytime500to1000")]
        O500_1000 = 5,
        [Description("LOCPLaytime1000plus")]
        O1000plus = 6
    }
}
