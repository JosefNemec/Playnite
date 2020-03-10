using AmazonGamesLibrary.Services;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AmazonGamesLibrary
{
    public class AmazonGamesLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        internal readonly string TokensPath;
        private const string dbImportMessageId = "amazonlibImportError";

        public override Guid Id { get; } = Guid.Parse("402674cd-4af6-4886-b6ec-0e695bfa0688");

        internal AmazonGamesLibrarySettings LibrarySettings { get; private set; }

        public override LibraryClient Client => new AmazonGamesLibraryClient();

        public override string Name => "Amazon Games";

        public override string LibraryIcon => AmazonGames.Icon;

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

        public AmazonGamesLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new AmazonGamesLibrarySettings(this, PlayniteApi);
            TokensPath = Path.Combine(GetPluginUserDataPath(), "tokens.json");
        }

        public static GameAction GetPlayAction(string gameId)
        {
            return new GameAction()
            {
                Type = GameActionType.URL,
                Path = $"amazon-games://play/{gameId}",
                IsHandledByPlugin = true
            };
        }

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                if (program.UninstallString?.Contains("Amazon Game Remover.exe") != true)
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var match = Regex.Match(program.UninstallString, @"-p\s+([a-zA-Z0-9\-]+)");
                var gameId = match.Groups[1].Value;
                if (!games.ContainsKey(gameId))
                {
                    var game = new GameInfo()
                    {
                        InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                        GameId = gameId,
                        Source = "Amazon",
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
            var games = new List<GameInfo>();
            var client = new AmazonAccountClient(this);
            var entitlements = client.GetAccountEntitlements().GetAwaiter().GetResult();
            foreach (var item in entitlements)
            {
                if (item.product.productLine == "Twitch:FuelEntitlement")
                {
                    continue;
                }

                var game = new GameInfo()
                {
                    Source = "Amazon",
                    GameId = item.product.id,
                    Name = item.product.title
                };

                games.Add(game);
            }

            return games;
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new AmazonGamesLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new AmazonGameController(game, this, PlayniteApi);
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
                    logger.Debug($"Found {installedGames.Count} installed Amazon games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e) when (!PlayniteApi.ApplicationInfo.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to import installed Amazon games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ConnectAccount)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library Amazon games.");

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
                    logger.Error(e, "Failed to import linked account Amazon games details.");
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
            return new AmazonGamesMetadataProvider(this);
        }
    }
}