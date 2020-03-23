using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes application settings API.
    /// </summary>
    public interface IPlayniteSettingsAPI
    {
        /// <summary>
        /// Checks if game is added on import exclusion list.
        /// </summary>
        /// <param name="gameId">Game ID.</param>
        /// <param name="libraryId">Library plugin ID.</param>
        /// <returns>True if game is on exclusion list.</returns>
        bool GetGameExcludedFromImport(string gameId, Guid libraryId);
    }
}
