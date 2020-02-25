using GogLibrary.Models;
using GogLibrary.Services;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GogLibrary
{
    public class GogLibrary : LibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "goglibImportError";

        internal GogLibrarySettings LibrarySettings { get; private set; }

        public GogLibrary(IPlayniteAPI api) : base(api)
        {
            LibrarySettings = new GogLibrarySettings(this, api);
        }

        internal Tuple<GameAction, List<GameAction>> GetGameTasks(string gameId, string installDir)
        {
            var gameInfoPath = Path.Combine(installDir, string.Format("goggame-{0}.info", gameId));
            if (!File.Exists(gameInfoPath))
            {
                return new Tuple<GameAction, List<GameAction>>(null, null);
            }

            var gameTaskData = JsonConvert.DeserializeObject<GogGameActionInfo>(File.ReadAllText(gameInfoPath));
            var playTask = gameTaskData.playTasks.FirstOrDefault(a => a.isPrimary)?.ConvertToGenericTask(installDir);
            if (playTask != null)
            {
                playTask.IsHandledByPlugin = true;
            }

            var otherTasks = new List<GameAction>();

            foreach (var task in gameTaskData.playTasks.Where(a => !a.isPrimary))
            {
                otherTasks.Add(task.ConvertToGenericTask(installDir));
            }

            if (gameTaskData.supportTasks != null)
            {
                foreach (var task in gameTaskData.supportTasks)
                {
                    otherTasks.Add(task.ConvertToGenericTask(installDir));
                }
            }

            return new Tuple<GameAction, List<GameAction>>(playTask, otherTasks.Count > 0 ? otherTasks : null);
        }

        internal Dictionary<string, GameInfo> GetInstalledGames()
        {
            var games = new Dictionary<string, GameInfo>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                var match = Regex.Match(program.RegistryKeyName, @"(\d+)_is1");
                if (!match.Success || program.Publisher != "GOG.com" || program.RegistryKeyName.StartsWith("GOGPACK"))
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var gameId = match.Groups[1].Value;
                var game = new GameInfo()
                {
                    InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                    GameId = gameId,
                    Source = "GOG",
                    Name = program.DisplayName,
                    IsInstalled = true
                };

                var tasks = GetGameTasks(game.GameId, game.InstallDirectory);
                // Empty play task = DLC
                if (tasks.Item1 == null)
                {
                    continue;
                }

                game.PlayAction = tasks.Item1;
                game.OtherActions = tasks.Item2;
                games.Add(game.GameId, game);
            }

            return games;
        }

        internal List<GameInfo> GetLibraryGames()
        {
            using (var view = PlayniteApi.WebViews.CreateOffscreenView())
            {
                var api = new GogAccountClient(view);
                if (!api.GetIsUserLoggedIn())
                {
                    throw new Exception("User is not logged in to GOG account.");
                }

                var libGames = api.GetOwnedGames();
                if (libGames == null)
                {
                    throw new Exception("Failed to obtain libary data.");
                }

                return LibraryGamesToGames(libGames).ToList();
            }
        }

        internal List<GameInfo> GetLibraryGames(string accountName)
        {
            var api = new GogAccountClient(null);
            var games = new List<GameInfo>();
            var libGames = api.GetOwnedGamesFromPublicAccount(accountName);
            if (libGames == null)
            {
                throw new Exception("Failed to obtain libary data.");
            }

            return LibraryGamesToGames(libGames).ToList();
        }

        internal IEnumerable<GameInfo> LibraryGamesToGames(List<LibraryGameResponse> libGames)
        {
            foreach (var game in libGames)
            {
                var newGame = new GameInfo()
                {
                    Source = "GOG",
                    GameId = game.game.id,
                    Name = game.game.title,
                    Links = new List<Link>()
                    {
                        new Link("Store", @"https://www.gog.com" + game.game.url)
                    }
                };

                if (game.stats?.Keys?.Any() == true)
                {
                    var acc = game.stats.Keys.First();
                    newGame.Playtime = game.stats[acc].playtime * 60;
                    newGame.LastActivity = game.stats[acc].lastSession;
                }

                yield return newGame;
            }
        }

        #region ILibraryPlugin

        public override LibraryClient Client => new GogClient();

        public override string Name => "GOG";

        public override string LibraryIcon => Gog.Icon;

        public override Guid Id => Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");

        public override LibraryPluginCapabilities Capabilities { get; } = new LibraryPluginCapabilities
        {
            CanShutdownClient = true
        };

        public override ISettings GetSettings(bool firstRunSettings)
        {
            LibrarySettings.IsFirstRunUse = firstRunSettings;
            return LibrarySettings;
        }

        public override UserControl GetSettingsView(bool firstRunView)
        {
            return new GogLibrarySettingsView();
        }

        public override IGameController GetGameController(Game game)
        {
            return new GogGameController(game, this, LibrarySettings, PlayniteApi);
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
                    logger.Debug($"Found {installedGames.Count} installed GOG games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed GOG games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ConnectAccount)
            {
                try
                {
                    var libraryGames = GetLibraryGames();
                    logger.Debug($"Found {libraryGames.Count} library GOG games.");

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
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import linked account GOG games details.");
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
            return new GogMetadataProvider(this);
        }

        #endregion ILibraryPlugin
    }
}
