using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents type of game database collection.
    /// </summary>
    public enum GameDatabaseCollection
    {
        /// <summary>
        ///
        /// </summary>
        Uknown,
        /// <summary>
        ///
        /// </summary>
        Games,
        /// <summary>
        ///
        /// </summary>
        Platforms,
        /// <summary>
        ///
        /// </summary>
        Emulators,
        /// <summary>
        ///
        /// </summary>
        Genres,
        /// <summary>
        ///
        /// </summary>
        Companies,
        /// <summary>
        ///
        /// </summary>
        Tags,
        /// <summary>
        ///
        /// </summary>
        Categories,
        /// <summary>
        ///
        /// </summary>
        Series,
        /// <summary>
        ///
        /// </summary>
        AgeRatings,
        /// <summary>
        ///
        /// </summary>
        Regions,
        /// <summary>
        ///
        /// </summary>
        Sources,
        /// <summary>
        ///
        /// </summary>
        Features,
        /// <summary>
        ///
        /// </summary>
        AppSoftware,
        /// <summary>
        ///
        /// </summary>
        GameScanners,
        /// <summary>
        ///
        /// </summary>
        FilterPresets,
        /// <summary>
        ///
        /// </summary>
        ImportExclusions,
        /// <summary>
        ///
        /// </summary>
        CompletionStatuses
    }

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
        /// Gets collection of game features.
        /// </summary>
        IItemCollection<GameFeature> Features { get; }

        /// <summary>
        /// Gets collection of game features.
        /// </summary>
        IItemCollection<GameScannerConfig> GameScanners { get; }

        /// <summary>
        /// Gets collection of game statuses.
        /// </summary>
        IItemCollection<CompletionStatus> CompletionStatuses { get; }

        /// <summary>
        /// Gets collection of import exclusions.
        /// </summary>
        IItemCollection<ImportExclusionItem> ImportExclusions { get; }

        /// <summary>
        /// Gets collection of filter presets.
        /// </summary>
        IItemCollection<FilterPreset> FilterPresets { get; }

        /// <summary>
        /// Gets value indicating whether database is opened.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Invoked when database is being opened.
        /// </summary>
        event EventHandler DatabaseOpened;

        /// <summary>
        /// Import new game into database.
        /// </summary>
        /// <param name="game">Game data to import.</param>
        /// <returns>Imported game.</returns>
        Game ImportGame(GameMetadata game);

        /// <summary>
        /// Import new game into database from a library plugin.
        /// </summary>
        /// <param name="game">Game data to import.</param>
        /// <param name="sourcePlugin">Source library plugin.</param>
        /// <returns>Imported game.</returns>
        Game ImportGame(GameMetadata game, LibraryPlugin sourcePlugin);

        /// <summary>
        /// Checks if the game matches specified filter settings.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filterSettings"></param>
        /// <returns></returns>
        bool GetGameMatchesFilter(Game game, FilterPresetSettings filterSettings);

        /// <summary>
        /// Returns enumeration of all games matching specified filter settings.
        /// </summary>
        /// <param name="filterSettings"></param>
        /// <returns></returns>
        IEnumerable<Game> GetFilteredGames(FilterPresetSettings filterSettings);

        /// <summary>
        /// Checks if the game matches specified filter settings.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="filterSettings"></param>
        /// <param name="useFuzzyNameMatch"></param>
        /// <returns></returns>
        bool GetGameMatchesFilter(Game game, FilterPresetSettings filterSettings, bool useFuzzyNameMatch);

        /// <summary>
        /// Returns enumeration of all games matching specified filter settings.
        /// </summary>
        /// <param name="filterSettings"></param>
        /// <param name="useFuzzyNameMatch"></param>
        /// <returns></returns>
        IEnumerable<Game> GetFilteredGames(FilterPresetSettings filterSettings, bool useFuzzyNameMatch);
    }
}
