using System;
using System.Collections.Generic;
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
        Custom,
        /// <summary>
        /// CD Project's gog.com library provider.
        /// </summary>
        GOG,
        /// <summary>
        /// EA's Origin library provider.
        /// </summary>
        Origin,
        /// <summary>
        /// Valve's Steam library provider.
        /// </summary>
        Steam,
        /// <summary>
        /// Ubisoft's Uplay library provider.
        /// </summary>
        Uplay,
        /// <summary>
        /// Activision-Blizzard's library provider.
        /// </summary>
        BattleNet
    }
}
