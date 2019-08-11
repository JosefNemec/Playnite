using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class ItchioClient : LibraryClient
    {
        public override string Icon => Itch.Icon;

        public override bool IsInstalled => Itch.IsInstalled;

        public override void Open()
        {
            Itch.StartClient();
        }
    }
}
