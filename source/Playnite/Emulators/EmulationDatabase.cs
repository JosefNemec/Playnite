using Playnite.SDK;
using SqlNado;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Emulators
{
    public class EmulationDatabase
    {
        public interface IEmulationDatabaseReader : IDisposable
        {
            string DatabaseName { get; }
            IEnumerable<DatGame> GetByCrc(string checksum);
            IEnumerable<DatGame> GetBySerial(string serial);
            IEnumerable<DatGame> GetByRomName(string romName);
            IEnumerable<DatGame> GetByRomNamePartial(string romNamePart);
        }

        public class EmulationDatabaseReader : IEmulationDatabaseReader
        {
            private readonly SQLiteDatabase db;

            public string DatabaseName { get; }

            public EmulationDatabaseReader(string dbPath)
            {
                DatabaseName = Path.GetFileNameWithoutExtension(dbPath);
                db = new SQLiteDatabase(dbPath, SQLiteOpenOptions.SQLITE_OPEN_READONLY);
            }

            public IEnumerable<DatGame> GetByCrc(string checksum)
            {
                if (db.TableExists("DatGame"))
                {
                    return db.Load<DatGame>($"WHERE UPPER({nameof(DatGame.RomCrc)}) = '{checksum.ToUpper().Replace("'", "''")}'");
                }
                else
                {
                    return new List<DatGame>();
                }
            }

            public IEnumerable<DatGame> GetBySerial(string serial)
            {
                if (db.TableExists("DatGame"))
                {
                    return db.Load<DatGame>($"WHERE UPPER({nameof(DatGame.Serial)}) = '{serial.ToUpper().Replace("'", "''")}'");
                }
                else
                {
                    return new List<DatGame>();
                }
            }

            public IEnumerable<DatGame> GetByRomName(string romName)
            {
                if (db.TableExists("DatGame"))
                {
                    return db.Load<DatGame>($"WHERE UPPER({nameof(DatGame.RomName)}) = '{romName.ToUpper().Replace("'", "''")}'");
                }
                else
                {
                    return new List<DatGame>();
                }
            }

            public IEnumerable<DatGame> GetByRomNamePartial(string romNamePart)
            {
                if (db.TableExists("DatGame"))
                {
                    return db.Load<DatGame>($"WHERE INSTR(UPPER({nameof(DatGame.RomName)}), '{romNamePart.ToUpper().Replace("'", "''")}') > 0");
                }
                else
                {
                    return new List<DatGame>();
                }
            }

            public void Dispose()
            {
                db.Dispose();
            }
        }

        private static readonly ILogger logger = LogManager.GetLogger();

        public static EmulationDatabaseReader GetDatabase(string databaseName, string databaseDir)
        {
            var dbFile = Path.Combine(databaseDir, $"{databaseName}.db");
            if (File.Exists(dbFile))
            {
                return new EmulationDatabaseReader(dbFile);
            }
            else
            {
                return null;
            }
        }
    }
}
