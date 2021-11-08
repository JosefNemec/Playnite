using LiteDB;
using Newtonsoft.Json.Linq;
using Playnite.Common;
using Playnite.Database.OldModels;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public partial class GameDatabase
    {
        public static void MigrateNewDatabaseFormat(string databasePath)
        {
            var dbSettings = GetSettingsFromDbPath(databasePath);
            var gamesDir = Path.Combine(databasePath, gamesDirName);

            // 1 to 2
            if (dbSettings.Version == 1 && NewFormatVersion > 1)
            {
                void convetList<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : Ver2_DatabaseObject
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

                void covertObject<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : Ver2_DatabaseObject
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

                void saveCollection<T>(Dictionary<string, T> collection, string collPath) where T : Ver2_DatabaseObject
                {
                    if (collection.Any())
                    {
                        foreach (var item in collection.Values)
                        {
                            FileSystem.WriteStringToFileSafe(Path.Combine(collPath, item.Id + ".json"), Serialization.ToJson(item));
                        }
                    }
                }

                var allGenres = new Dictionary<string, Ver2_Genre>(StringComparer.CurrentCultureIgnoreCase);
                var allCompanies = new Dictionary<string, Ver2_Company>(StringComparer.CurrentCultureIgnoreCase);
                var allTags = new Dictionary<string, Ver2_Tag>(StringComparer.CurrentCultureIgnoreCase);
                var allCategories = new Dictionary<string, Ver2_Category>(StringComparer.CurrentCultureIgnoreCase);
                var allSeries = new Dictionary<string, Ver2_Series>(StringComparer.CurrentCultureIgnoreCase);
                var allRatings = new Dictionary<string, Ver2_AgeRating>(StringComparer.CurrentCultureIgnoreCase);
                var allRegions = new Dictionary<string, Ver2_Region>(StringComparer.CurrentCultureIgnoreCase);
                var allSources = new Dictionary<string, Ver2_GameSource>(StringComparer.CurrentCultureIgnoreCase);

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
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Genres), nameof(Ver2_Game.GenreIds), allGenres);

                    // Developers
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Developers), nameof(Ver2_Game.DeveloperIds), allCompanies);

                    // Publishers
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Publishers), nameof(Ver2_Game.PublisherIds), allCompanies);

                    // Tags
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Tags), nameof(Ver2_Game.TagIds), allTags);

                    // Categories
                    convetList(game, nameof(OldModels.NewVer1.OldGame.Categories), nameof(Ver2_Game.CategoryIds), allCategories);

                    // Series
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Series), nameof(Ver2_Game.SeriesId), allSeries);

                    // AgeRating
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.AgeRating), nameof(Ver2_Game.AgeRatingId), allRatings);

                    // Region
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Region), nameof(Ver2_Game.RegionId), allRegions);

                    // Source
                    covertObject(game, nameof(OldModels.NewVer1.OldGame.Source), nameof(Ver2_Game.SourceId), allSources);

                    FileSystem.WriteStringToFileSafe(file, Serialization.ToJson(game));
                }

                saveCollection(allGenres, Path.Combine(databasePath, genresDirName));
                saveCollection(allCompanies, Path.Combine(databasePath, companiesDirName));
                saveCollection(allTags, Path.Combine(databasePath, tagsDirName));
                saveCollection(allCategories, Path.Combine(databasePath, categoriesDirName));
                saveCollection(allSeries, Path.Combine(databasePath, seriesDirName));
                saveCollection(allRatings, Path.Combine(databasePath, ageRatingsDirName));
                saveCollection(allRegions, Path.Combine(databasePath, regionsDirName));
                saveCollection(allSources, Path.Combine(databasePath, sourcesDirName));

                dbSettings.Version = 2;
                SaveSettingsToDbPath(dbSettings, databasePath);
            }

            // 2 to 3
            if (dbSettings.Version == 2 && NewFormatVersion > 2)
            {
                var mapper = new BsonMapper()
                {
                    SerializeNullValues = false,
                    TrimWhitespace = false,
                    EmptyStringToNull = true,
                    IncludeFields = false,
                    IncludeNonPublic = false
                };

                void convertList<TOld, TNew>(string dir, Action<TOld, TNew> propertyMapper = null) where TOld : Ver2_DatabaseObject where TNew : DatabaseObject
                {
                    var dbFile = dir + ".db";
                    if (File.Exists(dbFile))
                    {
                        logger.Warn($"Migration database file {dbFile} already exists!");
                        File.Delete(dbFile);
                    }

                    using (var db = new LiteDatabase($"Filename={dbFile};Mode=Exclusive;Journal=false", mapper))
                    {
                        var col = db.GetCollection<TNew>();
                        col.EnsureIndex(a => a.Id, true);
                        foreach (var file in Directory.GetFiles(dir, "*.json"))
                        {
                            if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out _))
                            {
                                TOld oldItem = null;
                                try
                                {
                                    oldItem = Serialization.FromJsonFile<TOld>(file);
                                }
                                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                                {
                                    logger.Error(e, $"Failed to load old database file {file}.");
                                    continue;
                                }

                                if (oldItem == null)
                                {
                                    logger.Warn($"Failed to load old database file {file}, it's empty.");
                                    continue;
                                }

                                var newItem = typeof(TNew).CrateInstance<TNew>(oldItem.Name);
                                newItem.Id = oldItem.Id;
                                propertyMapper?.Invoke(oldItem, newItem);
                                col.Insert(newItem);
                            }
                        }
                    }
                }

                LiteDatabase createDb<T>(string dir) where T : DatabaseObject
                {
                    var dbFile = dir + ".db";
                    if (File.Exists(dbFile))
                    {
                        logger.Warn($"Migration database file {dbFile} already exists!");
                        File.Delete(dbFile);
                    }

                    var db = new LiteDatabase($"Filename={dbFile};Mode=Exclusive;Journal=false", mapper);
                    var col = db.GetCollection<T>();
                    col.EnsureIndex(a => a.Id, true);
                    return db;
                }

                // Convert completion statuses
                CompletionStatusesCollection.MapLiteDbEntities(mapper);
                var convertedCompStatuses = new List<CompletionStatus>();
                using (var db = createDb<CompletionStatus>(Path.Combine(databasePath, completionStatusesDirName)))
                {
                    var col = db.GetCollection<CompletionStatus>();
                    foreach (Ver2_CompletionStatus value in Enum.GetValues(typeof(Ver2_CompletionStatus)))
                    {
                        var newStatus = new CompletionStatus(value.GetDescription());
                        convertedCompStatuses.Add(newStatus);
                        col.Insert(newStatus);
                    }

                    var setCol = db.GetCollection<CompletionStatusSettings>();
                    setCol.Insert(new CompletionStatusSettings
                    {
                        DefaultStatus = convertedCompStatuses[0].Id,
                        PlayedStatus = convertedCompStatuses[1].Id
                    });
                }

                // Generate default filter presets
                FilterPresetsCollection.MapLiteDbEntities(mapper);
                using (var db = createDb<FilterPreset>(Path.Combine(databasePath, filterPresetsDirName)))
                {
                    var col = db.GetCollection<FilterPreset>();
                    col.InsertBulk(new List<FilterPreset>
                    {
                        new FilterPreset
                        {
                            Name = "All",
                            ShowInFullscreeQuickSelection = true,
                            GroupingOrder = GroupableField.None,
                            SortingOrder = SortOrder.Name,
                            SortingOrderDirection = SortOrderDirection.Ascending,
                            Settings = new FilterSettings()
                        },
                        new FilterPreset
                        {
                            Name = "Recently Played",
                            ShowInFullscreeQuickSelection = true,
                            GroupingOrder = GroupableField.None,
                            SortingOrder = SortOrder.LastActivity,
                            SortingOrderDirection = SortOrderDirection.Descending,
                            Settings = new FilterSettings { IsInstalled = true }
                        },
                        new FilterPreset
                        {
                            Name = "Favorites",
                            ShowInFullscreeQuickSelection = true,
                            GroupingOrder = GroupableField.None,
                            SortingOrder = SortOrder.Name,
                            SortingOrderDirection = SortOrderDirection.Ascending,
                            Settings = new FilterSettings { Favorite = true }
                        },
                        new FilterPreset
                        {
                            Name = "Most Played",
                            ShowInFullscreeQuickSelection = true,
                            GroupingOrder = GroupableField.None,
                            SortingOrder = SortOrder.Playtime,
                            SortingOrderDirection = SortOrderDirection.Descending,
                            Settings = new FilterSettings()
                        }
                    });
                }

                // Convert import exclusion list
                ImportExclusionsCollection.MapLiteDbEntities(mapper);
                using (var db = createDb<CompletionStatus>(Path.Combine(databasePath, importExclusionsDirName)))
                {
                    var listPath = Path.Combine(PlaynitePaths.ConfigRootPath, "exclusionList.json");
                    if (File.Exists(listPath))
                    {
                        Ver2_ImportExclusionList exclusionList = null;
                        try
                        {
                            exclusionList = Serialization.FromJsonFile<Ver2_ImportExclusionList>(listPath);
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to load old exclusion list.");
                        }

                        if (exclusionList != null)
                        {
                            var col = db.GetCollection<ImportExclusionItem>();
                            col.Insert(exclusionList.Items.Select(a => new ImportExclusionItem(a.GameId, a.GameName, a.LibraryId, a.LibraryName)));
                            File.Delete(listPath);
                        }
                    }
                }

                var dirToConvert = new string[] {
                    "games", "platforms", "emulators", "genres", "companies", "tags", "features",
                    "categories", "series", "ageratings", "regions", "sources", "tools" };
                foreach (var dir in Directory.GetDirectories(databasePath))
                {
                    switch (Path.GetFileName(dir))
                    {
                        case "games":
                            GameAction convertAction(Ver2_GameAction oldAction)
                            {
                                var newAction = new GameAction
                                {
                                    Name = oldAction.Name,
                                    Type = (GameActionType)oldAction.Type,
                                    AdditionalArguments = oldAction.AdditionalArguments,
                                    Arguments = oldAction.Arguments,
                                    EmulatorId = oldAction.EmulatorId,
                                    OverrideDefaultArgs = oldAction.OverrideDefaultArgs,
                                    Path = oldAction.Path,
                                    WorkingDir = oldAction.WorkingDir
                                };

                                if (oldAction.EmulatorProfileId != Guid.Empty)
                                {
                                    newAction.EmulatorProfileId = CustomEmulatorProfile.ProfilePrefix + oldAction.EmulatorProfileId;
                                }

                                return newAction;
                            }

                            string convertScript(string source, Ver2_ScriptLanguage runtime)
                            {
                                if (source.IsNullOrWhiteSpace())
                                {
                                    return null;
                                }

                                if (runtime == Ver2_ScriptLanguage.PowerShell)
                                {
                                    return source;
                                }
                                else if (runtime == Ver2_ScriptLanguage.Batch)
                                {
                                    source = "$scriptPath = (Join-Path $env:TEMP 'playniteScript.bat')\n@\"\n" + source + "\n\"@ | Out-File $scriptPath -Encoding ascii\n";
                                    source = source + "Start-Process \"cmd.exe\" \"/c $scriptPath\" -Wait";
                                    source = "# Batch support has been removed in Playnite 9\n# This conversion was automatically generated\n" + source;
                                    return source;
                                }
                                else
                                {
                                    source = "throw \"IronPython support has been removed in Playnite 9\"\n" + source;
                                    source = source.Replace("\n", "\n#");
                                    return source;
                                }
                            }

                            GamesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Game, Game>(dir, (oldGame, newGame) =>
                            {
                                newGame.BackgroundImage = oldGame.BackgroundImage;
                                newGame.Description = oldGame.Description;
                                newGame.Notes = oldGame.Notes;
                                newGame.GenreIds = oldGame.GenreIds;
                                newGame.Hidden = oldGame.Hidden;
                                newGame.Favorite = oldGame.Favorite;
                                newGame.Icon = oldGame.Icon;
                                newGame.CoverImage = oldGame.CoverImage;
                                newGame.InstallDirectory = oldGame.InstallDirectory;
                                newGame.LastActivity = oldGame.LastActivity;
                                newGame.SortingName = oldGame.SortingName;
                                newGame.GameId = oldGame.GameId;
                                newGame.PluginId = oldGame.PluginId;
                                newGame.PublisherIds = oldGame.PublisherIds;
                                newGame.DeveloperIds = oldGame.DeveloperIds;
                                newGame.CategoryIds = oldGame.CategoryIds;
                                newGame.TagIds = oldGame.TagIds;
                                newGame.FeatureIds = oldGame.FeatureIds;
                                newGame.IsInstalled = oldGame.IsInstalled;
                                newGame.Playtime = (ulong)oldGame.Playtime;
                                newGame.Added = oldGame.Added;
                                newGame.Modified = oldGame.Modified;
                                newGame.PlayCount = (ulong)oldGame.PlayCount;
                                newGame.Version = oldGame.Version;
                                newGame.SourceId = oldGame.SourceId;
                                newGame.UserScore = oldGame.UserScore;
                                newGame.CommunityScore = oldGame.CommunityScore;
                                newGame.CriticScore = oldGame.CriticScore;
                                newGame.UseGlobalGameStartedScript = oldGame.UseGlobalGameStartedScript;
                                newGame.UseGlobalPostScript = oldGame.UseGlobalPostScript;
                                newGame.UseGlobalPreScript = oldGame.UseGlobalPreScript;
                                newGame.Manual = oldGame.Manual;

                                newGame.CompletionStatusId = convertedCompStatuses[(int)oldGame.CompletionStatus].Id;
                                newGame.PreScript = convertScript(oldGame.PreScript, oldGame.ActionsScriptLanguage);
                                newGame.PostScript = convertScript(oldGame.PostScript, oldGame.ActionsScriptLanguage);
                                newGame.GameStartedScript = convertScript(oldGame.GameStartedScript, oldGame.ActionsScriptLanguage);

                                var allActions = new List<GameAction>();
                                if (oldGame.PlayAction != null)
                                {
                                    newGame.IncludeLibraryPluginAction = oldGame.PlayAction.IsHandledByPlugin;
                                    if (!oldGame.PlayAction.IsHandledByPlugin)
                                    {
                                        var playAction = convertAction(oldGame.PlayAction);
                                        playAction.Name = "Play";
                                        playAction.IsPlayAction = true;
                                        allActions.Add(playAction);
                                    }
                                }

                                oldGame.OtherActions?.Where(a => a != null).ForEach(a => allActions.Add(convertAction(a)));
                                if (allActions.HasItems())
                                {
                                    newGame.GameActions = allActions.ToObservable();
                                }

                                if (!oldGame.GameImagePath.IsNullOrEmpty())
                                {
                                    string romName = null;
                                    try
                                    {
                                        romName = Path.GetFileNameWithoutExtension(oldGame.GameImagePath);
                                    }
                                    catch (Exception e)
                                    {
                                        // This sometimes crashes on weird ROM paths
                                        logger.Error(e, $"Failed to get rom name from {oldGame.GameImagePath}");
                                    }

                                    newGame.Roms = new ObservableCollection<GameRom> { new GameRom(romName ?? oldGame.Name, oldGame.GameImagePath) };
                                }

                                if (oldGame.ReleaseDate != null)
                                {
                                    newGame.ReleaseDate = new ReleaseDate(oldGame.ReleaseDate.Value);
                                }

                                if (oldGame.PlatformId != Guid.Empty)
                                {
                                    newGame.PlatformIds = new List<Guid> { oldGame.PlatformId };
                                }

                                if (oldGame.SeriesId != Guid.Empty)
                                {
                                    newGame.SeriesIds = new List<Guid> { oldGame.SeriesId };
                                }

                                if (oldGame.AgeRatingId != Guid.Empty)
                                {
                                    newGame.AgeRatingIds = new List<Guid> { oldGame.AgeRatingId };
                                }

                                if (oldGame.RegionId != Guid.Empty)
                                {
                                    newGame.RegionIds = new List<Guid> { oldGame.RegionId };
                                }

                                if (oldGame.Links.HasItems())
                                {
                                    newGame.Links = oldGame.Links.Where(a => a != null).Select(a => new Link(a.Name, a.Url)).ToObservable();
                                }
                            });
                            break;

                        case "platforms":
                            PlatformsCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Platform, Platform>(dir, (oldPlat, newPlat) =>
                            {
                                newPlat.Icon = oldPlat.Icon;
                                newPlat.Cover = oldPlat.Cover;
                                newPlat.Background = oldPlat.Background;
                                if (newPlat.Name == "PC")
                                {
                                    newPlat.Name = "PC (Windows)";
                                }
                                else if (newPlat.Name == "DOS")
                                {
                                    newPlat.Name = "PC (DOS)";
                                }

                                var platSpec = Emulation.Platforms.FirstOrDefault(a => a.Name.Equals(newPlat.Name, StringComparison.OrdinalIgnoreCase));
                                if (platSpec != null)
                                {
                                    newPlat.SpecificationId = platSpec.Id;
                                }
                            });
                            break;

                        case "emulators":
                            EmulatorsCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Emulator, Emulator>(dir, (oldEmu, newEmu) =>
                            {
                                if (!oldEmu.Profiles.HasItems())
                                {
                                    return;
                                }

                                newEmu.CustomProfiles = new ObservableCollection<CustomEmulatorProfile>();
                                foreach (var oldProfile in oldEmu.Profiles)
                                {
                                    newEmu.CustomProfiles.Add(new CustomEmulatorProfile
                                    {
                                        Id = CustomEmulatorProfile.ProfilePrefix + oldProfile.Id,
                                        Name = oldProfile.Name,
                                        Platforms = oldProfile.Platforms,
                                        ImageExtensions = oldProfile.ImageExtensions,
                                        Executable = oldProfile.Executable,
                                        Arguments = oldProfile.Arguments,
                                        WorkingDirectory = oldProfile.WorkingDirectory
                                    });
                                }

                                if (!newEmu.CustomProfiles[0].WorkingDirectory.IsNullOrEmpty())
                                {
                                    newEmu.InstallDir = newEmu.CustomProfiles[0].WorkingDirectory;
                                }
                            });
                            break;

                        case "genres":
                            GenresCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Genre, Genre>(dir);
                            break;

                        case "companies":
                            CompaniesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Company, Company>(dir);
                            break;

                        case "tags":
                            TagsCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Tag, Tag>(dir);
                            break;

                        case "features":
                            FeaturesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_GameFeature, GameFeature>(dir);
                            break;

                        case "categories":
                            CategoriesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Category, Category>(dir);
                            break;

                        case "series":
                            SeriesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Series, Series>(dir);
                            break;

                        case "ageratings":
                            AgeRatingsCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_AgeRating, AgeRating>(dir);
                            break;

                        case "regions":
                            RegionsCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_Region, Region>(dir, (oldRegion, newRegion) =>
                            {
                                var regSpec = Emulation.Regions.FirstOrDefault(a => a.Name.Equals(newRegion.Name, StringComparison.OrdinalIgnoreCase));
                                if (regSpec != null)
                                {
                                    newRegion.SpecificationId = regSpec.Id;
                                }
                            });
                            break;

                        case "sources":
                            GamesSourcesCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_GameSource, GameSource>(dir);
                            break;

                        case "tools":
                            AppSoftwareCollection.MapLiteDbEntities(mapper);
                            convertList<Ver2_AppSoftware, AppSoftware>(dir, (oldApp, newApp) =>
                            {
                                newApp.Arguments = oldApp.Arguments;
                                newApp.Icon = oldApp.Icon;
                                newApp.Path = oldApp.Path;
                                newApp.WorkingDir = oldApp.WorkingDir;
                            });
                            break;
                    }
                }

                foreach (var dir in Directory.GetDirectories(databasePath))
                {
                    if (!dirToConvert.Contains(Path.GetFileName(dir)))
                    {
                        continue;
                    }

                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to delete old database files.");
                    }
                }

                dbSettings.Version = 3;
                SaveSettingsToDbPath(dbSettings, databasePath);
            }

            // 3 to 4
            // No data format change, only to cleanup mess caused by bug #2618
            if (dbSettings.Version == 3 && NewFormatVersion > 3)
            {
                var filesDir = Path.Combine(databasePath, filesDirName);
                foreach (var dir in Directory.GetDirectories(filesDir))
                {
                    try
                    {
                        Directory.GetFiles(dir, "*.exe").ForEach(a =>
                        {
                            // Only delete files named as guid as those are 99% made by 2618 bug
                            // People sometimes put foreign files into libary folder :|, so we don't want to delete something else.
                            if (Guid.TryParse(Path.GetFileNameWithoutExtension(a), out var _))
                            {
                                File.Delete(a);
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Failed to delete file.");
                    }
                }

                dbSettings.Version = 4;
                SaveSettingsToDbPath(dbSettings, databasePath);
            }
        }

        public static bool GetMigrationRequired(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new ArgumentNullException(nameof(databasePath));
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
