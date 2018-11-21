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

        public RegionsCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string regionName)
        {
            if (string.IsNullOrEmpty(regionName)) throw new ArgumentNullException(nameof(regionName));
            var region = this.FirstOrDefault(a => a.Name.Equals(regionName, StringComparison.OrdinalIgnoreCase));
            if (region != null)
            {
                return region.Id;
            }
            else
            {
                var newRegion = new Region(regionName);
                base.Add(newRegion);
                return newRegion.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> regions)
        {
            var toAdd = new List<Region>();
            foreach (var region in regions)
            {
                var exRegion = this.FirstOrDefault(a => a.Name.Equals(region, StringComparison.OrdinalIgnoreCase));
                if (exRegion != null)
                {
                    yield return exRegion.Id;
                }
                else
                {
                    var newRegion = new Region(region);
                    toAdd.Add(newRegion);
                    yield return newRegion.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
