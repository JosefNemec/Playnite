using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Playnite.Models;
using Playnite.Providers.GOG;
using Playnite.Providers.Steam;
using Playnite.Providers.Origin;
using System.Windows;

namespace Playnite.Database
{
    public class GameDatabase
    {
        // LiteDB file storage is not thread safe, so we need to lock all file operations.        
        private object fileLock = new object();

        private static GameDatabase instance;
        public static GameDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameDatabase();
                }

                return instance;
            }
        }

        private LiteDatabase database;
        public LiteDatabase Database
        {
            get
            {
                return database;
            }
        }

        private LiteCollection<IGame> dbGames;

        private ObservableCollection<IGame> games = new ObservableCollection<IGame>();
        public  ObservableCollection<IGame> Games
        {
            get
            {
                return games;
            }
        }

        private void CheckDbState()
        {
            if (dbGames == null)
            {
                throw new Exception("Database is not opened.");
            }
        }

        public LiteDatabase OpenDatabase(string path, bool loadGames = false)
        {
            CloseDatabase();
            database = new LiteDatabase(path);
            dbGames = database.GetCollection<IGame>("games");
            if (loadGames == true)
            {
                LoadGamesFromDb();
            }

            return database;
        }

        public void LoadGamesFromDb()
        {
            games.Clear();

            foreach (var game in dbGames.FindAll())
            {
                games.Add(game);
            }
        }

        public void LoadGamesFromDb(Settings settings)
        {
            games.Clear();

            foreach (var game in dbGames.FindAll())
            {
                if (game.Provider == Provider.Steam && !settings.SteamSettings.IntegrationEnabled)
                {
                    continue;
                }

                if (game.Provider == Provider.GOG && !settings.GOGSettings.IntegrationEnabled)
                {
                    continue;
                }

                if (game.Provider == Provider.Origin && !settings.OriginSettings.IntegrationEnabled)
                {
                    continue;
                }

                games.Add(game);
            }
        }

        public void CloseDatabase()
        {
            if (database == null)
            {
                return;
            }

            database.Dispose();
        }

        public void AddGame(IGame game)
        {
            CheckDbState();

            lock (fileLock)
            {
                dbGames.Insert(game);
            }

            games.Add(game);
        }

        public void AddGames(IEnumerable<IGame> games)
        {
            CheckDbState();

            if (games.Count() == 0)
            {
                return;
            }

            foreach (var game in games)
            {
                AddGame(game);
            }
        }
                
        public void DeleteGame(IGame game)
        {
            CheckDbState();

            lock (fileLock)
            {
                DeleteImageSafe(game.Icon, game);
                DeleteImageSafe(game.Image, game);
                dbGames.Delete(game.Id);
            }

            games.Remove(game);
        }

        public void AddImage(string id, string name, byte[] data)
        {
            CheckDbState();

            lock (fileLock)
            {
                using (var stream = new MemoryStream(data))
                {
                    Database.FileStorage.Upload(id, name, stream);
                }
            }
        }

        public MemoryStream GetFileStream(string id)
        {
            CheckDbState();

            var file = Database.FileStorage.FindById(id);

            lock (fileLock)
            {
                using (var fStream = file.OpenRead())
                {
                    var stream = new MemoryStream();
                    fStream.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            }
        }

        /// <summary>
        /// Deletes image from database only if it's not used by any object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="game"></param>
        public void DeleteImageSafe(string id, IGame game)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            CheckDbState();
            dbGames = database.GetCollection<IGame>("games");

            foreach (var gm in dbGames.FindAll())
            {
                if (gm.Id == game.Id)
                {
                    continue;
                }

                if (gm.Icon == id)
                {
                    return;
                }

                if (gm.Image == id)
                {
                    return;
                }

                if (gm.BackgroundImage == id)
                {
                    return;
                }
            }

            Database.FileStorage.Delete(id);
        }

        public void UpdateGame(IGame game)
        {
            CheckDbState();

            if (!Games.Contains(game))
            {
                throw new Exception(string.Format("Trying to update game which is not loaded, id:{0}, provider:{1}", game.ProviderId, game.Provider));
            }

            lock (fileLock)
            {
                dbGames.Update(game);
            }
        }

        public void UpdateGameWithMetadata(IGame game)
        {
            switch (game.Provider)
            {
                case Provider.Steam:
                    UpdateSteamGameWithMetadata(game);
                    break;
                case Provider.GOG:
                    UpdateGogGameWithMetadata(game);
                    break;
                case Provider.Origin:
                    UpdateOriginGameWithMetadata(game);
                    break;
                case Provider.Custom:
                    break;
                default:
                    break;
            }
        }

        #region Origin
        public void UpdateOriginGameWithMetadata(IGame game)
        {
            var metadata = Origin.DownloadGameMetadata(game.ProviderId);
            game.Name = metadata.StoreDetails.i18n.displayName.Replace("™", "");
            game.CommunityHubUrl = metadata.StoreDetails.i18n.gameForumURL;
            game.StoreUrl = "https://www.origin.com/store" + metadata.StoreDetails.offerPath;
            game.WikiUrl = @"http://pcgamingwiki.com/w/index.php?search=" + game.Name;
            game.Description = metadata.StoreDetails.i18n.longDescription;
            game.Developers = new List<string>() { metadata.StoreDetails.developerFacetKey };
            game.Publishers = new List<string>() { metadata.StoreDetails.publisherFacetKey };
            game.ReleaseDate = metadata.StoreDetails.platforms.First(a => a.platform == "PCWIN").releaseDate;

            if (!string.IsNullOrEmpty(metadata.StoreDetails.i18n.gameManualURL))
            {
                game.OtherTasks = new ObservableCollection<GameTask>()
                {
                    new GameTask()
                    {
                        IsBuiltIn = true,
                        Type = GameTaskType.URL,
                        Path = metadata.StoreDetails.i18n.gameManualURL,
                        Name = "Manual"
                    }
                };
            }
            
            var image = string.Format("images/origin/{0}/{1}", game.ProviderId.Replace(":", ""), metadata.Image.Name);
            AddImage(image, metadata.Image.Name, metadata.Image.Data);
            game.Image = image;

            // There's not icon available on Origin servers so we will load one from EXE
            if (game.IsInstalled && string.IsNullOrEmpty(game.Icon))
            {
                var exeIcon = IconExtension.ExtractIconFromExe(game.PlayTask.Path, true);
                if (exeIcon != null)
                {
                    var iconName = Guid.NewGuid() + ".png";
                    var iconId = string.Format("images/origin/{0}/{1}", game.ProviderId.Replace(":", ""), iconName);
                    AddImage(iconId, iconName, exeIcon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                    game.Icon = iconId;
                }
            }

            game.IsProviderDataUpdated = true;
            UpdateGame(game);
        }

        public void UpdateOriginInstalledGames()
        {
            var importedGames = Origin.GetInstalledGames(true);

            foreach (var game in importedGames)
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.ProviderId);

                if (existingGame == null)
                {
                    AddGame(game);
                }
                else
                {
                    existingGame.PlayTask = game.PlayTask;
                    existingGame.InstallDirectory = game.InstallDirectory;
                    UpdateGame(existingGame);
                }
            }

            foreach (var game in Games.Where(a => a.Provider == Provider.Origin))
            {
                if (importedGames.FirstOrDefault(a => a.ProviderId == game.ProviderId) == null)
                {
                    game.PlayTask = null;
                    game.InstallDirectory = string.Empty;
                    UpdateGame(game);
                }
            }
        }

        public void UpdateOriginLibrary()
        {
            var importedGames = Origin.GetOwnedGames();
            foreach (var game in importedGames.Where(a => a.offerType == "basegame"))
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.offerId);
                if (existingGame == null)
                {
                    AddGame(new Game()
                    {
                        Provider = Provider.Origin,
                        ProviderId = game.offerId,
                        Name = game.offerId
                    });
                }
            }
        }
        #endregion Origin

        #region GOG
        public void UpdateGogGameWithMetadata(IGame game)
        {
            var metadata = Gog.DownloadGameMetadata(game.ProviderId, game.StoreUrl);
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

            var icon = string.Format("images/gog/{0}/{1}", game.ProviderId, metadata.Icon.Name);
            AddImage(icon, metadata.Icon.Name, metadata.Icon.Data);
            game.Icon = icon;
            
            using (var imageStream = new MemoryStream())
            {
                using (var tempStream = new MemoryStream(metadata.Image.Data))
                {
                    using (var backStream = Application.GetResourceStream(new Uri("pack://application:,,,/Playnite;component/Resources/Images/gog_cover_background.png")).Stream)
                    {
                        CoverCreator.CreateCover(backStream, tempStream, imageStream);
                        imageStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                var image = string.Format("images/gog/{0}/{1}", game.ProviderId, metadata.Image.Name);
                AddImage(image, metadata.Image.Name, imageStream.ToArray());
                game.Image = image;
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            game.IsProviderDataUpdated = true;
            UpdateGame(game);
        }

        public void UpdateGogInstalledGames()
        {
            var importedGames = Gog.GetInstalledGames();

            foreach (var game in importedGames)
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.ProviderId);

                if (existingGame == null)
                {
                    AddGame(game);
                }
                else
                {
                    existingGame.PlayTask = game.PlayTask;
                    existingGame.OtherTasks = game.OtherTasks;
                    existingGame.InstallDirectory = game.InstallDirectory;
                    UpdateGame(existingGame);
                }
            }

            foreach (var game in Games.Where(a => a.Provider == Provider.GOG))
            {
                if (importedGames.FirstOrDefault(a => a.ProviderId == game.ProviderId) == null)
                {
                    game.PlayTask = null;
                    game.OtherTasks = null;
                    game.InstallDirectory = string.Empty;
                    UpdateGame(game);
                }
            }
        }

        public void UpdateGogLibrary()
        {
            var importedGames = Gog.GetOwnedGames();
            foreach (var game in importedGames)
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.id.ToString());
                if (existingGame == null)
                {
                    // User library has more accurate data (like release date and url),
                    // so we will update them during import and not impor them again during metadata update
                    AddGame(new Game()
                    {
                        Provider = Provider.GOG,
                        ProviderId = game.id.ToString(),
                        Name = game.title,
                        ReleaseDate = game.releaseDate.date,
                        StoreUrl = @"https://www.gog.com" + game.url
                    });
                }
            }
        }
        #endregion GOG

        #region Steam

        public void UpdateSteamGameWithMetadata(IGame game)
        {
            var metadata = Steam.DownloadGameMetadata(int.Parse(game.ProviderId));
            game.Name = metadata.ProductDetails["common"]["name"].Value;
            game.CommunityHubUrl = @"https://steamcommunity.com/app/" + game.ProviderId;
            game.StoreUrl = @"http://store.steampowered.com/app/" + game.ProviderId;
            game.WikiUrl = @"http://pcgamingwiki.com/api/appid.php?appid=" + game.ProviderId;

            if (metadata.StoreDetails != null)
            {
                game.Description = metadata.StoreDetails.detailed_description;
                game.Genres = metadata.StoreDetails.genres?.Select(a => a.description).ToList();
                game.Developers = metadata.StoreDetails.developers;
                game.Publishers = metadata.StoreDetails.publishers;                
                game.ReleaseDate = metadata.StoreDetails.release_date.date;
            }

            var tasks = new ObservableCollection<GameTask>();
            var launchList = metadata.ProductDetails["config"]["launch"].Children;
            foreach (var task in launchList.Skip(1))
            {
                var properties = task["config"];
                if (properties.Name != null)
                {
                    if (properties["oslist"].Name != null)
                    {
                        if (properties["oslist"].Value != "windows")
                        {
                            continue;
                        }
                    }
                }

                // Ignore action without name  - shoudn't be visible to end user
                if (task["description"].Name != null)
                {
                    var newTask = new GameTask()
                    {
                        Name = task["description"].Value,
                        Arguments = task["arguments"].Value ?? string.Empty,
                        Path = task["executable"].Value,
                        IsBuiltIn = true,
                        WorkingDir = game.InstallDirectory
                    };

                    tasks.Add(newTask);
                }
            }

            var manual = metadata.ProductDetails["extended"]["gamemanualurl"];
            if (manual.Name != null)
            {
                tasks.Add((new GameTask()
                {
                    Name = "Manual",
                    Type = GameTaskType.URL,
                    Path = manual.Value,
                    IsBuiltIn = true
                }));
            }

            game.OtherTasks = tasks;

            if (metadata.Icon != null)
            {
                var icon = string.Format("images/steam/{0}/{1}", game.ProviderId, metadata.Icon.Name);
                AddImage(icon, metadata.Icon.Name, metadata.Icon.Data);
                game.Icon = icon;
            }

            if (metadata.Image != null)
            {
                //var image = string.Format("images/steam/{0}/{1}", game.ProviderId, metadata.Image.Name);
                //AddImage(image, metadata.Image.Name, metadata.Image.Data);
                //game.Image = image;

                using (var imageStream = new MemoryStream())
                {
                    using (var tempStream = new MemoryStream(metadata.Image.Data))
                    {
                        using (var backStream = Application.GetResourceStream(new Uri("pack://application:,,,/Playnite;component/Resources/Images/steam_cover_background.png")).Stream)
                        {
                            CoverCreator.CreateCover(backStream, tempStream, imageStream);
                            imageStream.Seek(0, SeekOrigin.Begin);
                        }
                    }

                    var image = string.Format("images/steam/{0}/{1}", game.ProviderId, metadata.Image.Name);
                    AddImage(image, metadata.Image.Name, imageStream.ToArray());
                    game.Image = image;
                }
            }

            if (!string.IsNullOrEmpty(metadata.BackgroundImage))
            {
                game.BackgroundImage = metadata.BackgroundImage;
            }

            game.IsProviderDataUpdated = true;
            UpdateGame(game);
        }

        public void UpdateSteamInstalledGames()
        {
            var importedGames = Steam.GetInstalledGames();
            foreach (var game in importedGames)
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.ProviderId);

                if (existingGame == null)
                {
                    AddGame(game);
                }
                else
                {
                    existingGame.PlayTask = game.PlayTask;
                    existingGame.InstallDirectory = game.InstallDirectory;

                    if (existingGame.OtherTasks != null)
                    {
                        foreach (var task in existingGame.OtherTasks.Where(a => a.Type == GameTaskType.File && a.IsBuiltIn))
                        {
                            task.WorkingDir = game.InstallDirectory;
                        }
                    }

                    UpdateGame(existingGame);
                }
            }

            foreach (var game in Games.Where(a => a.Provider == Provider.Steam))
            {
                if (importedGames.FirstOrDefault(a => a.ProviderId == game.ProviderId) == null)
                {
                    game.PlayTask = null;
                    game.InstallDirectory = string.Empty;
                    UpdateGame(game);
                }
            }
        }

        public void UpdateSteamLibrary(string userName)
        {
            var importedGames = Steam.GetOwnedGames(userName);
            foreach (var game in importedGames)
            {
                var existingGame = Games.FirstOrDefault(a => a.ProviderId == game.appid.ToString());
                if (existingGame == null)
                {
                    AddGame(new Game()
                    {
                        Provider = Provider.Steam,
                        ProviderId = game.appid.ToString(),
                        Name = game.name
                    });
                }
            }
        }
        #endregion Steam
    }
}
