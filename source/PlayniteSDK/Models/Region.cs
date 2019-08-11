using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes Region object.
    /// </summary>
    public class Region : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Region"/>.
        /// </summary>
        public Region() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Region"/>.
        /// </summary>
        /// <param name="name"></param>
        public Region(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty region.
        /// </summary>
        public static readonly Region Empty = new Region { Id = Guid.Empty, Name = string.Empty };
    }
}
