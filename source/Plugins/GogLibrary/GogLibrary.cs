using GogLibrary.Models;
using GogLibrary.Services;
using Newtonsoft.Json;
using Playnite.Common.System;
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
    public class GogLibrary : ILibraryPlugin
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "goglibImportError";

        internal GogLibrarySettings LibrarySettings { get; private set; }

        public GogLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            LibrarySettings = new GogLibrarySettings(this, api);
            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\gogicon.png");
        }

        internal Tuple<GameAction, ObservableCollection<GameAction>> GetGameTasks(string gameId, string installDir)
        {
            var gameInfoPath = Path.Combine(installDir, string.Format("goggame-{0}.info", gameId));
            if (!File.Exists(gameInfoPath))
            {
                return new Tuple<GameAction, ObservableCollection<GameAction>>(null, null);
            }

            var gameTaskData = JsonConvert.DeserializeObject<GogGameActionInfo>(File.ReadAllText(gameInfoPath));
            var playTask = gameTaskData.playTasks.FirstOrDefault(a => a.isPrimary)?.ConvertToGenericTask(installDir);
            if (playTask != null)
            {
                playTask.IsHandledByPlugin = true;
            }

            var otherTasks = new ObservableCollection<GameAction>();

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

            return new Tuple<GameAction, ObservableCollection<GameAction>>(playTask, otherTasks.Count > 0 ? otherTasks : null);
        }

        internal Dictionary<string, Game> GetInstalledGames()
        {
            var games = new Dictionary<string, Game>();
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
                var game = new Game()
                {
                    InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                    GameId = gameId,
                    PluginId = Id,
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

        internal List<Game> GetLibraryGames()
        {
            using (var view = playniteApi.WebViews.CreateOffscreenView())
            {
                var api = new GogAccountClient(view);
                if (!api.GetIsUserLoggedIn())
                {
                    throw new Exception("User is not logged in to GOG account.");
                }

                var accInfo = api.GetAccountInfo();                
                var libGames = api.GetOwnedGames(accInfo);
                if (libGames == null)
                {
                    throw new Exception("Failed to obtain libary data.");
                }

                return LibraryGamesToGames(libGames).ToList();
            }
        }

        internal List<Game> GetLibraryGames(string accountName)
        {
            var api = new GogAccountClient(null);
            var games = new List<Game>();
            var libGames = api.GetOwnedGamesFromPublicAccount(accountName);
            if (libGames == null)
            {
                throw new Exception("Failed to obtain libary data.");
            }

            return LibraryGamesToGames(libGames).ToList();
        }

        internal IEnumerable<Game> LibraryGamesToGames(List<LibraryGameResponse> libGames)
        {
            foreach (var game in libGames)
            {
                var newGame = new Game()
                {
                    PluginId = Id,
                    Source = "GOG",
                    GameId = game.game.id,
                    Name = game.game.title,
                    Links = new ObservableCollection<Link>()
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

        public ILibraryClient Client { get; } = new GogClient();

        public string Name { get; } = "GOG";

        public string LibraryIcon { get; }

        public Guid Id { get; } = Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");

        public bool IsClientInstalled => Gog.IsInstalled;

        public void Dispose()
        {
            
        }

        public ISettings GetSettings(bool firstRunSettings)
        {
            return LibrarySettings;
        }

        public UserControl GetSettingsView(bool firstRunView)
        {
            return new GogLibrarySettingsView();
        }

        public IGameController GetGameController(Game game)
        {
            return new GogGameController(game, this, LibrarySettings, playniteApi);
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
                    logger.Debug($"Found {installedGames.Count} installed GOG games.");
                    allGames.AddRange(installedGames.Values.ToList());
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to import installed Origin games.");
                    importError = e;
                }
            }

            if (LibrarySettings.ImportUninstalledGames)
            {
                try
                {
                    var uninstalled = LibrarySettings.UsePublicAccount ? GetLibraryGames(LibrarySettings.AccountName) : GetLibraryGames();
                    logger.Debug($"Found {uninstalled.Count} library GOG games.");

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
                    logger.Error(e, "Failed to import uninstalled GOG games.");
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
            return new GogMetadataProvider();
        }

        #endregion ILibraryPlugin

    }
}
