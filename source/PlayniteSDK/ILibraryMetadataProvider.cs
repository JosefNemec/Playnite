using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes metadata provider for library games.
    /// </summary>
    public interface ILibraryMetadataProvider : IDisposable
    {
        /// <summary>
        /// Gets metadata for specified games.
        /// </summary>
        /// <param name="game">Game to get data for.</param>
        /// <returns>Game metadata.</returns>
        GameMetadata GetMetadata(Game game);
    }
}
