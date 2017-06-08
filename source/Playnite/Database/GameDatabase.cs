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

namespace Playnite.Database
{
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

        private LiteDatabase database;
        public LiteDatabase Database
        {
            get
            {
                return database;
            }
        }
        
        private LiteCollection<IGame> dbCollection;

        private ObservableCollection<IGame> games = new ObservableCollection<IGame>();
        public  ObservableCollection<IGame> Games
        {
            get
            {
                return games;
            }
        }

        private IGogLibrary gogLibrary;
        private ISteamLibrary steamLibrary;
        private IOriginLibrary originLibrary;

        public readonly ushort DBVersion = 1;

        public string SteamUserName
        {
            get; set;
        } = string.Empty;

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
            if (dbCollection == null)
            {
                throw new Exception("Database is not opened.");
            }
        }

        public void MigrateDatabase()
        {
            if (database == null)
            {
                throw new Exception("Database is not opened.");
            }

            if (database.Engine.UserVersion == DBVersion)
            {
                return;
            }

            // 0 to 1 migration
            if (database.Engine.UserVersion == 0 && DBVersion == 1)
            {
                // Create: ObservableCollection<Link>Links
                // Migrate: CommunityHubUrl, StoreUrl, WikiUrl to Links
                // Remove: CommunityHubUrl, StoreUrl, WikiUrl
                logger.Info("Migrating database from 0 to 1 version.");

                var collection = database.GetCollection("games");
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

                database.Engine.UserVersion = 1;
            }

                      
        }

        public LiteDatabase OpenDatabase(string path, bool loadGames = false)
        {
            logger.Info("Opening db " + path);
            CloseDatabase();
            database = new LiteDatabase(path);
            MigrateDatabase();

            // To force litedb to try to open file, should throw exceptuion if something is wrong with db file
            database.GetCollectionNames();

            dbCollection = database.GetCollection<IGame>("games");
            if (loadGames == true)
            {
                LoadGamesFromDb();
            }

            return database;
        }

        public void LoadGamesFromDb()
        {
            logger.Info("Loading games from db");
            games.Clear();

            foreach (var game in dbCollection.FindAll())
            {
                games.Add(game);
            }
        }

        public void LoadGamesFromDb(Settings settings)
        {
            logger.Info("Loading games from db with specific settings.");
            logger.Info("Steam: " + settings.SteamSettings.ToJson());
            logger.Info("Origin: " + settings.OriginSettings.ToJson());
            logger.Info("GOG: " + settings.GOGSettings.ToJson());

            games.Clear();

            foreach (var game in dbCollection.FindAll())
            {
                if (game.Provider == Provider.Steam && !settings.SteamSettings.IntegrationEnabled)
                {
                    continue;
                }
                else if (game.Provider == Provider.Steam && !game.IsInstalled && !settings.SteamSettings.LibraryDownloadEnabled)
                {
                    continue;
                }

                if (game.Provider == Provider.GOG && !settings.GOGSettings.IntegrationEnabled)
                {
                    continue;
                }
                else if (game.Provider == Provider.GOG && !game.IsInstalled && !settings.GOGSettings.LibraryDownloadEnabled)
                {
                    continue;
                }

                if (game.Provider == Provider.Origin && !settings.OriginSettings.IntegrationEnabled)
                {
                    continue;
                }
                else if (game.Provider == Provider.Origin && !game.IsInstalled && !settings.OriginSettings.LibraryDownloadEnabled)
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
                dbCollection.Insert(game);
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
            logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
            CheckDbState();

            lock (fileLock)
            {
                DeleteImageSafe(game.Icon, game);
                DeleteImageSafe(game.Image, game);
                dbCollection.Delete(game.Id);
            }

            var existingGame = games.FirstOrDefault(a => a.Id == game.Id);
            if (existingGame != null)
            {
                games.Remove(existingGame);
            }
            else
            {
                logger.Error("Attempt to delete game not present in database.");
            }
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
            dbCollection = database.GetCollection<IGame>("games");

            foreach (var gm in dbCollection.FindAll())
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

        public void UpdateGameInDatabase(IGame game)
        {
            CheckDbState();

            lock (fileLock)
            {
                dbCollection.Update(game);

                // Update loaded instance of a game
                var loadedGame = Games.First(a => a.Id == game.Id);
                game.CopyProperties(loadedGame, true);
            }
        }

        public void UnloadNotInstalledGames(Provider provider)
        {
            var notInstalledGames = games.Where(a => a.Provider == provider && !a.IsInstalled).ToList();

            foreach (var game in notInstalledGames)
            {
                games.Remove(game);
            }
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
                var existingGame = dbCollection.FindAll().FirstOrDefault(a => a.ProviderId == newGame.ProviderId && a.Provider == provider);

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

                    // Game may have been not installed prviously and may not be loaded currently
                    var loaded = Games.FirstOrDefault(a => a.ProviderId == existingGame.ProviderId && a.Provider == existingGame.Provider) != null;
                    if (!loaded)
                    {
                        Games.Add(existingGame);
                    }

                    UpdateGameInDatabase(existingGame);
                }
            }

            // No longer installed games must be updated
            foreach (var game in Games.Where(a => a.Provider == provider))
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
                var gameNotPresent = Games.FirstOrDefault(a => a.ProviderId == game.ProviderId && a.Provider == provider) == null;
                if (gameNotPresent)
                {
                    logger.Info("Adding new game {0} into library from {1} provider", game.ProviderId, game.Provider);
                    AddGame(game);
                }
            }

            // Delete games that are no longer in library
            foreach (IGame dbGame in dbCollection.FindAll().Where(a => a.Provider == provider))
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
