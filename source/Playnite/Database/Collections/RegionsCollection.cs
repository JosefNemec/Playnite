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
    public class RegionsCollection : ItemCollection<Region>
    {
        private readonly GameDatabase db;

        public RegionsCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.Regions)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<Region>().Id(a => a.Id, false);
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games.Where(a => a.RegionIds?.Contains(id) == true))
            {
                game.RegionIds.Remove(id);
                db.Games.Update(game);
            }
        }

        public override bool Remove(Region itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Region> itemsToRemove)
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

        public override Region Add(MetadataProperty property)
        {
            if (property is MetadataSpecProperty specProp)
            {
                var exRegion = this.FirstOrDefault(a => a.SpecificationId == specProp.Id);
                if (exRegion != null)
                {
                    return exRegion;
                }

                var reg = Emulation.Regions.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                if (reg != null)
                {
                    exRegion = this.FirstOrDefault(a => a.SpecificationId == reg.Id);
                    if (exRegion != null)
                    {
                        return exRegion;
                    }
                    else
                    {
                        var newReg = new Region(reg.Name) { SpecificationId = reg.Id };
                        Add(newReg);
                        return newReg;
                    }
                }
                else
                {
                    var newReg = new Region(reg.Id);
                    Add(newReg);
                    return newReg;
                }
            }
            else
            {
                return base.Add(property);
            }
        }

        public override IEnumerable<Region> Add(IEnumerable<MetadataProperty> properties)
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

        public override Region GetOrGenerate(MetadataProperty property)
        {
            if (property is MetadataSpecProperty specProp)
            {
                var exRegion = this.FirstOrDefault(a => a.SpecificationId == specProp.Id);
                if (exRegion != null)
                {
                    return exRegion;
                }

                var reg = Emulation.Regions.FirstOrDefault(a => a.Id == specProp.Id || a.Name == specProp.Id);
                if (reg != null)
                {
                    exRegion = this.FirstOrDefault(a => a.SpecificationId == reg.Id);
                    if (exRegion != null)
                    {
                        return exRegion;
                    }
                    else
                    {
                        return new Region(reg.Name) { SpecificationId = reg.Id };
                    }
                }

                return null;
            }
            else
            {
                return base.GetOrGenerate(property);
            }
        }

        public override IEnumerable<Region> GetOrGenerate(IEnumerable<MetadataProperty> properties)
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
