using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SteamLibrary
{
    public class SteamLibrary : IGameLibrary
    {
        private readonly IPlayniteAPI playniteApi;

        public Guid LibraryId { get; } = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");

        public IEditableObject Settings { get; private set; }

        public SteamLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            Settings = new SteamLibrarySettings();
        }

        public void Dispose()
        {

        }
    }
}
