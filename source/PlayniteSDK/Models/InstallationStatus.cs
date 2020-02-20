using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game installation status.
    /// </summary>
    public enum InstallationStatus
    {
        /// <summary>
        /// Game is installed.
        /// </summary>
        [Description("LOCGameIsInstalledTitle")]
        Installed = 0,

        /// <summary>
        /// Game is not installed.
        /// </summary>
        [Description("LOCGameIsUnInstalledTitle")]
        Uninstalled = 1
    }
}
