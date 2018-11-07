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
    public class TestPlugin : IGenericPlugin
    {
        private ILogger logger;
        private IPlayniteAPI api;

        public ISettings Settings { get; private set; } = new TestPluginSettings();

        public Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C9");

        public TestPlugin(IPlayniteAPI api)
        {
            logger = api.CreateLogger();
            this.api = api;
        }

        public void Dispose()
        {
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new TestPluginSettingsView();
        }

        public IEnumerable<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from TestPlugin",
                    () =>
                    {
                        logger.Info($"TestPluginDev ExtensionFunction {api.Database.GetGames().Count()}");
                    })
            };
        }

        public void OnGameInstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameInstalled {game.Name}");
        }

        public void OnGameStarted(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarted {game.Name}");
        }

        public void OnGameStarting(Game game)
        {
            logger.Info($"TestPluginDev OnGameStarting {game.Name}");
        }

        public void OnGameStopped(Game game, long ellapsedSeconds)
        {
            logger.Info($"TestPluginDev OnGameStopped {game.Name}");
        }

        public void OnGameUninstalled(Game game)
        {
            logger.Info($"TestPluginDev OnGameUninstalled {game.Name}");
        }

        public void OnApplicationStarted()
        {
            logger.Info("TestPluginDev OnApplicationStarted");
        }
    }
}
