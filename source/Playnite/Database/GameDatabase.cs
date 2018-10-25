using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.Windows.Media.Imaging;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Metadata;
using Playnite.Common;

namespace Playnite.Database
{
    public class GameDatabase
    {
        private static ILogger logger = LogManager.GetLogger();

        #region Locks

        private readonly object databaseConfigFileLock = new object();
        private readonly object fileFilesLock = new object();

        #endregion Locks

        #region Paths

        public string DatabasePath
        {
            get; private set;
        }

        private string GamesDirectoryPath { get => Path.Combine(DatabasePath, "games"); }
        private string PlatformsDirectoryPath { get => Path.Combine(DatabasePath, "platforms"); }
        private string EmulatorsDirectoryPath { get => Path.Combine(DatabasePath, "emulators"); }
        private string FilesDirectoryPath { get => Path.Combine(DatabasePath, "files"); }

        private string DatabaseFileSettingsPath { get => Path.Combine(DatabasePath, "database.json"); }

        #endregion Paths

        #region Lists

        public GamesCollection Games { get; private set; }
        public PlatformsCollection Platforms { get; private set; }
        public ItemCollection<Emulator> Emulators { get; private set; }       

        #endregion Lists

        public bool IsOpen
        {
            get; private set;
        }

        private DatabaseSettings settings;
        public DatabaseSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    if (File.Exists(DatabaseFileSettingsPath))
                    {
                        settings = Serialization.FromJson<DatabaseSettings>(FileSystem.FileReadAsString(DatabaseFileSettingsPath));
                    }
                    else
                    {
                        settings = new DatabaseSettings() { Version = DBVersion };
                    }
                }

                return settings;
            }

            set
            {
                lock (databaseConfigFileLock)
                {
                    settings = value;
                    FileSystem.FileWriteString(DatabaseFileSettingsPath, Serialization.ToJson(settings));
                }
            }
        }

        public static readonly ushort DBVersion = 5;

        #region Events

        public event EventHandler DatabaseOpened;

        #endregion Events

        #region Initialization

        private void LoadCollections()
        {
            Platforms.InitializeCollection(PlatformsDirectoryPath);
            Emulators.InitializeCollection(EmulatorsDirectoryPath);
            Games.InitializeCollection(GamesDirectoryPath);
        }

        #endregion Intialization

        public GameDatabase() : this(null)
        {
        }

        public GameDatabase(string path)
        {
            DatabasePath = path;
            Platforms = new PlatformsCollection(this);
            Games = new GamesCollection(this);
            Emulators = new ItemCollection<Emulator>();
        }

        private void CheckDbState()
        {
            if (!IsOpen)
            {
                throw new Exception("Database is not opened.");
            }
        }

        // TODO: REMOVE
        public void SetDatabasePath(string path)
        {
            if (IsOpen)
            {
                throw new Exception("Cannot change database path when database is open.");
            }

            DatabasePath = path;
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

                            var profiles = new List<OldModels.Ver3.EmulatorProfile>
                            {
                                new OldModels.Ver3.EmulatorProfile()
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
                        var emusCollection = db.GetCollection<OldModels.Ver3.Emulator>("emulators");
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

                    // 3 to 4
                    if (db.Engine.UserVersion == 3 && DBVersion > 3)
                    {
                        var conPlatforms = new Dictionary<object, Guid>();
                        var platCollection = db.GetCollection("platforms");
                        foreach (var platform in platCollection.FindAll().ToList())
                        {
                            var oldId = platform["_id"];
                            var newId = Guid.NewGuid();
                            conPlatforms.Add(oldId, newId);
                            platCollection.Delete(oldId);
                            platform["_id"] = newId;
                            platCollection.Insert(platform);
                        }

                        var conEmulators = new Dictionary<object, Guid>();
                        var conEmuProfiles = new Dictionary<string, Guid>();
                        var emuCollection = db.GetCollection("emulators");
                        foreach (var emulator in emuCollection.FindAll().ToList())
                        {
                            var oldId = emulator["_id"];
                            var newId = Guid.NewGuid();
                            conEmulators.Add(oldId, newId);
                            emulator["_id"] = newId;

                            var profiles = emulator["Profiles"];
                            if (!profiles.IsNull)
                            {
                                foreach (BsonDocument profile in profiles.AsArray)
                                {
                                    var oldProfId = profile["_id"];
                                    var newProfId = Guid.NewGuid();
                                    conEmuProfiles.Add(oldId.AsString + oldProfId.AsString, newProfId);
                                    profile["_id"] = newProfId;

                                    var profPlatforms = profile["Platforms"];
                                    var newPlatforms = new BsonArray();
                                    if (!profPlatforms.IsNull)
                                    {
                                        foreach (var platform in profPlatforms.AsArray)
                                        {
                                            if (conPlatforms.TryGetValue(platform, out var newPlat))
                                            {
                                                newPlatforms.Add(newPlat);
                                            }
                                        }
                                    }

                                    profile["Platforms"] = newPlatforms;
                                }
                            }

                            emuCollection.Delete(oldId);
                            emuCollection.Insert(emulator);
                        }

                        var providerTable = new Dictionary<string, Guid>()
                        {
                            { "Custom", Guid.Empty },
                            { "GOG", Guid.Parse("AEBE8B7C-6DC3-4A66-AF31-E7375C6B5E9E") },
                            { "Origin", Guid.Parse("85DD7072-2F20-4E76-A007-41035E390724") },
                            { "Steam", Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB") },
                            { "Uplay", Guid.Parse("C2F038E5-8B92-4877-91F1-DA9094155FC5") },
                            { "BattleNet", Guid.Parse("E3C26A3D-D695-4CB7-A769-5FF7612C7EDD") },
                        };

                        var gameCol = db.GetCollection("games");
                        foreach (var game in gameCol.FindAll().ToList())
                        {
                            if (!game["Image"].IsNull)
                            {
                                game.Add("CoverImage", game["Image"]);
                                game.Remove("Image");
                            }

                            if (!game["IsoPath"].IsNull)
                            {
                                game.Add("GameImagePath", game["IsoPath"]);
                                game.Remove("IsoPath");
                            }

                            if (!game["ProviderId"].IsNull)
                            {
                                game.Add("GameId", game["ProviderId"]);
                                game.Remove("ProviderId");
                            }

                            var platform = game["PlatformId"];
                            if (!platform.IsNull)
                            {
                                if (conPlatforms.TryGetValue(platform, out var newPlat))
                                {
                                    game["PlatformId"] = newPlat;
                                }
                                else
                                {
                                    game.Remove("PlatformId");
                                }
                            }

                            var playAction = game["PlayTask"];
                            if (!playAction.IsNull)
                            {
                                MigrateGameAction(playAction.AsDocument, game["Provider"].AsString != "Custom");
                                game.Remove("PlayTask");
                                game.Add("PlayAction", playAction);
                            }

                            var otherActions = game["OtherTasks"];
                            if (!otherActions.IsNull)
                            {
                                foreach (BsonDocument task in otherActions.AsArray)
                                {
                                    MigrateGameAction(task, false);
                                }

                                game.Remove("OtherTasks");
                                game.Add("OtherActions", otherActions);
                            }

                            var provider = game["Provider"].AsString;
                            game.Add("PluginId", providerTable[provider]);
                            game.Remove("Provider");
                            gameCol.Update(game);
                        }

                        void MigrateGameAction(BsonDocument action, bool handleByPlugin)
                        {
                            action.Remove("IsPrimary");
                            action.Remove("IsBuiltIn");
                            action.Add("IsHandledByPlugin", handleByPlugin);

                            var oldEmulator = action["EmulatorId"];
                            if (!oldEmulator.IsNull)
                            {
                                if (conEmulators.TryGetValue(oldEmulator, out var newEmu))
                                {
                                    action["EmulatorId"] = newEmu;
                                }
                                else
                                {
                                    action.Remove("EmulatorId");
                                }
                            }

                            var oldProfile = action["EmulatorProfileId"];
                            if (!oldProfile.IsNull)
                            {
                                if (conEmuProfiles.TryGetValue(oldEmulator.AsString + oldProfile.AsString, out var newProf))
                                {
                                    action["EmulatorProfileId"] = newProf;
                                }
                                else
                                {
                                    action.Remove("EmulatorProfileId");
                                }
                            }
                        }

                        db.Engine.UserVersion = 4;
                    }

                    // 4 to 5
                    if (db.Engine.UserVersion == 4 && DBVersion > 4)
                    {
                        // Fix Game action that have invalid emualtor ids
                        var gameCol = db.GetCollection("games");
                        foreach (var game in gameCol.FindAll().ToList())
                        {
                            var fixApplied = false;
                            var playAction = game["PlayAction"];
                            if (!playAction.IsNull)
                            {
                                if (FixGameAction(playAction.AsDocument))
                                {
                                    fixApplied = true;
                                }

                            }

                            var otherActions = game["OtherActions"];
                            if (!otherActions.IsNull)
                            {
                                foreach (BsonDocument task in otherActions.AsArray)
                                {
                                    if (FixGameAction(task))
                                    {
                                        fixApplied = true;
                                    }
                                }
                            }

                            if (fixApplied)
                            {
                                gameCol.Update(game);
                            }
                        }

                        bool FixGameAction(BsonDocument action)
                        {
                            var fixedAny = false;
                            if (action["Type"].AsString != "Emulator")
                            {
                                var oldEmulator = action["EmulatorId"];
                                if (!oldEmulator.IsNull)
                                {
                                    action.Remove("EmulatorId");
                                    fixedAny = true;
                                }

                                var oldProfile = action["EmulatorProfileId"];
                                if (!oldProfile.IsNull)
                                {
                                    action.Remove("EmulatorProfileId");
                                    fixedAny = true;
                                }
                            }

                            return fixedAny;
                        }

                        db.Engine.UserVersion = 5;
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
            var settingsPath = Path.Combine(path, "database.json");
            if (!File.Exists(settingsPath))
            {
                return false;
            }

            var st = Serialization.FromJson<DatabaseSettings>(FileSystem.FileReadAsString(settingsPath));
            return st.Version < DBVersion;
        }

        public bool GetMigrationRequired()
        {
            return Settings.Version < DBVersion;
        }

        public void OpenDatabase()
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("Database path cannot be empty.");
            }

            var dbExists = Directory.Exists(DatabasePath);
            logger.Info("Opening db " + DatabasePath);
            CloseDatabase();

            if (!dbExists)
            {
                FileSystem.CreateDirectory(DatabasePath);
                FileSystem.CreateDirectory(GamesDirectoryPath);
                FileSystem.CreateDirectory(FilesDirectoryPath);
            }

            if (!dbExists)
            {
                Settings = new DatabaseSettings() { Version = DBVersion };
            }
            else
            {
                if (Settings.Version > DBVersion)
                {
                    throw new Exception($"Database version {Settings.Version} is not supported.");
                }

                if (GetMigrationRequired())
                {
                    throw new Exception("Database must be migrated before opening.");
                }
            }

            LoadCollections();

            // New DB setup
            if (!dbExists)
            {
                // Generate default platforms
                if (File.Exists(EmulatorDefinition.DefinitionsPath))
                {
                    var platforms = EmulatorDefinition.GetDefinitions()
                        .SelectMany(a => a.Profiles.SelectMany(b => b.Platforms)).Distinct()
                        .Select(a => new Platform(a)).ToList();
                    Platforms.Add(platforms);
                }
            }

            DatabaseOpened?.Invoke(this, null);
            IsOpen = true;
        }

        public void CloseDatabase()
        {
            if (!IsOpen)
            {
                return;
            }

            Games = null;
            Platforms = null;
            Emulators = null;
            IsOpen = false;
        }

        #region Files

        internal string GetFilePath(string dbPath)
        {
            return Path.Combine(FilesDirectoryPath, dbPath);
        }

        public string AddFile(MetadataFile file, Guid parentId)
        {
            return AddFile(file.FileName, file.Content, parentId);
        }

        public string AddFile(string path, Guid parentId)
        {
            return AddFile(Path.GetFileName(path), File.ReadAllBytes(path), parentId);
        }

        public string AddFile(string fileName, byte[] content, Guid parentId)
        {
            CheckDbState();
            var dbPath = Path.Combine(parentId.ToString(), Guid.NewGuid().ToString() + Path.GetExtension(fileName));
            var targetPath = Path.Combine(FilesDirectoryPath, dbPath);
            lock (fileFilesLock)
            {
                FileSystem.PrepareSaveFile(targetPath);
                File.WriteAllBytes(targetPath, content);
                return dbPath;
            }
        }

        public void RemoveFile(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                return;
            }

            CheckDbState();
            var filePath = GetFilePath(dbPath);
            lock (fileFilesLock)
            {
                FileSystem.DeleteFile(filePath);
                var dir = Path.GetDirectoryName(filePath);
                if (FileSystem.IsDirectoryEmpty(dir))
                {
                    FileSystem.DeleteDirectory(dir);
                }
            }
        }

        public BitmapImage GetFileAsImage(string dbPath)
        {
            CheckDbState();
            var filePath = GetFilePath(dbPath);
            if (!File.Exists(filePath))
            {
                return null;
            }

            lock (fileFilesLock)
            {
                using (var fStream = new FileStream(filePath, System.IO.FileMode.Open))
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
        }

        public void CopyFile(string dbPath, string targetPath)
        {
            CheckDbState();
            lock (fileFilesLock)
            {
                var filePath = GetFilePath(dbPath);
                FileSystem.PrepareSaveFile(targetPath);
                File.Copy(filePath, targetPath);
            }
        }

        #endregion Files

        public void AssignPcPlatform(Game game)
        {
            var platform = Platforms.FirstOrDefault(a => a.Name == "PC");
            if (platform == null)
            {
                platform = new Platform("PC");
                Platforms.Add(platform);
            }

            game.PlatformId = platform.Id;
        }

        public void AssignPcPlatform(List<Game> games)
        {
            var platform = Platforms.FirstOrDefault(a => a.Name == "PC");
            if (platform == null)
            {
                platform = new Platform("PC");
                Platforms.Add(platform);
            }

            foreach (var game in games)
            {
                game.PlatformId = platform.Id;
            }
        }

        public void ImportCategories(List<Game> sourceGames)
        {
            using (var buffer = BufferedUpdate())
            {
                foreach (var game in sourceGames)
                {
                    var dbGame = Games.FirstOrDefault(a => a.PluginId == game.PluginId && a.GameId == game.GameId);
                    if (dbGame == null)
                    {
                        continue;
                    }

                    dbGame.Categories = game.Categories;
                    Games.Update(dbGame);
                }
            }
        }

        public void BeginBufferUpdate()
        {
            Platforms.BeginBufferUpdate();
            Emulators.BeginBufferUpdate();
            Games.BeginBufferUpdate();
        }

        public void EndBufferUpdate()
        {
            Platforms.EndBufferUpdate();
            Emulators.EndBufferUpdate();
            Games.EndBufferUpdate();
        }

        public IDisposable BufferedUpdate()
        {
            return new EventBufferHandler(this);
        }
    }
}
