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
        /// Gets or sets game icon.
        /// </summary>
        public MetadataFile Icon
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets game cover image.
        /// </summary>
        public MetadataFile CoverImage
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets game background image.
        /// </summary>
        public MetadataFile BackgroundImage
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
        /// <param name="icon">Game icon.</param>
        /// <param name="coverImage">Game cover image.</param>
        /// <param name="background">Game background image.</param>
        public GameMetadata(GameInfo gameInfo, MetadataFile icon, MetadataFile coverImage, MetadataFile background)
        {
            GameInfo = gameInfo;
            Icon = icon;
            CoverImage = coverImage;
            BackgroundImage = background;
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
