using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscordLibrary.Services;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace DiscordLibrary
{
    public class DiscordLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private const string dbImportMessageId = "discordlibImportError";
        internal readonly DiscordLibrarySettings librarySettings;

        public DiscordLibrary(IPlayniteAPI api) : base(api)
        {
            this.api = api;
            librarySettings = new DiscordLibrarySettings(this, api);
        }

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            foreach (var program in Programs.GetUnistallProgramsList())
            {
                if (string.IsNullOrEmpty(program.UninstallString))
                {
                    continue;
                }

                var match = Regex.Match(program.UninstallString, @"cmd /c start discord:///library/(\d+)/uninstall");
                if (!match.Success)
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var gameId = match.Groups[1].Value;
                if (games.ContainsKey(gameId))
                {
                    // There are duplicate keys for each game under Wow6432Node, just skip all duplicates
                    continue;
                }

                var game = new GameInfo()
                {
                    InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                    GameId = gameId,
                    Source = Name,
                    Name = program.DisplayName,
                    IsInstalled = true,
                    PlayAction = CreatePlayTask(gameId)
                };

                games.Add(game.GameId, game);
            }

            return games;
        }

        internal static GameAction CreatePlayTask(string gameId)
        {
            return new GameAction()
            {
                Name = "Play",
                Type = GameActionType.URL,
                Path = $"discord:///library/{gameId}/launch",
                IsHandledByPlugin = true
            };
        }

        #region ILibraryPlugin

        public override LibraryClient Client { get; } = new DiscordClient();

        public override string LibraryIcon => Discord.Icon;

        public override string Name { get; } = "Discord";

        public override Guid Id { get; } = Guid.Parse("6f3c4ba7-6540-4e0c-834a-d119f71ed81e");

        public override void Dispose()
        {

        }

        public override IGameController GetGameController(Game game)
        {
            return new DiscordGameController(game, this);
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            var allGames = new List<GameInfo>();
            var installedGames = new Dictionary<string, GameInfo>();
            Exception importError = null;

            if (librarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed Discord games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Discord games.");
                    importError = e;
                }
            }

            if (librarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = GetLibraryGames();
                    logger.Debug($"Found {uninstalled.Count} library Discord games.");

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
                    logger.Error(e, "Failed to import uninstalled Discord games.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                api.Notifications.Add(
                    dbImportMessageId,
                    string.Format(api.Resources.GetString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error);
            }
            else
            {
                api.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        private List<GameInfo> GetLibraryGames()
        {
            var token = librarySettings.ApiToken;
            if (token == null)
            {
                throw new Exception("User is not logged in.");
            }

            var library = DiscordAccountClient.GetLibrary(token);
            var lastPlayed = DiscordAccountClient.GetLastPlayed(token).ToDictionary(x => x.application_id);

            var games = new List<GameInfo>();
            foreach (var libraryGame in library)
            {
                var applicationId = libraryGame.application.id;
                var game = new GameInfo()
                {
                    GameId = applicationId.ToString(),
                    Source = Name,
                    Name = libraryGame.application.name,
                    PlayAction = CreatePlayTask(applicationId.ToString())
                };

                if (lastPlayed.TryGetValue(applicationId, out var gameLastPlayed)) {
                    game.LastActivity = gameLastPlayed.last_played_at;
                    game.Playtime = gameLastPlayed.total_discord_sku_duration;
                }
    
                games.Add(game);
            }

            return games;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new DiscordMetadataProvider(this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return librarySettings;
        }

        public override System.Windows.Controls.UserControl GetSettingsView(bool firstRunView)
        {
            return new DiscordLibrarySettingsView();
        }

        #endregion
    }
}
