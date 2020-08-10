using HumbleLibrary.Models;
using HumbleLibrary.Services;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HumbleLibrary
{
    public class HumbleLibrary : LibraryPlugin
    {
        private const string dbImportMessageId = "humblelibImportError";
        private static readonly ILogger logger = LogManager.GetLogger();

        internal HumbleLibrarySettings Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("96e8c4bc-ec5c-4c8b-87e7-18ee5a690626");

        public override string Name => "Humble";

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = false,
            HasCustomizedGameImport = true
        };

        public static string Icon { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");

        public override string LibraryIcon => Icon;

        public HumbleLibrary(IPlayniteAPI api) : base(api)
        {
            Settings = new HumbleLibrarySettings(this);
        }

        private static string GetGameId(Order.SubProduct product)
        {
            return $"{product.machine_name}_{product.human_name}";
        }

        private static string GetGameId(TroveGame troveGame)
        {
            return $"{troveGame.machine_name}_{troveGame.human_name}_TROVE";
        }

        public static List<GameInfo> GetTroveGames()
        {
            var chunkDataUrlBase = @"https://www.humblebundle.com/api/v1/trove/chunk?property=popularity&direction=desc&index=";
            var games = new List<GameInfo>();

            using (var webClient = new WebClient { Encoding = Encoding.UTF8 })
            {
                var initialPageSrc = webClient.DownloadString(@"https://www.humblebundle.com/subscription/trove");
                var chunkMatch = Regex.Match(initialPageSrc, @"chunks"":\s*(\d+)");
                if (chunkMatch.Success)
                {
                    var chunks = int.Parse(chunkMatch.Groups[1].Value);
                    for (int i = 0; i < chunks; i++)
                    {
                        var chunkDataStr = webClient.DownloadString(chunkDataUrlBase + i);
                        foreach (var troveGame in Serialization.FromJson<List<TroveGame>>(chunkDataStr))
                        {
                            var game = new GameInfo
                            {
                                Name = troveGame.human_name,
                                GameId = GetGameId(troveGame),
                                Description = troveGame.description_text,
                                Publishers = troveGame.publishers?.Select(a => a.publisher_name).ToList(),
                                Developers = troveGame.developers?.Select(a => a.developer_name).ToList(),
                                Platform = "PC",
                                Source = "Humble"
                            };

                            games.Add(game);
                        }
                    }
                }
                else
                {
                    logger.Warn("Failed to get number of trove chunks.");
                }
            }

            return games.OrderBy(a => a.Name).ToList();
        }

        public override IEnumerable<Game> ImportGames()
        {
            var importedGames = new List<Game>();
            Exception importError = null;
            if (!Settings.ConnectAccount)
            {
                return importedGames;
            }

            try
            {
                var orders = new List<Order>();
                using (var view = PlayniteApi.WebViews.CreateOffscreenView(
                    new WebViewSettings
                    {
                        JavaScriptEnabled = false
                    }))
                {
                    var api = new HumbleAccountClient(view);
                    var keys = api.GetLibraryKeys();
                    orders = api.GetOrders(keys);
                }

                var selectedProducts = new List<Order.SubProduct>();
                var allTpks = orders.SelectMany(a => a.tpkd_dict?.all_tpks).ToList();

                foreach (var order in orders)
                {
                    if (order.subproducts.HasItems())
                    {
                        foreach (var product in order.subproducts)
                        {
                            if (product.downloads?.Any(a => a.platform == "windows") == true)
                            {
                                if (Settings.IgnoreThirdPartyStoreGames && order.tpkd_dict?.all_tpks.HasItems() == true)
                                {
                                    var exst = allTpks.FirstOrDefault(a =>
                                    !a.human_name.IsNullOrEmpty() &&
                                    (a.human_name == product.human_name || Regex.IsMatch(a.human_name, product.human_name + @".+\sKey$")));
                                    if (exst != null)
                                    {
                                        continue;
                                    }
                                }

                                var alreadyAdded = selectedProducts.FirstOrDefault(a => a.human_name == product.human_name);
                                if (alreadyAdded == null)
                                {
                                    selectedProducts.Add(product);
                                }
                            }
                        }
                    }
                }

                foreach (var product in selectedProducts)
                {
                    var gameId = GetGameId(product);
                    var alreadyImported = PlayniteApi.Database.Games.FirstOrDefault(a => a.GameId == gameId && a.PluginId == Id);
                    if (alreadyImported == null)
                    {
                        importedGames.Add(PlayniteApi.Database.ImportGame(new GameInfo()
                        {
                            Name = product.human_name,
                            GameId = GetGameId(product),
                            Icon = product.icon,
                            Platform = "PC",
                            Source = "Humble"
                        }, this));
                    }
                }

                if (Settings.ImportTroveGames)
                {
                    foreach (var troveGame in GetTroveGames())
                    {
                        var alreadyImported = PlayniteApi.Database.Games.FirstOrDefault(a => a.GameId == troveGame.GameId && a.PluginId == Id);
                        if (alreadyImported == null)
                        {
                            importedGames.Add(PlayniteApi.Database.ImportGame(troveGame, this));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to import installed Humble games.");
                importError = e;
            }

            if (importError != null)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error,
                    () => OpenSettingsView()));
            }
            else
            {
                PlayniteApi.Notifications.Remove(dbImportMessageId);
            }

            return importedGames;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new HumbleLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new HumbleGameController(game);
        }
    }
}