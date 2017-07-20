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
using Playnite.Providers;
using NLog;
using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace Playnite.Database
{
    public delegate void GameUpdatedEventHandler(object sender, GameUpdatedEventArgs args);
    public class GameUpdatedEventArgs : EventArgs
    {
        public IGame OldData
        {
            get; set;
        }

        public IGame NewData
        {
            get; set;
        }

        public GameUpdatedEventArgs(IGame oldData, IGame newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void GamesCollectionChangedEventHandler(object sender, GamesCollectionChangedEventArgs args);
    public class GamesCollectionChangedEventArgs : EventArgs
    {
        public List<IGame> AddedGames
        {
            get; set;
        }

        public List<IGame> RemovedGames
        {
            get; set;
        }

        public GamesCollectionChangedEventArgs(List<IGame> addedGames, List<IGame> removedGames)
        {
            AddedGames = addedGames;
            RemovedGames = removedGames;
        }
    }

    public class FileDefinition
    {
        public string Path
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public byte[] Data
        {
            get; set;
        }

        public FileDefinition()
        {
        }

        public FileDefinition(string path, string name, byte[] data)
        {
            Path = path;
            Name = name;
            Data = data;
        }
    }

    public class GameDatabase
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

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
        public LiteDatabase Database
        {
            get;
            private set;
        }

        public LiteCollection<IGame> GamesCollection
        {
            get;
            private set;
        }

        private IGogLibrary gogLibrary;
        private ISteamLibrary steamLibrary;
        private IOriginLibrary originLibrary;

        public readonly ushort DBVersion = 1;

        public string SteamUserName
        {
            get; set;
        } = string.Empty;
        
        public event GamesCollectionChangedEventHandler GamesCollectionChanged;
        public event GameUpdatedEventHandler GameUpdated;
        public event EventHandler DatabaseOpened;

        public GameDatabase()
        {
            gogLibrary = new GogLibrary();
            steamLibrary = new SteamLibrary();
            originLibrary = new OriginLibrary();
        }

        public GameDatabase(IGogLibrary gogLibrary, ISteamLibrary steamLibrary, IOriginLibrary originLibrary)
        {
            this.gogLibrary = gogLibrary;
            this.steamLibrary = steamLibrary;
            this.originLibrary = originLibrary;
        }

        private void CheckDbState()
        {
            if (GamesCollection == null)
            {
                throw new Exception("Database is not opened.");
            }
        }

        public void MigrateDatabase()
        {
            if (Database == null)
            {
                throw new Exception("Database is not opened.");
            }

            if (Database.Engine.UserVersion == DBVersion)
            {
                return;
            }

            // 0 to 1 migration
            if (Database.Engine.UserVersion == 0 && DBVersion == 1)
            {
                // Create: ObservableCollection<Link>Links
                // Migrate: CommunityHubUrl, StoreUrl, WikiUrl to Links
                // Remove: CommunityHubUrl, StoreUrl, WikiUrl
                logger.Info("Migrating database from 0 to 1 version.");

                var collection = Database.GetCollection("games");
                var dbGames = collection.FindAll();
                foreach (var game in dbGames)
                {
                    var links = new ObservableCollection<Link>();

                    if (game.ContainsKey("CommunityHubUrl"))
                    {
                        links.Add(new Link("Forum", game["CommunityHubUrl"].AsString));
                        game.Remove("CommunityHubUrl");
                    }

                    if (game.ContainsKey("StoreUrl"))
                    {
                        links.Add(new Link("Store", game["StoreUrl"].AsString));
                        game.Remove("StoreUrl");
                    }

                    if (game.ContainsKey("WikiUrl"))
                    {
                        links.Add(new Link("Wiki", game["WikiUrl"].AsString));
                        game.Remove("WikiUrl");
                    }

                    if (links.Count() > 0)
                    {                        
                        game.Add("Links", new BsonArray(links.Select(a => BsonMapper.Global.ToDocument(a))));
                    }

                    collection.Update(game);
                }

                Database.Engine.UserVersion = 1;
            }                      
        }

        public LiteDatabase OpenDatabase(string path)
        {
            logger.Info("Opening db " + path);
            CloseDatabase();
            Database = new LiteDatabase(path);
            MigrateDatabase();

            // To force litedb to try to open file, should throw exceptuion if something is wrong with db file
            Database.GetCollectionNames();

            GamesCollection = Database.GetCollection<IGame>("games");
            DatabaseOpened?.Invoke(this, null);
            return Database;
        }

        public void CloseDatabase()
        {
            if (Database == null)
            {
                return;
            }

            Database.Dispose();
            GamesCollection = null;
        }

        public void AddGame(IGame game)
        {
            CheckDbState();

            lock (fileLock)
            {
                GamesCollection.Insert(game);
            }

            GamesCollectionChanged?.Invoke(this, new GamesCollectionChangedEventArgs(new List<IGame>() { game }, new List<IGame>()));
        }

        public void AddGames(IEnumerable<IGame> games)
        {
            CheckDbState();

            if (games.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                foreach (var game in games)
                {
                    GamesCollection.Insert(game);
                }
            }

            GamesCollectionChanged?.Invoke(this, new GamesCollectionChangedEventArgs(games.ToList(), new List<IGame>()));
        }
                
        public void DeleteGame(IGame game)
        {
            logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
            CheckDbState();

            lock (fileLock)
            {
                DeleteImageSafe(game.Icon, game);
                DeleteImageSafe(game.Image, game);
                GamesCollection.Delete(game.Id);
            }

            GamesCollectionChanged?.Invoke(this, new GamesCollectionChangedEventArgs(new List<IGame>(), new List<IGame>() { game }));
        }

        public void AddImage(string id, string name, byte[] data)
        {
            CheckDbState();

            lock (fileLock)
            {
                using (var stream = new MemoryStream(data))
                {
                    var file = Database.FileStorage.Upload(id, name, stream);
                }
            }
        }

        public MemoryStream GetFileStream(string id)
        {
            CheckDbState();

            var file = Database.FileStorage.FindById(id);
            if (file == null)
            {
                return null;
            }

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

        public LiteFileInfo GetFile(string id)
        {
            lock (fileLock)
            {
                return Database.FileStorage.FindById(id);
            }
        }

        public BitmapImage GetFileImage(string id)
        {
            CheckDbState();

            var file = Database.FileStorage.FindById(id);
            if (file == null)
            {
                return null;
            }

            lock (fileLock)
            {
                using (var fStream = GetFileStream(id))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = fStream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
        }

        public void SaveFile(string id, string path)
        {
            CheckDbState();

            var file = Database.FileStorage.FindById(id);

            lock (fileLock)
            {
                file.SaveAs(path, true);
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
            GamesCollection = Database.GetCollection<IGame>("games");

            var games = GamesCollection.Find(a => (a.Icon == id || a.Image == id || a.BackgroundImage == id) && a.Id != game.Id);
            if (games.Count() == 0)
            {
                Database.FileStorage.Delete(id);
            }
        }

        public void UpdateGameInDatabase(IGame game)
        {
            CheckDbState();
            IGame oldData;

            lock (fileLock)
            {
                oldData = GamesCollection.FindById(game.Id);
                GamesCollection.Update(game);
            }

            GameUpdated?.Invoke(this, new GameUpdatedEventArgs(oldData, game));
        }

        public void UpdateGameWithMetadata(IGame game)
        {
            GameMetadata metadata;

            switch (game.Provider)
            {
                case Provider.Steam:
                    metadata = steamLibrary.UpdateGameWithMetadata(game);
                    break;
                case Provider.GOG:
                    metadata = gogLibrary.UpdateGameWithMetadata(game);
                    break;
                case Provider.Origin:
                    metadata = originLibrary.UpdateGameWithMetadata(game);
                    break;
                case Provider.Custom:
                    return;
                default:
                    return;
            }

            if (metadata.Icon != null)
            {
                AddImage(metadata.Icon.Path, metadata.Icon.Name, metadata.Icon.Data);
                game.Icon = metadata.Icon.Path;
            }

            if (metadata.Image != null)
            {
                AddImage(metadata.Image.Path, metadata.Image.Name, metadata.Image.Data);
                game.Image = metadata.Image.Path;
            }

            UpdateGameInDatabase(game);
        }

        public void UpdateInstalledGames(Provider provider)
        {
            List<IGame> installedGames = null;

            switch (provider)
            {
                case Provider.Custom:
                    return;
                case Provider.GOG:
                    installedGames = gogLibrary.GetInstalledGames();
                    break;
                case Provider.Origin:
                    installedGames = originLibrary.GetInstalledGames(true);
                    break;
                case Provider.Steam:
                    installedGames = steamLibrary.GetInstalledGames();
                    break;
                default:
                    return;
            }

            foreach (var newGame in installedGames)
            {
                var existingGame = GamesCollection.FindOne(a => a.ProviderId == newGame.ProviderId && a.Provider == provider);
                if (existingGame == null)
                {
                    logger.Info("Adding new installed game {0} from {1} provider", newGame.ProviderId, newGame.Provider);
                    AddGame(newGame);
                }
                else
                {
                    existingGame.PlayTask = newGame.PlayTask;
                    existingGame.InstallDirectory = newGame.InstallDirectory;

                    if (newGame.OtherTasks != null)
                    {
                        if (existingGame.OtherTasks == null)
                        {
                            existingGame.OtherTasks = new ObservableCollection<GameTask>();
                        }
                        else
                        {
                            existingGame.OtherTasks = new ObservableCollection<GameTask>(existingGame.OtherTasks.Where(a => !a.IsBuiltIn));
                        }

                        foreach (var task in newGame.OtherTasks.Reverse())
                        {
                            existingGame.OtherTasks.Insert(0, task);
                        }

                        if (provider == Provider.Steam)
                        {
                            foreach (var task in existingGame.OtherTasks.Where(a => a.Type == GameTaskType.File && a.IsBuiltIn))
                            {
                                task.WorkingDir = newGame.InstallDirectory;
                            }
                        }
                    }

                    UpdateGameInDatabase(existingGame);
                }
            }
            
            foreach (var game in GamesCollection.Find(a => a.Provider == provider))
            {
                if (installedGames.FirstOrDefault(a => a.ProviderId == game.ProviderId) == null)
                {
                    game.PlayTask = null;
                    game.InstallDirectory = string.Empty;
                    if (game.OtherTasks != null)
                    {
                        game.OtherTasks = new ObservableCollection<GameTask>(game.OtherTasks.Where(a => !a.IsBuiltIn));
                    }

                    UpdateGameInDatabase(game);
                }
            }
        }

        public void UpdateOwnedGames(Provider provider)
        {
            List<IGame> importedGames = null;

            switch (provider)
            {
                case Provider.Custom:
                    return;
                case Provider.GOG:
                    importedGames = gogLibrary.GetLibraryGames();
                    break;
                case Provider.Origin:
                    importedGames = originLibrary.GetLibraryGames();
                    break;
                case Provider.Steam:
                    importedGames = steamLibrary.GetLibraryGames(SteamUserName);
                    break;
                default:
                    return;
            }

            foreach (var game in importedGames)
            {
                var existingGame = GamesCollection.FindOne(a => a.ProviderId == game.ProviderId && a.Provider == provider);
                if (existingGame == null)
                {
                    logger.Info("Adding new game {0} into library from {1} provider", game.ProviderId, game.Provider);
                    AddGame(game);
                }
            }

            // Delete games that are no longer in library
            foreach (IGame dbGame in GamesCollection.Find(a => a.Provider == provider))
            {
                if (importedGames.FirstOrDefault(a => a.ProviderId == dbGame.ProviderId) == null)
                {
                    // Except games that are installed, in case that game is part of free weekend, beta or similar events
                    if (dbGame.IsInstalled)
                    {
                        continue;
                    }

                    logger.Info("Removing game {0} which is no longer in {1} library", dbGame.ProviderId, dbGame.Provider);
                    DeleteGame(dbGame);
                }
            }
        }
    }
}
