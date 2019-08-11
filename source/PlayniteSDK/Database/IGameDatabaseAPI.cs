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
    public interface IGameDatabaseAPI : IGameDatabase
    {
        /// <summary>
        /// Gets full path to database directory location.
        /// </summary>
        string DatabasePath { get; }

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