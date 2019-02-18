using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class GameSource : DatabaseObject
    {
        public GameSource() : base()
        {
        }

        public GameSource(string name) : base()
        {
            Name = name;
        }

        public static readonly GameSource Empty = new GameSource { Id = Guid.Empty, Name = string.Empty };
    }
}
