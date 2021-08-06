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

        public override PluginProperties Properties { get; } = new PluginProperties
        {
            HasSettings = true
        };

        public TestPlugin(IPlayniteAPI api) : base(api)
        {
            Settings = new TestPluginSettingsViewModel(this, api);
            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                ElementList = new List<string> { "TestUserControl" },
                SourceName = "TestPlugin",
            });

            AddSettingsSupport(new AddSettingsSupportArgs
            {
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

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameInstalled {args.Game.Name}");
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameStarted {args.Game.Name}");
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameStarting {args.Game.Name}");
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameStopped {args.Game.Name}");
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameUninstalled {args.Game.Name}");
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            logger.Info("TestPluginDev OnApplicationStarted");
            //CrashTest();
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            //logger.Warn($"TestPluginDev OnGameSelected {args.NewValue?[0].Name}");
        }

        private async void CrashTest()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
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
                    Description = "-"
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

        public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = "window test",
                    MenuSection = "test plugin"
                },
                new GameMenuItem
                {
                    Description = "-",
                    MenuSection = "test plugin"
                },
                new GameMenuItem
                {
                    Description = "serialization test",
                    MenuSection = "test plugin"
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
                    Text = char.ConvertFromUtf32(0xef08),
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                    Foreground = Brushes.OrangeRed
                };
                ProgressValue = 40;
                ProgressMaximum = 100;
                Activated = () => Process.Start("calc");
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
                    Text = char.ConvertFromUtf32(0xeaf1),
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                };
                Opened = () => new Button { Content = "test" }; ;
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

        public override List<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            return new List<PlayController>
            {
                new AutomaticPlayController(args.Game)
                {
                    Name = "Test Action",
                    Path = "cmd.exe",
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

        public override List<TopPanelItem> GetTopPanelItems()
        {
            return new List<TopPanelItem>
            {
                new TopPanelItem()
                {
                    Icon = new TextBlock
                    {
                        Text = char.ConvertFromUtf32(0xeaf1),
                        FontSize = 20,
                        FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                    },
                    Title = "Steam fields",
                    Activated = () => Process.Start(@"steam://open/friends")
                }
            };
        }
    }
}
