using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
 
namespace TestPlugin
{
    public class TestPlugin : Plugin
    {
        private ILogger logger;

        public ISettings Settings { get; private set; } = new TestPluginSettings();

        public override Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C9");

        public TestPlugin(IPlayniteAPI api) : base(api)
        {
            logger = api.CreateLogger();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new TestPluginSettingsView();
        }

        public override IEnumerable<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from TestPlugin",
                    () =>
                    {
                        logger.Info($"TestPluginDev ExtensionFunction {PlayniteApi.Database.Games.Count}");
                    })
            };
        }

        public override void OnGameInstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameInstalled {game.Name}");
        }

        public override void OnGameStarted(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarted {game.Name}");
        }

        public override void OnGameStarting(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarting {game.Name}");
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            logger.Info($"TestPluginDev OnGameStopped {game.Name}");
        }

        public override void OnGameUninstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameUninstalled {game.Name}");
        }

        public override void OnApplicationStarted()
        {
            logger.Info("TestPluginDev OnApplicationStarted");
        }
    }
}
