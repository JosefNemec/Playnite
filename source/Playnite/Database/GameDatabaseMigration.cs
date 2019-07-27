using LiteDB;
using Newtonsoft.Json.Linq;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public partial class GameDatabase
    {
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

        public static void MigrateNewDatabaseFormat(string path)
        {
            var dbSettings = GetSettingsFromDbPath(path);
            var gamesDir = Path.Combine(path, gamesDirName);

            // 1 to 2
            if (dbSettings.Version == 1 && NewFormatVersion > 1)
            {
                void convetList<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : DatabaseObject
                {
                    if (game.TryGetValue(origKey, out var storedObj))
                    {
                        if (storedObj == null)
                        {
                            return;
                        }

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
                                if (!gameObjs.Contains(curObj.Id))
                                {
                                    gameObjs.Add(curObj.Id);
                                }
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
                    if (game == null)
                    {
                        // Some users have 0 sized game files for uknown reason.
                        File.Delete(file);
                        continue;
                    }

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
                return Path.Combine(ExpandableVariables.PlayniteDirectory, Path.GetFileNameWithoutExtension(originalPath));
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
    }
}
