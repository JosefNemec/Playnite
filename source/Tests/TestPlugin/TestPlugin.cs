using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TestPlugin
{
    public class DefaultSearchContext : SearchContext
    {
        public DefaultSearchContext()
        {
            Delay = 500;
            Description = "Default search description";
            Label = "test search";
            Hint = "search hint goes here";
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            var game = API.Instance.Database.Games.First();
            yield return new GameSearchItem(
                game,
                "test",
                () => API.Instance.Dialogs.ShowErrorMessage(game.Name));

            yield return new SearchItem($"test plugin: {args.SearchTerm}", new SearchItemAction("Blow up", () => { }))
            {
                Description = "test plugin description",
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png")
            };

            yield return new SearchItem($"test plugin #2: {args.SearchTerm}", new SearchItemAction("Blow up", () => { }))
            {
                Description = "test plugin description",
                SecondaryAction = new SearchItemAction("blow up more", () => { })
            };

            yield return new SearchItem($"icon test", new SearchItemAction("Blow up", () => {
                API.Instance.Database.ImportGame(new GameMetadata() { Name = "# import from search" });
            }))
            {
                Icon = @"https://playnite.link/applogo.png",
                Description = "http icon test"
            };
        }
    }

    public class SlowSearchContext : SearchContext
    {
        public SlowSearchContext()
        {
            Delay = 500;
            Description = "Slow search description";
        }

        public override IEnumerable<SearchItem> GetSearchResults(GetSearchResultsArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.SearchTerm))
            {
                yield break;
            }

            Thread.Sleep(3000);
            if (args.CancelToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return new SearchItem($"slow result: {args.SearchTerm}", new SearchItemAction("Blow up slowly", () => { }))
            {
                Description = "test plugin description",
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png")
            };
        }
    }

    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() + " converted";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestPlugin : GenericPlugin
    {
        private static ILogger logger = LogManager.GetLogger();

        public TestPluginSettingsViewModel Settings { get; private set; }

        public override Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C9");

        public TestPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

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

            Searches = new List<SearchSupport>
            {
                new SearchSupport("test", "Testing plugin search", new DefaultSearchContext()),
                new SearchSupport("slow", "Slow plugin search test", new SlowSearchContext())
            };

            AddConvertersSupport(new AddConvertersSupportArgs
            {
                Converters = new List<IValueConverter> { new TestConverter() },
                SourceName = "TestPlugin",
            });

            api.Notifications.Add("test", "some test ", NotificationType.Info);
            api.Notifications.Add("test2", "some test longer notification that's overlowing to aa lines aa likely", NotificationType.Error);
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
            logger.Info($"TestPluginDev OnGameStarted {args.Game.Name} {args.StartedProcessId}");
            logger.Warn(PlayniteApi.ApplicationSettings.CompletionStatus.PlayedStatus.ToString());
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            logger.Info($"TestPluginDev OnGameStarting {args.Game.Name}");
            logger.Warn(args.SourceAction?.Name);
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
            //logger.Warn($"TestPluginDev OnGameSelected {args.NewValue?.Count}");
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
        }

        private async void CrashTest()
        {
            await Task.Delay(100);
            throw new NotImplementedException();
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
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
            };
            yield return new MainMenuItem
            {
                Description = "-"
            };

            yield return new MainMenuItem
            {
                Description = "serialization test",
                Action = (_) =>
                {
                    var filtered = PlayniteApi.MainView.FilteredGames;
                    PlayniteApi.MainView.SelectGame(filtered[1].Id);
                }
            };

            yield return new MainMenuItem
            {
                MenuSection = "@",
                Description = "this is in extension menu",
                Action = (_) =>
                {
                }
            };

            yield return new MainMenuItem
            {
                MenuSection = "@nested|nested2",
                Description = "nested test menu menu",
                Action = (_) =>
                {
                }
            };

            yield return new MainMenuItem
            {
                MenuSection = "test|test2",
                Description = "filtered item test",
                Action = (_) =>
                {
                    var filtered = PlayniteApi.MainView.FilteredGames;
                    logger.Warn(filtered.Count.ToString());
                    if (filtered.Count > 2)
                    {
                        PlayniteApi.MainView.SelectGames(filtered.Select(a => a.Id).Take(2));
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "test plugin root test",
                Action = (_) => PlayniteApi.Dialogs.ShowMessage("test plugin root test")
            };
            yield return new GameMenuItem
            {
                Description = "window test",
                MenuSection = "test plugin",
                Action = (_) => PlayniteApi.Dialogs.ShowMessage("window test")
            };
            yield return new GameMenuItem
            {
                Description = "-",
                MenuSection = "test plugin"
            };
            yield return new GameMenuItem
            {
                Description = "serialization test",
                MenuSection = "test plugin",
                Action = (_) => PlayniteApi.Dialogs.ShowMessage("serialization test"),
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png"),
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

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = "direct test",
                Activated = () => Process.Start("calc"),
                Icon = new TextBlock
                {
                    Text = char.ConvertFromUtf32(0xebdf),
                    FontSize = 20,
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                },
                ProgressValue = 40
            };
            yield return new ViewSidebarTest();
            yield return new CalcSidebar()
            {
                Title = "zaltulator",
                Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png"),
                IconPadding = new System.Windows.Thickness(4),
                ProgressValue = 0
            };
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            return null;
            //yield return new AutomaticPlayController(args.Game)
            //{
            //    Type = AutomaticPlayActionType.File,
            //    TrackingMode = TrackingMode.Process,
            //    Name = "Notepad",
            //    Path = "notepad.exe"
            //};
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args.Name == "TestUserControl")
            {
                logger.Warn(PlayniteApi.MainView.ActiveDesktopView.ToString());
                return new TestPluginUserControl(Settings);
            }

            return null;
        }

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            yield return new TopPanelItem()
            {
                Icon = new TextBlock
                {
                    Text = char.ConvertFromUtf32(0xebdf),
                    FontSize = 20,
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                },
                Title = "Calculator",
                Activated = () =>
                {
                    PlayniteApi.MainView.OpenSearch(new DefaultSearchContext(), null);
                }
            };
            //new TopPanelItem()
            //{
            //    Title = "Steam fields",
            //    Activated = () => Process.Start(@"steam://open/friends")
            //}
        }

        public override IEnumerable<SearchItem> GetSearchGlobalCommands()
        {
            TextBlock icon = null;
            PlayniteApi.MainView.UIDispatcher.Invoke(() => icon = new TextBlock
            {
                Text = char.ConvertFromUtf32(0xef08),
                FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                Foreground = Brushes.OrangeRed
            });

            yield return new SearchItem(
                "test command",
                "activate",
                () =>
                {
                    var threadTest = new TextBlock
                    {
                        Text = char.ConvertFromUtf32(0xef08),
                        FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily,
                        Foreground = Brushes.OrangeRed
                    };
                    PlayniteApi.Dialogs.ShowMessage("teste command");
                },
                icon)
            {
                Description = "some description goes here to describe things"
            };

            yield return new SearchItem(
                "test command 2",
                "activate", () => PlayniteApi.Dialogs.ShowMessage("teste command 2"),
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "icon.png"));
        }
    }
}
