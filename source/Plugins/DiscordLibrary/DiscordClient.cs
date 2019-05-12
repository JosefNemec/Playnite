using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite;
using Playnite.Common;
using Playnite.SDK;

namespace DiscordLibrary
{
    class DiscordClient : LibraryClient
    {

        public override bool IsInstalled => Discord.IsInstalled;

        public override void Open()
        {
            // Launching Discord.exe requires running:
            //     Update.exe --processStart Discord.exe
            // Just start it by URL instead:
            ProcessStarter.StartUrl("discord:///");
        }
    }
}
