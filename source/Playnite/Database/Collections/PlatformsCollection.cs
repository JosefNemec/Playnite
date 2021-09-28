using Playnite.Emulators;
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

        public PlatformsCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.Platforms)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<Platform>().Id(a => a.Id, false);
        }

        private void RemoveUsage(Guid platformId)
        {
            foreach (var game in db.Games.Where(a => a.PlatformIds?.Contains(platformId) == true))
            {
                game.PlatformIds.Remove(platformId);
                db.Games.Update(game);
            }

            foreach (var emulator in db.Emulators)
            {
                if (!emulator.CustomProfiles.HasItems())
                {
                    continue;
                }

                var updated = false;
                foreach (var profile in emulator.CustomProfiles.Where(a => a.Platforms?.Contains(platformId) == true))
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

        public override Platform Add(MetadataProperty property)
        {
            if (property is MetadataSpecProperty specProp)
            {
                var exPlat = this.FirstOrDefault(a => a.SpecificationId == specProp.Id);
                if (exPlat != null)
                {
                    return exPlat;
                }

                var plat = Emulation.Platforms.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                if (plat != null)
                {
                    exPlat = this.FirstOrDefault(a => a.SpecificationId == plat.Id);
                    if (exPlat != null)
                    {
                        return exPlat;
                    }
                    else
                    {
                        var newPlat = new Platform(plat.Name) { SpecificationId = plat.Id };
                        Add(newPlat);
                        return newPlat;
                    }
                }
                else
                {
                    var newPlat = new Platform(plat.Id);
                    Add(newPlat);
                    return newPlat;
                }
            }
            else
            {
                return base.Add(property);
            }
        }

        public override IEnumerable<Platform> Add(IEnumerable<MetadataProperty> properties)
        {
            foreach (var property in properties)
            {
                if (property is MetadataSpecProperty specProp)
                {
                    yield return Add(specProp);
                }
                else
                {
                    yield return base.Add(property);
                }
            }
        }

        public override Platform GetOrGenerate(MetadataProperty property)
        {
            if (property is MetadataSpecProperty specProp)
            {
                var exPlat = this.FirstOrDefault(a => a.SpecificationId == specProp.Id);
                if (exPlat != null)
                {
                    return exPlat;
                }

                var plat = Emulation.Platforms.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                if (plat != null)
                {
                    exPlat = this.FirstOrDefault(a => a.SpecificationId == plat.Id);
                    if (exPlat != null)
                    {
                        return exPlat;
                    }
                    else
                    {
                        return new Platform(plat.Name) { SpecificationId = plat.Id };
                    }
                }

                return null;
            }
            else
            {
                return base.GetOrGenerate(property);
            }
        }

        public override IEnumerable<Platform> GetOrGenerate(IEnumerable<MetadataProperty> properties)
        {
            foreach (var property in properties)
            {
                if (property is MetadataSpecProperty specProp)
                {
                    yield return GetOrGenerate(specProp);
                }
                else
                {
                    yield return base.GetOrGenerate(property);
                }
            }
        }
    }
}
