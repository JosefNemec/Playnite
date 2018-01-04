using NLog;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers.BattleNet;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.MetaProviders
{
    public interface IMetadataProvider
    {
        bool GetSupportsIdSearch();
        List<MetadataSearchResult> SearchGames(string gameName);
        GameMetadata GetGameData(string gameId);
    }

    public enum MetadataGamesSource
    {
        AllFromDB,
        Selected,
        Filtered
    }

    public enum MetadataSource
    {
        Store,
        IGDB,
        IGDBOverStore,
        StoreOverIGDB
    }

    public class MetadataFieldSettings : ObservableObject
    {
        private bool import = true;
        public bool Import
        {
            get => import;
            set
            {
                import = value;
                OnPropertyChanged("Import");
            }
        }

        private MetadataSource source = MetadataSource.StoreOverIGDB;
        public MetadataSource Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }

        public MetadataFieldSettings()
        {
        }

        public MetadataFieldSettings(bool import, MetadataSource source)
        {
            Import = import;
            Source = source;
        }
    }

    public class MetadataDownloaderSettings : ObservableObject
    {
        private MetadataGamesSource gamesSource = MetadataGamesSource.AllFromDB;
        public MetadataGamesSource GamesSource
        {
            get
            {
                return gamesSource;
            }

            set
            {
                gamesSource = value;
                OnPropertyChanged("GamesSource");
            }
        }

        private MetadataFieldSettings name = new MetadataFieldSettings(false, MetadataSource.Store);
        public MetadataFieldSettings Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private MetadataFieldSettings genre = new MetadataFieldSettings();
        public MetadataFieldSettings Genre
        {
            get => genre;
            set
            {
                genre = value;
                OnPropertyChanged("Genre");
            }
        }

        private MetadataFieldSettings releaseDate = new MetadataFieldSettings();
        public MetadataFieldSettings ReleaseDate
        {
            get => releaseDate;
            set
            {
                releaseDate = value;
                OnPropertyChanged("ReleaseDate");
            }
        }

        private MetadataFieldSettings developer = new MetadataFieldSettings();
        public MetadataFieldSettings Developer
        {
            get => developer;
            set
            {
                developer = value;
                OnPropertyChanged("Developer");
            }
        }

        private MetadataFieldSettings publisher = new MetadataFieldSettings();
        public MetadataFieldSettings Publisher
        {
            get => publisher;
            set
            {
                publisher = value;
                OnPropertyChanged("Publisher");
            }
        }

        private MetadataFieldSettings tag = new MetadataFieldSettings();
        public MetadataFieldSettings Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged("Tag");
            }
        }

        private MetadataFieldSettings description = new MetadataFieldSettings();
        public MetadataFieldSettings Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        private MetadataFieldSettings coverImage = new MetadataFieldSettings() { Source = MetadataSource.IGDBOverStore };
        public MetadataFieldSettings CoverImage
        {
            get => coverImage;
            set
            {
                coverImage = value;
                OnPropertyChanged("CoverImage");
            }
        }

        private MetadataFieldSettings backgroundImage = new MetadataFieldSettings() { Source = MetadataSource.Store };
        public MetadataFieldSettings BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        private MetadataFieldSettings icon = new MetadataFieldSettings() { Source = MetadataSource.Store };
        public MetadataFieldSettings Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private MetadataFieldSettings links = new MetadataFieldSettings();
        public MetadataFieldSettings Links
        {
            get => links;
            set
            {
                links = value;
                OnPropertyChanged("Links");
            }
        }

        public void ConfigureFields(MetadataSource source, bool import)
        {
            Genre.Import = import;
            Genre.Source = source;
            Description.Import = import;
            Description.Source = source;
            Developer.Import = import;
            Developer.Source = source;
            Publisher.Import = import;
            Publisher.Source = source;
            Tag.Import = import;
            Tag.Source = source;
            Links.Import = import;
            Links.Source = source;
            CoverImage.Import = import;
            CoverImage.Source = source;
            BackgroundImage.Import = import;
            BackgroundImage.Source = source;
            Icon.Import = import;
            Icon.Source = source;
            ReleaseDate.Import = import;
            ReleaseDate.Source = source;
        }
    }

    public class MetadataSearchResult
    {
        public string Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public DateTime? ReleaseDate
        {
            get; set;
        }

        public MetadataSearchResult()
        {
        }

        public MetadataSearchResult(string id, string name, DateTime? releaseDate)
        {
            Id = id;
            Name = name;
            ReleaseDate = releaseDate;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MetadataDownloader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        IMetadataProvider steamProvider;
        IMetadataProvider originProvider;
        IMetadataProvider gogProvider;
        IMetadataProvider battleNetProvider;
        IMetadataProvider igdbProvider;

        public MetadataDownloader()
        {
            steamProvider = new SteamMetadataProvider();
            originProvider = new OriginMetadataProvider();
            gogProvider = new GogMetadataProvider();
            battleNetProvider = new BattleNetMetadataProvider();
            igdbProvider = new IGDBMetadataProvider();
        }

        public MetadataDownloader(string igdbApiKey)
        {
            steamProvider = new SteamMetadataProvider();
            originProvider = new OriginMetadataProvider();
            gogProvider = new GogMetadataProvider();
            battleNetProvider = new BattleNetMetadataProvider();
            igdbProvider = new IGDBMetadataProvider(igdbApiKey);
        }

        public MetadataDownloader(
            IMetadataProvider steamProvider,
            IMetadataProvider originProvider,
            IMetadataProvider gogProvider,
            IMetadataProvider battleNetProvider,
            IMetadataProvider igdbProvider)
        {
            this.steamProvider = steamProvider;
            this.originProvider = originProvider;
            this.gogProvider = gogProvider;
            this.battleNetProvider = battleNetProvider;
            this.igdbProvider = igdbProvider;
        }

        private IMetadataProvider GetMetaProviderByProvider(Provider provider)
        {
            switch (provider)
            {
                case Provider.GOG:
                    return gogProvider;
                case Provider.Origin:
                    return originProvider;
                case Provider.Steam:
                    return steamProvider;
                case Provider.BattleNet:
                    return battleNetProvider;
                case Provider.Uplay:
                case Provider.Custom:
                default:
                    return null;
            }
        }

        private string ReplaceNumsForRomans(Match m)
        {
            return Roman.To(int.Parse(m.Value));
        }

        private GameMetadata ProcessDownload(IGame game, ref GameMetadata data)
        {
            if (data == null)
            {
                data = DownloadGameData(game.Name, game.ProviderId, GetMetaProviderByProvider(game.Provider));
            }

            return data;
        }

        private GameMetadata ProcessField(
            IGame game,
            MetadataFieldSettings field,
            ref GameMetadata storeData,
            ref GameMetadata igdbData,
            Func<GameMetadata, object> propertySelector)
        {
            if (field.Import)
            {
                if (field.Source == MetadataSource.Store && game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
                {
                    storeData = ProcessDownload(game, ref storeData);
                    return storeData;
                }
                else if (field.Source == MetadataSource.IGDB)
                {
                    if (igdbData == null)
                    {
                        igdbData = DownloadGameData(game.Name, "", igdbProvider);
                    }

                    return igdbData;
                }
                else if (field.Source == MetadataSource.IGDBOverStore)
                {
                    if (igdbData == null)
                    {
                        igdbData = DownloadGameData(game.Name, "", igdbProvider);
                    }

                    if (igdbData.GameData == null && game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
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
                        else if (game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
                        {
                            if (storeData == null)
                            {
                                storeData = ProcessDownload(game, ref storeData);
                            }

                            if (storeData.GameData != null)
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
                    if (game.Provider != Provider.Custom && game.Provider != Provider.Uplay)
                    {
                        if (storeData == null)
                        {
                            storeData = ProcessDownload(game, ref storeData);
                        }

                        if (storeData.GameData == null)
                        {
                            if (igdbData == null)
                            {
                                igdbData = DownloadGameData(game.Name, "", igdbProvider);
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
                                    igdbData = DownloadGameData(game.Name, "", igdbProvider);
                                }

                                return igdbData;
                            }
                        }
                    }
                    else
                    {
                        if (igdbData == null)
                        {
                            igdbData = DownloadGameData(game.Name, "", igdbProvider);
                        }

                        return igdbData;
                    }
                }
            }

            return null;
        }

        public async Task DownloadMetadataThreaded(
            List<IGame> games,
            GameDatabase database,
            MetadataDownloaderSettings settings,
            Action<IGame, int, int> processCallback,
            CancellationTokenSource cancelToken)
        {
            int index = 0;
            int total = games.Count;
            var tasks = new List<Task>();

            await Task.Factory.StartNew(() =>
            {
                var grouped = games.GroupBy(a => a.Provider);
                foreach (IGrouping<Provider, IGame> group in grouped)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        var gms = group.ToList();
                        DownloadMetadata(gms, database, settings, (g, i, t) =>
                        {
                            index++;
                            processCallback?.Invoke(g, index, total);
                        }, cancelToken).Wait();
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            });
        }

        public async Task DownloadMetadata(
        List<IGame> games,
        GameDatabase database,
        MetadataDownloaderSettings settings,
        Action<IGame, int, int> processCallback,
        CancellationTokenSource cancelToken)
        {
            await Task.Factory.StartNew(() =>
            {
                if (games == null || games.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < games.Count; i++)
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
                    var game = database.GamesCollection.FindOne(a => a.ProviderId == games[i].ProviderId);
                    if (game == null)
                    {
                        processCallback?.Invoke(null, i, games.Count);
                        continue;
                    }

                    try
                    {
                        // Name
                        if (game.Provider != Provider.Custom && settings.Name.Import)
                        {
                            gameData = ProcessField(game, settings.Name, ref storeData, ref igdbData, (a) => a.GameData?.Name);
                            if (!string.IsNullOrEmpty(gameData?.GameData?.Name))
                            {
                                game.Name = StringExtensions.NormalizeGameName(gameData.GameData.Name);
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
                            gameData = ProcessField(game, settings.Genre, ref storeData, ref igdbData, (a) => a.GameData?.Genres);
                            game.Genres = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Genres) ? game.Genres : gameData.GameData.Genres;
                        }

                        // Release Date
                        if (settings.ReleaseDate.Import)
                        {
                            gameData = ProcessField(game, settings.ReleaseDate, ref storeData, ref igdbData, (a) => a.GameData?.ReleaseDate);
                            game.ReleaseDate = gameData?.GameData?.ReleaseDate ?? game.ReleaseDate;
                        }

                        // Developer
                        if (settings.Developer.Import)
                        {
                            gameData = ProcessField(game, settings.Developer, ref storeData, ref igdbData, (a) => a.GameData?.Developers);
                            game.Developers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Developers) ? game.Developers : gameData.GameData.Developers;
                        }

                        // Publisher
                        if (settings.Publisher.Import)
                        {
                            gameData = ProcessField(game, settings.Publisher, ref storeData, ref igdbData, (a) => a.GameData?.Publishers);
                            game.Publishers = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Publishers) ? game.Publishers : gameData.GameData.Publishers;
                        }

                        // Tags / Features
                        if (settings.Tag.Import)
                        {
                            gameData = ProcessField(game, settings.Tag, ref storeData, ref igdbData, (a) => a.GameData?.Tags);
                            game.Tags = ListExtensions.IsNullOrEmpty(gameData?.GameData?.Tags) ? game.Tags : gameData.GameData.Tags;
                        }

                        // Description
                        if (settings.Description.Import)
                        {
                            gameData = ProcessField(game, settings.Description, ref storeData, ref igdbData, (a) => a.GameData?.Description);
                            game.Description = string.IsNullOrEmpty(gameData?.GameData?.Description) == true ? game.Description : gameData.GameData.Description;
                        }

                        // Links
                        if (settings.Links.Import)
                        {
                            gameData = ProcessField(game, settings.Links, ref storeData, ref igdbData, (a) => a.GameData?.Links);
                            game.Links = gameData?.GameData?.Links ?? game.Links;
                        }

                        // BackgroundImage
                        if (settings.BackgroundImage.Import)
                        {
                            gameData = ProcessField(game, settings.BackgroundImage, ref storeData, ref igdbData, (a) => a.BackgroundImage);
                            game.BackgroundImage = string.IsNullOrEmpty(gameData?.BackgroundImage) ? game.BackgroundImage : gameData.BackgroundImage;
                        }

                        // Cover
                        if (settings.CoverImage.Import)
                        {
                            gameData = ProcessField(game, settings.CoverImage, ref storeData, ref igdbData, (a) => a.Image);
                            if (gameData?.Image != null)
                            {
                                if (!string.IsNullOrEmpty(game.Image))
                                {
                                    database.DeleteImageSafe(game.Image, game);
                                }

                                var imageId = database.AddFileNoDuplicate(gameData.Image);
                                game.Image = imageId;
                            }
                        }

                        // Icon
                        if (settings.Icon.Import)
                        {
                            gameData = ProcessField(game, settings.Icon, ref storeData, ref igdbData, (a) => a.Icon);
                            if (gameData?.Icon != null)
                            {
                                if (!string.IsNullOrEmpty(game.Icon))
                                {
                                    database.DeleteImageSafe(game.Icon, game);
                                }

                                var iconId = database.AddFileNoDuplicate(gameData.Icon);
                                game.Icon = iconId;
                            }
                        }

                        // Just to be sure check if somebody didn't remove game while downloading data
                        if (database.GamesCollection.FindOne(a => a.ProviderId == games[i].ProviderId) != null)
                        {
                            database.UpdateGameInDatabase(game);
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to download metadata for game {game.Name}, {game.ProviderId}");
                    }
                    finally
                    {
                        processCallback?.Invoke(game, i, games.Count);
                    }
                }
            });
        }

        public async Task DownloadMetadata(
            List<IGame> games,
            GameDatabase database,
            MetadataDownloaderSettings settings,
            Action<IGame, int, int> processCallback)
        {
            await DownloadMetadata(games, database, settings, processCallback, null);
        }

        public async Task DownloadMetadata(
            List<IGame> games,
            GameDatabase database,
            MetadataDownloaderSettings settings)
        {
            await DownloadMetadata(games, database, settings, null, null);
        }

        public virtual GameMetadata DownloadGameData(string gameName, string id, IMetadataProvider provider)
        {
            if (provider.GetSupportsIdSearch())
            {
                return provider.GetGameData(id);
            }
            else
            {
                var name = StringExtensions.NormalizeGameName(gameName);
                var results = provider.SearchGames(name);
                results.ForEach(a => a.Name = StringExtensions.NormalizeGameName(a.Name));

                GameMetadata matchFun(string matchName, List<MetadataSearchResult> list)
                {
                    var res = list.FirstOrDefault(a => string.Equals(matchName, a.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (res != null)
                    {
                        return provider.GetGameData(res.Id);
                    }
                    else
                    {
                        return null;
                    }
                }

                GameMetadata data = null;
                string testName = string.Empty;

                // Direct comparison
                data = matchFun(name, results);
                if (data != null)
                {
                    return data;
                }

                // Try replacing roman numerals: 3 => III
                testName = Regex.Replace(name, @"\d+", ReplaceNumsForRomans);
                data = matchFun(testName, results);
                if (data != null)
                {
                    return data;
                }

                // Try adding The
                testName = "The " + name;
                data = matchFun(testName, results);
                if (data != null)
                {
                    return data;
                }

                // Try chaning & / and
                testName = Regex.Replace(name, @"\s+and\s+", " & ", RegexOptions.IgnoreCase);
                data = matchFun(testName, results);
                if (data != null)
                {
                    return data;
                }

                // Try removing all ":"
                testName = Regex.Replace(testName, @"\s*:\s*", " ");
                var resCopy = results.CloneJson();
                resCopy.ForEach(a => a.Name = Regex.Replace(a.Name, @"\s*:\s*", " "));
                data = matchFun(testName, resCopy);
                if (data != null)
                {
                    return data;
                }

                // Try without subtitle
                var testResult = results.OrderBy(a => a.ReleaseDate).FirstOrDefault(a =>
                {
                    if (a.ReleaseDate == null)
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(a.Name) && a.Name.Contains(":"))
                    {
                        return string.Equals(name, a.Name.Split(':')[0], StringComparison.InvariantCultureIgnoreCase);
                    }
                    
                    return false;
                });

                if (testResult != null)
                {
                    return provider.GetGameData(testResult.Id);
                }

                return data ?? new GameMetadata();
            }
        }
    }
}
