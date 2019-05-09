using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public class TwitchLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        internal readonly string TokensPath;
        private const string dbImportMessageId = "twitchlibImportError";

        internal TwitchLibrarySettings LibrarySettings { get; private set; }

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

        public TwitchLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new TwitchLibrarySettings(this, PlayniteApi);
            TokensPath = Path.Combine(GetPluginUserDataPath(), "tokens.json");
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

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                if (string.IsNullOrEmpty(program.UninstallString) || program.UninstallString.IndexOf("TwitchGameRemover", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var gameId = program.RegistryKeyName.Trim(new char[] { '{', '}' }).ToLower();
                if (!games.ContainsKey(gameId))
                {
                    var game = new GameInfo()
                    {
                        InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                        GameId = gameId,
                        Source = "Twitch",
                        Name = program.DisplayName,
                        IsInstalled = true,
                        PlayAction = GetPlayAction(gameId)
                    };

                    games.Add(game.GameId, game);
                }
            }

            return games;
        }

        public List<GameInfo> GetLibraryGames()
        {
            var login = LoginData;
            if (login == null)
            {
                throw new Exception("User is not logged in.");
            }

            var games = new List<GameInfo>();
            List<GoodsItem> libraryGames = null;

            try
            {
                libraryGames = AmazonEntitlementClient.GetAccountEntitlements(login.AccountId, login.AccessToken);
            }
            catch (WebException libExc)
            {
                // Token renew doesn't properly based on expiration date, so try always to renew token for now until it's fixed.
                logger.Warn(libExc, "Failed to download Twitch library at first attempt.");
                try
                {
                    var client = new TwitchAccountClient(null, TokensPath);
                    client.RenewTokens(login.AuthenticationToken, login.AccountId);
                    login = LoginData;
                }
                catch (Exception renewExc)
                {
                    logger.Error(renewExc, "Failed to renew Twitch authentication.");
                }

                try
                {
                    libraryGames = AmazonEntitlementClient.GetAccountEntitlements(login.AccountId, login.AccessToken);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to download Twitch library.");
                    throw new Exception("Authentication is required.");
                }
            }

            foreach (var item in libraryGames)
            {
                if (item.product.productLine != "Twitch:FuelGame")
                {
                    continue;
                }

                var game = new GameInfo()
                {
                    Source = "Twitch",
                    GameId = item.product.id,
                    Name = item.product.productTitle
                };

                games.Add(game);
            }

            return games;            
        }

        #region ILibraryPlugin

        public override LibraryClient Client => new TwitchClient();
        
        public override string Name => "Twitch";

        public override string LibraryIcon => Twitch.Icon;

        public override Guid Id => Guid.Parse("E2A7D494-C138-489D-BB3F-1D786BEEB675");

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new TwitchLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new TwitchGameController(game, this, PlayniteApi);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new Dictionary<string, GameInfo>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Twitch games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Twitch games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = GetLibraryGames();
                    logger.Debug($"Found {uninstalled.Count} library Twitch games.");

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
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import uninstalled Twitch games.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                PlayniteApi.Notifications.Add(
                    dbImportMessageId,
                    string.Format(PlayniteApi.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error);
            }
            else
            {
                PlayniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new TwitchMetadataProvider();
        }

        #endregion ILibraryPlugin
    }
}
