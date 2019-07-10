using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public class SteamClient : LibraryClient
    {
        public override string Icon => Steam.Icon;

        public override bool IsInstalled => Steam.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Steam.ClientExecPath, string.Empty);
        }
    }
}
