using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayClient : LibraryClient
    {
        public override string Icon => Uplay.Icon;

        public override bool IsInstalled => Uplay.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Uplay.ClientExecPath, string.Empty);
        }
    }
}
