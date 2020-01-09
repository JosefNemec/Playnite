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

        public string GetAuthToken()
        {
            if (!Twitch.CookiesPath.IsNullOrEmpty() && File.Exists(Twitch.CookiesPath))
            {
                try
                {
                    using (var db = new Sqlite(Twitch.CookiesPath, SqliteOpenFlags.ReadOnly))
                    {
                        var cookies = db.Query<TwitchCookie>("SELECT * FROM 'cookies' WHERE name='auth-token'");
                        if (cookies.Count > 0)
                        {
                            return cookies[0].value;
                        }
                        else
                        {
                            logger.Error("No Twitch auth token found.");
                        }
                    }
                }
                catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to get Twitch auth token.");
                }
            }

            return null;
        }

        public List<GameInfo> GetLibraryGames()
        {
            var token = GetAuthToken();
            if (token.IsNullOrEmpty())
            {
                throw new Exception("Authentication is required.");
            }


            var games = new List<GameInfo>();
            var entitlements = AmazonEntitlementClient.GetAccountEntitlements(token);

            foreach (var item in entitlements)
            {
                if (item.product.productLine != "Twitch:FuelGame")
                {
                    continue;
                }

                var game = new GameInfo()
                {
                    Source = "Twitch",
                    GameId = item.product.id,
                    Name = item.product.title
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

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

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
                catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to import installed Twitch games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ConnectAccount)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library Twitch games.");

                    if (!LibrarySettings.ImportUninstalledGames)
                    {
                        libraryGames = libraryGames.Where(lg => installedGames.ContainsKey(lg.GameId)).ToList();
                    }

                    foreach (var game in libraryGames)
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
                catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to import linked account Twitch games details.");
                    importError = e;
                }
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

            return allGames;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new TwitchMetadataProvider(this);
        }

        #endregion ILibraryPlugin
    }
}
