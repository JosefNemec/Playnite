using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class TagsCollection : ItemCollection<Tag>
    {
        private readonly GameDatabase db;

        public TagsCollection(GameDatabase database) : base(type: GameDatabaseCollection.Tags)
        {
            db = database;
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games.Where(a => a.TagIds?.Contains(id) == true))
            {
                game.TagIds.Remove(id);
                db.Games.Update(game);
            }
        }

        public override bool Remove(Tag itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Tag> itemsToRemove)
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
