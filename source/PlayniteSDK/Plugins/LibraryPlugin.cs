using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents arguments for <see cref="LibraryPlugin.GetGames(LibraryGetGamesArgs)"/> method.
    /// </summary>
    public class LibraryGetGamesArgs
    {
        /// <summary>
        /// Gets cancellation token.
        /// </summary>
        public CancellationToken CancelToken { get; internal set; }
    }

    /// <summary>
    /// Represents arguments for <see cref="LibraryPlugin.ImportGames(LibraryImportGamesArgs)"/> method.
    /// </summary>
    public class LibraryImportGamesArgs
    {
        /// <summary>
        /// Gets cancellation token.
        /// </summary>
        public CancellationToken CancelToken { get; internal set; }
    }

    /// <summary>
    /// Represents <see cref="LibraryPlugin"/> plugin properties.
    /// </summary>
    public class LibraryPluginProperties : PluginProperties
    {
        /// <summary>
        /// Gets or sets value indicating whether plugin is capable of closing down original game client.
        /// </summary>
        public bool CanShutdownClient { get; set; } = false;

        /// <summary>
        /// Gets or sets value indicated whether plugin uses customized mechanism for game import.
        /// </summary>
        public bool HasCustomizedGameImport { get; set; } = false;
    }

    /// <summary>
    /// Represents base game library plugin.
    /// </summary>
    public abstract class LibraryPlugin : Plugin
    {
        /// <summary>
        /// Gets plugin's properties.
        /// </summary>
        public LibraryPluginProperties Properties { get; protected set; }

        /// <summary>
        /// Creates new instance of <see cref="LibraryPlugin"/>.
        /// </summary>
        /// <param name="playniteAPI"></param>
        public LibraryPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        /// <summary>
        /// Gets library name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets library icon or null if no icon is available.
        /// </summary>
        /// <returns></returns>
        public virtual string LibraryIcon { get; }

        /// <summary>
        /// Gets library background image or null if no background is available.
        /// </summary>
        /// <returns></returns>
        public virtual string LibraryBackground { get; }

        /// <summary>
        /// Gets library client application or null if no client is associated with this library.
        /// </summary>
        public virtual LibraryClient Client { get; }

        /// <summary>
        /// Gets library games.
        /// </summary>
        /// <returns>List of library games.</returns>
        public virtual IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            return new List<GameMetadata>();
        }

        /// <summary>
        /// Initiates game import if "HasCustomizedGameImport" capability is enabled.
        /// </summary>
        /// <returns>List of newly imported games.</returns>
        public virtual IEnumerable<Game> ImportGames(LibraryImportGamesArgs args)
        {
            return new List<Game>();
        }

        /// <summary>
        /// Gets library metadata downloader or null if no metadata provider is available.
        /// </summary>
        /// <returns>Metadata downloader.</returns>
        public virtual LibraryMetadataProvider GetMetadataDownloader()
        {
            return null;
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
