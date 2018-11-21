using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class SeriesCollection : ItemCollection<Series>
    {
        private readonly GameDatabase db;

        public SeriesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string seriesName)
        {
            if (string.IsNullOrEmpty(seriesName)) throw new ArgumentNullException(nameof(seriesName));
            var series = this.FirstOrDefault(a => a.Name.Equals(seriesName, StringComparison.OrdinalIgnoreCase));
            if (series != null)
            {
                return series.Id;
            }
            else
            {
                var newSeries = new Series(seriesName);
                base.Add(newSeries);
                return newSeries.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> series)
        {
            var toAdd = new List<Series>();
            foreach (var ser in series)
            {
                var exSeries = this.FirstOrDefault(a => a.Name.Equals(ser, StringComparison.OrdinalIgnoreCase));
                if (exSeries != null)
                {
                    yield return exSeries.Id;
                }
                else
                {
                    var newSeries = new Series(ser);
                    toAdd.Add(newSeries);
                    yield return newSeries.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
