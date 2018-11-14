using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes object providing game database API.
    /// </summary>
    public interface IGameDatabaseAPI
    {
        /// <summary>
        /// Gets full path to database directory location.
        /// </summary>
        string DatabasePath { get; }

        /// <summary>
        /// Gets value indicating whether game database is open and available.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Adds new game to database.
        /// </summary>
        /// <param name="game">Game to be added.</param>
        void AddGame(Game game);

        /// <summary>
        /// Returns game from database.
        /// </summary>
        /// <param name="id">Game id.</param>
        /// <returns>Game based on specified id or null.</returns>
        Game GetGame(Guid id);

        /// <summary>
        /// Returns all games from database.
        /// </summary>
        /// <returns>All games stored in database.</returns>
        IEnumerable<Game> GetGames();

        /// <summary>
        /// Removes game from database.
        /// </summary>
        /// <param name="id">Game id.</param>
        void RemoveGame(Guid id);

        /// <summary>
        /// Updates game in database with new data.
        /// </summary>
        /// <param name="game">Game data to be updated in database.</param>
        void UpdateGame(Game game);

        /// <summary>
        /// Adds emulator to database.
        /// </summary>
        /// <param name="emulator">Emulator to be added.</param>
        void AddEmulator(Emulator emulator);

        /// <summary>
        /// Returns emulator from database.
        /// </summary>
        /// <param name="id">Emulator id.</param>
        /// <returns>Emulator based on specified id or null.</returns>
        Emulator GetEmulator(Guid id);

        /// <summary>
        /// Returns all emulators from database.
        /// </summary>
        /// <returns>All emulators stored in database.</returns>
        IEnumerable<Emulator> GetEmulators();

        /// <summary>
        /// Removes emulator from database.
        /// </summary>
        /// <param name="id">Emulator id.</param>
        void RemoveEmulator(Guid id);

        /// <summary>
        /// Adds new platform to database.
        /// </summary>
        /// <param name="platform">Platform to be added to database.</param>
        void AddPlatform(Platform platform);

        /// <summary>
        /// Returns platform from database.
        /// </summary>
        /// <param name="id">Platform id.</param>
        /// <returns>Platform based on speciifed id or null.</returns>
        Platform GetPlatform(Guid id);

        /// <summary>
        /// Returns all platforms from database.
        /// </summary>
        /// <returns>All platforms stored in database.</returns>
        IEnumerable<Platform> GetPlatforms();

        /// <summary>
        /// Removes platform from database.
        /// </summary>
        /// <param name="id">Platform id.</param>
        void RemovePlatform(Guid id);

        /// <summary>
        /// Add file to data storage.
        /// </summary>
        /// <param name="path">Path of the file to be added.</param>
        /// <param name="parentId">Databse item parent containning the file.</param>
        /// <returns>Database id of added file.</returns>
        string AddFile(string path, Guid parentId);

        /// <summary>
        /// Exports file from database.
        /// </summary>
        /// <param name="id">File id.</param>
        /// <param name="path">Full path to target file.</param>
        void SaveFile(string id, string path);

        /// <summary>
        /// Removes file from database.
        /// </summary>
        /// <param name="id">File id.</param>
        void RemoveFile(string id);

        /// <summary>
        /// Switches database to buffered mode. Suppresses all notification events until buffering is stopped.
        /// </summary>
        /// <returns>Buffer object.</returns>
        IDisposable BufferedUpdate();

        /// <summary>
        /// Returns full path to directory storing files for specified parent.
        /// </summary>
        /// <param name="parentId">Id of parent object.</param>
        /// <returns>Full path to directory.</returns>
        string GetFileStoragePath(Guid parentId);

        /// <summary>
        /// Return full path to a file based on database path.
        /// </summary>
        /// <param name="databasePath">Database path as set to game's field.</param>
        /// <returns>Full path to a file.</returns>
        string GetFullFilePath(string databasePath);        
    }
}