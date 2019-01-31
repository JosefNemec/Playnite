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
using Playnite.Settings;
using Playnite.Common.System;
using Newtonsoft.Json.Linq;
using Playnite.SDK.Plugins;
using Playnite.Web;

namespace Playnite.Database
{
    // TODO cleanup methods, remove duplicates
    public class GameDatabase : IGameDatabase
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

        private const string gamesDirName = "games";
        private const string platformsDirName = "platforms";
        private const string emulatorsDirName = "emulators";
        private const string filesDirName = "files";
        private const string genresDirName = "genres";
        private const string companiesDirName = "companies";
        private const string tagsDirName = "tags";
        private const string categoriesDirName = "categories";
        private const string seriesDirName = "series";
        private const string ageRatingsDirName = "ageratings";
        private const string regionsDirName = "regions";
        private const string sourcesDirName = "sources";
        private const string settingsFileName = "database.json";

        private string GamesDirectoryPath { get => Path.Combine(DatabasePath, gamesDirName); }
        private string PlatformsDirectoryPath { get => Path.Combine(DatabasePath, platformsDirName); }
        private string EmulatorsDirectoryPath { get => Path.Combine(DatabasePath, emulatorsDirName); }
        private string GenresDirectoryPath { get => Path.Combine(DatabasePath, genresDirName); }
        private string CompaniesDirectoryPath { get => Path.Combine(DatabasePath, companiesDirName); }
        private string TagsDirectoryPath { get => Path.Combine(DatabasePath, tagsDirName); }
        private string CategoriesDirectoryPath { get => Path.Combine(DatabasePath, categoriesDirName); }
        private string AgeRatingsDirectoryPath { get => Path.Combine(DatabasePath, ageRatingsDirName); }
        private string SeriesDirectoryPath { get => Path.Combine(DatabasePath, seriesDirName); }
        private string RegionsDirectoryPath { get => Path.Combine(DatabasePath, regionsDirName); }
        private string SourcesDirectoryPath { get => Path.Combine(DatabasePath, sourcesDirName); }
        private string FilesDirectoryPath { get => Path.Combine(DatabasePath, filesDirName); }
        private string DatabaseFileSettingsPath { get => Path.Combine(DatabasePath, settingsFileName); }

        #endregion Paths

        #region Lists

        public IItemCollection<Game> Games { get; private set; }
        public IItemCollection<Platform> Platforms { get; private set; }
        public IItemCollection<Emulator> Emulators { get; private set; }
        public IItemCollection<Genre> Genres { get; private set; }
        public IItemCollection<Company> Companies { get; private set; }
        public IItemCollection<Tag> Tags { get; private set; }
        public IItemCollection<Category> Categories { get; private set; }
        public IItemCollection<Series> Series { get; private set; }
        public IItemCollection<AgeRating> AgeRatings { get; private set; }
        public IItemCollection<Region> Regions { get; private set; }
        public IItemCollection<GameSource> Sources { get; private set; }

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
                        lock (databaseConfigFileLock)
                        {
                            settings = Serialization.FromJson<DatabaseSettings>(FileSystem.ReadFileAsStringSafe(DatabaseFileSettingsPath));
                            if (settings == null)
                            {
                                // This shouldn't in theory happen, but there are some wierd crash reports available for this.
                                settings = new DatabaseSettings() { Version = NewFormatVersion };
                            }
                        }
                    }
                    else
                    {
                        settings = new DatabaseSettings() { Version = NewFormatVersion };
                    }
                }

                return settings;
            }

            set
            {
                lock (databaseConfigFileLock)
                {
                    settings = value;
                    FileSystem.WriteStringToFileSafe(DatabaseFileSettingsPath, Serialization.ToJson(settings));
                }
            }
        }

        public static readonly ushort DBVersion = 6;

        public static readonly ushort NewFormatVersion = 2;

        #region Events

        public event EventHandler DatabaseOpened;

        public event DatabaseFileEventHandler DatabaseFileChanged;

        #endregion Events

        #region Initialization

        private void LoadCollections()
        {
            (Platforms as PlatformsCollection).InitializeCollection(PlatformsDirectoryPath);
            (Emulators as EmulatorsCollection).InitializeCollection(EmulatorsDirectoryPath);
            (Games as GamesCollection).InitializeCollection(GamesDirectoryPath);
            (Genres as GenresCollection).InitializeCollection(GenresDirectoryPath);
            (Companies as CompaniesCollection).InitializeCollection(CompaniesDirectoryPath);
            (Tags as TagsCollection).InitializeCollection(TagsDirectoryPath);
            (Categories as CategoriesCollection).InitializeCollection(CategoriesDirectoryPath);
            (AgeRatings as AgeRatingsCollection).InitializeCollection(AgeRatingsDirectoryPath);
            (Series as SeriesCollection).InitializeCollection(SeriesDirectoryPath);
            (Regions as RegionsCollection).InitializeCollection(RegionsDirectoryPath);
            (Sources as GamesSourcesCollection).InitializeCollection(SourcesDirectoryPath);
        }

        #endregion Intialization

        public GameDatabase() : this(null)
        {
        }

        public GameDatabase(string path)
        {
            DatabasePath = GetFullDbPath(path);
            Platforms = new PlatformsCollection(this);
            Games = new GamesCollection(this);
            Emulators = new EmulatorsCollection(this);
            Genres = new GenresCollection(this);
            Companies = new CompaniesCollection(this);
            Tags = new TagsCollection(this);
            Categories = new CategoriesCollection(this);
            AgeRatings = new AgeRatingsCollection(this);
            Series = new SeriesCollection(this);
            Regions = new RegionsCollection(this);
            Sources = new GamesSourcesCollection(this);
        }

        private void CheckDbState()
        {
            if (!IsOpen)
            {
                throw new Exception("Database is not opened.");
            }
        }

        internal static DatabaseSettings GetSettingsFromDbPath(string dbPath)
        {
            var settingsPath = Path.Combine(dbPath, settingsFileName);
            return Serialization.FromJson<DatabaseSettings>(FileSystem.ReadFileAsStringSafe(settingsPath));
        }

        internal static void SaveSettingsToDbPath(DatabaseSettings settings, string dbPath)
        {
            var settingsPath = Path.Combine(dbPath, settingsFileName);            
            FileSystem.WriteStringToFileSafe(settingsPath, Serialization.ToJson(settings));
        }

        // TODO: Remove this, we should only allow path to be set during instantiation.
        public void SetDatabasePath(string path)
        {
            if (IsOpen)
            {
                throw new Exception("Cannot change database path when database is open.");
            }

            DatabasePath = GetFullDbPath(path);
        }

        public static void MigrateOldDatabaseFormat(string path)
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

                            if (links.Count > 0)
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

                    // 5 to 6
                    if (db.Engine.UserVersion == 5 && DBVersion > 5)
                    {
                        // Remove _type from emulator profiles (was added by a bug in old versions)
                        var emuCollection = db.GetCollection("emulators");
                        foreach (var emulator in emuCollection.FindAll().ToList())
                        {
                            var update = false;
                            var profiles = emulator["Profiles"];
                            if (!profiles.IsNull)
                            {
                                foreach (BsonDocument profile in profiles.AsArray)
                                {
                                    if (profile.ContainsKey("_type"))
                                    {
                                        update = true;
                                        profile.Remove("_type");
                                    }
                                }
                            }

                            if (update)
                            {
                                emuCollection.Update(emulator);
                            }
                        }
                                                
                        var gameCol = db.GetCollection("games");
                        foreach (var game in gameCol.FindAll().ToList())
                        {
                            // Change game states object
                            var state = game["State"];
                            if (!state.IsNull)
                            {
                                game.Add("IsInstalled", state.AsDocument["Installed"].AsBoolean);
                            }

                            // Change game Id from int to Guid
                            gameCol.Delete(game["_id"].AsInt32);
                            game["_id"] = Guid.NewGuid();
                            gameCol.Insert(game);
                        }

                        db.Engine.UserVersion = 6;
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

        // TODO move to separete partial file
        public static void MigrateNewDatabaseFormat(string path)
        {
            // Todo implement fallback and revert in case conversion goes wrong.
            var dbSettings = GetSettingsFromDbPath(path);
            var gamesDir = Path.Combine(path, gamesDirName);

            // 1 to 2
            if (dbSettings.Version == 1 && NewFormatVersion > 1)
            {
                void convetList<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : DatabaseObject
                {
                    if (game.TryGetValue(origKey, out var storedObj))
                    {
                        var gameObjs = new List<Guid>();
                        var oldLIst = (storedObj as JArray).ToObject<List<string>>();
                        foreach (var oldObj in oldLIst)
                        {
                            if (string.IsNullOrEmpty(oldObj))
                            {
                                continue;
                            }

                            if (convertedList.TryGetValue(oldObj, out var curObj))
                            {
                                gameObjs.Add(curObj.Id);
                            }
                            else
                            {
                                var newObj = typeof(T).CrateInstance<T>(oldObj);
                                gameObjs.Add(newObj.Id);
                                convertedList.Add(oldObj, newObj);
                            }
                        }

                        game.Remove(origKey);
                        game[newKey] = gameObjs;
                    }
                }

                void covertObject<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : DatabaseObject
                {
                    if (game.TryGetValue(origKey, out var storedObj))
                    {                        
                        var oldObj = storedObj as string;
                        if (!string.IsNullOrEmpty(oldObj))
                        {
                            if (convertedList.TryGetValue(oldObj, out var curObj))
                            {
                                game[newKey] = curObj.Id;
                            }
                            else
                            {
                                var newObj = typeof(T).CrateInstance<T>(oldObj);
                                game[newKey] = newObj.Id;
                                convertedList.Add(oldObj, newObj);
                            }
                        }                        

                        game.Remove(origKey);
                    }
                }

                void saveCollection<T>(Dictionary<string, T> collection, string collPath) where T : DatabaseObject
                {
                    if (collection.Any())
                    {
                        foreach (var item in collection.Values)
                        {
                            FileSystem.WriteStringToFileSafe(Path.Combine(collPath, item.Id + ".json"), Serialization.ToJson(item));
                        }
                    }
                }

                var allGenres = new Dictionary<string, Genre>(StringComparer.CurrentCultureIgnoreCase);
                var allCompanies = new Dictionary<string, Company>(StringComparer.CurrentCultureIgnoreCase);
                var allTags = new Dictionary<string, Tag>(StringComparer.CurrentCultureIgnoreCase);
                var allCategories = new Dictionary<string, Category>(StringComparer.CurrentCultureIgnoreCase);
                var allSeries = new Dictionary<string, Series>(StringComparer.CurrentCultureIgnoreCase);
                var allRatings = new Dictionary<string, AgeRating>(StringComparer.CurrentCultureIgnoreCase);
                var allRegions = new Dictionary<string, Region>(StringComparer.CurrentCultureIgnoreCase);
                var allSources = new Dictionary<string, GameSource>(StringComparer.CurrentCultureIgnoreCase);

                // Convert following object to Id representations and store them in separete lists:
                foreach (var file in Directory.EnumerateFiles(gamesDir, "*.json"))
                {
                    var game = Serialization.FromJson<Dictionary<string, object>>(FileSystem.ReadFileAsStringSafe(file));

                    // Genres    
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Genres), nameof(Game.GenreIds), allGenres);

                    // Developers
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Developers), nameof(Game.DeveloperIds), allCompanies);

                    // Publishers
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Publishers), nameof(Game.PublisherIds), allCompanies);

                    // Tags
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Tags), nameof(Game.TagIds), allTags);

                    // Categories
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Categories), nameof(Game.CategoryIds), allCategories);

                    // Series
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Series), nameof(Game.SeriesId), allSeries);

                    // AgeRating
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.AgeRating), nameof(Game.AgeRatingId), allRatings);

                    // Region
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Region), nameof(Game.RegionId), allRegions);

                    // Source
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Source), nameof(Game.SourceId), allSources);

                    FileSystem.WriteStringToFileSafe(file, Serialization.ToJson(game));
                }

                saveCollection(allGenres, Path.Combine(path, genresDirName));
                saveCollection(allCompanies, Path.Combine(path, companiesDirName));
                saveCollection(allTags, Path.Combine(path, tagsDirName));
                saveCollection(allCategories, Path.Combine(path, categoriesDirName));
                saveCollection(allSeries, Path.Combine(path, seriesDirName));
                saveCollection(allRatings, Path.Combine(path, ageRatingsDirName));
                saveCollection(allRegions, Path.Combine(path, regionsDirName));
                saveCollection(allSources, Path.Combine(path, sourcesDirName));

                dbSettings.Version = 2;
                SaveSettingsToDbPath(dbSettings, path);
            }
        }

        public static void MigrateToNewFormat(string oldPath, string newPath)
        {
            using (var db = new LiteDatabase(oldPath))
            {
                string ExportFile(Guid parentId, string fileId)
                {
                    if (!string.IsNullOrEmpty(fileId))
                    {
                        if (fileId.IsHttpUrl())
                        {
                            return fileId;
                        }

                        var cover = db.FileStorage.FindById(fileId);
                        if (cover != null)
                        {
                            var newFileId = Path.Combine(parentId.ToString(), Guid.NewGuid() + Path.GetExtension(cover.Filename));
                            var targetPath = Path.Combine(newPath, filesDirName, newFileId);
                            FileSystem.PrepareSaveFile(targetPath);
                            cover.SaveAs(targetPath);
                            return newFileId;
                        }
                    }

                    return null;
                }

                var gamesDir = Path.Combine(newPath, gamesDirName);
                FileSystem.CreateDirectory(gamesDir);
                var gameCol = db.GetCollection("games");
                foreach (var game in gameCol.FindAll())
                {
                    var conGame = BsonMapper.Global.ToObject<OldModels.Ver6.OldGame>(game);
                    var targetFile = Path.Combine(gamesDir, $"{conGame.Id.ToString()}.json");
                    conGame.CoverImage = ExportFile(conGame.Id, conGame.CoverImage);
                    conGame.Icon = ExportFile(conGame.Id, conGame.Icon);
                    conGame.BackgroundImage = ExportFile(conGame.Id, conGame.BackgroundImage);
                    File.WriteAllText(targetFile, Serialization.ToJson(conGame, false));
                }

                var platformsDir = Path.Combine(newPath, platformsDirName);
                FileSystem.CreateDirectory(platformsDir);
                var platformsCol = db.GetCollection("platforms");
                foreach (var platform in platformsCol.FindAll())
                {
                    var conPlatform = BsonMapper.Global.ToObject<OldModels.Ver6.Platform>(platform);
                    var targetFile = Path.Combine(platformsDir, $"{conPlatform.Id.ToString()}.json");
                    conPlatform.Cover = ExportFile(conPlatform.Id, conPlatform.Cover);
                    conPlatform.Icon = ExportFile(conPlatform.Id, conPlatform.Icon);
                    File.WriteAllText(targetFile, Serialization.ToJson(conPlatform, false));
                }

                var emulatorsDir = Path.Combine(newPath, emulatorsDirName);
                FileSystem.CreateDirectory(emulatorsDir);
                var emulatorsCol = db.GetCollection("emulators");
                foreach (var emulator in emulatorsCol.FindAll())
                {
                    var conEmulator = BsonMapper.Global.ToObject<OldModels.Ver6.Emulator>(emulator);
                    var targetFile = Path.Combine(emulatorsDir, $"{conEmulator.Id.ToString()}.json");
                    File.WriteAllText(targetFile, Serialization.ToJson(conEmulator, false));
                }
            }

            var dbSet = new DatabaseSettings() { Version = 1 };
            File.WriteAllText(Path.Combine(newPath, settingsFileName), Serialization.ToJson(dbSet));
        }

        public static string GetMigratedDbPath(string originalPath)
        {
            if (Path.IsPathRooted(originalPath))
            {
                var rootDir = Path.GetDirectoryName(originalPath);
                var appData = Environment.ExpandEnvironmentVariables("%AppData%");
                rootDir = rootDir.Replace(appData, "%AppData%");                
                return Path.Combine(rootDir, Path.GetFileNameWithoutExtension(originalPath));
            }
            else
            {
                return Path.Combine("{PlayniteDir}", Path.GetFileNameWithoutExtension(originalPath));
            }
        }

        public static string GetFullDbPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.Contains("{PlayniteDir}", StringComparison.OrdinalIgnoreCase))
            {
                return path?.Replace("{PlayniteDir}", PlaynitePaths.ProgramPath);
            }
            else if (path.Contains("%AppData%", StringComparison.OrdinalIgnoreCase))
            {                
                return path?.Replace("%AppData%", Environment.ExpandEnvironmentVariables("%AppData%"), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return path;
            }
        }

        public static bool GetMigrationRequired(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new ArgumentNullException(nameof(databasePath));
            }

            if (databasePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var fullPath = GetFullDbPath(databasePath);
            var settingsPath = Path.Combine(fullPath, "database.json");
            if (!File.Exists(settingsPath))
            {
                return false;
            }

            var st = Serialization.FromJson<DatabaseSettings>(FileSystem.ReadFileAsStringSafe(settingsPath));
            if (st == null)
            {
                // This shouldn't in theory happen, but there are some wierd crash reports available for this.
                return false;
            }
            else
            {
                return st.Version < NewFormatVersion;
            }
        }

        public void OpenDatabase()
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new Exception("Database path cannot be empty.");
            }

            var dbExists = File.Exists(DatabaseFileSettingsPath);
            logger.Info("Opening db " + DatabasePath);

            if (!dbExists)
            {
                FileSystem.CreateDirectory(DatabasePath);
                FileSystem.CreateDirectory(GamesDirectoryPath);
                FileSystem.CreateDirectory(FilesDirectoryPath);
            }

            if (!dbExists)
            {
                Settings = new DatabaseSettings() { Version = NewFormatVersion };
            }
            else
            {
                if (Settings.Version > NewFormatVersion)
                {
                    throw new Exception($"Database version {Settings.Version} is not supported.");
                }

                if (GetMigrationRequired(DatabasePath))
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

        #region Files

        public string GetFileStoragePath(Guid parentId)
        {
            var path = Path.Combine(FilesDirectoryPath, parentId.ToString());
            FileSystem.CreateDirectory(path, false);
            return path;
        }

        public string GetFullFilePath(string dbPath)
        {
            return Path.Combine(FilesDirectoryPath, dbPath);
        }

        public string AddFile(MetadataFile file, Guid parentId)
        {
            return AddFile(file.FileName, file.Content, parentId);
        }

        public string AddFile(string path, Guid parentId)
        {
            CheckDbState();
            var fileName = Path.GetFileName(path);
            var targetDir = Path.Combine(FilesDirectoryPath, parentId.ToString());
            var dbPath = string.Empty;
            lock (fileFilesLock)
            {
                // Re-use file if already part of db folder, don't copy.
                if (Paths.AreEqual(targetDir, Path.GetDirectoryName(path)))
                {
                    dbPath = Path.Combine(parentId.ToString(), fileName);
                }
                else
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
                    FileSystem.CopyFile(path, Path.Combine(targetDir, fileName));
                    dbPath = Path.Combine(parentId.ToString(), fileName);
                }
            }

            DatabaseFileChanged?.Invoke(this, new DatabaseFileEventArgs(dbPath, FileEvent.Added));
            return dbPath;
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
            }

            DatabaseFileChanged?.Invoke(this, new DatabaseFileEventArgs(dbPath, FileEvent.Added));
            return dbPath;
        }

        public void RemoveFile(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                return;
            }

            CheckDbState();
            var filePath = GetFullFilePath(dbPath);
            if (!File.Exists(filePath))
            {
                return;
            }

            lock (fileFilesLock)
            {
                FileSystem.DeleteFileSafe(filePath);

                try
                {
                    var dir = Path.GetDirectoryName(filePath);
                    if (FileSystem.IsDirectoryEmpty(dir))
                    {
                        FileSystem.DeleteDirectory(dir);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    // Getting crash reports from Path.GetDirectoryName for some reason.
                    logger.Error(e, "Failed to clean up directory after removing file");
                }
            }

            DatabaseFileChanged?.Invoke(this, new DatabaseFileEventArgs(dbPath, FileEvent.Removed));
        }

        public BitmapImage GetFileAsImage(string dbPath)
        {
            CheckDbState();
            var filePath = GetFullFilePath(dbPath);
            if (!File.Exists(filePath))
            {
                return null;
            }

            lock (fileFilesLock)
            {
                using (var fStream = FileSystem.OpenFileStreamSafe(filePath))
                using (var wrapper = new WrappingStream(fStream))
                {
                    return BitmapExtensions.BitmapFromStream(wrapper);
                }
            }
        }

        public void CopyFile(string dbPath, string targetPath)
        {
            CheckDbState();
            lock (fileFilesLock)
            {
                var filePath = GetFullFilePath(dbPath);
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

        public void BeginBufferUpdate()
        {
            Platforms.BeginBufferUpdate();
            Emulators.BeginBufferUpdate();
            Games.BeginBufferUpdate();
            Genres.BeginBufferUpdate();
            Companies.BeginBufferUpdate();
            Tags.BeginBufferUpdate();
            Categories.BeginBufferUpdate();
            Series.BeginBufferUpdate();
            AgeRatings.BeginBufferUpdate();
            Regions.BeginBufferUpdate();
            Sources.BeginBufferUpdate();
        }

        public void EndBufferUpdate()
        {
            Platforms.EndBufferUpdate();
            Emulators.EndBufferUpdate();
            Games.EndBufferUpdate();
            Genres.EndBufferUpdate();
            Companies.EndBufferUpdate();
            Tags.EndBufferUpdate();
            Categories.EndBufferUpdate();
            Series.EndBufferUpdate();
            AgeRatings.EndBufferUpdate();
            Regions.EndBufferUpdate();
            Sources.EndBufferUpdate();
        }

        public IDisposable BufferedUpdate()
        {
            return new EventBufferHandler(this);
        }

        private string AddNewGameFile(string path, Guid gameId)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(path);
            MetadataFile metaFile = null;

            try
            {
                if (path.IsHttpUrl())
                {
                    metaFile = new MetadataFile(fileName, HttpDownloader.DownloadData(path));
                }
                else
                {
                    if (File.Exists(path))
                    {
                        if (path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            var icon = IconExtension.ExtractIconFromExe(path, true);
                            if (icon == null)
                            {
                                return null;
                            }

                            fileName = Path.ChangeExtension(fileName, ".png");
                            metaFile = new MetadataFile(fileName, icon.ToByteArray(System.Drawing.Imaging.ImageFormat.Png));
                        }
                        else
                        {
                            metaFile = new MetadataFile(fileName, File.ReadAllBytes(path));
                        }
                    }
                    else
                    {
                        logger.Error($"Can't add game file during game import, file doesn't exists: {path}");
                    }
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to import game file during game import from {path}");
            }

            if (metaFile != null)
            {
                if (metaFile.FileName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    metaFile.FileName = Path.ChangeExtension(metaFile.FileName, ".png");
                    metaFile.Content = BitmapExtensions.TgaToBitmap(metaFile.Content).ToPngArray();
                }

                return AddFile(metaFile, gameId);
            }

            return null;
        }

        private Game GameInfoToGame(GameInfo game, Guid pluginId)
        {
            var toAdd = new Game()
            {
                PluginId = pluginId,
                Name = game.Name,
                GameId = game.GameId,
                Description = game.Description,
                InstallDirectory = game.InstallDirectory,
                GameImagePath = game.GameImagePath,
                SortingName = game.SortingName,
                OtherActions = game.OtherActions == null ? null : new ObservableCollection<GameAction>(game.OtherActions),
                PlayAction = game.PlayAction,
                ReleaseDate = game.ReleaseDate,
                Links = game.Links == null ? null : new ObservableCollection<Link>(game.Links),
                IsInstalled = game.IsInstalled,
                Playtime = game.Playtime,
                PlayCount = game.PlayCount,
                LastActivity = game.LastActivity,
                Version = game.Version,
                CompletionStatus = game.CompletionStatus,
                UserScore = game.UserScore,
                CriticScore = game.CriticScore,
                CommunityScore = game.CommunityScore
            };

            if (string.IsNullOrEmpty(game.Platform))
            {
                AssignPcPlatform(toAdd);
            }
            else
            {
                toAdd.PlatformId = Platforms.Add(game.Platform).Id;
            }

            if (game.Developers?.Any() == true)
            {
                toAdd.DeveloperIds = Companies.Add(game.Developers).Select(a => a.Id).ToComparable();
            }

            if (game.Publishers?.Any() == true)
            {
                toAdd.PublisherIds = Companies.Add(game.Publishers).Select(a => a.Id).ToComparable();
            }

            if (game.Genres?.Any() == true)
            {
                toAdd.GenreIds = Genres.Add(game.Genres).Select(a => a.Id).ToComparable();
            }

            if (game.Categories?.Any() == true)
            {
                toAdd.CategoryIds = Categories.Add(game.Categories).Select(a => a.Id).ToComparable();
            }

            if (game.Tags?.Any() == true)
            {
                toAdd.TagIds = Tags.Add(game.Tags).Select(a => a.Id).ToComparable();
            }

            if (!string.IsNullOrEmpty(game.AgeRating))
            {
                toAdd.AgeRatingId = AgeRatings.Add(game.AgeRating).Id;
            }

            if (!string.IsNullOrEmpty(game.Series))
            {
                toAdd.SeriesId = Series.Add(game.Series).Id;
            }

            if (!string.IsNullOrEmpty(game.Region))
            {
                toAdd.RegionId = Regions.Add(game.Region).Id;
            }

            if (!string.IsNullOrEmpty(game.Source))
            {
                toAdd.SourceId = Sources.Add(game.Source).Id;
            }

            return toAdd;
        }

        public Game ImportGame(GameInfo game)
        {
            return ImportGame(game, Guid.Empty);
        }

        public Game ImportGame(GameInfo game, Guid pluginId)
        {
            var toAdd = GameInfoToGame(game, pluginId);
            toAdd.Icon = AddNewGameFile(game.Icon, game.Id);
            toAdd.CoverImage = AddNewGameFile(game.CoverImage, game.Id);
            if (!string.IsNullOrEmpty(game.BackgroundImage) && !game.BackgroundImage.IsHttpUrl())
            {
                toAdd.BackgroundImage = AddNewGameFile(game.BackgroundImage, game.Id);
            }

            Games.Add(toAdd);
            return toAdd;
        }

        public Game ImportGame(GameMetadata metadata)
        {
            var toAdd = GameInfoToGame(metadata.GameInfo, Guid.Empty);
            if (metadata.Icon != null)
            {
                toAdd.Icon = AddFile(metadata.Icon, toAdd.Id);
            }

            if (metadata.CoverImage != null)
            {
                toAdd.CoverImage = AddFile(metadata.CoverImage, toAdd.Id);
            }

            if (metadata.BackgroundImage != null)
            {
                if (metadata.BackgroundImage.Content == null)
                {
                    toAdd.BackgroundImage = metadata.BackgroundImage.OriginalUrl;
                }
                else
                {
                    toAdd.BackgroundImage = AddFile(metadata.BackgroundImage, toAdd.Id);
                }
            }

            Games.Add(toAdd);
            return toAdd;
        }

        public List<Game> ImportGames(ILibraryPlugin library, bool forcePlayTimeSync)
        {
            var addedGames = new List<Game>();
            foreach (var newGame in library.GetGames())
            {
                var existingGame = Games.FirstOrDefault(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                if (existingGame == null)
                {
                    logger.Info(string.Format("Adding new game {0} from {1} plugin", newGame.GameId, library.Name));
                    addedGames.Add(ImportGame(newGame, library.Id));
                }
                else
                {
                    existingGame.IsInstalled = newGame.IsInstalled;
                    existingGame.InstallDirectory = newGame.InstallDirectory;
                    if (existingGame.PlayAction == null || existingGame.PlayAction.IsHandledByPlugin)
                    {
                        existingGame.PlayAction = newGame.PlayAction;
                    }

                    if ((existingGame.Playtime == 0 && newGame.Playtime > 0) ||
                       (newGame.Playtime > 0 && forcePlayTimeSync))
                    {
                        existingGame.Playtime = newGame.Playtime;
                        if (existingGame.CompletionStatus == CompletionStatus.NotPlayed)
                        {
                            existingGame.CompletionStatus = CompletionStatus.Played;
                        }

                        if (existingGame.LastActivity == null && newGame.LastActivity != null)
                        {
                            existingGame.LastActivity = newGame.LastActivity;
                        }
                    }

                    if (existingGame.OtherActions?.Any() != true && newGame.OtherActions?.Any() == true)
                    {
                        existingGame.OtherActions = new ObservableCollection<GameAction>(newGame.OtherActions);
                    }

                    Games.Update(existingGame);
                }
            }

            return addedGames;        
        }
    }
}
