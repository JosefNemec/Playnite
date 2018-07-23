using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace Playnite.SDK.Metadata
{
    public class GameMetadata
    {
        public static GameMetadata Empty
        {
            get => new GameMetadata();
        }

        public bool IsEmpty
        {
            get => GameData == null;
        }

        public Game GameData
        {
            get; set;
        }

        public MetadataFile Icon
        {
            get; set;
        }

        public MetadataFile Image
        {
            get; set;
        }

        public string BackgroundImage
        {
            get; set;
        }

        public GameMetadata()
        {
        }

        public GameMetadata(Game gameData, MetadataFile icon, MetadataFile image, string background)
        {
            GameData = gameData;
            Icon = icon;
            Image = image;
            BackgroundImage = background;
        }
    }
}
