using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Region : DatabaseObject
    {
        public Region() : base()
        {
        }

        public Region(string name) : base()
        {
            Name = name;
        }

        public static readonly Region Empty = new Region { Id = Guid.Empty, Name = string.Empty };
    }
}
