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

        internal GameMetadata ProcessField(
            Game game,
            MetadataFieldSettings fieldSettings,
            MetadataField gameField,
            Func<GameMetadata, object> propertySelector,
            Dictionary<Guid, GameMetadata> existingStoreData,
            Dictionary<Guid, OnDemandMetadataProvider> existingPluginData)
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
                if (existingStoreData.ContainsKey(source))
                {
                    if (existingStoreData[source] != null && propertySelector(existingStoreData[source]) != null)
                    {
                        return existingStoreData[source];
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
                    existingStoreData.Add(source, metadata);
                }
                else
                {
                    var downloader = metadataDownloaders.FirstOrDefault(a => a.Id == source);
                    if (downloader == null)
                    {
                        continue;
                    }

                    OnDemandMetadataProvider provider = null;
                    if (existingPluginData.ContainsKey(source))
                    {
                        provider = existingPluginData[source];
                    }
                    else
                    {
                        provider = downloader.GetMetadataProvider(new MetadataRequestOptions(game, true));
                        existingPluginData.Add(source, provider);
                    }

                    if (provider == null)
                    {
                        continue;
                    }

                    if (!provider.AvailableFields.Contains(gameField))
                    {
                        continue;
                    }

                    var gameInfo = new GameInfo();
                    metadata = new GameMetadata { GameInfo = gameInfo };
                    switch (gameField)
                    {
                        case MetadataField.Name:
                            gameInfo.Name = provider.GetName();
                            break;
                        case MetadataField.Genres:
                            gameInfo.Genres = provider.GetGenres();
                            break;
                        case MetadataField.ReleaseDate:
                            gameInfo.ReleaseDate = provider.GetReleaseDate();
                            break;
                        case MetadataField.Developers:
                            gameInfo.Developers = provider.GetDevelopers();
                            break;
                        case MetadataField.Publishers:
                            gameInfo.Publishers = provider.GetPublishers();
                            break;
                        case MetadataField.Tags:
                            gameInfo.Tags = provider.GetTags();
                            break;
                        case MetadataField.Description:
                            gameInfo.Description = provider.GetDescription();
                            break;
                        case MetadataField.Links:
                            gameInfo.Links = provider.GetLinks();
                            break;
                        case MetadataField.CriticScore:
                            gameInfo.CriticScore = provider.GetCriticScore();
                            break;
                        case MetadataField.CommunityScore:
                            gameInfo.CommunityScore = provider.GetCommunityScore();
                            break;
                        case MetadataField.Icon:
                            metadata.Icon = provider.GetIcon();
                            break;
                        case MetadataField.CoverImage:
                            metadata.CoverImage = provider.GetCoverImage();
                            break;
                        case MetadataField.BackgroundImage:
                            metadata.BackgroundImage = provider.GetBackgroundImage();
                            break;
                        case MetadataField.Features:
                            gameInfo.Features = provider.GetFeatures();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

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
            PlayniteSettings playniteSettings,
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
                    var existingStoreData = new Dictionary<Guid, GameMetadata>();
                    var existingPluginData = new Dictionary<Guid, OnDemandMetadataProvider>();

                    try
                    {
                        if (cancelToken?.IsCancellationRequested == true)
                        {
                            return;
                        }

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

                        if (game != null)
                        {
                            progressCallback?.Invoke(game, i, games.Count);
                        }

                        logger.Debug($"Downloading metadata for {game.Name}, {game.GameId}, {game.PluginId}");

                        // Name
                        if (!game.IsCustomGame && settings.Name.Import)
                        {
                            gameData = ProcessField(game, settings.Name, MetadataField.Name, (a) => a.GameInfo?.Name, existingStoreData, existingPluginData);
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
                                gameData = ProcessField(game, settings.Genre, MetadataField.Genres, (a) => a.GameInfo?.Genres, existingStoreData, existingPluginData);
                                if (gameData?.GameInfo?.Genres.HasNonEmptyItems() == true)
                                {
                                    game.GenreIds = database.Genres.Add(gameData.GameInfo.Genres, LooseDbNameComparer).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Release Date
                        if (settings.ReleaseDate.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.ReleaseDate == null))
                            {
                                gameData = ProcessField(game, settings.ReleaseDate, MetadataField.ReleaseDate, (a) => a.GameInfo?.ReleaseDate, existingStoreData, existingPluginData);
                                game.ReleaseDate = gameData?.GameInfo?.ReleaseDate ?? game.ReleaseDate;
                            }
                        }

                        // Developer
                        if (settings.Developer.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.DeveloperIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Developer, MetadataField.Developers, (a) => a.GameInfo?.Developers, existingStoreData, existingPluginData);
                                if (gameData?.GameInfo?.Developers.HasNonEmptyItems() == true)
                                {
                                    game.DeveloperIds = database.Companies.Add(gameData.GameInfo.Developers, LooseDbNameComparer).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Publisher
                        if (settings.Publisher.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.PublisherIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Publisher, MetadataField.Publishers, (a) => a.GameInfo?.Publishers, existingStoreData, existingPluginData);
                                if (gameData?.GameInfo?.Publishers.HasNonEmptyItems() == true)
                                {
                                    game.PublisherIds = database.Companies.Add(gameData.GameInfo.Publishers, LooseDbNameComparer).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Tags
                        if (settings.Tag.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.TagIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Tag, MetadataField.Tags, (a) => a.GameInfo?.Tags, existingStoreData, existingPluginData);
                                if (gameData?.GameInfo?.Tags.HasNonEmptyItems() == true)
                                {
                                    game.TagIds = database.Tags.Add(gameData.GameInfo.Tags, LooseDbNameComparer).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Features
                        if (settings.Feature.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.FeatureIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Feature, MetadataField.Features, (a) => a.GameInfo?.Features, existingStoreData, existingPluginData);
                                if (gameData?.GameInfo?.Features.HasNonEmptyItems() == true)
                                {
                                    game.FeatureIds = database.Features.Add(gameData.GameInfo.Features, LooseDbNameComparer).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Description
                        if (settings.Description.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Description)))
                            {
                                gameData = ProcessField(game, settings.Description, MetadataField.Description, (a) => a.GameInfo?.Description, existingStoreData, existingPluginData);
                                game.Description = string.IsNullOrEmpty(gameData?.GameInfo?.Description) == true ? game.Description : gameData.GameInfo.Description;
                            }
                        }

                        // Links
                        if (settings.Links.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.Links.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Links, MetadataField.Links, (a) => a.GameInfo?.Links, existingStoreData, existingPluginData);
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
                                gameData = ProcessField(game, settings.CriticScore, MetadataField.CriticScore, (a) => a.GameInfo?.CriticScore, existingStoreData, existingPluginData);
                                game.CriticScore = gameData?.GameInfo?.CriticScore == null ? game.CriticScore : gameData.GameInfo.CriticScore;
                            }
                        }

                        // Community Score
                        if (settings.CommunityScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CommunityScore == null))
                            {
                                gameData = ProcessField(game, settings.CommunityScore, MetadataField.CommunityScore, (a) => a.GameInfo?.CommunityScore, existingStoreData, existingPluginData);
                                game.CommunityScore = gameData?.GameInfo?.CommunityScore == null ? game.CommunityScore : gameData.GameInfo.CommunityScore;
                            }
                        }

                        // BackgroundImage
                        if (settings.BackgroundImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.BackgroundImage)))
                            {
                                gameData = ProcessField(game, settings.BackgroundImage, MetadataField.BackgroundImage, (a) => a.BackgroundImage, existingStoreData, existingPluginData);
                                if (gameData?.BackgroundImage != null)
                                {
                                    if (playniteSettings.DownloadBackgroundsImmediately && gameData.BackgroundImage.HasImageData)
                                    {
                                        game.BackgroundImage = database.AddFile(gameData.BackgroundImage, game.Id);
                                    }
                                    else if (!playniteSettings.DownloadBackgroundsImmediately &&
                                             !gameData.BackgroundImage.OriginalUrl.IsNullOrEmpty())
                                    {
                                        game.BackgroundImage = gameData.BackgroundImage.OriginalUrl;
                                    }
                                    else if (gameData.BackgroundImage.HasImageData)
                                    {
                                        game.BackgroundImage = database.AddFile(gameData.BackgroundImage, game.Id);
                                    }
                                }
                            }
                        }

                        // Cover
                        if (settings.CoverImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.CoverImage)))
                            {
                                gameData = ProcessField(game, settings.CoverImage, MetadataField.CoverImage, (a) => a.CoverImage, existingStoreData, existingPluginData);
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
                                gameData = ProcessField(game, settings.Icon, MetadataField.Icon, (a) => a.Icon, existingStoreData, existingPluginData);
                                if (gameData?.Icon != null)
                                {
                                    game.Icon = database.AddFile(gameData.Icon, game.Id);
                                }
                            }
                        }

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
                        foreach (var plugin in existingPluginData.Values)
                        {
                            plugin.Dispose();
                        }
                    }
                }
            });
        }

        private bool LooseDbNameComparer<TItem>(TItem existingItem, string newName) where TItem : DatabaseObject
        {
            return string.Equals(
                Regex.Replace(existingItem.Name, @"[\s-]", ""),
                Regex.Replace(newName, @"[\s-]", ""), StringComparison.OrdinalIgnoreCase);
        }
    }
}
