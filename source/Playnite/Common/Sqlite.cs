using Playnite.SDK.Data;
using SqlNado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class Sqlite : ISQLite, IDisposable
    {
        private SQLiteDatabase db;

        public Sqlite(string dbPath, SqliteOpenFlags openFlags)
        {
            db = new SQLiteDatabase(dbPath, (SQLiteOpenOptions)openFlags);
        }

        public List<T> Query<T>(string query, params object[] args) where T : new()
        {
            return db.Load<T>(query, args).ToList();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
