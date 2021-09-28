using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Data
{
    /// <summary>
    ///
    /// </summary>
    [Flags]
    public enum SqliteOpenFlags
    {
        /// <summary>
        ///
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        ///
        /// </summary>
        ReadWrite = 2,
        /// <summary>
        ///
        /// </summary>
        Create = 4,
        /// <summary>
        ///
        /// </summary>
        NoMutex = 32768,
        /// <summary>
        ///
        /// </summary>
        FullMutex = 65536,
        /// <summary>
        ///
        /// </summary>
        SharedCache = 131072,
        /// <summary>
        ///
        /// </summary>
        PrivateCache = 262144,
        /// <summary>
        ///
        /// </summary>
        ProtectionComplete = 1048576,
        /// <summary>
        ///
        /// </summary>
        ProtectionCompleteUnlessOpen = 2097152,
        /// <summary>
        ///
        /// </summary>
        ProtectionCompleteUntilFirstUserAuthentication = 3145728,
        /// <summary>
        ///
        /// </summary>
        ProtectionNone = 4194304
    }

    /// <summary>
    ///
    /// </summary>
    public interface ISQLite : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        List<T> Query<T>(string query, params object[] args) where T : new();
    }

    /// <summary>
    ///
    /// </summary>
    public class SQLite
    {
        private static Func<string, SqliteOpenFlags, ISQLite> generatorFunc;

        internal static void Init(Func<string, SqliteOpenFlags, ISQLite> func)
        {
            generatorFunc = func;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="openFlags"></param>
        /// <returns></returns>
        public static ISQLite OpenDatabase(string dbPath, SqliteOpenFlags openFlags)
        {
            return generatorFunc(dbPath, openFlags);
        }
    }
}
