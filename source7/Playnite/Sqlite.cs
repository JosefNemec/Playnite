using SqlNado;

namespace Playnite;

public class Sqlite : IDisposable
{
    private readonly SQLiteDatabase db;

    public Sqlite(string dbPath, SQLiteOpenOptions openFlags)
    {
        db = new SQLiteDatabase(dbPath, openFlags);
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
