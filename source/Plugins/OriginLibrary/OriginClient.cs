using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginClient : LibraryClient
    {
        public override string Icon => Origin.Icon;

        public override bool IsInstalled => Origin.IsInstalled;

        public override void Open()
        {
            Origin.StartClient();
        }
    }
}
