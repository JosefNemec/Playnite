using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class AppSoftwareCollection : ItemCollection<AppSoftware>
    {
        private readonly GameDatabase db;

        public AppSoftwareCollection(GameDatabase database) : base(type: GameDatabaseCollection.AppSoftware)
        {
            db = database;
        }

        public override bool Remove(Guid id)
        {
            var dbItem = Get(id);
            db.RemoveFile(dbItem.Icon);
            return base.Remove(id);
        }

        public override bool Remove(AppSoftware item)
        {
            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<AppSoftware> itemsToRemove)
        {
            if (itemsToRemove.HasItems())
            {
                foreach (var item in itemsToRemove)
                {
                    var dbItem = Get(item.Id);
                    db.RemoveFile(dbItem.Icon);
                }
            }

            return base.Remove(itemsToRemove);
        }

        public override void Update(IEnumerable<AppSoftware> items)
        {
            foreach (var item in items)
            {
                var dbItem = Get(item.Id);
                if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != item.Icon)
                {
                    db.RemoveFile(dbItem.Icon);
                }
            }

            base.Update(items);
        }

        public override void Update(AppSoftware item)
        {
            var dbItem = Get(item.Id);
            if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != item.Icon)
            {
                db.RemoveFile(dbItem.Icon);
            }

            base.Update(item);
        }
    }
}
