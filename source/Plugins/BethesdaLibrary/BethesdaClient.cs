using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary
{
    public class BethesdaClient : ILibraryClient
    {
        public bool IsInstalled { get => Bethesda.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(Bethesda.ClientExecPath, string.Empty);
        }
    }
}
