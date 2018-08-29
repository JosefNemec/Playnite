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
            get => GameData == null;
        }

        /// <summary>
        /// Gets or sets game metadata.
        /// </summary>
        public Game GameData
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
        public MetadataFile Image
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets game background image.
        /// </summary>
        public string BackgroundImage
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
        /// <param name="gameData">Game metadata.</param>
        /// <param name="icon">Game icon.</param>
        /// <param name="image">Game cover image.</param>
        /// <param name="background">Game background image.</param>
        public GameMetadata(Game gameData, MetadataFile icon, MetadataFile image, string background)
        {
            GameData = gameData;
            Icon = icon;
            Image = image;
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
