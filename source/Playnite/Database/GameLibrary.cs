using NLog;
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
        public static Logger logger = LogManager.GetCurrentClassLogger();

        public static IEnumerable<Game> ImportGames(ILibraryPlugin library, GameDatabase database)
        {
            foreach (var newGame in library.GetGames())
            {
                var existingGame = database.GamesCollection.FindOne(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                if (existingGame == null)
                {
                    logger.Info("Adding new game {0} from {1} provider", newGame.GameId, newGame.PluginId);
                    newGame.State.Installed = true;
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

                    // Don't import custom action if imported already (user may changed them manually and this would overwrite it)
                    if (existingGame.OtherActions?.FirstOrDefault(a => a.IsHandledByPlugin) == null && newGame.OtherActions != null)
                    {
                        if (existingGame.OtherActions == null)
                        {
                            existingGame.OtherActions = new ObservableCollection<GameAction>();
                        }
                        else
                        {
                            existingGame.OtherActions = new ObservableCollection<GameAction>(existingGame.OtherActions.Where(a => !a.IsHandledByPlugin));
                        }

                        foreach (var task in newGame.OtherActions.Reverse())
                        {
                            existingGame.OtherActions.Insert(0, task);
                        }
                    }

                    database.UpdateGameInDatabase(existingGame);
                }
            }
        }
    }
}
