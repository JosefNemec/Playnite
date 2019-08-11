using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes game databse API.
    /// </summary>
    public interface IGameDatabase
    {
        /// <summary>
        /// Gets collection of games.
        /// </summary>
        IItemCollection<Game> Games { get; }

        /// <summary>
        /// Gets collections of platforms.
        /// </summary>
        IItemCollection<Platform> Platforms { get; }

        /// <summary>
        /// Gets collection of emulators.
        /// </summary>
        IItemCollection<Emulator> Emulators { get; }

        /// <summary>
        /// Gets collection of genres.
        /// </summary>
        IItemCollection<Genre> Genres { get; }

        /// <summary>
        /// Gets collection of companies.
        /// </summary>
        IItemCollection<Company> Companies { get; }

        /// <summary>
        /// Gets collection of tags.
        /// </summary>
        IItemCollection<Tag> Tags { get; }

        /// <summary>
        /// Gets collection of categories.
        /// </summary>
        IItemCollection<Category> Categories { get; }

        /// <summary>
        /// Gets collection of series.
        /// </summary>
        IItemCollection<Series> Series { get; }

        /// <summary>
        /// Gets collection of age ratings.
        /// </summary>
        IItemCollection<AgeRating> AgeRatings { get; }

        /// <summary>
        /// Gets collection of regions.
        /// </summary>
        IItemCollection<Region> Regions { get; }

        /// <summary>
        /// Gets collection of sources.
        /// </summary>
        IItemCollection<GameSource> Sources { get; }

        /// <summary>
        /// Gets value indicating whether database is opened.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Invoked when database is being opened.
        /// </summary>
        event EventHandler DatabaseOpened;
    }
}
