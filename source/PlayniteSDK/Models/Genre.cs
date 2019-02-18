using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Genre : DatabaseObject
    {
        public Genre() : base()
        {
        }

        public Genre(string name) : base()
        {
            Name = name;
        }

        public static readonly Genre Empty = new Genre { Id = Guid.Empty, Name = string.Empty };
    }
}
