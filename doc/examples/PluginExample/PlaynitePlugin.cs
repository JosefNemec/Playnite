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

        public override int GetCompatibilityVersion()
        {
            return 1;
        }

        public override PluginProperties GetPluginProperties()
        {
            return new PluginProperties("Test Plugin", "Test Author", "0.1");
        }

        public override List<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from C#",
                    () => PlayniteApi.Dialogs.ShowMessage("From compiled code"))
            };
        }

        public override void OnGameInstalled(Game game)
        {
        }

        public override void OnGameStarted(Game game)
        {
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
