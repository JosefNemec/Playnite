using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetClient : ILibraryClient
    {
        public bool IsInstalled { get => BattleNet.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(BattleNet.ClientExecPath, string.Empty);
        }
    }
}
