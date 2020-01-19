using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace _namespace_
{
    public class _name_ : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private _name_Settings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Change to something more appropriate
        public override string Name => "Custom Library";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new _name_Client();

        public _name_(IPlayniteAPI api) : base(api)
        {
            settings = new _name_Settings(this);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            // Return list of user's games.
            return new List<GameInfo>()
            {
                new GameInfo()
                {
                    Name = "Notepad",
                    GameId = "notepad",
                    PlayAction = new GameAction()
                    {
                        Type = GameActionType.File,
                        Path = "notepad.exe"
                    },
                    IsInstalled = true,
                    Icon = @"c:\Windows\notepad.exe"
                },
                new GameInfo()
                {
                    Name = "Calculator",
                    GameId = "calc",
                    PlayAction = new GameAction()
                    {
                        Type = GameActionType.File,
                        Path = "calc.exe"
                    },
                    IsInstalled = true,
                    Icon = @"https://playnite.link/applogo.png",
                    BackgroundImage =  @"https://playnite.link/applogo.png"
                }
            };
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new _name_SettingsView();
        }
    }
}