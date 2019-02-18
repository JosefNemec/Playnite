using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Category : DatabaseObject
    {
        public Category() : base()
        {
        }

        public Category(string name) : base()
        {
            Name = name;
        }

        public static readonly Category Empty = new Category { Id = Guid.Empty, Name = string.Empty };
    }
}
