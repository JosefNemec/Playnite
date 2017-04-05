using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Database;

namespace Playnite.Models
{
    public class GameMetadata
    {
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
    }
}
