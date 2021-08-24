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
        public static void MigrateNewDatabaseFormat(string databasePath)
        {
            var dbSettings = GetSettingsFromDbPath(databasePath);
            var gamesDir = Path.Combine(databasePath, gamesDirName);

            // 1 to 2
            if (dbSettings.Version == 1 && NewFormatVersion > 1)
            {
                //void convetList<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : DatabaseObject
                //{
                //    if (game.TryGetValue(origKey, out var storedObj))
                //    {
                //        if (storedObj == null)
                //        {
                //            return;
                //        }

                //        var gameObjs = new List<Guid>();
                //        var oldLIst = (storedObj as JArray).ToObject<List<string>>();
                //        foreach (var oldObj in oldLIst)
                //        {
                //            if (string.IsNullOrEmpty(oldObj))
                //            {
                //                continue;
                //            }

                //            if (convertedList.TryGetValue(oldObj, out var curObj))
                //            {
                //                if (!gameObjs.Contains(curObj.Id))
                //                {
                //                    gameObjs.Add(curObj.Id);
                //                }
                //            }
                //            else
                //            {
                //                var newObj = typeof(T).CrateInstance<T>(oldObj);
                //                gameObjs.Add(newObj.Id);
                //                convertedList.Add(oldObj, newObj);
                //            }
                //        }

                //        game.Remove(origKey);
                //        game[newKey] = gameObjs;
                //    }
                //}

                //void covertObject<T>(Dictionary<string, object> game, string origKey, string newKey, Dictionary<string, T> convertedList) where T : DatabaseObject
                //{
                //    if (game.TryGetValue(origKey, out var storedObj))
                //    {
                //        var oldObj = storedObj as string;
                //        if (!string.IsNullOrEmpty(oldObj))
                //        {
                //            if (convertedList.TryGetValue(oldObj, out var curObj))
                //            {
                //                game[newKey] = curObj.Id;
                //            }
                //            else
                //            {
                //                var newObj = typeof(T).CrateInstance<T>(oldObj);
                //                game[newKey] = newObj.Id;
                //                convertedList.Add(oldObj, newObj);
                //            }
                //        }

                //        game.Remove(origKey);
                //    }
                //}

                //void saveCollection<T>(Dictionary<string, T> collection, string collPath) where T : DatabaseObject
                //{
                //    if (collection.Any())
                //    {
                //        foreach (var item in collection.Values)
                //        {
                //            FileSystem.WriteStringToFileSafe(Path.Combine(collPath, item.Id + ".json"), Serialization.ToJson(item));
                //        }
                //    }
                //}

                //var allGenres = new Dictionary<string, Genre>(StringComparer.CurrentCultureIgnoreCase);
                //var allCompanies = new Dictionary<string, Company>(StringComparer.CurrentCultureIgnoreCase);
                //var allTags = new Dictionary<string, Tag>(StringComparer.CurrentCultureIgnoreCase);
                //var allCategories = new Dictionary<string, Category>(StringComparer.CurrentCultureIgnoreCase);
                //var allSeries = new Dictionary<string, Series>(StringComparer.CurrentCultureIgnoreCase);
                //var allRatings = new Dictionary<string, AgeRating>(StringComparer.CurrentCultureIgnoreCase);
                //var allRegions = new Dictionary<string, Region>(StringComparer.CurrentCultureIgnoreCase);
                //var allSources = new Dictionary<string, GameSource>(StringComparer.CurrentCultureIgnoreCase);

                //// Convert following object to Id representations and store them in separete lists:
                //foreach (var file in Directory.EnumerateFiles(gamesDir, "*.json"))
                //{
                //    var game = Serialization.FromJson<Dictionary<string, object>>(FileSystem.ReadFileAsStringSafe(file));
                //    if (game == null)
                //    {
                //        // Some users have 0 sized game files for uknown reason.
                //        File.Delete(file);
                //        continue;
                //    }

                //    // Genres
                //    convetList(game, nameof(OldModels.NewVer1.OldGame.Genres), nameof(Game.GenreIds), allGenres);

                //    // Developers
                //    convetList(game, nameof(OldModels.NewVer1.OldGame.Developers), nameof(Game.DeveloperIds), allCompanies);

                //    // Publishers
                //    convetList(game, nameof(OldModels.NewVer1.OldGame.Publishers), nameof(Game.PublisherIds), allCompanies);

                //    // Tags
                //    convetList(game, nameof(OldModels.NewVer1.OldGame.Tags), nameof(Game.TagIds), allTags);

                //    // Categories
                //    convetList(game, nameof(OldModels.NewVer1.OldGame.Categories), nameof(Game.CategoryIds), allCategories);

                //    // Series
                //    covertObject(game, nameof(OldModels.NewVer1.OldGame.Series), nameof(Game.SeriesId), allSeries);

                //    // AgeRating
                //    covertObject(game, nameof(OldModels.NewVer1.OldGame.AgeRating), nameof(Game.AgeRatingId), allRatings);

                //    // Region
                //    covertObject(game, nameof(OldModels.NewVer1.OldGame.Region), nameof(Game.RegionId), allRegions);

                //    // Source
                //    covertObject(game, nameof(OldModels.NewVer1.OldGame.Source), nameof(Game.SourceId), allSources);

                //    FileSystem.WriteStringToFileSafe(file, Serialization.ToJson(game));
                //}

                //saveCollection(allGenres, Path.Combine(databasePath, genresDirName));
                //saveCollection(allCompanies, Path.Combine(databasePath, companiesDirName));
                //saveCollection(allTags, Path.Combine(databasePath, tagsDirName));
                //saveCollection(allCategories, Path.Combine(databasePath, categoriesDirName));
                //saveCollection(allSeries, Path.Combine(databasePath, seriesDirName));
                //saveCollection(allRatings, Path.Combine(databasePath, ageRatingsDirName));
                //saveCollection(allRegions, Path.Combine(databasePath, regionsDirName));
                //saveCollection(allSources, Path.Combine(databasePath, sourcesDirName));

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

                void convertList<T>(string dir) where T : DatabaseObject
                {
                    mapper.Entity<T>().Id(a => a.Id, false);
                    using (var db = new LiteDatabase($"Filename={dir}.db;Mode=Exclusive;Journal=false", mapper))
                    {
                        var col = db.GetCollection<T>();
                        col.EnsureIndex(a => a.Id, true);
                        foreach (var file in Directory.GetFiles(dir, "*.json"))
                        {
                            if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out _))
                            {
                                T item = null;
                                try
                                {
                                    item = Serialization.FromJsonFile<T>(file);
                                }
                                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                                {
                                    logger.Error(e, "Failed to load database file.");
                                }

                                if (item != null)
                                {
                                    col.Insert(item);
                                }
                            }
                        }
                    }
                }

                foreach (var dir in Directory.GetDirectories(databasePath))
                {
                    if (Path.GetFileName(dir) == filesDirName)
                    {
                        continue;
                    }

                    // TODO use mapper from actuall collection
                    switch (Path.GetFileName(dir))
                    {
                        case gamesDirName:
                            mapper.Entity<Game>().
                                Ignore(a => a.Genres).
                                Ignore(a => a.Developers).
                                Ignore(a => a.Publishers).
                                Ignore(a => a.Tags).
                                Ignore(a => a.Features).
                                Ignore(a => a.Categories).
                                Ignore(a => a.Platforms).
                                Ignore(a => a.Series).
                                Ignore(a => a.AgeRatings).
                                Ignore(a => a.Regions).
                                Ignore(a => a.Source).
                                Ignore(a => a.ReleaseYear).
                                Ignore(a => a.UserScoreRating).
                                Ignore(a => a.CommunityScoreRating).
                                Ignore(a => a.CriticScoreRating).
                                Ignore(a => a.UserScoreGroup).
                                Ignore(a => a.CommunityScoreGroup).
                                Ignore(a => a.CriticScoreGroup).
                                Ignore(a => a.LastActivitySegment).
                                Ignore(a => a.AddedSegment).
                                Ignore(a => a.ModifiedSegment).
                                Ignore(a => a.PlaytimeCategory).
                                Ignore(a => a.IsCustomGame).
                                Ignore(a => a.InstallationStatus);
                            convertList<Game>(dir);
                            break;
                        case platformsDirName:
                            convertList<Platform>(dir);
                            break;
                        case emulatorsDirName:
                            convertList<Emulator>(dir);
                            break;
                        case genresDirName:
                            convertList<Genre>(dir);
                            break;
                        case companiesDirName:
                            convertList<Company>(dir);
                            break;
                        case tagsDirName:
                            convertList<Tag>(dir);
                            break;
                        case featuresDirName:
                            convertList<GameFeature>(dir);
                            break;
                        case categoriesDirName:
                            convertList<Category>(dir);
                            break;
                        case seriesDirName:
                            convertList<Series>(dir);
                            break;
                        case ageRatingsDirName:
                            convertList<AgeRating>(dir);
                            break;
                        case regionsDirName:
                            convertList<Region>(dir);
                            break;
                        case sourcesDirName:
                            convertList<GameSource>(dir);
                            break;
                        case toolsDirName:
                            convertList<AppSoftware>(dir);
                            break;
                    }
                }

                foreach (var dir in Directory.GetDirectories(databasePath))
                {
                    if (Path.GetFileName(dir) == filesDirName)
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
            if (dbSettings.Version == 3 && NewFormatVersion > 3)
            {
                throw new NotImplementedException();
                //dbSettings.Version = 4;
                //SaveSettingsToDbPath(dbSettings, databasePath);
                //migrate exclusions
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
