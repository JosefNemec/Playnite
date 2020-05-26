using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class SeriesCollection : ItemCollection<Series>
    {
        private readonly GameDatabase db;

        public SeriesCollection(GameDatabase database) : base(type: GameDatabaseCollection.Series)
        {
            db = database;
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games.Where(a => a.SeriesId == id))
            {
                game.SeriesId = Guid.Empty;
                db.Games.Update(game);
            }
        }

        public override bool Remove(Series itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Series> itemsToRemove)
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
