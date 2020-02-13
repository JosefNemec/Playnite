using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents capabilities of a library plugin.
    /// </summary>
    public class LibraryPluginCapabilities
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
        /// Creates new instance of <see cref="LibraryPlugin"/>.
        /// </summary>
        /// <param name="playniteAPI"></param>
        public LibraryPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        /// <summary>
        /// Gets library name;
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
        /// Gets library client application or null of no client is associated with this library.
        /// </summary>
        public virtual LibraryClient Client { get; }

        /// <summary>
        /// Gets plugin's library capabilities.
        /// </summary>
        public virtual LibraryPluginCapabilities Capabilities { get; }

        /// <summary>
        /// Gets library games.
        /// </summary>
        /// <returns>List of library games.</returns>
        public virtual IEnumerable<GameInfo> GetGames()
        {
            return new List<GameInfo>();
        }

        /// <summary>
        /// Initiates game import if "HasCustomizedGameImport" capability is enabled.
        /// </summary>
        /// <returns>List of newly imported games.</returns>
        public virtual IEnumerable<Game> ImportGames()
        {
            return new List<Game>();
        }

        /// <summary>
        /// Gets controller responsible for handling of library game or null if no specific controller is available.
        /// </summary>
        /// <param name="game">Game to be handled.</param>
        /// <returns>Game controller.</returns>
        public virtual IGameController GetGameController(Game game)
        {
            return null;
        }

        /// <summary>
        /// Gets library metadata downloader or null if no metadata provider is available.
        /// </summary>
        /// <returns>Metadata downloader.</returns>
        public virtual LibraryMetadataProvider GetMetadataDownloader()
        {
            return null;
        }
    }
}
