using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ItchioLibrary
{
    public class ItchioLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "itchiolibImportError";
        internal readonly ItchioLibrarySettings LibrarySettings;

        public ItchioLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibrarySettings = new ItchioLibrarySettings(this, api);
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\itchioicon.png");
        }

        internal Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
            using (var butler = new Butler())
            {
                var caves = butler.GetCaves();
                foreach (var cave in caves)
                {
                    // We don't support multiple version of one game at moment
                    if (games.ContainsKey(cave.game.id.ToString()))
                    {
                        continue;
                    }

                    var game = new Game()
                    {
                        PluginId = Id,
                        GameId = cave.game.id.ToString(),
                        Name = cave.game.title,
                        InstallDirectory = cave.installInfo.installFolder,
                        IsInstalled = true,
                        PlayAction = new GameAction()
                        {
                            Type = GameActionType.URL,
                            Path = @"itch://library/",
                            IsHandledByPlugin = true
                        },
                        CoverImage = cave.game.coverUrl
                    };

                    games.Add(game.GameId, game);
                }
            }

            return games;
        }

        internal List<Game> GetLibraryGames()
        {
            var games = new List<Game>();
            using (var butler = new Butler())
            {
                var profiles = butler.GetProfiles();
                if (profiles?.Any() != true)
                {
                    throw new Exception("User is not authenticated.");
                }

                var keys = butler.GetOwnedKeys(profiles.First().id);
                foreach (var key in keys)
                {
                    var game = new Game()
                    {
                        PluginId = Id,
                        GameId = key.game.id.ToString(),
                        Name = key.game.title, 
                        CoverImage = key.game.coverUrl
                    };

                    games.Add(game);
                }
            }

            return games;
        }

        #region ILibraryPlugin

        public ILibraryClient Client { get; } = new ItchioClient();

        public string LibraryIcon { get; }

        public string Name { get; } = "itch.io";

        public bool IsClientInstalled => Itch.IsInstalled;

        public Guid Id { get; } = Guid.Parse("00000001-EBB2-4EEC-ABCB-7C89937A42BB");

        public void Dispose()
        {

        }
               
        public IGameController GetGameController(Game game)
        {
            return null;
        }

        public IEnumerable<Game> GetGames()
        {
            var allGames = new List<Game>();
            var installedGames = new Dictionary<string, Game>();
            Exception importError = null;

            if (LibrarySettings.ImportInstalledGames)
            {
                try
                {
                    installedGames = GetInstalledGames();
                    logger.Debug($"Found {installedGames.Count} installed itch.io games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e) when (false)
                {
                    logger.Error(e, "Failed to import installed itch.io games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = GetLibraryGames();
                    logger.Debug($"Found {uninstalled.Count} library itch.io games.");

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
                catch (Exception e) when (false)
                {
                    logger.Error(e, "Failed to import uninstalled itch.io games.");
                    importError = e;
                }
            }

            if (importError != null)
            {
                playniteApi.Notifications.Add(
                    dbImportMessageId,
                    string.Format(playniteApi.Resources.FindString("LOCLibraryImportError"), Name) +
                    System.Environment.NewLine + importError.Message,
                    NotificationType.Error);
            }
            else
            {
                playniteApi.Notifications.Remove(dbImportMessageId);
            }

            return allGames;
        }

        public ILibraryMetadataProvider GetMetadataDownloader()
        {
            return null;
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new ItchioLibrarySettingsView();
        }

        #endregion ILibraryPlugin
    }
}
