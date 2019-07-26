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
    public abstract class LibraryMetadataProvider : IDisposable
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets metadata for specified games.
        /// </summary>
        /// <param name="game">Game to get data for.</param>
        /// <returns>Game metadata.</returns>
        public abstract GameMetadata GetMetadata(Game game);
    }
}
