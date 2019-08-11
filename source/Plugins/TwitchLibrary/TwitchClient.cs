using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary
{
    public class TwitchClient : LibraryClient
    {
        public override string Icon => Twitch.Icon;

        public override bool IsInstalled => Twitch.IsInstalled;

        public override void Open()
        {
            ProcessStarter.StartProcess(Twitch.ClientExecPath, string.Empty);
        }
    }
}
