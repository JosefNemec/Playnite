using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class AgeRatingsCollection : ItemCollection<AgeRating>
    {
        private readonly GameDatabase db;

        public AgeRatingsCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string ageRating)
        {
            if (string.IsNullOrEmpty(ageRating)) throw new ArgumentNullException(nameof(ageRating));
            var rating = this.FirstOrDefault(a => a.Name.Equals(ageRating, StringComparison.OrdinalIgnoreCase));
            if (rating != null)
            {
                return rating.Id;
            }
            else
            {
                var newRating = new AgeRating(ageRating);
                base.Add(newRating);
                return newRating.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> ratings)
        {
            var toAdd = new List<AgeRating>();
            foreach (var rating in ratings)
            {
                var exRating = this.FirstOrDefault(a => a.Name.Equals(rating, StringComparison.OrdinalIgnoreCase));
                if (exRating != null)
                {
                    yield return exRating.Id;
                }
                else
                {
                    var newRating = new AgeRating(rating);
                    toAdd.Add(newRating);
                    yield return newRating.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
