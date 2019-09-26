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

        private void RemoveUsage(Guid platformId)
        {
            foreach (var game in db.Games.Where(a => a.PlatformId == platformId))
            {
                game.PlatformId = Guid.Empty;
                db.Games.Update(game);
            }

            foreach (var emulator in db.Emulators)
            {
                if (!emulator.Profiles.HasItems())
                {
                    continue;
                }

                var updated = false;
                foreach (var profile in emulator.Profiles.Where(a => a.Platforms?.Contains(platformId) == true))
                {
                    profile.Platforms.Remove(platformId);
                    updated = true;
                }

                if (updated)
                {
                    db.Emulators.Update(emulator);
                }
            }
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            var dbItem = Get(id);
            db.RemoveFile(dbItem.Icon);
            db.RemoveFile(dbItem.Cover);
            db.RemoveFile(dbItem.Background);
            return base.Remove(id);
        }

        public override bool Remove(Platform item)
        {
            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<Platform> itemsToRemove)
        {
            if (itemsToRemove.HasItems())
            {
                foreach (var item in itemsToRemove)
                {
                    RemoveUsage(item.Id);
                    var dbItem = Get(item.Id);
                    db.RemoveFile(dbItem.Icon);
                    db.RemoveFile(dbItem.Cover);
                    db.RemoveFile(dbItem.Background);
                }
            }

            return base.Remove(itemsToRemove);
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

                if (!dbItem.Background.IsNullOrEmpty() && dbItem.Background != item.Background)
                {
                    db.RemoveFile(dbItem.Background);
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

            if (!dbItem.Background.IsNullOrEmpty() && dbItem.Background != item.Background)
            {
                db.RemoveFile(dbItem.Background);
            }

            base.Update(item);
        }
    }
}
