using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes category object.
    /// </summary>
    public class Category : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Category"/>.
        /// </summary>
        public Category() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Category"/>.
        /// </summary>
        /// <param name="name">Category name.</param>
        public Category(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty category.
        /// </summary>
        public static readonly Category Empty = new Category { Id = Guid.Empty, Name = string.Empty };
    }
}
