using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetClient : LibraryClient
    {
        public override string Icon => BattleNet.Icon;

        public override bool IsInstalled => BattleNet.IsInstalled;

        public override void Open()
        {
            BattleNet.StartClient();
        }
    }
}
