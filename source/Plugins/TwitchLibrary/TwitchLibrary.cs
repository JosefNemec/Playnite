using Newtonsoft.Json;
using Playnite.Common.System;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using TwitchLibrary.Models;
using TwitchLibrary.Services;

namespace TwitchLibrary
{
    public class TwitchLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        internal readonly string TokensPath;

        internal TwitchLibrarySettings LibrarySettings
        {
            get => (TwitchLibrarySettings)Settings;
        }

        internal TwitchLoginData LoginData
        {
            get
            {
                if (!File.Exists(TokensPath))
                {
                    return null;
                }

                try
                {
                    return JsonConvert.DeserializeObject<TwitchLoginData>(File.ReadAllText(TokensPath));
                }
                catch (Exception e) when (!Environment.IsDebugBuild)
                {
                    logger.Error(e, "Failed to load twitch login information.");
                    return null;
                }
            }
        }

        public TwitchLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\twitchicon.png");
            Settings = new TwitchLibrarySettings(this, playniteApi);
            TokensPath = Path.Combine(api.GetPluginUserDataPath(this), "tokens.json");
        }

        public static GameAction GetPlayAction(string gameId)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = $"twitch://fuel-launch/{gameId}",
                IsHandledByPlugin = true
            };
        }

        internal Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                if (string.IsNullOrEmpty(program.UninstallString) || program.UninstallString.IndexOf("TwitchGameRemover", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var gameId = program.RegistryKeyName.Trim(new char[] { '{', '}' }).ToLower();
                var game = new Game()
                {
                    InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                    GameId = gameId,
                    PluginId = Id,
                    Source = "Twitch",
                    Name = program.DisplayName,
                    State = new GameState() { Installed = true },
                    PlayAction = GetPlayAction(gameId)
                };

                games.Add(game.GameId, game);
            }

            return games;
        }

        public List<Game> GetLibraryGames()
        {
            var login = LoginData;
            if (login == null)
            {
                throw new Exception("User is not logged in.");
            }

            var games = new List<Game>();
            var libraryGames = AmazonEntitlementClient.GetAccountEntitlements(login.AccountId, login.AccessToken);
            foreach (var item in libraryGames)
            {
                if (item.product.productLine != "Twitch:FuelGame")
                {
                    continue;
                }

                var game = new Game()
                {
                    PluginId = Id,
                    Source = "Twitch",
                    GameId = item.product.id,
                    Name = item.product.productTitle,
                    State = new GameState() { Installed = false }
                };

                games.Add(game);
            }

            return games;            
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new TwitchClient();

        public UserControl SettingsView
        {
            get => new TwitchLibrarySettingsView();
        }

        public ISettings Settings { get; private set; }

        public string Name { get; } = "Twitch";

        public string LibraryIcon { get; }

        public Guid Id { get; } = Guid.Parse("E2A7D494-C138-489D-BB3F-1D786BEEB675");

        public void Dispose()
        {

        }

        public IGameController GetGameController(Game game)
        {
            return new TwitchGameController(game, this, playniteApi);
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
            return new TwitchMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
