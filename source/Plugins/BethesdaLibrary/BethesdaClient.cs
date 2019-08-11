using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary
{
    public class BethesdaClient : LibraryClient
    {
        public override string Icon => Bethesda.Icon;

        public override bool IsInstalled => Bethesda.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Bethesda.ClientExecPath, string.Empty);
        }
    }
}
