using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayClient : ILibraryClient
    {
        public bool IsInstalled { get => Uplay.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(Uplay.ClientExecPath, string.Empty);
        }
    }
}
