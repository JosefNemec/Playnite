using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes Tag object.
    /// </summary>
    public class Tag : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Tag"/>.
        /// </summary>
        public Tag() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Tag"/>.
        /// </summary>
        /// <param name="name"></param>
        public Tag(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty tag.
        /// </summary>
        public static readonly Tag Empty = new Tag { Id = Guid.Empty, Name = string.Empty };
    }
}
