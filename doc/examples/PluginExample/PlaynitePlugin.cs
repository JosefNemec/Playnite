using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaynitePluginExample
{
    public class PlaynitePlugin : Plugin
    {
        public PlaynitePlugin(IPlayniteAPI api) : base(api)
        {
        }

        public override PluginProperties GetPluginProperties()
        {
            return new PluginProperties("Test Plugin", "Test Author", "0.1", 1);
        }

        public override List<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from C#",
                    () =>
                    {
                        var logger = PlayniteApi.CreateLogger("test");
                        logger.Error("test");
                        PlayniteApi.Dialogs.ShowMessage(PlayniteApi.MainView.SelectedGames.Count().ToString());
                    })
            };
        }

        public override void OnGameInstalled(Game game)
        {
        }

        public override void OnGameStarted(Game game)
        {
            PlayniteApi.Dialogs.ShowMessage(game.Name);
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
        }

        public override void OnGameUninstalled(Game game)
        {
        }

        public override void OnLoaded()
        {
        }
    }
}
