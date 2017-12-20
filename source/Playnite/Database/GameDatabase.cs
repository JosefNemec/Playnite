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
using System.Threading;
using System.Windows.Threading;
using Playnite.Providers.Uplay;
using Playnite.Providers.BattleNet;
using Playnite.Emulators;
using System.Security.Cryptography;

namespace Playnite.Database
{
    public class EventBufferHandler : IDisposable
    {
        private GameDatabase database;

        public EventBufferHandler(GameDatabase db)
        {
            database = db;
            db.BeginBufferUpdate();
        }

        public void Dispose()
        {
            database.EndBufferUpdate();
        }
    }

    public class PlatformUpdateEvent
    {
        public Platform OldData
        {
            get; set;
        }

        public Platform NewData
        {
            get; set;
        }

        public PlatformUpdateEvent(Platform oldData, Platform newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void PlatformUpdatedEventHandler(object sender, PlatformUpdatedEventArgs args);
    public class PlatformUpdatedEventArgs : EventArgs
    {
        public List<PlatformUpdateEvent> UpdatedPlatforms
        {
            get; set;
        }

        public PlatformUpdatedEventArgs(Platform oldData, Platform newData)
        {
            UpdatedPlatforms = new List<PlatformUpdateEvent>() { new PlatformUpdateEvent(oldData, newData) };
        }

        public PlatformUpdatedEventArgs(List<PlatformUpdateEvent> updatedPlatforms)
        {
            UpdatedPlatforms = updatedPlatforms;
        }
    }

    public delegate void PlatformsCollectionChangedEventHandler(object sender, PlatformsCollectionChangedEventArgs args);
    public class PlatformsCollectionChangedEventArgs : EventArgs
    {
        public List<Platform> AddedPlatforms
        {
            get; set;
        }

        public List<Platform> RemovedPlatforms
        {
            get; set;
        }

        public PlatformsCollectionChangedEventArgs(List<Platform> addedPlatforms, List<Platform> removedPlatforms)
        {
            AddedPlatforms = addedPlatforms;
            RemovedPlatforms = removedPlatforms;
        }
    }

    public class GameUpdateEvent
    {
        public IGame OldData
        {
            get; set;
        }

        public IGame NewData
        {
            get; set;
        }

        public GameUpdateEvent(IGame oldData, IGame newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void GameUpdatedEventHandler(object sender, GameUpdatedEventArgs args);
    public class GameUpdatedEventArgs : EventArgs
    {
        public List<GameUpdateEvent> UpdatedGames
        {
            get; set;
        }

        public GameUpdatedEventArgs(IGame oldData, IGame newData)
        {
            UpdatedGames = new List<GameUpdateEvent>() { new GameUpdateEvent(oldData, newData) };
        }

        public GameUpdatedEventArgs(List<GameUpdateEvent> updatedGames)
        {
            UpdatedGames = updatedGames;
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

        private bool IsEventBufferEnabled = false;

        private List<Platform> AddedPlatformsEventBuffer = new List<Platform>();
        private List<Platform> RemovedPlatformsEventBuffer = new List<Platform>();
        private List<PlatformUpdateEvent> PlatformUpdatesEventBuffer = new List<PlatformUpdateEvent>();
        private List<IGame> AddedGamesEventBuffer = new List<IGame>();
        private List<IGame> RemovedGamesEventBuffer = new List<IGame>();
        private List<GameUpdateEvent> GameUpdatesEventBuffer = new List<GameUpdateEvent>();

        public LiteDatabase Database
        {
            get; private set;
        }

        public LiteCollection<Platform> PlatformsCollection
        {
            get; private set;
        }

        public LiteCollection<Emulator> EmulatorsCollection
        {
            get; private set;
        }

        public LiteCollection<IGame> GamesCollection
        {
            get; private set;
        }

        public string Path
        {
            get; private set;
        }

        private IGogLibrary gogLibrary;
        private ISteamLibrary steamLibrary;
        private IOriginLibrary originLibrary;
        private IUplayLibrary uplayLibrary;
        private IBattleNetLibrary battleNetLibrary;

        public static readonly ushort DBVersion = 2;

        public event PlatformsCollectionChangedEventHandler PlatformsCollectionChanged;
        public event PlatformUpdatedEventHandler PlatformUpdated;
        public event GamesCollectionChangedEventHandler GamesCollectionChanged;
        public event GameUpdatedEventHandler GameUpdated;
        public event EventHandler DatabaseOpened;

        public Settings AppSettings
        {
            get; set;
        }

        public GameDatabase(Settings settings, string path) : this(settings)
        {
            Path = path;
        }

        public GameDatabase(Settings settings, string path, IGogLibrary gogLibrary, ISteamLibrary steamLibrary, IOriginLibrary originLibrary, IUplayLibrary uplayLibrary, IBattleNetLibrary battleNetLibrary)
            : this(settings, gogLibrary, steamLibrary, originLibrary, uplayLibrary, battleNetLibrary)
        {
            Path = path;
        }

        public GameDatabase(Settings settings)
        {
            AppSettings = settings;
            gogLibrary = new GogLibrary();
            steamLibrary = new SteamLibrary();
            originLibrary = new OriginLibrary();
            uplayLibrary = new UplayLibrary();
            battleNetLibrary = new BattleNetLibrary();
        }

        public GameDatabase(Settings settings, IGogLibrary gogLibrary, ISteamLibrary steamLibrary, IOriginLibrary originLibrary, IUplayLibrary uplayLibrary, IBattleNetLibrary battleNetLibrary)
        {
            AppSettings = settings;
            this.gogLibrary = gogLibrary;
            this.steamLibrary = steamLibrary;
            this.originLibrary = originLibrary;
            this.uplayLibrary = uplayLibrary;
            this.battleNetLibrary = battleNetLibrary;
        }

        private void CheckDbState()
        {
            if (GamesCollection == null)
            {
                throw new Exception("Database is not opened.");
            }
        }

        public static void MigrateDatabase(string path)
        {
            using (var db = new LiteDatabase(path))
            {
                if (db.Engine.UserVersion == DBVersion)
                {
                    return;
                }

                var trans = db.BeginTrans();

                try
                {
                    // 0 to 1
                    if (db.Engine.UserVersion == 0 && DBVersion > 0)
                    {
                        // Create: ObservableCollection<Link>Links
                        // Migrate: CommunityHubUrl, StoreUrl, WikiUrl to Links
                        // Remove: CommunityHubUrl, StoreUrl, WikiUrl
                        logger.Info("Migrating database from 0 to 1 version.");

                        var collection = db.GetCollection("games");
                        foreach (var game in collection.FindAll())
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

                        db.Engine.UserVersion = 1;
                    }

                    // 1 to 2
                    if (db.Engine.UserVersion == 1 && DBVersion > 1)
                    {
                        // Migrate: Emulators collection
                        // From:
                        // -Name: DOSBox
                        //  DefaultArguments: '-conf "{ImagePath}" -exit'
                        //  Platforms: [MS - DOS, PC]
                        //  ImageExtensions: [.conf]
                        //  ExecutableLookup: ^ dosbox\.exe
                        //  WorkingDirectory: 
                        // To:
                        // -Name: DOSBox
                        //  Configurations:
                        //    -Name: Default
                        //     DefaultArguments: '-conf "{ImagePath}" -exit'
                        //     Platforms: [MS - DOS, PC]
                        //     ImageExtensions: [.conf]
                        //     ExecutableLookup: ^ dosbox\.exe
                        //     WorkingDirectory:
                        //
                        // Add: EmulatorProfile into game's PlayTask when using emulator
                        // Add: checksum to file metadata
                        // Convert: Platforms and Emulators Id from int to ObjectId
                        logger.Info("Migrating database from 1 to 2 version.");

                        var platCollection = db.GetCollection("platforms");
                        var conPlatforms = new Dictionary<int, ObjectId>();
                        foreach (var platform in platCollection.FindAll().ToList())
                        {
                            var oldId = platform["_id"].AsInt32;
                            var newId = ObjectId.NewObjectId();
                            conPlatforms.Add(oldId, newId);
                            platCollection.Delete(oldId);
                            platform["_id"] = newId;
                            platCollection.Insert(platform);
                        }

                        var conEmulators = new Dictionary<int, ObjectId>();
                        var emuCollection = db.GetCollection("emulators");
                        foreach (var emulator in emuCollection.FindAll().ToList())
                        {
                            var platforms = emulator["Platforms"]?.AsArray?
                                .Where(a => conPlatforms.ContainsKey(a.AsInt32))?
                                .Select(a => conPlatforms[a]).ToList();

                            var profiles = new List<EmulatorProfile>
                            {
                                new EmulatorProfile()
                                {
                                    Name = "Default",
                                    Arguments = emulator["Arguments"],
                                    Executable = emulator["Executable"],
                                    ImageExtensions = emulator["ImageExtensions"]?.AsArray?.Select(a => a.AsString.TrimStart('.'))?.ToList(),
                                    Platforms = platforms,
                                    WorkingDirectory = emulator["WorkingDirectory"]
                                }
                            };

                            emulator.Remove("Arguments");
                            emulator.Remove("Executable");
                            emulator.Remove("ImageExtensions");
                            emulator.Remove("Platforms");
                            emulator.Remove("WorkingDirectory");
                            emulator.Add("Profiles", new BsonArray(profiles.Select(a => BsonMapper.Global.ToDocument(a))));
                            var oldId = emulator["_id"].AsInt32;
                            var newId = ObjectId.NewObjectId();
                            conEmulators.Add(oldId, newId);
                            emuCollection.Delete(oldId);
                            emulator["_id"] = newId;
                            emuCollection.Insert(emulator);
                        }

                        var gameCol = db.GetCollection("games");
                        var emusCollection = db.GetCollection<Emulator>("emulators");
                        foreach (var game in gameCol.FindAll().ToList())
                        {
                            int? oldPlatId = game["PlatformId"]?.AsInt32;
                            if (oldPlatId != null)
                            {
                                if (conPlatforms.ContainsKey(oldPlatId.Value))
                                {
                                    game["PlatformId"] = conPlatforms[oldPlatId.Value];
                                }
                                else
                                {
                                    game.Remove("PlatformId");
                                }
                            }

                            if (game["PlayTask"].AsDocument != null)
                            {
                                var task = game["PlayTask"].AsDocument;
                                if (task.AsDocument["Type"].AsString == "Emulator")
                                {
                                    var oldEmuId = task.AsDocument["EmulatorId"].AsInt32;
                                    if (conEmulators.ContainsKey(oldEmuId))
                                    {
                                        var emulator = emusCollection.FindById(conEmulators[oldEmuId]);
                                        task.AsDocument["EmulatorId"] = emulator.Id;
                                        task.AsDocument["EmulatorProfileId"] = emulator.Profiles?.First().Id;
                                    }
                                    else
                                    {
                                        task.AsDocument.Remove("EmulatorId");
                                        task.AsDocument.Remove("EmulatorProfileId");
                                    }
                                }
                                else
                                {
                                    task.AsDocument.Remove("EmulatorId");
                                    task.AsDocument.Remove("EmulatorProfileId");
                                }
                            }

                            if (game["OtherTasks"].AsArray != null)
                            {
                                foreach (var task in game["OtherTasks"].AsArray)
                                {
                                    if (task.AsDocument["Type"].AsString == "Emulator")
                                    {
                                        var oldEmuId = task.AsDocument["EmulatorId"].AsInt32;
                                        if (conEmulators.ContainsKey(oldEmuId))
                                        {
                                            var emulator = emusCollection.FindById(conEmulators[oldEmuId]);
                                            task.AsDocument["EmulatorId"] = emulator.Id;
                                            task.AsDocument["EmulatorProfileId"] = emulator.Profiles?.First().Id;
                                        }
                                        else
                                        {
                                            task.AsDocument.Remove("EmulatorId");
                                            task.AsDocument.Remove("EmulatorProfileId");
                                        }
                                    }
                                    else
                                    {
                                        task.AsDocument.Remove("EmulatorId");
                                        task.AsDocument.Remove("EmulatorProfileId");
                                    }
                                }
                            }

                            gameCol.Update(game);
                        }

                        emusCollection.EnsureIndex(a => a.Id);
                        db.GetCollection<IGame>("games").EnsureIndex(a => a.Id);
                        db.GetCollection<Platform>("platforms").EnsureIndex(a => a.Id);

                        db.Engine.UserVersion = 2;
                    }

                    trans.Commit();
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to migrate database, reverting back.");
                    trans.Rollback();
                    throw;
                }

                // we must do this outside of transaction operation
                if (db.Engine.UserVersion == 2)
                {
                    foreach (var file in db.FileStorage.FindAll().ToList())
                    {
                        using (var fStream = file.OpenRead())
                        {
                            var hash = FileSystem.GetMD5(fStream);
                            file.Metadata.Set("checksum", hash);
                            db.FileStorage.SetMetadata(file.Id, file.Metadata);
                        }
                    }
                }
            }
        }

        public static bool GetMigrationRequired(string path)
        {
            using (var db = new LiteDatabase(path))
            {
                return GetMigrationRequired(db);
            }
        }

        public static bool GetMigrationRequired(LiteDatabase db)
        {
            return db.Engine.UserVersion < DBVersion;
        }

        public LiteDatabase OpenDatabase(string path)
        {
            Path = path;
            return OpenDatabase();
        }

        public LiteDatabase OpenDatabase()
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new Exception("Database path cannot be empty.");
            }

            var dbExists = File.Exists(Path);
            logger.Info("Opening db " + Path);
            CloseDatabase();
            Database = new LiteDatabase($"Filename={Path};Mode=Exclusive");

            // To force litedb to try to open file, should throw exceptuion if something is wrong with db file
            Database.GetCollectionNames();

            if (!dbExists)
            {
                Database.Engine.UserVersion = DBVersion;
            }
            else
            {
                if (Database.Engine.UserVersion > DBVersion)
                {
                    throw new Exception($"Database version {Database.Engine.UserVersion} is not supported.");
                }

                if (GetMigrationRequired(Database))
                {
                    throw new Exception("Database must be migrated before opening.");
                }
            }

            GamesCollection = Database.GetCollection<IGame>("games");
            PlatformsCollection = Database.GetCollection<Platform>("platforms");
            EmulatorsCollection = Database.GetCollection<Emulator>("emulators");

            if (!dbExists)
            {
                GamesCollection.EnsureIndex(a => a.Id);
                PlatformsCollection.EnsureIndex(a => a.Id);
                EmulatorsCollection.EnsureIndex(a => a.Id);

                // Generate default platforms
                if (File.Exists(EmulatorDefinition.DefinitionsPath))
                {
                    var platforms = EmulatorDefinition.GetDefinitions()
                        .SelectMany(a => a.Profiles.SelectMany(b => b.Platforms)).Distinct()
                        .Select(a => new Platform(a)).ToList();
                    AddPlatform(platforms);
                }
            }

            DatabaseOpened?.Invoke(this, null);
            return Database;
        }

        public void CloseDatabase()
        {
            if (Database == null)
            {
                return;
            }

            try
            {                
                Database.Dispose();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to dispose LiteDB database object.");                
            }

            GamesCollection = null;
            PlatformsCollection = null;
            EmulatorsCollection = null;
        }

        public void AddGame(IGame game)
        {
            CheckDbState();

            lock (fileLock)
            {
                GamesCollection.Insert(game);
            }

            OnGamesCollectionChanged(new List<IGame>() { game }, new List<IGame>());
        }

        public void AddGames(List<IGame> games)
        {
            CheckDbState();
            if (games == null || games.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                GamesCollection.InsertBulk(games);
            }

            OnGamesCollectionChanged(games.ToList(), new List<IGame>());
        }
                
        public void DeleteGame(IGame game)
        {
            logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
            CheckDbState();

            lock (fileLock)
            {
                GamesCollection.Delete(game.Id);
                DeleteImageSafe(game.Icon, game);
                DeleteImageSafe(game.Image, game);
            }

            OnGamesCollectionChanged(new List<IGame>(), new List<IGame>() { game });
        }

        public void DeleteGames(List<IGame> games)
        {
            CheckDbState();

            lock (fileLock)
            {
                foreach (var game in games)
                {
                    logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
                    GamesCollection.Delete(game.Id);
                    DeleteImageSafe(game.Icon, game);
                    DeleteImageSafe(game.Image, game);
                }
            }

            OnGamesCollectionChanged(new List<IGame>(), games);
        }

        public void AddPlatform(Platform platform)
        {
            CheckDbState();

            lock (fileLock)
            {
                PlatformsCollection.Insert(platform);
            }

            OnPlatformsCollectionChanged(new List<Platform>() { platform }, new List<Platform>());
        }

        public void AddPlatform(List<Platform> platforms)
        {
            CheckDbState();
            if (platforms == null || platforms.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                PlatformsCollection.InsertBulk(platforms);
            }

            OnPlatformsCollectionChanged(platforms.ToList(), new List<Platform>());
        }

        public void RemovePlatform(Platform platform)
        {
            CheckDbState();

            lock (fileLock)
            {
                PlatformsCollection.Delete(platform.Id);
            }

            OnPlatformsCollectionChanged(new List<Platform>(), new List<Platform>() { platform });
                
            lock (fileLock)
            {
                foreach (var game in GamesCollection.Find(a => a.PlatformId == platform.Id))
                {
                    game.PlatformId = null;
                    UpdateGameInDatabase(game);
                }
            }
        }

        public void RemovePlatform(IEnumerable<Platform> platforms)
        {
            CheckDbState();
            if (platforms == null || platforms.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                foreach (var platform in platforms)
                {
                    PlatformsCollection.Delete(platform.Id);
                }
            }

            OnPlatformsCollectionChanged(new List<Platform>(), platforms.ToList());

            lock (fileLock)
            {
                foreach (var platform in platforms)
                {
                    foreach (var game in GamesCollection.Find(a => a.PlatformId == platform.Id))
                    {
                        game.PlatformId = null;
                        UpdateGameInDatabase(game);
                    }
                }
            }
        }

        public void UpdatePlatform(Platform platform)
        {
            CheckDbState();
            Platform oldData;

            lock (fileLock)
            {
                oldData = PlatformsCollection.FindById(platform.Id);
                PlatformsCollection.Update(platform);
            }

            OnPlatformUpdated(new List<PlatformUpdateEvent>() { new PlatformUpdateEvent(oldData, platform) });
        }

        public void UpdatePlatform(List<Platform> platforms)
        {
            CheckDbState();            
            var updates = new List<PlatformUpdateEvent>();

            lock (fileLock)
            {
                foreach (var platform in platforms)
                {
                    var oldData = PlatformsCollection.FindById(platform.Id);
                    PlatformsCollection.Update(platform);

                    updates.Add(new PlatformUpdateEvent(oldData, platform));
                }
            }

            OnPlatformUpdated(updates);
        }

        public void AddEmulator(Emulator emulator)
        {
            CheckDbState();

            lock (fileLock)
            {
                EmulatorsCollection.Insert(emulator);
            }
        }

        public void AddEmulator(IEnumerable<Emulator> emulators)
        {
            CheckDbState();
            if (emulators == null || emulators.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                EmulatorsCollection.InsertBulk(emulators);
            }
        }

        public void RemoveEmulator(Emulator emulator)
        {
            CheckDbState();

            lock (fileLock)
            {
                EmulatorsCollection.Delete(emulator.Id);
            }
        }

        public void RemoveEmulator(IEnumerable<Emulator> emulators)
        {
            CheckDbState();
            if (emulators == null || emulators.Count() == 0)
            {
                return;
            }

            lock (fileLock)
            {
                foreach (var emulator in emulators)
                {
                    EmulatorsCollection.Delete(emulator.Id);
                }
            }
        }

        public void UpdateEmulator(Emulator emulator)
        {
            CheckDbState();

            lock (fileLock)
            {
                EmulatorsCollection.Update(emulator);
            }
        }

        public void UpdateEmulator(List<Emulator> emulators)
        {
            CheckDbState();

            lock (fileLock)
            {
                foreach (var emulator in emulators)
                {
                    EmulatorsCollection.Update(emulator);
                }
            }
        }

        public string AddFileNoDuplicate(FileDefinition file)
        {
            return AddFileNoDuplicate(file.Path, file.Name, file.Data);
        }

        public string AddFileNoDuplicate(string id, string name, byte[] data)
        {
            CheckDbState();

            lock (fileLock)
            {
                using (var stream = new MemoryStream(data))
                {
                    var hash = FileSystem.GetMD5(stream);
                    var dbFile = Database.FileStorage.FindAll().FirstOrDefault(a => a.Metadata.ContainsKey("checksum") && a.Metadata["checksum"].AsString == hash);
                    if (dbFile != null)
                    {
                        return dbFile.Id;
                    }
                    
                    stream.Seek(0, SeekOrigin.Begin);
                    var file = Database.FileStorage.Upload(id, name, stream);
                    file.Metadata.Add("checksum", hash);
                    Database.FileStorage.SetMetadata(id, file.Metadata);
                    return file.Id;
                }
            }
        }

        public void AddFile(string id, string name, byte[] data)
        {
            CheckDbState();

            lock (fileLock)
            {
                using (var stream = new MemoryStream(data))
                {
                    var file = Database.FileStorage.Upload(id, name, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var hash = FileSystem.GetMD5(stream);
                    file.Metadata.Add("checksum", hash);                    
                    Database.FileStorage.SetMetadata(id, file.Metadata);
                }
            }
        }

        public void DeleteFile(string id)
        {
            CheckDbState();

            lock (fileLock)
            {
                if (Database.FileStorage.Delete(id) == false)
                {
                    logger.Warn($"Failed to delte file {id} for uknown reason.");
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
            CheckDbState();

            lock (fileLock)
            {
                return Database.FileStorage.FindById(id);
            }
        }

        public BitmapImage GetFileImage(string id)
        {
            CheckDbState();

            var file = GetFile(id);
            if (file == null)
            {
                return null;
            }

            using (var fStream = GetFileStream(id))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = fStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        public void SaveFile(string id, string path)
        {
            CheckDbState();

            var file = Database.FileStorage.FindById(id);
            if (file == null)
            {
                throw new Exception($"File {id} not found in database.");
            }

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

            lock (fileLock)
            {
                var games = GamesCollection.Find(a => (a.Icon == id || a.Image == id || a.BackgroundImage == id) && a.Id != game.Id);
                if (games.Count() == 0)
                {
                    Database.FileStorage.Delete(id);
                }
            }
        }

        public void UpdateGamesInDatabase(List<IGame> games)
        {
            CheckDbState();
            var updates = new List<GameUpdateEvent>();

            lock (fileLock)
            {
                foreach (var game in games)
                {
                    var oldData = GamesCollection.FindById(game.Id);
                    GamesCollection.Update(game);
                    updates.Add(new GameUpdateEvent(oldData, game));
                }
            }

            OnGameUpdated(updates);
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

            OnGameUpdated(new List<GameUpdateEvent>() { new GameUpdateEvent(oldData, game) });
        }

        public List<IGame> UpdateInstalledGames(Provider provider)
        {
            List<IGame> installedGames = null;
            List<IGame> newGames = new List<IGame>();

            switch (provider)
            {
                case Provider.Custom:
                    return newGames;
                case Provider.GOG:
                    var source = InstalledGamesSource.Registry;
                    if (AppSettings.GOGSettings.RunViaGalaxy)
                    {
                        source = InstalledGamesSource.Galaxy;
                    }

                    installedGames = gogLibrary.GetInstalledGames(source);
                    break;
                case Provider.Origin:
                    installedGames = originLibrary.GetInstalledGames(true);
                    break;
                case Provider.Steam:
                    installedGames = steamLibrary.GetInstalledGames();
                    break;
                case Provider.Uplay:
                    installedGames = uplayLibrary.GetInstalledGames();
                    break;
                case Provider.BattleNet:
                    installedGames = battleNetLibrary.GetInstalledGames();
                    break;
                default:
                    return newGames;
            }

            foreach (var newGame in installedGames)
            {
                var existingGame = GamesCollection.FindOne(a => a.ProviderId == newGame.ProviderId && a.Provider == provider);
                if (existingGame == null)
                {
                    logger.Info("Adding new installed game {0} from {1} provider", newGame.ProviderId, newGame.Provider);
                    AssignPcPlatform(newGame);                    
                    AddGame(newGame);
                    newGames.Add(newGame);
                }
                else
                {
                    existingGame.PlayTask = newGame.PlayTask;
                    existingGame.InstallDirectory = newGame.InstallDirectory;

                    // Don't import custom action if imported already (user may changed them manually and this would overwrite it)
                    if (existingGame.OtherTasks?.FirstOrDefault(a => a.IsBuiltIn) == null && newGame.OtherTasks != null)
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
                    UpdateGameInDatabase(game);
                }
            }

            return newGames;
        }

        public List<IGame> UpdateOwnedGames(Provider provider)
        {
            List<IGame> importedGames = null;
            List<IGame> newGames = new List<IGame>();

            switch (provider)
            {
                case Provider.Custom:
                    return newGames;
                case Provider.GOG:
                    importedGames = gogLibrary.GetLibraryGames();
                    break;
                case Provider.Origin:
                    importedGames = originLibrary.GetLibraryGames();
                    break;
                case Provider.Steam:                    
                    importedGames = steamLibrary.GetLibraryGames(AppSettings.SteamSettings);
                    break;
                case Provider.Uplay:
                    return newGames;
                case Provider.BattleNet:
                    importedGames = battleNetLibrary.GetLibraryGames();
                    break;
                default:
                    return newGames;
            }

            foreach (var game in importedGames)
            {
                var existingGame = GamesCollection.FindOne(a => a.ProviderId == game.ProviderId && a.Provider == provider);
                if (existingGame == null)
                {
                    logger.Info("Adding new game {0} into library from {1} provider", game.ProviderId, game.Provider);
                    AssignPcPlatform(game);
                    AddGame(game);
                    newGames.Add(game);
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

            return newGames;
        }

        public void AssignPcPlatform(IGame game)
        {
            var platform = PlatformsCollection.FindOne(a => a.Name == "PC");
            if (platform == null)
            {
                platform = new Platform("PC");
                AddPlatform(platform);
            }

            game.PlatformId = platform.Id;
        }

        public void ImportCategories(List<IGame> sourceGames)
        {
            foreach (var game in sourceGames)
            {
                var dbGame = GamesCollection.FindOne(a => a.Provider == game.Provider && a.ProviderId == game.ProviderId);
                if (dbGame == null)
                {
                    continue;
                }

                dbGame.Categories = game.Categories;
                UpdateGameInDatabase(dbGame);
            }
        }

        public void BeginBufferUpdate()
        {
            IsEventBufferEnabled = true;
        }

        public void EndBufferUpdate()
        {
            IsEventBufferEnabled = false;

            if (AddedPlatformsEventBuffer.Count > 0 || RemovedPlatformsEventBuffer.Count > 0)
            {
                OnPlatformsCollectionChanged(AddedPlatformsEventBuffer.ToList(), RemovedPlatformsEventBuffer.ToList());
                AddedPlatformsEventBuffer.Clear();
                RemovedPlatformsEventBuffer.Clear();
            }

            if (PlatformUpdatesEventBuffer.Count > 0)
            {
                OnPlatformUpdated(PlatformUpdatesEventBuffer.ToList());
                PlatformUpdatesEventBuffer.Clear();
            }

            if (AddedGamesEventBuffer.Count > 0 || RemovedGamesEventBuffer.Count > 0)
            {
                OnGamesCollectionChanged(AddedGamesEventBuffer.ToList(), RemovedGamesEventBuffer.ToList());
                AddedGamesEventBuffer.Clear();
                RemovedGamesEventBuffer.Clear();
            }

            if (GameUpdatesEventBuffer.Count > 0)
            {
                OnGameUpdated(GameUpdatesEventBuffer.ToList());
                GameUpdatesEventBuffer.Clear();
            }
        }

        public IDisposable BufferedUpdate()
        {
            return new EventBufferHandler(this);
        }

        private void OnPlatformsCollectionChanged(List<Platform> addedPlatforms, List<Platform> removedPlatforms)
        {
            if (!IsEventBufferEnabled)
            {
                PlatformsCollectionChanged?.Invoke(this, new PlatformsCollectionChangedEventArgs(addedPlatforms, removedPlatforms));
            }
            else
            {
                AddedPlatformsEventBuffer.AddRange(addedPlatforms);
                RemovedPlatformsEventBuffer.AddRange(removedPlatforms);
            }
        }

        private void OnPlatformUpdated(List<PlatformUpdateEvent> updates)
        {
            if (!IsEventBufferEnabled)
            {
                PlatformUpdated?.Invoke(this, new PlatformUpdatedEventArgs(updates));
            }
            else
            {
                PlatformUpdatesEventBuffer.AddRange(updates);
            }
        }

        private void OnGamesCollectionChanged(List<IGame> addedGames, List<IGame> removedGames)
        {
            if (!IsEventBufferEnabled)
            {
                GamesCollectionChanged?.Invoke(this, new GamesCollectionChangedEventArgs(addedGames, removedGames));
            }
            else
            {
                AddedGamesEventBuffer.AddRange(addedGames);
                RemovedGamesEventBuffer.AddRange(removedGames);
            }
        }

        private void OnGameUpdated(List<GameUpdateEvent> updates)
        {
            if (!IsEventBufferEnabled)
            {
                GameUpdated?.Invoke(this, new GameUpdatedEventArgs(updates));
            }
            else
            {
                GameUpdatesEventBuffer.AddRange(updates);
            }
        }
    }
}
