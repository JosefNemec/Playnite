using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CategoriesCollection : ItemCollection<Category>
    {
        private readonly GameDatabase db;

        public CategoriesCollection(GameDatabase database) : base(type: GameDatabaseCollection.Categories)
        {
            db = database;
        }

        private void RemoveUsage(Guid categoryId)
        {
            foreach (var game in db.Games.Where(a => a.CategoryIds?.Contains(categoryId) == true))
            {
                game.CategoryIds.Remove(categoryId);
                db.Games.Update(game);
            }
        }

        public override bool Remove(Category itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Category> itemsToRemove)
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
