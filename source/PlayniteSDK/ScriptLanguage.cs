using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents script language.
    /// </summary>
    public enum ScriptLanguage
    {
        /// <summary>
        /// Represents PowerShell scripting language.
        /// </summary>
        PowerShell,
        /// <summary>
        /// Represents IronPython scripting language.
        /// </summary>
        IronPython,
        /// <summary>
        /// Represents Batch scripting language.
        /// </summary>
        [Description("Batch (.bat script)")]
        Batch
    }
}
