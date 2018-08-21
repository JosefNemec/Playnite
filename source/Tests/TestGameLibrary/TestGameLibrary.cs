using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestGameLibrary
{
    public class TestGameLibrary : ILibraryPlugin
    {
        public ISettings Settings { get; }

        public Guid Id { get; } = Guid.Parse("D625A3B7-1AA4-41CB-9CD7-74448D28E99B");

        public string Name { get; } = "Test";

        public string LibraryIcon { get; }

        public UserControl SettingsView { get; }

        public TestGameLibrary(IPlayniteAPI api)
        {
        }

        public void Dispose()
        {

        }

        public IEnumerable<Game> GetGames()
        {
            throw new NotImplementedException();
        }

        public IGameController GetGameController(Game game)
        {
            throw new NotImplementedException();
        }

        public IMetadataProvider GetMetadataDownloader()
        {
            throw new NotImplementedException();
        }
    }
}
