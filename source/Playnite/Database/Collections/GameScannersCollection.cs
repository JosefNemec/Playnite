using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GameScannersCollection : ItemCollection<GameScannerConfig>
    {
        private readonly GameDatabase db;

        public GameScannersCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.GameScanners)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<GameScannerConfig>().Id(a => a.Id, false);
        }
    }
}
