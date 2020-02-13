using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class FeaturesCollection : ItemCollection<GameFeature>
    {
        private readonly GameDatabase db;

        public FeaturesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games.Where(a => a.FeatureIds?.Contains(id) == true))
            {
                game.FeatureIds.Remove(id);
                db.Games.Update(game);
            }
        }

        public override bool Remove(GameFeature itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<GameFeature> itemsToRemove)
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
