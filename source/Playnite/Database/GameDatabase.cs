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
using Playnite.SDK.Models;

namespace Playnite.Database
{
    public class DatabaseSettings
    {
        [BsonId]
        public int Id
        {
            get; set;
        } = 1;

        /// <summary>
        /// Indicated if game states for custom games has been fixed (during update from 3.x to 4.x)
        /// </summary>
        public bool InstStatesFixed
        {
            get; set;
        }

        /// <summary>
        /// Indicates if games Source field has been set to default values (for example for Steam game to "Steam").
        /// For update from 3.x to 4.x versions.
        /// </summary>
        public bool GameSourcesUpdated
        {
            get; set;
        }

        public DatabaseSettings()
        {
        }
    }

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
        public Game OldData
        {
            get; set;
        }

        public Game NewData
        {
            get; set;
        }

        public GameUpdateEvent(Game oldData, Game newData)
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

        public GameUpdatedEventArgs(Game oldData, Game newData)
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
        public List<Game> AddedGames
        {
            get; set;
        }

        public List<Game> RemovedGames
        {
            get; set;
        }

        public GamesCollectionChangedEventArgs(List<Game> addedGames, List<Game> removedGames)
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
        private bool IsEventBufferEnabled = false;
        private List<Platform> AddedPlatformsEventBuffer = new List<Platform>();
        private List<Platform> RemovedPlatformsEventBuffer = new List<Platform>();
        private List<PlatformUpdateEvent> PlatformUpdatesEventBuffer = new List<PlatformUpdateEvent>();
        private List<Game> AddedGamesEventBuffer = new List<Game>();
        private List<Game> RemovedGamesEventBuffer = new List<Game>();
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

        public LiteCollection<Game> GamesCollection
        {
            get; private set;
        }

        public LiteCollection<ActiveController> ActiveControllersCollection
        {
            get; private set;
        }

        public string Path
        {
            get; private set;
        }

        public bool IsOpen
        {
            get; private set;
        }

        private IGogLibrary gogLibrary;
        private ISteamLibrary steamLibrary;
        private IOriginLibrary originLibrary;
        private IUplayLibrary uplayLibrary;
        private IBattleNetLibrary battleNetLibrary;

        public static readonly ushort DBVersion = 3;

        public event PlatformsCollectionChangedEventHandler PlatformsCollectionChanged;
        public event PlatformUpdatedEventHandler PlatformUpdated;
        public event GamesCollectionChangedEventHandler GamesCollectionChanged;
        public event GameUpdatedEventHandler GameUpdated;
        public event EventHandler DatabaseOpened;

        public Settings AppSettings
        {
            get; set;
        }

        public GameDatabase() : this(null)
        {
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

        public DatabaseSettings GetDatabaseSettings()
        {
            CheckDbState();
            var coll = Database.GetCollection<DatabaseSettings>("settings");
            return coll.FindById(1);
        }

        public void UpdateDatabaseSettings(DatabaseSettings settings)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                var coll = Database.GetCollection<DatabaseSettings>("settings");
                coll.Upsert(settings);
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

                var originalVersion = db.Engine.UserVersion;
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

                        emusCollection.EnsureIndex("Id");
                        db.GetCollection("games").EnsureIndex("Id");
                        db.GetCollection("platforms").EnsureIndex("Id");

                        db.Engine.UserVersion = 2;
                    }

                    // 2 to 3
                    if (db.Engine.UserVersion == 2 && DBVersion > 2)
                    {
                        // Remove "_type" field of "games" collection
                        var gameCol = db.GetCollection("games");
                        foreach (var game in gameCol.FindAll().ToList())
                        {
                            game.Remove("_type");
                            gameCol.Update(game);
                        }

                        db.Engine.UserVersion = 3;
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
                if (originalVersion <= 2)
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
            if (!File.Exists(path))
            {
                return false;
            }

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

            if (!dbExists)
            {
                FileSystem.CreateDirectory(Path);
            }

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

            GamesCollection = Database.GetCollection<Game>("games");
            PlatformsCollection = Database.GetCollection<Platform>("platforms");
            EmulatorsCollection = Database.GetCollection<Emulator>("emulators");
            ActiveControllersCollection = Database.GetCollection<ActiveController>("controllers");

            // New DB setup
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

            // Reset game states in case they were not released properly
            if (ActiveControllersCollection.Count() > 0)
            {
                foreach (var controller in ActiveControllersCollection.FindAll())
                {
                    var game = GamesCollection.FindById(controller.Game.Id);
                    if (game != null)
                    {
                        game.State.SetState(null, false, false, false, false);
                        UpdateGameInDatabase(game);
                    }

                    RemoveActiveController(controller.Game.Id);
                }
            }

            var settings = GetDatabaseSettings();

            // Fix for custom games.
            // Needed when updating from 3.x to 4.x because installation states are handled differently.
            if (settings?.InstStatesFixed != true)
            {
                foreach (var game in GamesCollection.Find(a => a.Provider == Provider.Custom).ToList())
                {
                    if (!string.IsNullOrEmpty(game.InstallDirectory) || !string.IsNullOrEmpty(game.IsoPath))
                    {
                        game.State.Installed = true;
                    }
                    else
                    {
                        // For UWP games which don't have installed dir
                        if (game.PlayTask?.Path == "explorer.exe")
                        {
                            game.State.Installed = true;
                        }
                    }

                    UpdateGameInDatabase(game);
                }

                if (settings != null)
                {
                    settings.InstStatesFixed = true;
                }
            }

            // Update game source.
            // Needed when updating from 3.x to 4.x to retrospectively apply to non-custom games.
            if (settings?.GameSourcesUpdated != true)
            {
                foreach (var game in GamesCollection.Find(a => a.Provider != Provider.Custom).ToList())
                {
                    if (string.IsNullOrEmpty(game.Source))
                    {
                        game.Source = Enums.GetEnumDescription(game.Provider);
                        UpdateGameInDatabase(game);
                    }
                }

                if (settings != null)
                {
                    settings.GameSourcesUpdated = true;
                }
            }

            if (settings == null)
            {
                settings = new DatabaseSettings()
                {
                    InstStatesFixed = true,
                    GameSourcesUpdated = true
                };
            }
            
            UpdateDatabaseSettings(settings);
            DatabaseOpened?.Invoke(this, null);
            IsOpen = true;
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
            ActiveControllersCollection = null;
            IsOpen = false;
        }

        public List<Game> GetGames()
        {
            CheckDbState();
            return GamesCollection.FindAll().ToList();
        }

        public Game GetGame(int id)
        {
            CheckDbState();
            return GamesCollection.FindById(id);
        }

        public void AddGame(Game game)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                game.Added = DateTime.Today;
                GamesCollection.Insert(game);
            }

            OnGamesCollectionChanged(new List<Game>() { game }, new List<Game>());
        }

        public void AddGames(List<Game> games)
        {
            CheckDbState();
            if (games == null || games.Count() == 0)
            {
                return;
            }

            foreach (var game in games)
            {
                game.Added = DateTime.Today;
            }

            using (Database.Engine.Locker.Reserved())
            {
                GamesCollection.InsertBulk(games);
            }

            OnGamesCollectionChanged(games.ToList(), new List<Game>());
        }

        public void DeleteGame(int id)
        {
            var game = GetGame(id);
            DeleteGame(game);
        }

        public void DeleteGame(Game game)
        {
            logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                GamesCollection.Delete(game.Id);
                DeleteImageSafe(game.Icon, game);
                DeleteImageSafe(game.Image, game);
            }

            OnGamesCollectionChanged(new List<Game>(), new List<Game>() { game });
        }

        public void DeleteGames(List<Game> games)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                foreach (var game in games)
                {
                    logger.Info("Deleting game from database {0}, {1}", game.ProviderId, game.Provider);
                    GamesCollection.Delete(game.Id);
                    DeleteImageSafe(game.Icon, game);
                    DeleteImageSafe(game.Image, game);
                }
            }

            OnGamesCollectionChanged(new List<Game>(), games);
        }

        public void AddPlatform(Platform platform)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
            {
                PlatformsCollection.InsertBulk(platforms);
            }

            OnPlatformsCollectionChanged(platforms.ToList(), new List<Platform>());
        }

        public Platform GetPlatform(ObjectId id)
        {
            CheckDbState();
            return PlatformsCollection.FindById(id);
        }

        public List<Platform> GetPlatforms()
        {
            CheckDbState();
            return PlatformsCollection.FindAll().ToList();
        }

        public void RemovePlatform(ObjectId id)
        {
            var platform = GetPlatform(id);
            RemovePlatform(platform);
        }

        public void RemovePlatform(Platform platform)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                PlatformsCollection.Delete(platform.Id);
            }

            OnPlatformsCollectionChanged(new List<Platform>(), new List<Platform>() { platform });

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
            {
                foreach (var platform in platforms)
                {
                    PlatformsCollection.Delete(platform.Id);
                }
            }

            OnPlatformsCollectionChanged(new List<Platform>(), platforms.ToList());

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
            {
                EmulatorsCollection.InsertBulk(emulators);
            }
        }

        public void RemoveEmulator(ObjectId id)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                EmulatorsCollection.Delete(id);
            }
        }

        public void RemoveEmulator(Emulator emulator)
        {
            RemoveEmulator(emulator.Id);
        }

        public void RemoveEmulator(IEnumerable<Emulator> emulators)
        {
            CheckDbState();
            if (emulators == null || emulators.Count() == 0)
            {
                return;
            }

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
            {
                EmulatorsCollection.Update(emulator);
            }
        }

        public void UpdateEmulator(List<Emulator> emulators)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                foreach (var emulator in emulators)
                {
                    EmulatorsCollection.Update(emulator);
                }
            }
        }

        public Emulator GetEmulator(ObjectId id)
        {
            CheckDbState();
            return EmulatorsCollection.FindById(id);
        }

        public List<Emulator> GetEmulators()
        {
            CheckDbState();
            return EmulatorsCollection.FindAll().ToList();
        }

        public string AddFileNoDuplicate(FileDefinition file)
        {
            return AddFileNoDuplicate(file.Path, file.Name, file.Data);
        }

        public string AddFileNoDuplicate(string id, string name, byte[] data)
        {
            CheckDbState();

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
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

            using (Database.Engine.Locker.Reserved())
            {
                file.SaveAs(path, true);
            }
        }

        /// <summary>
        /// Deletes image from database only if it's not used by any object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="game"></param>
        public void DeleteImageSafe(string id, Game game)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            CheckDbState();

            using (Database.Engine.Locker.Reserved())
            {
                var games = GamesCollection.Find(a => (a.Icon == id || a.Image == id || a.BackgroundImage == id) && a.Id != game.Id);
                if (games.Count() == 0)
                {
                    Database.FileStorage.Delete(id);
                }
            }
        }

        public void UpdateGamesInDatabase(List<Game> games)
        {
            CheckDbState();
            var updates = new List<GameUpdateEvent>();

            using (Database.Engine.Locker.Reserved())
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

        public void UpdateGameInDatabase(Game game)
        {
            CheckDbState();
            Game oldData;

            using (Database.Engine.Locker.Reserved())
            {
                oldData = GamesCollection.FindById(game.Id);
                GamesCollection.Update(game);
            }

            OnGameUpdated(new List<GameUpdateEvent>() { new GameUpdateEvent(oldData, game) });
        }

        public List<Game> UpdateInstalledGames(Provider provider)
        {
            List<Game> installedGames = null;
            List<Game> newGames = new List<Game>();

            switch (provider)
            {
                case Provider.Custom:
                    return newGames;
                case Provider.GOG:
                    installedGames = gogLibrary.GetInstalledGames();
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
                    newGame.State.Installed = true;
                    AssignPcPlatform(newGame);
                    AddGame(newGame);
                    newGames.Add(newGame);
                }
                else
                {
                    existingGame.State.Installed = true;
                    existingGame.InstallDirectory = newGame.InstallDirectory;
                    if (existingGame.PlayTask == null)
                    {
                        existingGame.PlayTask = newGame.PlayTask;
                    }

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
                    game.InstallDirectory = string.Empty;
                    game.State.Installed = false;
                    UpdateGameInDatabase(game);
                }
            }

            return newGames;
        }

        public List<Game> UpdateOwnedGames(Provider provider)
        {
            List<Game> importedGames = null;
            List<Game> newGames = new List<Game>();
            List<Game> updatedGames = new List<Game>();

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
                    newGames.Add(game);
                }
                else
                {
                    if (existingGame.Playtime == 0 && game.Playtime > 0)
                    {
                        existingGame.Playtime = game.Playtime;
                        if (existingGame.CompletionStatus == CompletionStatus.NotPlayed)
                        {
                            existingGame.CompletionStatus = CompletionStatus.Played;
                        }

                        if (existingGame.LastActivity == null && game.LastActivity != null)
                        {
                            existingGame.LastActivity = game.LastActivity;
                        }

                        updatedGames.Add(existingGame);
                    }
                }
            }

            AddGames(newGames);
            UpdateGamesInDatabase(updatedGames);
            return newGames;
        }

        public void AssignPcPlatform(Game game)
        {
            var platform = PlatformsCollection.FindOne(a => a.Name == "PC");
            if (platform == null)
            {
                platform = new Platform("PC");
                AddPlatform(platform);
            }

            game.PlatformId = platform.Id;
        }

        public void AssignPcPlatform(List<Game> games)
        {
            var platform = PlatformsCollection.FindOne(a => a.Name == "PC");
            if (platform == null)
            {
                platform = new Platform("PC");
                AddPlatform(platform);
            }

            foreach (var game in games)
            {
                game.PlatformId = platform.Id;
            }

            UpdateGamesInDatabase(games);
        }

        public void ImportCategories(List<Game> sourceGames)
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

        private void OnGamesCollectionChanged(List<Game> addedGames, List<Game> removedGames)
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

        public ActiveController AddActiveController(IGameController controller)
        {
            CheckDbState();
            var ctrl = new ActiveController(controller);
            ActiveControllersCollection.Upsert(ctrl);
            return ctrl;
        }

        public ActiveController GetActiveController(int gameId)
        {
            CheckDbState();
            return ActiveControllersCollection.FindOne(a => a.Game.Id == gameId);
        }

        public void RemoveActiveController(int gameId)
        {
            CheckDbState();
            ActiveControllersCollection.Delete(a => a.Game.Id == gameId);
        }
    }
}
