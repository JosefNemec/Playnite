using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GameLibrary
    {
        private static ILogger logger = LogManager.GetLogger();

        private static string AddNewGameFile(string path, Guid gameId, GameDatabase database)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(path);
            MetadataFile metaFile = null;

            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    metaFile = new MetadataFile(fileName, HttpDownloader.DownloadData(path));
                }
                else
                {
                    if (File.Exists(path))
                    {
                        if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            var icon = IconExtension.ExtractIconFromExe(path, true);
                            if (icon == null)
                            {
                                return null;
                            }

                            fileName = Path.ChangeExtension(fileName, ".png");
                            metaFile = new MetadataFile(fileName, icon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                        }
                        else
                        {
                            metaFile = new MetadataFile(fileName, File.ReadAllBytes(path));
                        }
                    }
                    else
                    {
                        logger.Error($"Can't add game file during game import, file doesn't exists: {path}");
                    }
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to import game file during game import from {path}");
            }

            if (metaFile != null)
            {
                if (metaFile.FileName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    metaFile.FileName = Path.ChangeExtension(metaFile.FileName, ".png");
                    metaFile.Content = BitmapExtensions.TgaToBitmap(metaFile.Content).ToPngArray();
                }
                
                return database.AddFile(metaFile, gameId);
            }

            return null;
        }

        public static IEnumerable<Game> ImportGames(ILibraryPlugin library, GameDatabase database)
        {
            foreach (var newGame in library.GetGames())
            {
                var existingGame = database.Games.FirstOrDefault(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                if (existingGame == null)
                {
                    logger.Info(string.Format("Adding new game {0} from {1} plugin", newGame.GameId, library.Name));
                    if (!string.IsNullOrEmpty(newGame.Icon))
                    {
                        newGame.Icon = AddNewGameFile(newGame.Icon, newGame.Id, database);
                    }

                    if (!string.IsNullOrEmpty(newGame.CoverImage))
                    {
                        newGame.CoverImage = AddNewGameFile(newGame.CoverImage, newGame.Id, database);
                    }

                    if (!string.IsNullOrEmpty(newGame.BackgroundImage))
                    {
                        if (!newGame.BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            newGame.BackgroundImage = AddNewGameFile(newGame.BackgroundImage, newGame.Id, database);
                        }
                    }

                    database.AssignPcPlatform(newGame);
                    database.Games.Add(newGame);
                    yield return newGame;
                }
                else
                {
                    existingGame.State.Installed = newGame.State.Installed;
                    existingGame.InstallDirectory = newGame.InstallDirectory;
                    if (existingGame.PlayAction == null || existingGame.PlayAction.IsHandledByPlugin)
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

                    database.Games.Update(existingGame);
                }
            }
        }
    }
}
