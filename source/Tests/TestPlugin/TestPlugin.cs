using Playnite.SDK;
using Playnite.SDK.Data;
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

        public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "window test",
                    Action = (_) =>
                    {
                        var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions()
                        {
                            ShowCloseButton = false,
                            ShowMaximizeButton = false
                        }
                        );
                        window.Title = "window plugin test";
                        window.Content = new TestPluginSettingsView();
                        window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
                        window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                        window.Height = 640;
                        window.Width = 480;
                        window.ShowDialog();
                    }
                },
                new MainMenuItem
                {
                    Description = "serialization test",
                    Action = (_) =>
                    {
                        var obj = new TestPluginSettings { Option1 = "test", Option2 = 2 };
                        PlayniteApi.Dialogs.ShowMessage(Serialization.ToJson(obj));
                    }
                }
            };
        }
    }
}
