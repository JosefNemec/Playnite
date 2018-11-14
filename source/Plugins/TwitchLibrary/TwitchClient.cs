using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary
{
    public class TwitchClient : ILibraryClient
    {
        public bool IsInstalled { get => Twitch.IsInstalled; }

        public void Open()
        {
            ProcessStarter.StartProcess(Twitch.ClientExecPath, string.Empty);
        }
    }
}
