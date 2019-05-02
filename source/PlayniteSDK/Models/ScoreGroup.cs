using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public enum ScoreGroup : int
    {
        [Description("0x")]
        O0x = 0,
        [Description("1x")]
        O1x = 1,
        [Description("2x")]
        O2x = 2,
        [Description("3x")]
        O3x = 3,
        [Description("4x")]
        O4x = 4,
        [Description("5x")]
        O5x = 5,
        [Description("6x")]
        O6x = 6,
        [Description("7x")]
        O7x = 7,
        [Description("8x")]
        O8x = 8,
        [Description("9x")]
        O9x = 9,
        [Description("LOCNone")]
        None = 10
    }
}
