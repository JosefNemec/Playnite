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
    // TODO move everything to GameDatabase instance
    public class GameLibrary
    {
        private static ILogger logger = LogManager.GetLogger();

        private static string AddNewGameFile(string path, Guid gameId, GameDatabase database)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(path);
            MetadataFile metaFile = null;

            try
            {
                if (path.IsHttpUrl())
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

        private static Game GameInfoToGame(GameInfo game, GameDatabase database, Guid pluginId)
        {
            var toAdd = new Game()
            {
                PluginId = pluginId,
                Name = game.Name,
                GameId = game.GameId,
                Description = game.Description,
                InstallDirectory = game.InstallDirectory,
                GameImagePath = game.GameImagePath,
                SortingName = game.SortingName,
                OtherActions = new ObservableCollection<GameAction>(game.OtherActions),
                PlayAction = game.PlayAction,
                ReleaseDate = game.ReleaseDate,
                Links = new ObservableCollection<Link>(game.Links),
                IsInstalled = game.IsInstalled,
                Playtime = game.Playtime,
                PlayCount = game.PlayCount,
                LastActivity = game.LastActivity,
                Version = game.Version,
                CompletionStatus = game.CompletionStatus,
                UserScore = game.UserScore,
                CriticScore = game.CriticScore,
                CommunityScore = game.CommunityScore
            };

            if (string.IsNullOrEmpty(game.Platform))
            {
                database.AssignPcPlatform(toAdd);
            }
            else
            {
                toAdd.PlatformId = database.Platforms.Add(game.Platform);
            }

            if (game.Developers?.Any() == true)
            {
                toAdd.DeveloperIds = database.Companies.Add(game.Developers).ToComparable();
            }

            if (game.Publishers?.Any() == true)
            {
                toAdd.PublisherIds = database.Companies.Add(game.Publishers).ToComparable();
            }

            if (game.Genres?.Any() == true)
            {
                toAdd.GenreIds = database.Genres.Add(game.Genres).ToComparable();
            }

            if (game.Categories?.Any() == true)
            {
                toAdd.CategoryIds = database.Categories.Add(game.Categories).Select(a => a.Id).ToComparable();
            }

            if (game.Tags?.Any() == true)
            {
                toAdd.TagIds = database.Tags.Add(game.Tags).ToComparable();
            }

            if (!string.IsNullOrEmpty(game.AgeRating))
            {
                toAdd.AgeRatingId = database.AgeRatings.Add(game.AgeRating);
            }

            if (!string.IsNullOrEmpty(game.Series))
            {
                toAdd.SeriesId = database.Series.Add(game.Series);
            }

            if (!string.IsNullOrEmpty(game.Region))
            {
                toAdd.RegionId = database.Regions.Add(game.Region);
            }

            if (!string.IsNullOrEmpty(game.Source))
            {
                toAdd.SourceId = database.Sources.Add(game.Source);
            }
            
            return toAdd;
        }

        public static Game ImportGame(GameInfo game, GameDatabase database)
        {
            return ImportGame(game, database, Guid.Empty);
        }

        public static Game ImportGame(GameInfo game, GameDatabase database, Guid pluginId)
        {
            var toAdd = GameInfoToGame(game, database, pluginId);
            toAdd.Icon = AddNewGameFile(game.Icon, game.Id, database);
            toAdd.CoverImage = AddNewGameFile(game.CoverImage, game.Id, database);
            if (!string.IsNullOrEmpty(game.BackgroundImage) && !game.BackgroundImage.IsHttpUrl())
            {
                toAdd.BackgroundImage = AddNewGameFile(game.BackgroundImage, game.Id, database);
            }

            database.Games.Add(toAdd);
            return toAdd;
        }

        public static Game ImportGame(GameMetadata metadata, GameDatabase database)
        {
            var toAdd = GameInfoToGame(metadata.GameInfo, database, Guid.Empty);
            if (metadata.Icon != null)
            {
                toAdd.Icon = database.AddFile(metadata.Icon, toAdd.Id);
            }

            if (metadata.CoverImage != null)
            {
                toAdd.CoverImage = database.AddFile(metadata.CoverImage, toAdd.Id);
            }

            if (metadata.BackgroundImage != null)
            {
                if (metadata.BackgroundImage.Content == null)
                {
                    toAdd.BackgroundImage = metadata.BackgroundImage.OriginalUrl;
                }
                else
                {
                    toAdd.BackgroundImage = database.AddFile(metadata.BackgroundImage, toAdd.Id);
                }
            }

            database.Games.Add(toAdd);
            return toAdd;
        }

        public static IEnumerable<Game> ImportGames(ILibraryPlugin library, GameDatabase database)
        {
            foreach (var newGame in library.GetGames())
            {
                var existingGame = database.Games.FirstOrDefault(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                if (existingGame == null)
                {
                    logger.Info(string.Format("Adding new game {0} from {1} plugin", newGame.GameId, library.Name));                    
                    yield return ImportGame(newGame, database, library.Id);
                }
                else
                {
                    existingGame.IsInstalled = newGame.IsInstalled;
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
                        existingGame.OtherActions = new ObservableCollection<GameAction>(newGame.OtherActions);
                    }

                    database.Games.Update(existingGame);
                }
            }
        }
    }
}
