using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes age rating object.
    /// </summary>
    public class AgeRating : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="AgeRating"/>.
        /// </summary>
        public AgeRating() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="AgeRating"/>.
        /// </summary>
        /// <param name="name">Rating name.</param>
        public AgeRating(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty age rating.
        /// </summary>
        public static readonly AgeRating Empty = new AgeRating { Id = Guid.Empty, Name = string.Empty };
    }
}
