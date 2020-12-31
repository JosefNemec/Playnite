using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CompaniesCollection : ItemCollection<Company>
    {
        private readonly GameDatabase db;

        public CompaniesCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.Companies)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<Company>().Id(a => a.Id, false);
        }

        private void RemoveUsage(Guid companyId)
        {
            foreach (var game in db.Games)
            {
                var modified = false;
                if (game.PublisherIds?.Contains(companyId) == true)
                {
                    game.PublisherIds.Remove(companyId);
                    modified = true;
                }

                if (game.DeveloperIds?.Contains(companyId) == true)
                {
                    game.DeveloperIds.Remove(companyId);
                    modified = true;
                }

                if (modified)
                {
                    db.Games.Update(game);
                }
            }
        }

        public override bool Remove(Company itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Company> itemsToRemove)
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
