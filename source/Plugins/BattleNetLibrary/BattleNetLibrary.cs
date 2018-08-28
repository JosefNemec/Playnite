using AngleSharp.Parser.Html;
using BattleNetLibrary.Models;
using BattleNetLibrary.Services;
using Playnite.Common.System;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BattleNetLibrary
{
    public class BattleNetLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        
        internal BattleNetLibrarySettings LibrarySettings
        {
            get => (BattleNetLibrarySettings)Settings;
        }

        public BattleNetLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\battleneticon.png");
            Settings = new BattleNetLibrarySettings(this, playniteApi);
        }

        public static UninstallProgram GetUninstallEntry(BNetApp app)
        {
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (app.Type == BNetAppType.Classic)
                {
                    if (prog.DisplayName == app.InternalId)
                    {
                        return prog;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(prog.UninstallString))
                    {
                        continue;
                    }

                    var match = Regex.Match(prog.UninstallString, string.Format(@"Battle\.net.*--uid={0}.*\s", app.InternalId));
                    if (match.Success)
                    {
                        return prog;
                    }
                }
            }

            return null;
        }

        public static GameAction GetGamePlayTask(string id)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = $"battlenet://{id}/",
                IsHandledByPlugin = true
            };
        }

        public Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
            foreach (var prog in Programs.GetUnistallProgramsList())
            {
                if (string.IsNullOrEmpty(prog.UninstallString))
                {
                    continue;
                }

                if (prog.Publisher == "Blizzard Entertainment" && BattleNetGames.Games.Any(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId))
                {
                    var products = BattleNetGames.Games.Where(a => a.Type == BNetAppType.Classic && prog.DisplayName == a.InternalId);
                    foreach (var product in products)
                    {
                        if (!Directory.Exists(prog.InstallLocation))
                        {
                            continue;
                        }

                        var game = new Game()
                        {
                            PluginId = Id,
                            GameId = product.ProductId,
                            Source = "Battle.net",
                            Name = product.Name,
                            PlayAction = new GameAction()
                            {
                                Type = GameActionType.File,
                                WorkingDir = @"{InstallDir}",
                                Path = product.ClassicExecutable
                            },
                            InstallDirectory = prog.InstallLocation,
                            State = new GameState() { Installed = true }
                        };

                        games.Add(game.GameId, game);
                    }
                }
                else
                {
                    var match = Regex.Match(prog.UninstallString, @"Battle\.net.*--uid=(.*?)\s");
                    if (!match.Success)
                    {
                        continue;
                    }

                    if (prog.DisplayName.EndsWith("Test"))
                    {
                        continue;
                    }

                    var iId = match.Groups[1].Value;
                    var product = BattleNetGames.Games.FirstOrDefault(a => a.Type == BNetAppType.Default && iId.StartsWith(a.InternalId));
                    if (product == null)
                    {
                        continue;
                    }

                    var game = new Game()
                    {
                        PluginId = Id,
                        GameId = product.ProductId,
                        Source = "Battle.net",
                        Name = product.Name,
                        PlayAction = GetGamePlayTask(product.ProductId),
                        InstallDirectory = prog.InstallLocation,
                        State = new GameState() { Installed = true }
                    };

                    games.Add(game.GameId, game);
                }
            }

            return games;
        }

        public List<Game> GetLibraryGames()
        {
            using (var view = playniteApi.WebViews.CreateOffscreenView())
            {
                var api = new BattleNetAccountClient(view);

                if (!api.GetIsUserLoggedIn())
                {
                    throw new Exception("User is not logged in.");
                }

                var page = api.GetOwnedGames();
                var games = new List<Game>();
                var parser = new HtmlParser();
                var document = parser.Parse(page);

                foreach (var product in BattleNetGames.Games)
                {
                    if (product.Type == BNetAppType.Default)
                    {
                        var documentProduct = document.QuerySelector($"#{product.WebLibraryId}");
                        if (documentProduct == null)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(product.PurchaseId))
                        {
                            var saleOffer = documentProduct.QuerySelector($"#{product.PurchaseId}");
                            if (saleOffer != null)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        var documentProduct = document.QuerySelector($".{product.WebLibraryId}");
                        if (documentProduct == null)
                        {
                            continue;
                        }
                    }

                    var game = new Game()
                    {
                        PluginId = Id,
                        Source = "Battle.net",
                        GameId = product.ProductId,
                        Name = product.Name,
                        State = new GameState() { Installed = false }
                    };

                    games.Add(game);
                }

                return games;
            }
        }

        #region ILibraryPlugin

        public string LibraryIcon { get; }

        public string Name { get; } = "Battle.net";

        public UserControl SettingsView
        {
            get => new BattleNetLibrarySettingsView();
        }

        public ISettings Settings { get; private set; }

        public Guid Id { get; } = Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD");

        public void Dispose()
        {

        }

        public IGameController GetGameController(Game game)
        {
            return new BattleNetGameController(game, playniteApi);
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = new Dictionary<string, Game>();

            if (LibrarySettings.ImportInstalledGames)
            {
                installedGames = GetInstalledGames();
                allGames.AddRange(installedGames.Values.ToList());
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                var uninstalled = GetLibraryGames();
                foreach (var game in uninstalled)
                {
                    if (installedGames.TryGetValue(game.GameId, out var installed))
                    {
                        installed.Playtime = game.Playtime;
                        installed.LastActivity = game.LastActivity;
                    }
                    else
                    {
                        allGames.Add(game);
                    }
                }
            }

            return allGames;
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return new BattleNetMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
