using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes GameSource object.
    /// </summary>
    public class GameSource : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="GameSource"/>.
        /// </summary>
        public GameSource() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GameSource"/>.
        /// </summary>
        /// <param name="name"></param>
        public GameSource(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty game source.
        /// </summary>
        public static readonly GameSource Empty = new GameSource { Id = Guid.Empty, Name = string.Empty };
    }
}
