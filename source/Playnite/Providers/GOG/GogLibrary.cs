﻿using Newtonsoft.Json;
using NLog;
using Playnite.Metadata;
using Playnite.SDK.Models;
using Playnite.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace Playnite.Providers.GOG
{
    public enum InstalledGamesSource
    {
        Registry,
        Galaxy
    }

    public class GogLibrary : IGogLibrary
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        
        public void CacheGogDatabases(string targetPath, string dbfile)
        {
            FileSystem.CreateDirectory(targetPath);
            var source = Path.Combine(GogSettings.DBStoragePath, dbfile);
            File.Copy(source, Path.Combine(targetPath, dbfile), true);
        }

        public Tuple<GameTask, ObservableCollection<GameTask>> GetGameTasks(string gameId, string installDir)
        {
            var gameInfoPath = Path.Combine(installDir, string.Format("goggame-{0}.info", gameId));
            var gameTaskData = JsonConvert.DeserializeObject<GogGameTaskInfo>(File.ReadAllText(gameInfoPath));
            var playTask = gameTaskData.playTasks.FirstOrDefault(a => a.isPrimary)?.ConvertToGenericTask(installDir);
            var otherTasks = new ObservableCollection<GameTask>();

            foreach (var task in gameTaskData.playTasks.Where(a => !a.isPrimary))
            {
                otherTasks.Add(task.ConvertToGenericTask(installDir));
            }

            if (gameTaskData.supportTasks != null)
            {
                foreach (var task in gameTaskData.supportTasks)
                {
                    otherTasks.Add(task.ConvertToGenericTask(installDir));
                }
            }

            return new Tuple<GameTask, ObservableCollection<GameTask>>(playTask, otherTasks.Count > 0 ? otherTasks : null);
        }

        public List<Game> GetInstalledGames()
        {
            var games = new List<Game>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                var match = Regex.Match(program.RegistryKeyName, @"(\d+)_is1");
                if (!match.Success || program.Publisher != "GOG.com")
                {
                    continue;
                }

                if (!Directory.Exists(program.InstallLocation))
                {
                    continue;
                }

                var gameId = match.Groups[1].Value;
                var game = new Game()
                {
                    InstallDirectory = Paths.FixSeparators(program.InstallLocation),
                    ProviderId = gameId,
                    Provider = Provider.GOG,
                    Source = Enums.GetEnumDescription(Provider.GOG),
                    Name = program.DisplayName
                };

                try
                {
                    var tasks = GetGameTasks(game.ProviderId, game.InstallDirectory);
                    // Empty play task = DLC
                    if (tasks.Item1 == null)
                    {
                        continue;
                    }

                    game.PlayTask = tasks.Item1;
                    game.OtherTasks = tasks.Item2;
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get action for GOG game {game.ProviderId}, game not imported.");
                }

                games.Add(game);
            }

            return games;
        }

        public List<Game> GetLibraryGames()
        {
            using (var api = new WebApiClient())
            {
                if (api.GetLoginRequired())
                {
                    throw new Exception("User is not logged in.");
                }

                var games = new List<Game>();
                var acc = api.GetAccountInfo();
                var libGames = api.GetOwnedGames(acc);
                if (libGames == null)
                {
                    throw new Exception("Failed to obtain libary data.");
                }

                foreach (var game in libGames)
                {
                    var newGame = new Game()
                    {
                        Provider = Provider.GOG,
                        Source = Enums.GetEnumDescription(Provider.GOG),
                        ProviderId = game.game.id,
                        Name = game.game.title,
                        Links = new ObservableCollection<Link>()
                        {
                            new Link("Store", @"https://www.gog.com" + game.game.url)
                        }
                    };                    

                    if (game.stats != null && game.stats.ContainsKey(acc.userId))
                    {
                        newGame.Playtime = game.stats[acc.userId].playtime * 60;
                        newGame.LastActivity = game.stats[acc.userId].lastSession;
                    }

                    games.Add(newGame);
                }

                return games;
            }
        }

        public GogGameMetadata DownloadGameMetadata(string id, string storeUrl = null)
        {
            var metadata = new GogGameMetadata();
            var gameDetail = WebApiClient.GetGameDetails(id);
            metadata.GameDetails = gameDetail;

            if (gameDetail != null)
            {
                if (gameDetail.links.product_card != @"https://www.gog.com/" && !string.IsNullOrEmpty(gameDetail.links.product_card))
                {
                    metadata.StoreDetails = WebApiClient.GetGameStoreData(gameDetail.links.product_card);
                }
                else if (!string.IsNullOrEmpty(storeUrl))
                {
                    metadata.StoreDetails = WebApiClient.GetGameStoreData(storeUrl);
                }

                var icon = HttpDownloader.DownloadData("http:" + gameDetail.images.icon);
                var iconName = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath);
                var image = HttpDownloader.DownloadData("http:" + gameDetail.images.logo2x);
                var imageName = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath);

                metadata.Icon = new MetadataFile(
                    string.Format("images/gog/{0}/{1}", id, iconName),
                    iconName,
                    icon
                );

                metadata.Image = new MetadataFile(
                    string.Format("images/gog/{0}/{1}", id, imageName),
                    imageName,
                    image
                );

                metadata.BackgroundImage = "http:" + gameDetail.images.background;
            }

            return metadata;
        }

        public GogGameMetadata UpdateGameWithMetadata(Game game)
        {
            var currentUrl = string.Empty;
            var metadata = DownloadGameMetadata(game.ProviderId, currentUrl);
            if(metadata.GameDetails == null)
            {
                logger.Warn($"Could not gather metadata for game {game.ProviderId}");
                return metadata;
            }

            game.Name = StringExtensions.NormalizeGameName(metadata.GameDetails.title);
            game.Description = metadata.GameDetails.description.full;
            game.Links = new ObservableCollection<Link>()
            {
                new Link("Wiki", @"http://pcgamingwiki.com/w/index.php?search=" + metadata.GameDetails.title)
            };
            
            if (!string.IsNullOrEmpty(metadata.GameDetails.links.forum))
            {
                game.Links.Add(new Link("Forum", metadata.GameDetails.links.forum));
            };

            if (string.IsNullOrEmpty(currentUrl) && !string.IsNullOrEmpty(metadata.GameDetails.links.product_card))
            {
                game.Links.Add(new Link("Store", metadata.GameDetails.links.product_card));
            };

            if (metadata.StoreDetails != null)
            {
                game.Genres = new ComparableList<string>(metadata.StoreDetails.genres.Select(a => a.name));
                game.Developers = new ComparableList<string>() { metadata.StoreDetails.developer.name };
                game.Publishers = new ComparableList<string>() { metadata.StoreDetails.publisher.name };
                game.CommunityScore = metadata.StoreDetails.rating != 0 ? metadata.StoreDetails.rating * 2 : (int?)null;

                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                game.Tags = new ComparableList<string>(metadata.StoreDetails.features?.Select(a => cultInfo.ToTitleCase(a.title)));

                if (game.ReleaseDate == null && metadata.StoreDetails.releaseDate != null)
                {
                    game.ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(metadata.StoreDetails.releaseDate.Value).DateTime;
                }
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            return metadata;
        }
    }

    public class GogGameMetadata : GameMetadata
    {
        public ProductApiDetail GameDetails
        {
            get;set;
        }

        public StorePageResult.ProductDetails StoreDetails
        {
            get;set;
        }
    }
}
