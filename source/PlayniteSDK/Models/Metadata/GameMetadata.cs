using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace Playnite.SDK.Metadata
{
    /// <summary>
    /// Represents game metadata.
    /// </summary>
    public class GameMetadata
    {
        /// <summary>
        /// Gets value indicating wheter the data are empty.
        /// </summary>
        public bool IsEmpty
        {
            get => GameInfo == null;
        }

        /// <summary>
        /// Gets or sets game information.
        /// </summary>
        public GameInfo GameInfo
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="GameMetadata"/>.
        /// </summary>
        public GameMetadata()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GameMetadata"/>.
        /// </summary>
        /// <param name="gameInfo">Game metadata.</param>
        public GameMetadata(GameInfo gameInfo)
        {
            GameInfo = gameInfo;
        }

        /// <summary>
        /// Returns empty metadata object.
        /// </summary>
        /// <returns>Empty metadata object.</returns>
        public static GameMetadata GetEmptyData()
        {
            return new GameMetadata();
        }
    }
}
