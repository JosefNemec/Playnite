using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.OldModels.NewVer1
{
    /// <summary>
    /// Represents game completion status.
    /// </summary>
    public enum OldCompletionStatus
    {
        /// <summary>
        /// Game has not been played yet.
        /// </summary>
        NotPlayed,
        /// <summary>
        /// Game has been played.
        /// </summary>
        Played,
        /// <summary>
        /// Main storyline has been beaten.
        /// </summary>
        Beaten,
        /// <summary>
        /// Game has been fully completed.
        /// </summary>
        Completed
    }
}
