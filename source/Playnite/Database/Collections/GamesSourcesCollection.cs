using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GamesSourcesCollection : ItemCollection<GameSource>
    {
        private readonly GameDatabase db;

        public GamesSourcesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName)) throw new ArgumentNullException(nameof(sourceName));
            var source = this.FirstOrDefault(a => a.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
            if (source != null)
            {
                return source.Id;
            }
            else
            {
                var newSource = new GameSource(sourceName);
                base.Add(newSource);
                return newSource.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> sources)
        {
            var toAdd = new List<GameSource>();
            foreach (var source in sources)
            {
                var exSource = this.FirstOrDefault(a => a.Name.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (exSource != null)
                {
                    yield return exSource.Id;
                }
                else
                {
                    var newSource = new GameSource(source);
                    toAdd.Add(newSource);
                    yield return newSource.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
