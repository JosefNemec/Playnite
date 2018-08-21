using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GameLibrary
    {
        private static ILogger logger = LogManager.GetLogger();

        public static IEnumerable<Game> ImportGames(ILibraryPlugin library, GameDatabase database)
        {
            foreach (var newGame in library.GetGames())
            {
                var existingGame = database.GamesCollection.FindOne(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                if (existingGame == null)
                {
                    logger.Info(string.Format("Adding new game {0} from {1} provider", newGame.GameId, library.Name));
                    database.AssignPcPlatform(newGame);
                    database.AddGame(newGame);
                    yield return newGame;
                }
                else
                {
                    existingGame.State.Installed = newGame.State.Installed;
                    existingGame.InstallDirectory = newGame.InstallDirectory;
                    if (existingGame.PlayAction == null)
                    {
                        existingGame.PlayAction = newGame.PlayAction;
                    }

                    if (existingGame.Playtime == 0 && newGame.Playtime > 0)
                    {
                        existingGame.Playtime = newGame.Playtime;
                        if (existingGame.CompletionStatus == CompletionStatus.NotPlayed)
                        {
                            existingGame.CompletionStatus = CompletionStatus.Played;
                        }

                        if (existingGame.LastActivity == null && newGame.LastActivity != null)
                        {
                            existingGame.LastActivity = newGame.LastActivity;
                        }
                    }
                    
                    if (existingGame.OtherActions?.Any() != true && newGame.OtherActions?.Any() == true)
                    {
                        existingGame.OtherActions = newGame.OtherActions;
                    }

                    database.UpdateGameInDatabase(existingGame);
                }
            }
        }
    }
}
