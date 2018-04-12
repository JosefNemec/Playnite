using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Playnite.SDK.Models;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes object providing game database API.
    /// </summary>
    public interface IGameDatabaseAPI
    {
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
        Game GetGame(int id);

        /// <summary>
        /// Returns all games from database.
        /// </summary>
        /// <returns>All games stored in database.</returns>
        List<Game> GetGames();

        /// <summary>
        /// Removes game from database.
        /// </summary>
        /// <param name="id">Game id.</param>
        void RemoveGame(int id);

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
        Emulator GetEmulator(ObjectId id);

        /// <summary>
        /// Returns all emulators from database.
        /// </summary>
        /// <returns>All emulators stored in database.</returns>
        List<Emulator> GetEmulators();

        /// <summary>
        /// Removes emulator from database.
        /// </summary>
        /// <param name="id">Emulator id.</param>
        void RemoveEmulator(ObjectId id);

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
        Platform GetPlatform(ObjectId id);

        /// <summary>
        /// Returns all platforms from database.
        /// </summary>
        /// <returns>All platforms stored in database.</returns>
        List<Platform> GetPlatforms();

        /// <summary>
        /// Removes platform from database.
        /// </summary>
        /// <param name="id">Platform id.</param>
        void RemovePlatform(ObjectId id);

        /// <summary>
        /// Adds new file to database. If file with the same content already exists then new file is not added.
        /// </summary>
        /// <param name="id">File id of new file.</param>
        /// <param name="path">Full path to file.</param>
        /// <returns>File id of file added to database or id of an existing file.</returns>
        string AddFile(string id, string path);

        /// <summary>
        /// Saves file from database to file.
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
        /// Removes image file from database. Doesn't remove file if some other game also uses it.
        /// </summary>
        /// <param name="id">File id.</param>
        /// <param name="game">Original file holding the image.</param>
        void RemoveImage(string id, Game game);

        /// <summary>
        /// Returns all files from database.
        /// </summary>
        /// <returns>All files stored in database.</returns>
        List<DatabaseFile> GetFiles();

        /// <summary>
        /// Returns specific file from database.
        /// </summary>
        /// <param name="id">File id.</param>
        /// <returns>All files stored in database.</returns>
        DatabaseFile GetFile(string id);
    }
}