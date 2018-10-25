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
            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<Platform> items)
        {
            var result = base.Remove(items);
            foreach (var item in items)
            {
                db.RemoveFile(item.Icon);
                db.RemoveFile(item.Cover);
            }

            return result;
        }
    }
}
