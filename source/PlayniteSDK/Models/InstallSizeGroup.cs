using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Specifies Install Size categories.
    /// </summary>
    public enum InstallSizeGroup : int
    {
        /// <summary>
        /// Not playtime.
        /// </summary>
        [Description("LOCNone")]
        None = 0,

        /// <summary>
        /// 0 to 100MB.
        /// </summary>
        [Description("LOCSizeZeroTo100Mb")]
        S0_0MB_100MB = 1,

        /// <summary>
        /// 100MB to 1GB.
        /// </summary>
        [Description("LOCSize100MbTo1Gb")]
        S1_100MB_1GB = 2,

        /// <summary>
        /// 1GB to 5GB.
        /// </summary>
        [Description("LOCSize1GbTo5Gb")]
        S2_1GB_5GB = 3,

        /// <summary>
        /// 5GB to 10GB.
        /// </summary>
        [Description("LOCSize5GbTo10Gb")]
        S3_5GB_10GB = 4,

        /// <summary>
        /// 10GB to 20GB.
        /// </summary>
        [Description("LOCSize10GbTo20Gb")]
        S4_10GB_20GB = 5,

        /// <summary>
        /// 20GB to 40GB.
        /// </summary>
        [Description("LOCSize20GbTo40Gb")]
        S5_20GB_40GB = 6,

        /// <summary>
        /// 40GB to 100GB.
        /// </summary>
        [Description("LOCSize40GbTo100Gb")]
        S6_40GB_100GB = 7,

        /// <summary>
        /// 100GB or more.
        /// </summary>
        [Description("LOCSizeMoreThan100Gb")]
        S7_100GBPlus = 8
    }
}