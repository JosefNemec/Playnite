using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HumbleBundleLibrary
{
    public class HumbleBundleLibrary : ILibraryPlugin
    {
        private readonly IPlayniteAPI api;

        public Guid Id => Guid.Parse("563198cb-063e-41f8-ad51-6b4b22ff7899");

        public string Name => "HumbleBundle";

        public ILibraryClient Client => null;

        public string LibraryIcon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\humblebundle.png");

        public bool IsClientInstalled => false;

        public HumbleBundleLibrary(IPlayniteAPI api)
        {
            this.api = api;
        }
        public void Dispose()
        {
        }

        public IGameController GetGameController(Game game)
        {
            return new GameController(game);
        }

        public IEnumerable<Game> GetGames()
        {
            var webview = this.api.WebViews.CreateOffscreenView();
            var client = new HumbleBundleApi(webview);
            foreach (var order in client.GetOrders())
            {
                foreach (var subproduct in order.SubProducts)
                {
                    foreach (var download in subproduct.Downloads)
                    {
                        if (download.Platform != "windows")
                            continue;

                        var links = new ObservableCollection<Link>
                        {
                            new Link
                            {
                                Name = subproduct.Payee.HumanName,
                                Url = subproduct.Url,
                            }
                        };

                        string downloadUrl = null;
                        foreach (var file in download.Files)
                        {
                            if (file.Url == null)
                                continue;

                            if (!string.IsNullOrWhiteSpace(file.Url.BitTorrent))
                            {
                                links.Add(new Link
                                {
                                    Name = string.Format("{0} (Torrent)", file.Name),
                                    Url = file.Url.BitTorrent,
                                });
                            }

                            if (!string.IsNullOrWhiteSpace(file.Url.Web))
                            {
                                downloadUrl = file.Url.Web;
                                links.Add(new Link
                                {
                                    Name = file.Name,
                                    Url = file.Url.Web,
                                });
                            }
                        }

                        yield return new Game
                        {
                            Added = order.Created,
                            Icon = subproduct.Icon,
                            GameId = subproduct.MachineName,
                            GameImagePath = downloadUrl,
                            IsInstalled = false,
                            Links = links,
                            Name = subproduct.HumanName,
                            PluginId = this.Id,
                            Source = "HumbleBundle",
                        };
                    }
                }
            }
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new LibraryMetadataProvider();
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return new Settings(firstRunSettings, this.api);
        }

        public System.Windows.Controls.UserControl GetSettingsView(bool firstRunView)
        {
            return new SettingsView();
        }
    }
}
