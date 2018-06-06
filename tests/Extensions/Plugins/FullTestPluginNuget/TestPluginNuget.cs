using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTestPlugin
{
    public class TestPluginNuget : Plugin
    {
        private ILogger logger;

        public TestPluginNuget(IPlayniteAPI api) : base(api)
        {
            logger = PlayniteApi.CreateLogger("TestPluginNuget");
        }

        public override PluginProperties GetPluginProperties()
        {
            return new PluginProperties("Test Plugin Nuget", "Josef Nemec", "1.0");
        }

        public override List<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from C#",
                    () =>
                    {
                        logger.Info($"TestPluginNuget ExtensionFunction {PlayniteApi.Database.GetGames().Count}");
                    })
            };
        }
        
        public override void OnLoaded()
        {
            logger.Info("TestPluginNuget OnLoaded");
        }

        public override void OnGameInstalled(Game game)
        {
            logger.Info($"TestPluginNuget OnGameInstalled {game.Name}");
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            logger.Info($"TestPluginNuget OnGameStopped {game.Name}");
        }

        public override void OnGameUninstalled(Game game)
        {
            logger.Info($"TestPluginNuget OnGameUninstalled {game.Name}");
        }

        public override void OnGameStarted(Game game)
        {
            logger.Info($"TestPluginNuget OnGameStarted {game.Name}");
        }

        public override void Dispose()
        {
            logger.Info($"TestPluginNuget Dispose");
        }
    }
}
