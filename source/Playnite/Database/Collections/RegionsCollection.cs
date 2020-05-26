using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class RegionsCollection : ItemCollection<Region>
    {
        private readonly GameDatabase db;

        public RegionsCollection(GameDatabase database) : base(type: GameDatabaseCollection.Regions)
        {
            db = database;
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games.Where(a => a.RegionId == id))
            {
                game.RegionId = Guid.Empty;
                db.Games.Update(game);
            }
        }

        public override bool Remove(Region itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Region> itemsToRemove)
        {
            if (itemsToRemove.HasItems())
            {
                foreach (var item in itemsToRemove)
                {
                    RemoveUsage(item.Id);
                }
            }
            return base.Remove(itemsToRemove);
        }
    }
}
