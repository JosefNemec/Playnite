using NLog;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers.BattleNet;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
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

namespace Playnite.Metadata
{
    //public class MetadataDownloader
    //{
    //    private static Logger logger = LogManager.GetCurrentClassLogger();

    //    private IMetadataProvider steamProvider;
    //    private IMetadataProvider originProvider;
    //    private IMetadataProvider gogProvider;
    //    private IMetadataProvider battleNetProvider;
    //    private IMetadataProvider igdbProvider;
    //    private Settings appSettings;

    //    public MetadataDownloader(Settings appSettings)
    //    {
    //        this.appSettings = appSettings;
    //        steamProvider = new SteamMetadataProvider(new Services.ServicesClient(), appSettings.SteamSettings);
    //        originProvider = new OriginMetadataProvider();
    //        gogProvider = new GogMetadataProvider();
    //        battleNetProvider = new BattleNetMetadataProvider();
    //        igdbProvider = new IGDBMetadataProvider();
    //    }

    //    public MetadataDownloader(
    //        IMetadataProvider steamProvider,
    //        IMetadataProvider originProvider,
    //        IMetadataProvider gogProvider,
    //        IMetadataProvider battleNetProvider,
    //        IMetadataProvider igdbProvider)
    //    {
    //        this.steamProvider = steamProvider;
    //        this.originProvider = originProvider;
    //        this.gogProvider = gogProvider;
    //        this.battleNetProvider = battleNetProvider;
    //        this.igdbProvider = igdbProvider;
    //    }

    //    internal IMetadataProvider GetMetaProviderByProvider(Provider provider)
    //    {
    //        switch (provider)
    //        {
    //            case Provider.GOG:
    //                return gogProvider;
    //            case Provider.Origin:
    //                return originProvider;
    //            case Provider.Steam:
    //                return steamProvider;
    //            case Provider.BattleNet:
    //                return battleNetProvider;
    //            case Provider.Uplay:
    //            case Provider.Custom:
    //            default:
    //                return null;
    //        }
    //    }

    //    private string ReplaceNumsForRomans(Match m)
    //    {
    //        return Roman.To(int.Parse(m.Value));
    //    }

    //    private GameMetadata ProcessDownload(Game game, ref GameMetadata data)
    //    {
    //        if (data == null)
    //        {
    //            data = GetMetaProviderByProvider(game.Provider).GetMetadata(game);
    //        }

    //        return data;
    //    }

    //    private GameMetadata ProcessField(
    //        Game game,
    //        MetadataFieldSettings field,
    //        ref GameMetadata storeData,
    //        ref GameMetadata igdbData,
    //        Func<GameMetadata, object> propertySelector)
    //    {
    //        if (field.Import)
    //        {
    //            if (field.Source == MetadataSource.Store && game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
    //            {
    //                storeData = ProcessDownload(game, ref storeData);
    //                return storeData;
    //            }
    //            else if (field.Source == MetadataSource.IGDB)
    //            {
    //                if (igdbData == null)
    //                {
    //                    igdbData = igdbProvider.GetMetadata(game);
    //                }

    //                return igdbData;
    //            }
    //            else if (field.Source == MetadataSource.IGDBOverStore)
    //            {
    //                if (igdbData == null)
    //                {
    //                    igdbData = igdbProvider.GetMetadata(game);
    //                }

    //                if (igdbData.GameData == null && game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
    //                {
    //                    if (storeData == null)
    //                    {
    //                        storeData = ProcessDownload(game, ref storeData);
    //                    }

    //                    return storeData;
    //                }
    //                else
    //                {
    //                    var igdbValue = propertySelector(igdbData);
    //                    if (igdbValue != null)
    //                    {
    //                        return igdbData;
    //                    }
    //                    else if (game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
    //                    {
    //                        if (storeData == null)
    //                        {
    //                            storeData = ProcessDownload(game, ref storeData);
    //                        }

    //                        if (storeData.GameData != null)
    //                        {
    //                            return storeData;
    //                        }
    //                        else
    //                        {
    //                            return igdbData;
    //                        }
    //                    }
    //                }
    //            }
    //            else if (field.Source == MetadataSource.StoreOverIGDB)
    //            {
    //                if (game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
    //                {
    //                    if (storeData == null)
    //                    {
    //                        storeData = ProcessDownload(game, ref storeData);
    //                    }

    //                    if (storeData.GameData == null)
    //                    {
    //                        if (igdbData == null)
    //                        {
    //                            igdbData = igdbProvider.GetMetadata(game);
    //                        }

    //                        return igdbData;
    //                    }
    //                    else
    //                    {
    //                        var storeValue = propertySelector(storeData);
    //                        if (storeValue != null)
    //                        {
    //                            return storeData;
    //                        }
    //                        else
    //                        {
    //                            if (igdbData == null)
    //                            {
    //                                igdbData = igdbProvider.GetMetadata(game);
    //                            }

    //                            return igdbData;
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (igdbData == null)
    //                    {
    //                        igdbData = igdbProvider.GetMetadata(game);
    //                    }

    //                    return igdbData;
    //                }
    //            }
    //        }

    //        return null;
    //    }

    //    public async Task DownloadMetadataGroupedAsync(
    //        List<Game> games,
    //        GameDatabase database,
    //        MetadataDownloaderSettings settings,
    //        Action<Game, int, int> processCallback,
    //        CancellationTokenSource cancelToken)
    //    {
    //        int index = 0;
    //        int total = games.Count;
    //        var tasks = new List<Task>();

    //        await Task.Run(() =>
    //        {
    //            var grouped = games.GroupBy(a => a.Provider);
    //            logger.Info($"Downloading metadata using {grouped.Count()} threads.");
    //            foreach (IGrouping<Provider, Game> group in grouped)
    //            {
    //                tasks.Add(Task.Run(() =>
    //                {
    //                    var gms = group.ToList();
    //                    DownloadMetadataAsync(gms, database, settings, (g, i, t) =>
    //                    {
    //                        index++;
    //                        processCallback?.Invoke(g, index, total);
    //                    }, cancelToken).Wait();
    //                }));
    //            }

    //            Task.WaitAll(tasks.ToArray());
    //        });
    //    }

    //    public async Task DownloadMetadataAsync(
    //        List<Game> games,
    //        GameDatabase database,
    //        MetadataDownloaderSettings settings,
    //        Action<Game, int, int> processCallback,
    //        CancellationTokenSource cancelToken)
    //    {
    //        await Task.Run(() =>
    //        {
    //            if (games == null || games.Count == 0)
    //            {
    //                return;
    //            }

    //            for (int i = 0; i < games.Count; i++)
    //            {
    //                if (cancelToken?.IsCancellationRequested == true)
    //                {
    //                    return;
    //                }

    //                GameMetadata storeData = null;
    //                GameMetadata igdbData = null;
    //                GameMetadata gameData = null;

    //                // We need to get new instance from DB in case game got edited or deleted.
    //                // We don't want to block game editing while metadata is downloading for other games.
    //                var game = database.GamesCollection.FindOne(a => a.GameId == games[i].GameId);
    //                if (game == null)
    //                {
    //                    logger.Warn($"Game {game.GameId} no longer in DB, skipping metadata download.");
    //                    processCallback?.Invoke(null, i, games.Count);
    //                    continue;
    //                }

    //                try
    //                {
    //                    logger.Debug($"Downloading metadata for {game.PluginId} game {game.Name}, {game.GameId}");

    //                    // Name
    //                    if (game.Provider != Provider.Custom && settings.Name.Import)
    //                    {
    //                        gameData = ProcessField(game, settings.Name, ref storeData, ref igdbData, (a) => a.GameData?.Name);
    //                        if (!string.IsNullOrEmpty(gameData?.GameData?.Name))
    //                        {
    //                            game.Name = StringExtensions.RemoveTrademarks(gameData.GameData.Name);
    //                            var sortingName = StringExtensions.ConvertToSortableName(game.Name);
    //                            if (sortingName != game.Name)
    //                            {
    //                                game.SortingName = sortingName;
    //                            }
    //                        }
    //                    }

    //                    // Genre
    //                    if (settings.Genre.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Genres)))
    //                        {
    //                            gameData = ProcessField(game, settings.Genre, ref storeData, ref igdbData, (a) => a.GameData?.Genres);
    //                            game.Genres = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Genres) ? game.Genres : gameData.GameData.Genres;
    //                        }
    //                    }

    //                    // Release Date
    //                    if (settings.ReleaseDate.Import)
    //                    {

    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.ReleaseDate == null))
    //                        {
    //                            gameData = ProcessField(game, settings.ReleaseDate, ref storeData, ref igdbData, (a) => a.GameData?.ReleaseDate);
    //                            game.ReleaseDate = gameData?.GameData?.ReleaseDate ?? game.ReleaseDate;
    //                        }
    //                    }

    //                    // Developer
    //                    if (settings.Developer.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Developers)))
    //                        {
    //                            gameData = ProcessField(game, settings.Developer, ref storeData, ref igdbData, (a) => a.GameData?.Developers);
    //                            game.Developers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Developers) ? game.Developers : gameData.GameData.Developers;
    //                        }
    //                    }

    //                    // Publisher
    //                    if (settings.Publisher.Import)
    //                    {

    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Publishers)))
    //                        {
    //                            gameData = ProcessField(game, settings.Publisher, ref storeData, ref igdbData, (a) => a.GameData?.Publishers);
    //                            game.Publishers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Publishers) ? game.Publishers : gameData.GameData.Publishers;
    //                        }
    //                    }

    //                    // Tags / Features
    //                    if (settings.Tag.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && ListExtensions.IsNullOrEmpty(game.Tags)))
    //                        {
    //                            gameData = ProcessField(game, settings.Tag, ref storeData, ref igdbData, (a) => a.GameData?.Tags);
    //                            game.Tags = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Tags) ? game.Tags : gameData.GameData.Tags;
    //                        }
    //                    }

    //                    // Description
    //                    if (settings.Description.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Description)))
    //                        {
    //                            gameData = ProcessField(game, settings.Description, ref storeData, ref igdbData, (a) => a.GameData?.Description);
    //                            game.Description = string.IsNullOrEmpty(gameData?.GameData?.Description) == true ? game.Description : gameData.GameData.Description;
    //                        }
    //                    }

    //                    // Links
    //                    if (settings.Links.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.Links == null))
    //                        {
    //                            gameData = ProcessField(game, settings.Links, ref storeData, ref igdbData, (a) => a.GameData?.Links);
    //                            game.Links = gameData?.GameData?.Links ?? game.Links;
    //                        }
    //                    }

    //                    // Critic Score
    //                    if (settings.CriticScore.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CriticScore == null))
    //                        {
    //                            gameData = ProcessField(game, settings.CriticScore, ref storeData, ref igdbData, (a) => a.GameData?.CriticScore);
    //                            game.CriticScore = gameData?.GameData?.CriticScore == null ? game.CriticScore : gameData.GameData.CriticScore;
    //                        }
    //                    }

    //                    // Community Score
    //                    if (settings.CommunityScore.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && game.CommunityScore == null))
    //                        {
    //                            gameData = ProcessField(game, settings.CommunityScore, ref storeData, ref igdbData, (a) => a.GameData?.CommunityScore);
    //                            game.CommunityScore = gameData?.GameData?.CommunityScore == null ? game.CommunityScore : gameData.GameData.CommunityScore;
    //                        }
    //                    }

    //                    // BackgroundImage
    //                    if (settings.BackgroundImage.Import)
    //                    {

    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.BackgroundImage)))
    //                        {
    //                            gameData = ProcessField(game, settings.BackgroundImage, ref storeData, ref igdbData, (a) => a.BackgroundImage);
    //                            game.BackgroundImage = string.IsNullOrEmpty(gameData?.BackgroundImage) ? game.BackgroundImage : gameData.BackgroundImage;
    //                        }
    //                    }

    //                    // Cover
    //                    if (settings.CoverImage.Import)
    //                    {

    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Image)))
    //                        {
    //                            gameData = ProcessField(game, settings.CoverImage, ref storeData, ref igdbData, (a) => a.Image);
    //                            if (gameData?.Image != null)
    //                            {
    //                                if (!string.IsNullOrEmpty(game.Image))
    //                                {
    //                                    database.DeleteImageSafe(game.Image, game);
    //                                }

    //                                var imageId = database.AddFileNoDuplicate(gameData.Image);
    //                                game.Image = imageId;
    //                            }
    //                        }
    //                    }

    //                    // Icon
    //                    if (settings.Icon.Import)
    //                    {
    //                        if (!settings.SkipExistingValues || (settings.SkipExistingValues && string.IsNullOrEmpty(game.Icon)))
    //                        {
    //                            gameData = ProcessField(game, settings.Icon, ref storeData, ref igdbData, (a) => a.Icon);
    //                            if (gameData?.Icon != null)
    //                            {
    //                                if (!string.IsNullOrEmpty(game.Icon))
    //                                {
    //                                    database.DeleteImageSafe(game.Icon, game);
    //                                }

    //                                var iconId = database.AddFileNoDuplicate(gameData.Icon);
    //                                game.Icon = iconId;
    //                            }
    //                        }
    //                    }

    //                    // We need to download and set aditional Steam tasks here because they are only part of metadata
    //                    if (game.Provider == Provider.Steam)
    //                    {
    //                        // Only update them if they don't exist yet
    //                        if (game.OtherTasks?.FirstOrDefault(a => a.IsBuiltIn) == null)
    //                        {
    //                            if (storeData == null)
    //                            {
    //                                storeData = steamProvider.GetMetadata(game.GameId);
    //                            }

    //                            if (storeData?.GameData?.OtherTasks != null)
    //                            {
    //                                if (game.OtherTasks == null)
    //                                {
    //                                    game.OtherTasks = new System.Collections.ObjectModel.ObservableCollection<GameTask>();
    //                                }

    //                                foreach (var task in storeData.GameData.OtherTasks)
    //                                {
    //                                    game.OtherTasks.Add(task);
    //                                }
    //                            }
    //                        }
    //                    }

    //                    // Just to be sure check if somebody didn't remove game while downloading data
    //                    if (database.GamesCollection.FindOne(a => a.GameId == games[i].GameId) != null)
    //                    {
    //                        database.UpdateGameInDatabase(game);
    //                    }
    //                    else
    //                    {
    //                        logger.Warn($"Game {game.GameId} no longer in DB, skipping metadata update in DB.");
    //                    }
    //                }
    //                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
    //                {
    //                    logger.Error(e, $"Failed to download metadata for game {game.Name}, {game.GameId}");
    //                }
    //                finally
    //                {
    //                    processCallback?.Invoke(game, i, games.Count);
    //                }
    //            }
    //        });
    //    }
    //}
}
