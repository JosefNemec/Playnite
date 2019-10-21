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
        private readonly List<MetadataPlugin> metadataDownloaders;
        private Dictionary<Guid, LibraryMetadataProvider> libraryDownloaders = new Dictionary<Guid, LibraryMetadataProvider>();
        
        public MetadataDownloader(GameDatabase database, List<MetadataPlugin> metadataDownloaders, List<LibraryPlugin> libraryPlugins)
        {
            this.database = database;
            this.metadataDownloaders = metadataDownloaders;
            foreach (var plugin in libraryPlugins)
            {
                LibraryMetadataProvider downloader = null;
                try
                {
                    downloader = plugin.GetMetadataDownloader();
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get metadata downloader from {plugin.Name} library plugin.");
                }

                libraryDownloaders.Add(plugin.Id, downloader);
            }            
        }

        public MetadataDownloader(GameDatabase database, List<MetadataPlugin> metadataDownloaders, Dictionary<Guid, LibraryMetadataProvider> libraryDownloaders)
        {
            this.database = database;
            this.metadataDownloaders = metadataDownloaders;
            this.libraryDownloaders = libraryDownloaders;
        }

        public void Dispose()
        {
            foreach (var downloader in libraryDownloaders.Values)
            {
                // Null check because downloader might be from library without official metadata provider
                downloader?.Dispose();
            }

            foreach (var downloader in metadataDownloaders)
            {
                downloader.Dispose();
            }
        }

        private LibraryMetadataProvider GetLibraryMetadataDownloader(Guid pluginId)
        {
            if (libraryDownloaders.ContainsKey(pluginId))
            {
                return libraryDownloaders[pluginId];
            }

            return null;
        }

        private GameMetadata ProcessStoreDownload(Game game)
        {
            var downloader = GetLibraryMetadataDownloader(game.PluginId);
            try
            {
                return downloader?.GetMetadata(game);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to download metadat from library plugin downloader {game.PluginId}.");
                return null;
            }
        }

        private GameMetadata ProcessPluginDownload(Game game, Guid pluginId)
        {
            var downloader = metadataDownloaders.FirstOrDefault(a => a.Id == pluginId);
            try
            {
                return downloader?.GetMetadata(new MetadataRequestOptions(game, true));
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to download metadat from metadata plugin downloader {downloader.Name}.");
                return null;
            }
        }

        internal GameMetadata ProcessField(
            Game game,
            MetadataFieldSettings fieldSettings,
            GameField gameField,
            Func<GameMetadata, object> propertySelector,
            Dictionary<Guid, GameMetadata> existingMetadata)
        {
            if (fieldSettings.Sources.Any() == false)
            {
                return null;
            }

            foreach (var source in fieldSettings.Sources)
            {
                // Skip Store source for manually added games.
                if (source == Guid.Empty && game.PluginId == Guid.Empty)
                {
                    continue;
                }

                // Check if metadata from this source are already downloaded.
                if (existingMetadata.ContainsKey(source))
                {
                    if (existingMetadata[source] != null && propertySelector(existingMetadata[source]) != null)
                    {
                        return existingMetadata[source];
                    }
                    else
                    {
                        continue;
                    }
                }

                // Check if downloader supports this game field.
                if (source != Guid.Empty)
                {
                    var downloader = metadataDownloaders.FirstOrDefault(a => a.Id == source);
                    if (downloader == null)
                    {
                        continue;
                    }
                    else if (downloader.SupportedFields?.Contains(gameField) != true)
                    {
                        continue;
                    }
                }

                // Download metadata.
                GameMetadata metadata = null;
                if (source == Guid.Empty)
                {
                    metadata = ProcessStoreDownload(game);
                }
                else
                {
                    metadata = ProcessPluginDownload(game, source);
                }

                existingMetadata.Add(source, metadata);
                if (metadata != null && propertySelector(metadata) != null)
                {
                    return metadata;
                }
                else
                {
                    continue;
                }
            }

            return null;
        }

        public Task DownloadMetadataAsync(
            List<Game> games,
            MetadataDownloaderSettings settings,
            Action<Game, int, int> progressCallback,
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

                        var existingMetadata = new Dictionary<Guid, GameMetadata>();
                        GameMetadata gameData = null;

                        // We need to get new instance from DB in case game got edited or deleted.
                        // We don't want to block game editing while metadata is downloading for other games.
                        game = database.Games[games[i].Id]?.GetClone();
                        if (game == null)
                        {
                            logger.Warn($"Game {game.Id} no longer in DB, skipping metadata download.");
                            progressCallback?.Invoke(null, i, games.Count);
                            continue;
                        }

                        logger.Debug($"Downloading metadata for {game.Name}, {game.GameId}, {game.PluginId}");

                        // Name
                        if (!game.IsCustomGame && settings.Name.Import)
                        {
                            gameData = ProcessField(game, settings.Name, GameField.Name, (a) => a.GameInfo?.Name, existingMetadata);
                            if (!string.IsNullOrEmpty(gameData?.GameInfo?.Name))
                            {
                                game.Name = StringExtensions.RemoveTrademarks(gameData.GameInfo.Name);
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
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.GenreIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Genre, GameField.Genres, (a) => a.GameInfo?.Genres, existingMetadata);
                                if (gameData?.GameInfo?.Genres.HasNonEmptyItems() == true)
                                {
                                    game.GenreIds = database.Genres.Add(gameData.GameInfo.Genres).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Release Date
                        if (settings.ReleaseDate.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.ReleaseDate == null))
                            {
                                gameData = ProcessField(game, settings.ReleaseDate, GameField.ReleaseDate, (a) => a.GameInfo?.ReleaseDate, existingMetadata);
                                game.ReleaseDate = gameData?.GameInfo?.ReleaseDate ?? game.ReleaseDate;
                            }
                        }

                        // Developer
                        if (settings.Developer.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.DeveloperIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Developer, GameField.Developers, (a) => a.GameInfo?.Developers, existingMetadata);
                                if (gameData?.GameInfo?.Developers.HasNonEmptyItems() == true)
                                {
                                    game.DeveloperIds = database.Companies.Add(gameData.GameInfo.Developers).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Publisher
                        if (settings.Publisher.Import)
                        {

                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.PublisherIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Publisher, GameField.Publishers, (a) => a.GameInfo?.Publishers, existingMetadata);
                                if (gameData?.GameInfo?.Publishers.HasNonEmptyItems() == true)
                                {
                                    game.PublisherIds = database.Companies.Add(gameData.GameInfo.Publishers).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Tags / Features
                        if (settings.Tag.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.TagIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Tag, GameField.Tags, (a) => a.GameInfo?.Tags, existingMetadata);
                                if (gameData?.GameInfo?.Tags.HasNonEmptyItems() == true)
                                {
                                    game.TagIds = database.Tags.Add(gameData.GameInfo.Tags).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Description
                        if (settings.Description.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Description)))
                            {
                                gameData = ProcessField(game, settings.Description, GameField.Description, (a) => a.GameInfo?.Description, existingMetadata);
                                game.Description = string.IsNullOrEmpty(gameData?.GameInfo?.Description) == true ? game.Description : gameData.GameInfo.Description;
                            }
                        }

                        // Links
                        if (settings.Links.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.Links.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Links, GameField.Links, (a) => a.GameInfo?.Links, existingMetadata);
                                if (gameData?.GameInfo?.Links.HasItems() == true)
                                {
                                    game.Links = gameData.GameInfo.Links.ToObservable();
                                }
                            }
                        }

                        // Critic Score
                        if (settings.CriticScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CriticScore == null))
                            {
                                gameData = ProcessField(game, settings.CriticScore, GameField.CriticScore, (a) => a.GameInfo?.CriticScore, existingMetadata);
                                game.CriticScore = gameData?.GameInfo?.CriticScore == null ? game.CriticScore : gameData.GameInfo.CriticScore;
                            }
                        }

                        // Community Score
                        if (settings.CommunityScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CommunityScore == null))
                            {
                                gameData = ProcessField(game, settings.CommunityScore, GameField.CommunityScore, (a) => a.GameInfo?.CommunityScore, existingMetadata);
                                game.CommunityScore = gameData?.GameInfo?.CommunityScore == null ? game.CommunityScore : gameData.GameInfo.CommunityScore;
                            }
                        }

                        // BackgroundImage
                        if (settings.BackgroundImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.BackgroundImage)))
                            {
                                gameData = ProcessField(game, settings.BackgroundImage, GameField.BackgroundImage, (a) => a.BackgroundImage, existingMetadata);
                                if (gameData?.BackgroundImage != null)
                                {
                                    if (gameData.BackgroundImage.HasContent)
                                    {
                                        game.BackgroundImage = database.AddFile(gameData.BackgroundImage, game.Id);
                                    }
                                    else
                                    {
                                        game.BackgroundImage = gameData.BackgroundImage.OriginalUrl;
                                    }
                                }
                            }
                        }

                        // Cover
                        if (settings.CoverImage.Import)
                        {

                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.CoverImage)))
                            {
                                gameData = ProcessField(game, settings.CoverImage, GameField.CoverImage, (a) => a.CoverImage, existingMetadata);
                                if (gameData?.CoverImage != null)
                                {
                                    game.CoverImage = database.AddFile(gameData.CoverImage, game.Id);
                                }
                            }
                        }

                        // Icon
                        if (settings.Icon.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Icon)))
                            {
                                gameData = ProcessField(game, settings.Icon, GameField.Icon, (a) => a.Icon, existingMetadata);
                                if (gameData?.Icon != null)
                                {
                                    game.Icon = database.AddFile(gameData.Icon, game.Id);
                                }
                            }
                        }

                        // TODO
                        // Only update them if they don't exist yet
                        //if (game.OtherActions?.Any() != true && storeData != null)
                        //{
                        //    if (storeData?.GameInfo?.OtherActions?.Any() == true)
                        //    {
                        //        game.OtherActions = new System.Collections.ObjectModel.ObservableCollection<GameAction>();
                        //        foreach (var task in storeData.GameInfo.OtherActions)
                        //        {
                        //            game.OtherActions.Add(task);
                        //        }
                        //    }
                        //}

                        // Just to be sure check if somebody didn't remove game while downloading data
                        if (database.Games.FirstOrDefault(a => a.Id == games[i].Id) != null)
                        {
                            database.Games.Update(game);
                        }
                        else
                        {
                            logger.Warn($"Game {game.Id} no longer in DB, skipping metadata update in DB.");
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to download metadata for game {game?.Name}, {game?.Id}");
                    }
                    finally
                    {
                        if (game != null)
                        {
                            progressCallback?.Invoke(game, i, games.Count);
                        }
                    }
                }
            });
        }
    }
}
