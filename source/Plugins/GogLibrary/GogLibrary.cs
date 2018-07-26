using GogLibrary.Models;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GogLibrary
{
    public class GogLibrary : ILibraryPlugin
    {
        private ILogger logger;
        private readonly IPlayniteAPI playniteApi;

        public GogLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            logger = playniteApi.CreateLogger("GogLibrary");
            var configPath = Path.Combine(api.GetPluginStoragePath(this), "config.json");
            Settings = new GogLibrarySettings(configPath);
        }

        internal Tuple<GameAction, ObservableCollection<GameAction>> GetGameTasks(string gameId, string installDir)
        {
            var gameInfoPath = Path.Combine(installDir, string.Format("goggame-{0}.info", gameId));
            var gameTaskData = JsonConvert.DeserializeObject<GogGameActionInfo>(File.ReadAllText(gameInfoPath));
            var playTask = gameTaskData.playTasks.FirstOrDefault(a => a.isPrimary)?.ConvertToGenericTask(installDir);
            playTask.IsHandledByPlugin = true;
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

        internal List<Game> GetInstalledGames()
        {
            var games = new List<Game>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                var match = Regex.Match(program.RegistryKeyName, @"(\d+)_is1");
                if (!match.Success || program.Publisher != "GOG.com")
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
                    Name = program.DisplayName
                };
   
                var tasks = GetGameTasks(game.GameId, game.InstallDirectory);
                // Empty play task = DLC
                if (tasks.Item1 == null)
                {
                    continue;
                }

                game.PlayAction = tasks.Item1;
                game.OtherActions = tasks.Item2;
                games.Add(game);
            }

            return games;
        }

        public List<Game> GetLibraryGames()
        {
            //using (var api = new WebApiClient())
            //{
            //    if (api.GetLoginRequired())
            //    {
            //        throw new Exception("User is not logged in.");
            //    }

            //    var games = new List<Game>();
            //    var acc = api.GetAccountInfo();
            //    var libGames = api.GetOwnedGames(acc);
            //    if (libGames == null)
            //    {
            //        throw new Exception("Failed to obtain libary data.");
            //    }

            //    foreach (var game in libGames)
            //    {
            //        var newGame = new Game()
            //        {
            //            Provider = Provider.GOG,
            //            Source = Enums.GetEnumDescription(Provider.GOG),
            //            GameId = game.game.id,
            //            Name = game.game.title,
            //            Links = new ObservableCollection<Link>()
            //                    {
            //                        new Link("Store", @"https://www.gog.com" + game.game.url)
            //                    }
            //        };

            //        if (game.stats != null && game.stats.ContainsKey(acc.userId))
            //        {
            //            newGame.Playtime = game.stats[acc.userId].playtime * 60;
            //            newGame.LastActivity = game.stats[acc.userId].lastSession;
            //        }

            //        games.Add(newGame);
            //    }

            //    return games;
            //}

            return null;
        }


        #region ILibraryPlugin

        public UserControl SettingsView
        {
            get => new GogLibrarySettingsView();
        }

        public IEditableObject Settings { get; private set; }

        public string Name { get; } = "GOG";

        public Guid Id { get; } = Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E");

        public void Dispose()
        {

        }

        public IGameController GetGameController(Game game)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Game> GetGames()
        {
            return GetInstalledGames();
        }

        public IMetadataProvider GetMetadataDownloader()
        {
            throw new NotImplementedException();
        }

        #endregion ILibraryPlugin

    }
}
