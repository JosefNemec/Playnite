using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NLog;
using Playnite.Models;

namespace Playnite.Providers.GOG
{
    public class Gog
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static List<GetOwnedGamesResult.Product> UserLibraryCache
        {
            get;
            set;
        }

        public static void CacheGogDatabases(string targetPath, string dbfile)
        {
            FileSystem.CreateFolder(targetPath);
            var source = Path.Combine(GogSettings.DBStoragePath, dbfile);
            File.Copy(source, Path.Combine(targetPath, dbfile), true);
        }

        public static List<Game> GetInstalledGames()
        {
            var targetIndexPath = Path.Combine(Paths.TempPath, "index.db");
            CacheGogDatabases(Paths.TempPath, "index.db");

            var games = new List<Game>();

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

        public static List<GetOwnedGamesResult.Product> GetOwnedGames()
        {
            var api = new WebApiClient();
            if (api.GetLoginRequired())
            {
                throw new Exception("User is not logged in.");
            }

            return api.GetOwnedGames();
        }       

        public static GogGameMetadata DownloadGameMetadata(string id, string storeUrl = null)
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
                var image = Web.DownloadData("http:" + gameDetail.images.logo2x);

                metadata.Icon = new GogGameMetadata.ImageData()
                {
                    Name = Path.GetFileName(new Uri(gameDetail.images.icon).AbsolutePath),
                    Data = icon
                };

                metadata.Image = new GogGameMetadata.ImageData()
                {
                    Name = Path.GetFileName(new Uri(gameDetail.images.logo2x).AbsolutePath),
                    Data = image
                };

                metadata.BackgroundImage = "http:" + gameDetail.images.background;
            }

            return metadata;
        }
    }

    public class GogGameMetadata
    {
        public class ImageData
        {
            public string Name
            {
                get;set;
            }

            public byte[] Data
            {
                get; set;
            }
        }

        public ProductApiDetail GameDetails
        {
            get;set;
        }

        public StorePageResult.ProductDetails StoreDetails
        {
            get;set;
        }

        public ImageData Icon
        {
            get;set;
        }

        public ImageData Image
        {
            get;set;
        }

        public string BackgroundImage
        {
            get; set;
        }
    }
}
