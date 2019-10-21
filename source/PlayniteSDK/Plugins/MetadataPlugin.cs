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
    /// Represents options for game metadata download.
    /// </summary>
    public class MetadataRequestOptions
    {
        /// <summary>
        /// Gets or sets game data are being requested for.
        /// </summary>
        public Game GameData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating whether the request is part of bulk metadata download.
        /// </summary>
        public bool IsBackgroundDownload
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataRequestOptions"/>.
        /// </summary>
        /// <param name="gameData"></param>
        /// <param name="backgroundDownload"></param>
        public MetadataRequestOptions(Game gameData, bool backgroundDownload)
        {
            GameData = gameData;
            IsBackgroundDownload = backgroundDownload;
        }
    }

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
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract GameMetadata GetMetadata(MetadataRequestOptions options);
    }
}
