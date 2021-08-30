using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestGameLibrary
{
    public class TestGameLibrary : LibraryPlugin
    {
        public override Guid Id { get; } = Guid.Parse("D625A3B7-1AA4-41CB-9CD7-74448D28E99B");

        public override string Name { get; } = "Test Library";

        public override string LibraryIcon { get; }

        public override LibraryClient Client { get; }

        public TestGameLibrary(IPlayniteAPI api) : base (api)
        {
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\installer.ico");
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return null;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return null;
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            return new List<GameMetadata>()
            {
                new GameMetadata()
                {
                    Name = "Notepad",
                    GameId = "notepad",
                    IsInstalled = true,
                    Icon = new MetadataFile(@"c:\Windows\notepad.exe")
                },
                new GameMetadata()
                {
                    Name = "Calculator",
                    GameId = "calc",
                    IsInstalled = true,
                    Icon = new MetadataFile(@"https://playnite.link/applogo.png"),
                    BackgroundImage =  new MetadataFile(@"https://playnite.link/applogo.png")
                },
                new GameMetadata()
                {
                    Name = "Paint",
                    GameId = "mspaint",
                    IsInstalled = true,
                    Icon = new MetadataFile(LibraryIcon)
                },
                new GameMetadata()
                {
                    Name = "WordPad",
                    GameId = "write",
                    IsInstalled = true,
                    Icon = new MetadataFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\icon.tga"))
                }
            };
        }
    }
}
