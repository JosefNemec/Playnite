using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    [Flags]
    public enum SqliteOpenFlags
    {
        ReadOnly = 1,
        ReadWrite = 2,
        Create = 4,
        NoMutex = 32768,
        FullMutex = 65536,
        SharedCache = 131072,
        PrivateCache = 262144,
        ProtectionComplete = 1048576,
        ProtectionCompleteUnlessOpen = 2097152,
        ProtectionCompleteUntilFirstUserAuthentication = 3145728,
        ProtectionNone = 4194304
    }

    public class Sqlite : IDisposable
    {
        private SQLiteConnection db;

        public Sqlite(string dbPath, SqliteOpenFlags openFlags)
        {
            db = new SQLiteConnection(dbPath, (SQLiteOpenFlags)openFlags);
        }

        public List<T> Query<T>(string query, params object[] args) where T : new()
        {
            return db.Query<T>(query, args);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
