using Newtonsoft.Json;
using NLog;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;

namespace Playnite.Providers.GOG
{
    public class GogLibrary : IGogLibrary
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        
        public void CacheGogDatabases(string targetPath, string dbfile)
        {
            FileSystem.CreateFolder(targetPath);
            var source = Path.Combine(GogSettings.DBStoragePath, dbfile);
            File.Copy(source, Path.Combine(targetPath, dbfile), true);
        }

        public List<IGame> GetInstalledGames()
        {
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
                    
                    var gameInfoPath = Path.Combine(game.InstallDirectory, string.Format("goggame-{0}.info", game.ProviderId));
                    var gameTaskData = JsonConvert.DeserializeObject<GogGameTaskInfo>(File.ReadAllText(gameInfoPath));

                    game.PlayTask = gameTaskData.playTasks.First(a => a.isPrimary).ConvertToGenericTask(game.InstallDirectory);
                    game.OtherTasks = new ObservableCollection<GameTask>();

                    foreach (var task in gameTaskData.playTasks.Where(a => !a.isPrimary))
                    {
                        game.OtherTasks.Add(task.ConvertToGenericTask(game.InstallDirectory));
                    }

                    if (gameTaskData.supportTasks != null)
                    {
                        foreach (var task in gameTaskData.supportTasks)
                        {
                            game.OtherTasks.Add(task.ConvertToGenericTask(game.InstallDirectory));
                        }
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

        public List<IGame> GetLibraryGames()
        {
            var api = new WebApiClient();
            if (api.GetLoginRequired())
            {
                throw new Exception("User is not logged in.");
            }

            var games = new List<IGame>();

            foreach (var game in api.GetOwnedGames())
            {
                games.Add(new Game()
                {
                    Provider = Provider.GOG,
                    ProviderId = game.id.ToString(),
                    Name = game.title,
                    ReleaseDate = game.releaseDate.date,
                    StoreUrl = @"https://www.gog.com" + game.url
                });
            }

            return games;
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
            var metadata = DownloadGameMetadata(game.ProviderId, game.StoreUrl);
            game.Name = metadata.GameDetails.title;
            game.CommunityHubUrl = metadata.GameDetails.links.forum;
            game.StoreUrl = string.IsNullOrEmpty(game.StoreUrl) ? metadata.GameDetails.links.product_card : game.StoreUrl;
            game.WikiUrl = @"http://pcgamingwiki.com/w/index.php?search=" + metadata.GameDetails.title;
            game.Description = metadata.GameDetails.description.full;

            if (metadata.StoreDetails != null)
            {
                game.Genres = metadata.StoreDetails.genres.Select(a => a.name).ToList();
                game.Developers = new List<string>() { metadata.StoreDetails.developer.name };
                game.Publishers = new List<string>() { metadata.StoreDetails.publisher.name };

                if (game.ReleaseDate == null)
                {
                    Int64 intDate = Convert.ToInt64(metadata.StoreDetails.releaseDate) * 1000;
                    game.ReleaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(intDate).ToUniversalTime();
                }
            }

            using (var imageStream = new MemoryStream())
            {
                using (var tempStream = new MemoryStream(metadata.Image.Data))
                {
                    using (var backStream = Application.GetResourceStream(new Uri("pack://application:,,,/Playnite;component/Resources/Images/gog_cover_background.png")).Stream)
                    {
                        CoverCreator.CreateCover(backStream, tempStream, imageStream);
                        imageStream.Seek(0, SeekOrigin.Begin);
                        metadata.Image.Data = imageStream.ToArray();
                    }
                }
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            game.IsProviderDataUpdated = true;
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
