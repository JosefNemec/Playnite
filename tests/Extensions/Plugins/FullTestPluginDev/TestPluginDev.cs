using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullTestPlugin
{
    public class TestPluginDev : Plugin
    {
        private ILogger logger;

        public TestPluginDev(IPlayniteAPI api) : base(api)
        {
            logger = PlayniteApi.CreateLogger("TestPluginDev");
        }

        public override PluginProperties GetPluginProperties()
        {
            return new PluginProperties("Test Plugin Dev", "Josef Nemec", "1.0");
        }

        public override List<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from C#",
                    () =>
                    {
                        logger.Info($"TestPluginDev ExtensionFunction {PlayniteApi.Database.GetGames().Count}");
                    })
            };
        }
        
        public override void OnLoaded()
        {
            logger.Info("TestPluginDev OnLoaded");
        }

        public override void OnGameInstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameInstalled {game.Name}");
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            logger.Info($"TestPluginDev OnGameStopped {game.Name}");
        }

        public override void OnGameUninstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameUninstalled {game.Name}");
        }

        public override void OnGameStarting(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarting {game.Name}");
        }

        public override void OnGameStarted(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarted {game.Name}");
        }

        public override void Dispose()
        {
            logger.Info($"TestPluginDev Dispose");
        }
    }
}
