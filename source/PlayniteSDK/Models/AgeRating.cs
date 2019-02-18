using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class AgeRating : DatabaseObject
    {
        public AgeRating() : base()
        {
        }

        public AgeRating(string name) : base()
        {
            Name = name;
        }

        public static readonly AgeRating Empty = new AgeRating { Id = Guid.Empty, Name = string.Empty };
    }
}
