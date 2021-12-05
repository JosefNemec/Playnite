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
            Dictionary<Guid, OnDemandMetadataProvider> existingPluginData,
            CancellationToken cancelToken)
        {
            if (fieldSettings.Sources.Any() == false)
            {
                return null;
            }

            var getFieldArgs = new GetMetadataFieldArgs { CancelToken = cancelToken };
            foreach (var source in fieldSettings.Sources)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                try
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

                        metadata = new GameMetadata();
                        switch (gameField)
                        {
                            case MetadataField.Name:
                                metadata.Name = provider.GetName(getFieldArgs);
                                break;
                            case MetadataField.Genres:
                                metadata.Genres = provider.GetGenres(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.ReleaseDate:
                                metadata.ReleaseDate = provider.GetReleaseDate(getFieldArgs);
                                break;
                            case MetadataField.Developers:
                                metadata.Developers = provider.GetDevelopers(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Publishers:
                                metadata.Publishers = provider.GetPublishers(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Tags:
                                metadata.Tags = provider.GetTags(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Description:
                                metadata.Description = provider.GetDescription(getFieldArgs);
                                break;
                            case MetadataField.Links:
                                metadata.Links = provider.GetLinks(getFieldArgs)?.Where(a => a != null).ToList();
                                break;
                            case MetadataField.CriticScore:
                                metadata.CriticScore = provider.GetCriticScore(getFieldArgs);
                                break;
                            case MetadataField.CommunityScore:
                                metadata.CommunityScore = provider.GetCommunityScore(getFieldArgs);
                                break;
                            case MetadataField.Icon:
                                metadata.Icon = provider.GetIcon(getFieldArgs);
                                break;
                            case MetadataField.CoverImage:
                                metadata.CoverImage = provider.GetCoverImage(getFieldArgs);
                                break;
                            case MetadataField.BackgroundImage:
                                metadata.BackgroundImage = provider.GetBackgroundImage(getFieldArgs);
                                break;
                            case MetadataField.Features:
                                metadata.Features = provider.GetFeatures(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.AgeRating:
                                metadata.AgeRatings = provider.GetAgeRatings(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Region:
                                metadata.Regions = provider.GetRegions(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Series:
                                metadata.Series = provider.GetSeries(getFieldArgs)?.Where(a => a != null).ToHashSet();
                                break;
                            case MetadataField.Platform:
                                metadata.Platforms = provider.GetPlatforms(getFieldArgs)?.Where(a => a != null).ToHashSet();
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
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to process metadata download: {gameField}, {source}");
                }
            }

            return null;
        }

        public Task DownloadMetadataAsync(
            List<Game> games,
            MetadataDownloaderSettings settings,
            PlayniteSettings playniteSettings,
            Action<Game, int, int> progressCallback,
            CancellationToken cancelToken)
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
                        if (cancelToken.IsCancellationRequested == true)
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

                        var dataModified = false;
                        game.PropertyChanged += (_, __) => dataModified = true;

                        if (game != null)
                        {
                            progressCallback?.Invoke(game, i, games.Count);
                        }

                        logger.Debug($"Downloading metadata for {game.Name}, {game.GameId}, {game.PluginId}");

                        // Name
                        if (!game.IsCustomGame && settings.Name.Import)
                        {
                            gameData = ProcessField(game, settings.Name, MetadataField.Name, (a) => a.Name, existingStoreData, existingPluginData, cancelToken);
                            if (!string.IsNullOrEmpty(gameData?.Name))
                            {
                                game.Name = StringExtensions.RemoveTrademarks(gameData.Name);
                            }
                        }

                        // Genre
                        if (settings.Genre.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.GenreIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Genre, MetadataField.Genres, (a) => a.Genres, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Genres.HasItems() == true)
                                {
                                    game.GenreIds = database.Genres.Add(gameData.Genres).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Release Date
                        if (settings.ReleaseDate.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.ReleaseDate == null))
                            {
                                gameData = ProcessField(game, settings.ReleaseDate, MetadataField.ReleaseDate, (a) => a.ReleaseDate, existingStoreData, existingPluginData, cancelToken);
                                game.ReleaseDate = gameData?.ReleaseDate ?? game.ReleaseDate;
                            }
                        }

                        // Developer
                        if (settings.Developer.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.DeveloperIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Developer, MetadataField.Developers, (a) => a.Developers, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Developers.HasItems() == true)
                                {
                                    game.DeveloperIds = database.Companies.Add(gameData.Developers).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Publisher
                        if (settings.Publisher.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.PublisherIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Publisher, MetadataField.Publishers, (a) => a.Publishers, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Publishers.HasItems() == true)
                                {
                                    game.PublisherIds = database.Companies.Add(gameData.Publishers).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Tags
                        if (settings.Tag.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.TagIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Tag, MetadataField.Tags, (a) => a.Tags, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Tags.HasItems() == true)
                                {
                                    game.TagIds = database.Tags.Add(gameData.Tags).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Features
                        if (settings.Feature.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.FeatureIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Feature, MetadataField.Features, (a) => a.Features, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Features.HasItems() == true)
                                {
                                    game.FeatureIds = database.Features.Add(gameData.Features).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Description
                        if (settings.Description.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Description)))
                            {
                                gameData = ProcessField(game, settings.Description, MetadataField.Description, (a) => a.Description, existingStoreData, existingPluginData, cancelToken);
                                game.Description = string.IsNullOrEmpty(gameData?.Description) == true ? game.Description : gameData.Description;
                            }
                        }

                        // Links
                        if (settings.Links.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.Links.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Links, MetadataField.Links, (a) => a.Links, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Links.HasItems() == true)
                                {
                                    game.Links = gameData.Links.ToObservable();
                                }
                            }
                        }

                        // Age rating
                        if (settings.AgeRating.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.AgeRatingIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.AgeRating, MetadataField.AgeRating, (a) => a.AgeRatings, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.AgeRatings.HasItems() == true)
                                {
                                    game.AgeRatingIds = database.AgeRatings.Add(gameData.AgeRatings).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Region
                        if (settings.Region.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.RegionIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Region, MetadataField.Region, (a) => a.Regions, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Regions.HasItems() == true)
                                {
                                    game.RegionIds = database.Regions.Add(gameData.Regions).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Series
                        if (settings.Series.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.SeriesIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Series, MetadataField.Series, (a) => a.Series, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Series.HasItems() == true)
                                {
                                    game.SeriesIds = database.Series.Add(gameData.Series).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Platform
                        if (settings.Platform.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && !game.PlatformIds.HasItems()))
                            {
                                gameData = ProcessField(game, settings.Platform, MetadataField.Platform, (a) => a.Platforms, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Platforms.HasItems() == true)
                                {
                                    game.PlatformIds = database.Platforms.Add(gameData.Platforms).Select(a => a.Id).ToList();
                                }
                            }
                        }

                        // Critic Score
                        if (settings.CriticScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CriticScore == null))
                            {
                                gameData = ProcessField(game, settings.CriticScore, MetadataField.CriticScore, (a) => a.CriticScore, existingStoreData, existingPluginData, cancelToken);
                                game.CriticScore = gameData?.CriticScore == null ? game.CriticScore : gameData.CriticScore;
                            }
                        }

                        // Community Score
                        if (settings.CommunityScore.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CommunityScore == null))
                            {
                                gameData = ProcessField(game, settings.CommunityScore, MetadataField.CommunityScore, (a) => a.CommunityScore, existingStoreData, existingPluginData, cancelToken);
                                game.CommunityScore = gameData?.CommunityScore == null ? game.CommunityScore : gameData.CommunityScore;
                            }
                        }

                        // BackgroundImage
                        if (settings.BackgroundImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.BackgroundImage)))
                            {
                                gameData = ProcessField(game, settings.BackgroundImage, MetadataField.BackgroundImage, (a) => a.BackgroundImage, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.BackgroundImage != null)
                                {
                                    if (playniteSettings.DownloadBackgroundsImmediately && gameData.BackgroundImage.HasImageData)
                                    {
                                        game.BackgroundImage = database.AddFile(gameData.BackgroundImage, game.Id, true);
                                    }
                                    else if (!playniteSettings.DownloadBackgroundsImmediately &&
                                             !gameData.BackgroundImage.Path.IsNullOrEmpty())
                                    {
                                        game.BackgroundImage = gameData.BackgroundImage.Path;
                                    }
                                    else if (gameData.BackgroundImage.HasImageData)
                                    {
                                        game.BackgroundImage = database.AddFile(gameData.BackgroundImage, game.Id, true);
                                    }
                                }
                            }
                        }

                        // Cover
                        if (settings.CoverImage.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.CoverImage)))
                            {
                                gameData = ProcessField(game, settings.CoverImage, MetadataField.CoverImage, (a) => a.CoverImage, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.CoverImage != null)
                                {
                                    game.CoverImage = database.AddFile(gameData.CoverImage, game.Id, true);
                                }
                            }
                        }

                        // Icon
                        if (settings.Icon.Import)
                        {
                            if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Icon)))
                            {
                                gameData = ProcessField(game, settings.Icon, MetadataField.Icon, (a) => a.Icon, existingStoreData, existingPluginData, cancelToken);
                                if (gameData?.Icon != null)
                                {
                                    game.Icon = database.AddFile(gameData.Icon, game.Id, true);
                                }
                            }
                        }

                        // Just to be sure check if somebody didn't remove game while downloading data
                        if (database.Games.FirstOrDefault(a => a.Id == games[i].Id) != null && dataModified)
                        {
                            game.Modified = DateTime.Now;
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
    }
}
