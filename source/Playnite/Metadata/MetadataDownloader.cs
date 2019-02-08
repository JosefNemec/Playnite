using Playnite.Database;
using Playnite.Metadata.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Metadata;
using System.Collections.Concurrent;

namespace Playnite.Metadata
{
    public class MetadataDownloader : IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();

        private GameDatabase database;
        private ILibraryMetadataProvider igdbProvider;
        private readonly IEnumerable<ILibraryPlugin> plugins;
        private ConcurrentDictionary<Guid, ILibraryMetadataProvider> downloaders = new ConcurrentDictionary<Guid, ILibraryMetadataProvider>();

        public MetadataDownloader(GameDatabase database, IEnumerable<ILibraryPlugin> plugins) : this(database, new IGDBMetadataProvider(), plugins)
        {
        }

        public MetadataDownloader(GameDatabase database, ILibraryMetadataProvider igdbProvider, IEnumerable<ILibraryPlugin> plugins)
        {
            this.igdbProvider = igdbProvider;
            this.plugins = plugins;
            this.database = database;
        }

        public void Dispose()
        {
            foreach (var downloader in downloaders.Values)
            {                
                // TODO move to proper disposable
                if (downloader?.HasMethod("Dispose") == true)
                {
                    (downloader as dynamic)?.Dispose();
                }
            }
        }

        internal ILibraryMetadataProvider GetMetadataDownloader(Guid pluginId)
        {
            if (downloaders.ContainsKey(pluginId))
            {
                return downloaders[pluginId];
            }

            var plugin = plugins?.FirstOrDefault(a => a.Id == pluginId);
            if (plugin == null)
            {
                downloaders.TryAdd(pluginId, null);
                return null;
            }

            try
            {
                var downloader = plugin.GetMetadataDownloader();
                downloaders.TryAdd(pluginId, downloader);
                return downloader;
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to get metadata downloader from {plugin.Name}");
                downloaders.TryAdd(pluginId, null);
                return null;
            }
        }

        private GameMetadata ProcessDownload(Game game, ref GameMetadata data)
        {
            if (data == null)
            {
                var downloader = GetMetadataDownloader(game.PluginId);
                try
                {
                    data = downloader?.GetMetadata(game);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to download metadat from plugin downloader.");
                    data = null;
                }
            }

            return data;
        }

        private GameMetadata ProcessField(
            Game game,
            MetadataFieldSettings field,
            ref GameMetadata storeData,
            ref GameMetadata igdbData,
            Func<GameMetadata, object> propertySelector)
        {
            if (field.Import)
            {
                if (field.Source == MetadataSource.Store && !game.IsCustomGame)
                {
                    storeData = ProcessDownload(game, ref storeData);
                    return storeData;
                }
                else if (field.Source == MetadataSource.IGDB)
                {
                    if (igdbData == null)
                    {
                        igdbData = igdbProvider.GetMetadata(game);
                    }

                    return igdbData;
                }
                else if (field.Source == MetadataSource.IGDBOverStore)
                {
                    if (igdbData == null)
                    {
                        igdbData = igdbProvider.GetMetadata(game);
                    }

                    if (igdbData?.GameData == null && !game.IsCustomGame)
                    {
                        if (storeData == null)
                        {
                            storeData = ProcessDownload(game, ref storeData);
                        }

                        return storeData;
                    }
                    else
                    {
                        var igdbValue = propertySelector(igdbData);
                        if (igdbValue != null)
                        {
                            return igdbData;
                        }
                        else if (!game.IsCustomGame)
                        {
                            if (storeData == null)
                            {
                                storeData = ProcessDownload(game, ref storeData);
                            }

                            if (storeData?.GameData != null)
                            {
                                return storeData;
                            }
                            else
                            {
                                return igdbData;
                            }
                        }
                    }
                }
                else if (field.Source == MetadataSource.StoreOverIGDB)
                {
                    if (!game.IsCustomGame)
                    {
                        if (storeData == null)
                        {
                            storeData = ProcessDownload(game, ref storeData);
                        }

                        if (storeData?.GameData == null)
                        {
                            if (igdbData == null)
                            {
                                igdbData = igdbProvider.GetMetadata(game);
                            }

                            return igdbData;
                        }
                        else
                        {
                            var storeValue = propertySelector(storeData);
                            if (storeValue != null)
                            {
                                return storeData;
                            }
                            else
                            {
                                if (igdbData == null)
                                {
                                    igdbData = igdbProvider.GetMetadata(game);
                                }

                                return igdbData;
                            }
                        }
                    }
                    else
                    {
                        if (igdbData == null)
                        {
                            igdbData = igdbProvider.GetMetadata(game);
                        }

                        return igdbData;
                    }
                }
            }

            return null;
        }

        public Task DownloadMetadataGroupedAsync(
            List<Game> games,
            MetadataDownloaderSettings settings,
            Action<Game, int, int> processCallback,
            CancellationTokenSource cancelToken)
        {
            int index = 0;
            int total = games.Count;
            var tasks = new List<Task>(); 
            var grouped = games.GroupBy(a => a.PluginId);
            logger.Info($"Downloading metadata using {grouped.Count()} threads.");
            foreach (IGrouping<Guid, Game> group in grouped)
            {
                tasks.Add(DownloadMetadataAsync(group.ToList(), settings, (g, i, t) =>
                {
                    index++;
                    processCallback?.Invoke(g, index, total);
                }, cancelToken));
            }

            return Task.WhenAll(tasks);
        }

        public Task DownloadMetadataAsync(
            List<Game> games,
            MetadataDownloaderSettings settings,
            Action<Game, int, int> processCallback,
            CancellationTokenSource cancelToken)
        {
            return Task.Run(() =>
            {
                if (games == null || games.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < games.Count; i++)
                {
                    Game game = null;

                    try
                    {
                        if (cancelToken?.IsCancellationRequested == true)
                        {
                            return;
                        }

                        GameMetadata storeData = null;
                        GameMetadata igdbData = null;
                        GameMetadata gameData = null;

                        // We need to get new instance from DB in case game got edited or deleted.
                        // We don't want to block game editing while metadata is downloading for other games.
                        game = database.Games[games[i].Id];
                        if (game == null)
                        {
                            logger.Warn($"Game {game.GameId} no longer in DB, skipping metadata download.");
                            processCallback?.Invoke(null, i, games.Count);
                            continue;
                        }

                        logger.Debug($"Downloading metadata for {game.Name}, {game.GameId}, {game.PluginId}");

                        // Name
                        if (!game.IsCustomGame && settings.Name.Import)
                        {
                            gameData = ProcessField(game, settings.Name, ref storeData, ref igdbData, (a) => a.GameData?.Name);
                            if (!string.IsNullOrEmpty(gameData?.GameData?.Name))
                            {
                                game.Name = StringExtensions.RemoveTrademarks(gameData.GameData.Name);
                                var sortingName = StringExtensions.ConvertToSortableName(game.Name);
                                if (sortingName != game.Name)
                                {
                                    game.SortingName = sortingName;
                                }
                            }
                        }

                        // Genre
                        if (settings.Genre.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Genres)))
                            {
                                gameData = ProcessField(game, settings.Genre, ref storeData, ref igdbData, (a) => a.GameData?.Genres);
                                game.Genres = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Genres) ? game.Genres : gameData.GameData.Genres;
                            }
                        }

                        // Release Date
                        if (settings.ReleaseDate.Import)
                        {

                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.ReleaseDate == null))
                            {
                                gameData = ProcessField(game, settings.ReleaseDate, ref storeData, ref igdbData, (a) => a.GameData?.ReleaseDate);
                                game.ReleaseDate = gameData?.GameData?.ReleaseDate ?? game.ReleaseDate;
                            }
                        }

                        // Developer
                        if (settings.Developer.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Developers)))
                            {
                                gameData = ProcessField(game, settings.Developer, ref storeData, ref igdbData, (a) => a.GameData?.Developers);
                                game.Developers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Developers) ? game.Developers : gameData.GameData.Developers;
                            }
                        }

                        // Publisher
                        if (settings.Publisher.Import)
                        {

                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Publishers)))
                            {
                                gameData = ProcessField(game, settings.Publisher, ref storeData, ref igdbData, (a) => a.GameData?.Publishers);
                                game.Publishers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Publishers) ? game.Publishers : gameData.GameData.Publishers;
                            }
                        }

                        // Tags / Features
                        if (settings.Tag.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Tags)))
                            {
                                gameData = ProcessField(game, settings.Tag, ref storeData, ref igdbData, (a) => a.GameData?.Tags);
                                game.Tags = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Tags) ? game.Tags : gameData.GameData.Tags;
                            }
                        }

                        // Description
                        if (settings.Description.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Description)))
                            {
                                gameData = ProcessField(game, settings.Description, ref storeData, ref igdbData, (a) => a.GameData?.Description);
                                game.Description = string.IsNullOrEmpty(gameData?.GameData?.Description) == true ? game.Description : gameData.GameData.Description;
                            }
                        }

                        // Links
                        if (settings.Links.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.Links == null))
                            {
                                gameData = ProcessField(game, settings.Links, ref storeData, ref igdbData, (a) => a.GameData?.Links);
                                game.Links = gameData?.GameData?.Links ?? game.Links;
                            }
                        }

                        // Critic Score
                        if (settings.CriticScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CriticScore == null))
                            {
                                gameData = ProcessField(game, settings.CriticScore, ref storeData, ref igdbData, (a) => a.GameData?.CriticScore);
                                game.CriticScore = gameData?.GameData?.CriticScore == null ? game.CriticScore : gameData.GameData.CriticScore;
                            }
                        }

                        // Community Score
                        if (settings.CommunityScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CommunityScore == null))
                            {
                                gameData = ProcessField(game, settings.CommunityScore, ref storeData, ref igdbData, (a) => a.GameData?.CommunityScore);
                                game.CommunityScore = gameData?.GameData?.CommunityScore == null ? game.CommunityScore : gameData.GameData.CommunityScore;
                            }
                        }

                        // BackgroundImage
                        if (settings.BackgroundImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.BackgroundImage)))
                            {
                                gameData = ProcessField(game, settings.BackgroundImage, ref storeData, ref igdbData, (a) => a.BackgroundImage);
                                if (!string.IsNullOrEmpty(gameData?.BackgroundImage))
                                {
                                    RemoveGameMedia(game.BackgroundImage);
                                    game.BackgroundImage = gameData.BackgroundImage;
                                }
                            }
                        }

                        // Cover
                        if (settings.CoverImage.Import)
                        {

                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.CoverImage)))
                            {
                                gameData = ProcessField(game, settings.CoverImage, ref storeData, ref igdbData, (a) => a.Image);
                                if (gameData?.Image != null)
                                {
                                    RemoveGameMedia(game.CoverImage);
                                    game.CoverImage = database.AddFile(gameData.Image, game.Id);
                                }
                            }
                        }

                        // Icon
                        if (settings.Icon.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Icon)))
                            {
                                gameData = ProcessField(game, settings.Icon, ref storeData, ref igdbData, (a) => a.Icon);
                                if (gameData?.Icon != null)
                                {
                                    RemoveGameMedia(game.Icon);
                                    game.Icon = database.AddFile(gameData.Icon, game.Id);
                                }
                            }
                        }

                        // TODO make this configurable and re-downalodable manually
                        // Only update them if they don't exist yet
                        if (game.OtherActions?.Any() != true && storeData != null)
                        {
                            if (storeData?.GameData?.OtherActions?.Any() == true)
                            {
                                game.OtherActions = new System.Collections.ObjectModel.ObservableCollection<GameAction>();
                                foreach (var task in storeData.GameData.OtherActions)
                                {
                                    game.OtherActions.Add(task);
                                }
                            }
                        }

                        // Just to be sure check if somebody didn't remove game while downloading data
                        if (database.Games.FirstOrDefault(a => a.GameId == games[i].GameId) != null)
                        {
                            database.Games.Update(game);
                        }
                        else
                        {
                            logger.Warn($"Game {game.GameId} no longer in DB, skipping metadata update in DB.");
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to download metadata for game {game?.Name}, {game?.GameId}");
                    }
                    finally
                    {
                        if (game != null)
                        {
                            processCallback?.Invoke(game, i, games.Count);
                        }
                    }
                }
            });
        }

        private void RemoveGameMedia(string mediaId)
        {
            if (string.IsNullOrEmpty(mediaId))
            {
                return;
            }

            if (mediaId.IsHttpUrl())
            {
                HttpFileCache.ClearCache(mediaId);
            }
            else
            {
                database.RemoveFile(mediaId);
            }
        }
    }
}
