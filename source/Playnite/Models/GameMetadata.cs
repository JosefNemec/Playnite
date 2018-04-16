using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Database;
using Playnite.SDK.Models;

namespace Playnite.Models
{
    public class GameMetadata
    {
        public Game GameData
        {
            get; set;
        }

        public FileDefinition Icon
        {
            get; set;
        }

        public FileDefinition Image
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

        public GameMetadata(Game gameData, FileDefinition icon, FileDefinition image, string background)
        {
            GameData = gameData;
            Icon = icon;
            Image = image;
            BackgroundImage = background;
        }
    }
}
