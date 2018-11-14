using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary
{
    public class GogClient : ILibraryClient
    {
        public bool IsInstalled { get => Gog.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(Gog.ClientExecPath, string.Empty);
        }
    }
}
