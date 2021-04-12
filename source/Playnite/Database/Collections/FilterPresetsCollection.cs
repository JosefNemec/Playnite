using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            mapper.Entity<FilterSettings>().
                Ignore(a => a.SearchActive).
                Ignore(a => a.IsActive).
                Ignore(a => a.SuppressFilterChanges);
            mapper.Entity<StringFilterItemProperites>().
                Ignore(a => a.IsSet);
            mapper.Entity<EnumFilterItemProperites>().
                Ignore(a => a.IsSet);
            mapper.Entity<FilterItemProperites>().
                Ignore(a => a.IsSet).
                Ignore(a => a.Texts);
        }
    }
}
