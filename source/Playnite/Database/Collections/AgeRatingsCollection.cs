using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class AgeRatingsCollection : ItemCollection<AgeRating>
    {
        private readonly GameDatabase db;

        public AgeRatingsCollection(GameDatabase database) : base(type: GameDatabaseCollection.AgeRatings)
        {
            db = database;
        }

        private void RemoveUsage(Guid ageRatingId)
        {
            foreach (var game in db.Games.Where(a => a.AgeRatingId == ageRatingId))
            {
                game.AgeRatingId = Guid.Empty;
                db.Games.Update(game);
            }
        }

        public override bool Remove(AgeRating itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<AgeRating> itemsToRemove)
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
