using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game library provider.
    /// </summary>
    public enum Provider
    {
        /// <summary>
        /// User created game.
        /// </summary>
        [Description("Custom")]
        Custom,
        /// <summary>
        /// CD Project's gog.com library provider.
        /// </summary>
        [Description("GOG")]
        GOG,
        /// <summary>
        /// EA's Origin library provider.
        /// </summary>
        [Description("Origin")]
        Origin,
        /// <summary>
        /// Valve's Steam library provider.
        /// </summary>
        [Description("Steam")]
        Steam,
        /// <summary>
        /// Ubisoft's Uplay library provider.
        /// </summary>
        [Description("Uplay")]
        Uplay,
        /// <summary>
        /// Activision-Blizzard's library provider.
        /// </summary>
        [Description("Battle.net")]
        BattleNet
    }
}
