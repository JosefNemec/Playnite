using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary
{
    public class EpicClient : ILibraryClient
    {
        public bool IsInstalled { get => EpicLauncher.IsInstalled; }

        public void Open()
        {
            EpicLauncher.StartClient();
        }
    }
}
