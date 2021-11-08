using LiteDB;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GameScannersSettings
    {
        [BsonId(false)]
        public int Id { get; set; } = 0;
        public List<string> CrcExcludeFileTypes { get; set; }
    }

    public class GameScannersCollection : ItemCollection<GameScannerConfig>
    {
        private readonly GameDatabase db;

        private LiteCollection<GameScannersSettings> settingsCollection;
        private LiteCollection<GameScannersSettings> SettingsCollection
        {
            get
            {
                if (settingsCollection == null)
                {
                    settingsCollection = liteDb.GetCollection<GameScannersSettings>();
                }

                return settingsCollection;
            }
        }

        public GameScannersCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.GameScanners)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<GameScannerConfig>().Id(a => a.Id, false);
        }

        public GameScannersSettings GetSettings()
        {
            if (SettingsCollection.Count() == 0)
            {
                var settings = new GameScannersSettings { CrcExcludeFileTypes = new List<string> { "*.chd" } };
                SettingsCollection.Insert(settings);
                return settings;
            }
            else
            {
                return SettingsCollection.FindAll().First();
            }
        }

        public void SetSettings(GameScannersSettings settings)
        {
            settings.Id = 0;
            SettingsCollection.Upsert(settings);
        }
    }
}
