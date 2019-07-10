using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary
{
    public class GogClient : LibraryClient
    {
        public override string Icon => Gog.Icon;

        public override bool IsInstalled => Gog.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Gog.ClientExecPath, string.Empty);
        }
    }
}
