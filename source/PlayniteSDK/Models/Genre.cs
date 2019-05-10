using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes Genre object.
    /// </summary>
    public class Genre : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Genre"/>.
        /// </summary>
        public Genre() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Genre"/>.
        /// </summary>
        /// <param name="name"></param>
        public Genre(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty genre.
        /// </summary>
        public static readonly Genre Empty = new Genre { Id = Guid.Empty, Name = string.Empty };
    }
}
