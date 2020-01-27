using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes fame feature object.
    /// </summary>
    public class GameFeature : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="GameFeature"/>.
        /// </summary>
        public GameFeature() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GameFeature"/>.
        /// </summary>
        /// <param name="name"></param>
        public GameFeature(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty tag.
        /// </summary>
        public static readonly GameFeature Empty = new GameFeature { Id = Guid.Empty, Name = string.Empty };
    }
}
