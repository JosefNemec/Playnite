using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GenresCollection : ItemCollection<Genre>
    {
        private readonly GameDatabase db;

        public GenresCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string genreName)
        {
            if (string.IsNullOrEmpty(genreName)) throw new ArgumentNullException(nameof(genreName));
            var genre = this.FirstOrDefault(a => a.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));
            if (genre != null)
            {
                return genre.Id;
            }
            else
            {
                var newGenre = new Genre(genreName);
                base.Add(newGenre);
                return newGenre.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> genres)
        {
            var toAdd = new List<Genre>();
            foreach (var gen in genres)
            {
                var genre = this.FirstOrDefault(a => a.Name.Equals(gen, StringComparison.OrdinalIgnoreCase));
                if (genre != null)
                {
                    yield return genre.Id;
                }
                else
                {
                    var newGenre = new Genre(gen);
                    toAdd.Add(newGenre);
                    yield return newGenre.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
