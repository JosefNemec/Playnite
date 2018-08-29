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
    /// Describes game library plugin.
    /// </summary>
    public interface ILibraryPlugin : IPlugin
    {
        /// <summary>
        /// Gets library client application.
        /// </summary>
        ILibraryClient Client { get; }

        /// <summary>
        /// Gets library icon.
        /// </summary>
        string LibraryIcon { get; }

        /// <summary>
        /// Gets library name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets library games.
        /// </summary>
        /// <returns>List of games.</returns>
        IEnumerable<Game> GetGames();

        /// <summary>
        /// Gets controller responsible for handling of library game.
        /// </summary>
        /// <param name="game">Game to be handled.</param>
        /// <returns>Game controller.</returns>
        IGameController GetGameController(Game game);

        /// <summary>
        /// Gets library metadata downloader.
        /// </summary>
        /// <returns>Metadata downloader.</returns>
        ILibraryMetadataProvider GetMetadataDownloader();       
    }
}
