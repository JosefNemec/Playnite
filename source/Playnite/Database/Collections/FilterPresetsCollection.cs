using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;

namespace Playnite.Database
{
    public class FilterPresetsCollection : ItemCollection<FilterPreset>
    {
        public FilterPresetsCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.FilterPresets)
        {
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<FilterPreset>().Id(a => a.Id, false);
        }
    }
}
