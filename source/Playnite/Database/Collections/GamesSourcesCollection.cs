using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GamesSourcesCollection : ItemCollection<GameSource>
    {
        private readonly GameDatabase db;

        public GamesSourcesCollection(GameDatabase database) : base(type: GameDatabaseCollection.Sources)
        {
            db = database;
        }

        private void RemoveUsage(Guid sourceId)
        {
            foreach (var game in db.Games.Where(a => a.SourceId == sourceId))
            {
                game.SourceId = Guid.Empty;
                db.Games.Update(game);
            }
        }

        public override bool Remove(GameSource itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<GameSource> itemsToRemove)
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
