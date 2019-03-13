using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite;
using Playnite.SDK;

namespace DiscordLibrary
{
    class DiscordClient : ILibraryClient
    {
        public bool IsInstalled
        {
            get => Discord.IsInstalled;
        }

        public void Open()
        {
            // Launching Discord.exe requires running:
            //     Update.exe --processStart Discord.exe
            // Just start it by URL instead:
            ProcessStarter.StartUrl("discord:///");
        }
    }
}
