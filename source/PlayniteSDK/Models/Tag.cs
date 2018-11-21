using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Tag : DatabaseObject
    {
        public Tag() : base()
        {
        }

        public Tag(string name) : base()
        {
            Name = name;
        }
    }
}
