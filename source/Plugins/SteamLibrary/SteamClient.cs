using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary
{
    public class SteamClient : ILibraryClient
    {
        public bool IsInstalled { get => Steam.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(Steam.ClientExecPath, string.Empty);
        }
    }
}
