using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GenresCollection : ItemCollection<Genre>
    {
        private readonly GameDatabase db;

        public GenresCollection(GameDatabase database) : base(type: GameDatabaseCollection.Genres)
        {
            db = database;
        }

        private void RemoveUsage(Guid genreId)
        {
            foreach (var game in db.Games.Where(a => a.GenreIds?.Contains(genreId) == true))
            {
                game.GenreIds.Remove(genreId);
                db.Games.Update(game);
            }
        }

        public override bool Remove(Genre itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Genre> itemsToRemove)
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
