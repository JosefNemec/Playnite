using Newtonsoft.Json;
using NLog;
using Playnite.Database;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
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
            FileSystem.CreateFolder(targetPath);
            var source = Path.Combine(GogSettings.DBStoragePath, dbfile);
            File.Copy(source, Path.Combine(targetPath, dbfile), true);
        }

        public Tuple<GameTask, ObservableCollection<GameTask>> GetGameTasks(string gameId, string installDir)
        {
            var gameInfoPath = Path.Combine(installDir, string.Format("goggame-{0}.info", gameId));
            var gameTaskData = JsonConvert.DeserializeObject<GogGameTaskInfo>(File.ReadAllText(gameInfoPath));
            var playTask = gameTaskData.playTasks.First(a => a.isPrimary).ConvertToGenericTask(installDir);
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

        public List<IGame> GetInstalledGamesFromRegistry()
        {
            var games = new List<IGame>();
            var programs = Programs.GetUnistallProgramsList();
            foreach (var program in programs)
            {
                var match = Regex.Match(program.RegistryKeyName, @"(\d+)_is1");
                if (!match.Success || program.Publisher != "GOG.com")
                {
                    continue;
                }

                var gameId = match.Groups[1].Value;
                var game = new Game()
                {
                    InstallDirectory = program.InstallLocation,
                    ProviderId = gameId,
                    Provider = Provider.GOG,
                    Name = program.DisplayName
                };

                try
                {
                    var tasks = GetGameTasks(game.ProviderId, game.InstallDirectory);
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

        public List<IGame> GetInstalledGamesFromGalaxy()
        {
            if (!File.Exists(Path.Combine(GogSettings.DBStoragePath, "index.db")))
            {
                throw new Exception("Cannot import GOG installed games, GOG Galaxy is not installed.");
            }

            var targetIndexPath = Path.Combine(Paths.TempPath, "index.db");
            CacheGogDatabases(Paths.TempPath, "index.db");

            var games = new List<IGame>();

            var db = new SQLiteConnection(@"Data Source=" + targetIndexPath);
            db.Open();

            try
            {
                SQLiteCommand command;
                SQLiteDataReader reader;

                var gameNames = new Dictionary<int, string>();
                string productsNames = "select * from AvailableGameIDNames";
                command = new SQLiteCommand(productsNames, db);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var productId = int.Parse(reader["productId"].ToString());
                    var gameID = int.Parse(reader["gameID"].ToString());

                    // Ignore DLC
                    if (productId == gameID)
                    {
                        gameNames.Add(productId, reader["name"].ToString());
                    }
                }

                string products = "select * from Products";
                command = new SQLiteCommand(products, db);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = int.Parse(reader["productId"].ToString());

                    // Load only installed games
                    int instState = int.Parse(reader["installationState"].ToString());
                    if (instState != 3)
                    {
                        logger.Info("Skipping game " + id + ", not fully installed yet.");
                        continue;
                    }

                    var game = new Game()
                    {
                        InstallDirectory = reader["localpath"].ToString(),
                        ProviderId = id.ToString(),
                        Provider = Provider.GOG,
                        Name = gameNames[id]
                    };

                    try
                    {
                        var tasks = GetGameTasks(game.ProviderId, game.InstallDirectory);
                        game.PlayTask = tasks.Item1;
                        game.OtherTasks = tasks.Item2;
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to get action for GOG game {game.ProviderId}, game not imported.");
                    }

                    games.Add(game);
                }
            }
            finally
            {
                db.Close();
            }

            return games;
        }

        public List<IGame> GetInstalledGames(InstalledGamesSource source)
        {
            if (source == InstalledGamesSource.Galaxy)
            {
                logger.Info("Importing installed GOG games via Galaxy.");
                return GetInstalledGamesFromGalaxy();
            }
            else
            {
                logger.Info("Importing installed GOG games via registry.");
                return GetInstalledGamesFromRegistry();
            }
        }

        public List<IGame> GetLibraryGames()
        {
            using (var api = new WebApiClient())
            {
                if (api.GetLoginRequired())
                {
                    throw new Exception("User is not logged in.");
                }

                var games = new List<IGame>();
                var libGames = api.GetOwnedGames();
                if (libGames == null)
                {
                    throw new Exception("Failed to obtain libary data.");
                }

                foreach (var game in libGames)
                {
                    games.Add(new Game()
                    {
                        Provider = Provider.GOG,
                        ProviderId = game.id.ToString(),
                        Name = game.title,
                        ReleaseDate = game.releaseDate.date,
                        Links = new ObservableCollection<Link>()
                    {
                        new Link("Store", @"https://www.gog.com" + game.url)
                    }
                    });
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

                var icon = Web.DownloadData("http:" + gameDetail.images.icon);
                var iconName = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath);
                var image = Web.DownloadData("http:" + gameDetail.images.logo2x);
                var imageName = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath);

                metadata.Icon = new FileDefinition(
                    string.Format("images/gog/{0}/{1}", id, iconName),
                    iconName,
                    icon
                );

                metadata.Image = new FileDefinition(
                    string.Format("images/gog/{0}/{1}", id, imageName),
                    imageName,
                    image
                );

                metadata.BackgroundImage = "http:" + gameDetail.images.background;
            }

            return metadata;
        }

        public GogGameMetadata UpdateGameWithMetadata(IGame game)
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
