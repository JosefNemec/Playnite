using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestPlugin
{
    public class TestPlugin : Plugin
    {
        private static ILogger logger = LogManager.GetLogger();

        public TestPluginSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C9");

        public TestPlugin(IPlayniteAPI api) : base(api)
        {
            Settings = new TestPluginSettingsViewModel(this, api);
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "TestUserControl" },
                SourceName = "TestPlugin",
                SettingsRoot = $"{nameof(Settings)}.{nameof(Settings.Settings)}"
            });
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
            //CrashTest();
        }

        private async void CrashTest()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            logger.Info("TestPluginDev INFO OnGameSelected");
            logger.Trace("TestPluginDev TRACE OnGameSelected");
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

        public class CalcSidebar : SidebarItem
        {
            public CalcSidebar()
            {
                Type = SiderbarItemType.Button;
                Title = "Calculator";
                Icon = new TextBlock
                {
                    Text = char.ConvertFromUtf32(Convert.ToInt32(0xef08)),
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                    Foreground = Brushes.OrangeRed
                };
                ProgressValue = 40;
                ProgressMaximum = 100;
            }

            public override void Activated()
            {
                Process.Start("calc");
            }
        }

        public class ViewSidebarTest : SidebarItem
        {
            public ViewSidebarTest()
            {
                Type = SiderbarItemType.View;
                Title = "TestView";
                Icon = new TextBlock
                {
                    Text = char.ConvertFromUtf32(Convert.ToInt32(0xeaf1)),
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                };
            }

            public override Control Opened()
            {
                return new Button { Content = "test" };
            }
        }

        public override List<SidebarItem> GetSidebarItems()
        {
            var items = new List<SidebarItem>
            {
                new CalcSidebar(),
                new ViewSidebarTest(),
                new CalcSidebar()
                {
                    Title = "zaltulator",
                    Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png"),
                    IconPadding = new System.Windows.Thickness(4),
                    ProgressValue = 0
                }
            };

            //for (int i = 0; i < 20; i++)
            //{
            //    items.Add(new CalcSidebar { Title = i.ToString() });
            //}

            return items;
        }

        public override List<PlayAction> GetPlayActions(GetPlayActionsArgs args)
        {
            return new List<PlayAction>
            {
                new GenericPlayAction
                {
                    Name = "Test Action",
                    Path = "calc",
                    Type = GenericPlayActionType.File
                }
            };
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "TestUserControl")
            {
                return new TestPluginUserControl();
            }

            return null;
        }
    }
}
