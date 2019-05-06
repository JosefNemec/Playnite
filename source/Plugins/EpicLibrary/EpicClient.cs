using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary
{
    public class EpicClient : LibraryClient
    {
        public override string Icon => EpicLauncher.Icon;

        public override bool IsInstalled => EpicLauncher.IsInstalled;

        public override void Open()
        {
            EpicLauncher.StartClient();
        }
    }
}
