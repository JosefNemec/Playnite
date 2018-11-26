using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class PlatformsCollection : ItemCollection<Platform>
    {
        private readonly GameDatabase db;

        public PlatformsCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public override bool Remove(Guid id)
        {
            var item = Get(id);
            var result = base.Remove(id);
            db.RemoveFile(item.Icon);
            db.RemoveFile(item.Cover);
            return result;
        }

        public override bool Remove(Platform item)
        {
            using (db.BufferedUpdate())
            {
                foreach (var game in db.Games.Where(a => a.PlatformId == item.Id))
                {
                    game.PlatformId = Guid.Empty;
                    db.Games.Update(game);
                }
            }

            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<Platform> items)
        {
            var ids = items.Select(a => a.Id).ToList();
            using (db.BufferedUpdate())
            {
                foreach (var game in db.Games.Where(a => ids.Contains(a.PlatformId)))
                {
                    game.PlatformId = Guid.Empty;
                    db.Games.Update(game);
                }
            }

            foreach (var item in items)
            {
                // Get item from in case that passed platform doesn't have actual metadata.
                var dbItem = Get(item.Id);
                db.RemoveFile(dbItem.Icon);
                db.RemoveFile(dbItem.Cover);
            }

            var result = base.Remove(items);
            return result;
        }

        public override void Update(IEnumerable<Platform> items)
        {
            foreach (var item in items)
            {
                var dbItem = Get(item.Id);
                if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != item.Icon)
                {
                    db.RemoveFile(dbItem.Icon);
                }

                if (!dbItem.Cover.IsNullOrEmpty() && dbItem.Cover != item.Cover)
                {
                    db.RemoveFile(dbItem.Cover);
                }
            }

            base.Update(items);
        }

        public override void Update(Platform item)
        {
            var dbItem = Get(item.Id);
            if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != item.Icon)
            {
                db.RemoveFile(dbItem.Icon);
            }

            if (!dbItem.Cover.IsNullOrEmpty() && dbItem.Cover != item.Cover)
            {
                db.RemoveFile(dbItem.Cover);
            }

            base.Update(item);
        }
    }
}
