using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents plugin providing game metadata.
    /// </summary>
    public abstract class MetadataPlugin : Plugin
    {
        /// <summary>
        /// Gets metadata source name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets list of game fields this metadata provider can provide.
        /// </summary>
        public abstract List<GameField> SupportedFields { get; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataPlugin"/>.
        /// </summary>
        /// <param name="playniteAPI"></param>
        public MetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        /// <summary>
        /// Gets metadata for specific game.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public abstract GameMetadata GetMetadata(Game game);

        /// <summary>
        /// Gets metadata based on specific metadata search result.
        /// </summary>
        /// <param name="searchResult"></param>
        /// <returns></returns>
        public virtual GameMetadata GetMetadata(MetadataSearchResult searchResult)
        {
            return null;
        }

        /// <summary>
        /// Gets possible metadata results for search term.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public virtual List<MetadataSearchResult> SearchMetadata(string searchTerm)
        {
            return new List<MetadataSearchResult>();
        }
    }
}
