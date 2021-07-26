﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Metadata;
using Playnite.Common;
using Playnite.Settings;
using Playnite.SDK.Plugins;
using System.Net;
using Playnite.Common.Web;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections.Concurrent;
using Playnite.Common.Media.Icons;

namespace Playnite.Database
{
    public partial class GameDatabase : IGameDatabase
    {
        public const double MaximumRecommendedIconSize = 0.1;
        public const double MaximumRecommendedCoverSize = 1;
        public const double MaximumRecommendedBackgroundSize = 4;

        private static ILogger logger = LogManager.GetLogger();

        #region Locks

        private readonly object databaseConfigFileLock = new object();
        private readonly ConcurrentDictionary<string, object> fileLocks = new ConcurrentDictionary<string, object>();

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
        private const string featuresDirName = "features";
        private const string categoriesDirName = "categories";
        private const string seriesDirName = "series";
        private const string ageRatingsDirName = "ageratings";
        private const string regionsDirName = "regions";
        private const string sourcesDirName = "sources";
        private const string toolsDirName = "tools";
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
        private string FeaturesDirectoryPath { get => Path.Combine(DatabasePath, featuresDirName); }
        private string ToolsDirectoryPath { get => Path.Combine(DatabasePath, toolsDirName); }

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
        public IItemCollection<GameFeature> Features { get; private set; }
        public AppSoftwareCollection SoftwareApps { get; private set; }

        public List<Guid> UsedPlatforms { get; } = new List<Guid>();
        public List<Guid> UsedGenres { get; } = new List<Guid>();
        public List<Guid> UsedDevelopers { get; } = new List<Guid>();
        public List<Guid> UsedPublishers { get; } = new List<Guid>();
        public List<Guid> UsedTags { get; } = new List<Guid>();
        public List<Guid> UsedCategories { get; } = new List<Guid>();
        public List<Guid> UsedSeries { get; } = new List<Guid>();
        public List<Guid> UsedAgeRatings { get; } = new List<Guid>();
        public List<Guid> UsedRegions { get; } = new List<Guid>();
        public List<Guid> UsedSources { get; } = new List<Guid>();
        public List<Guid> UsedFeastures { get; } = new List<Guid>();

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
        public event EventHandler<DatabaseFileEventArgs> DatabaseFileChanged;

        public event EventHandler PlatformsInUseUpdated;
        public event EventHandler GenresInUseUpdated;
        public event EventHandler DevelopersInUseUpdated;
        public event EventHandler PublishersInUseUpdated;
        public event EventHandler TagsInUseUpdated;
        public event EventHandler CategoriesInUseUpdated;
        public event EventHandler AgeRatingsInUseUpdated;
        public event EventHandler SeriesInUseUpdated;
        public event EventHandler RegionsInUseUpdated;
        public event EventHandler SourcesInUseUpdated;
        public event EventHandler FeaturesInUseUpdated;

        #endregion Events

        #region Initialization

        private void LoadCollections()
        {
            using (var timer = new ExecutionTimer("DatabaseLoadCollections"))
            {
                (Platforms as PlatformsCollection).InitializeCollection(PlatformsDirectoryPath);
                (Emulators as EmulatorsCollection).InitializeCollection(EmulatorsDirectoryPath);
                (Genres as GenresCollection).InitializeCollection(GenresDirectoryPath);
                (Companies as CompaniesCollection).InitializeCollection(CompaniesDirectoryPath);
                (Tags as TagsCollection).InitializeCollection(TagsDirectoryPath);
                (Categories as CategoriesCollection).InitializeCollection(CategoriesDirectoryPath);
                (AgeRatings as AgeRatingsCollection).InitializeCollection(AgeRatingsDirectoryPath);
                (Series as SeriesCollection).InitializeCollection(SeriesDirectoryPath);
                (Regions as RegionsCollection).InitializeCollection(RegionsDirectoryPath);
                (Sources as GamesSourcesCollection).InitializeCollection(SourcesDirectoryPath);
                (Features as FeaturesCollection).InitializeCollection(FeaturesDirectoryPath);
                (Games as GamesCollection).InitializeCollection(GamesDirectoryPath);
                SoftwareApps.InitializeCollection(ToolsDirectoryPath);

                Games.ItemUpdated += Games_ItemUpdated;
                Games.ItemCollectionChanged += Games_ItemCollectionChanged;
                Platforms.ItemCollectionChanged += Platforms_ItemCollectionChanged;
                Genres.ItemCollectionChanged += Genres_ItemCollectionChanged;
                Companies.ItemCollectionChanged += Companies_ItemCollectionChanged;
                Tags.ItemCollectionChanged += Tags_ItemCollectionChanged;
                Categories.ItemCollectionChanged += Categories_ItemCollectionChanged;
                AgeRatings.ItemCollectionChanged += AgeRatings_ItemCollectionChanged;
                Series.ItemCollectionChanged += Series_ItemCollectionChanged;
                Regions.ItemCollectionChanged += Regions_ItemCollectionChanged;
                Sources.ItemCollectionChanged += Sources_ItemCollectionChanged;
                Features.ItemCollectionChanged += Features_ItemCollectionChanged;
            }
        }

        private void LoadUsedItems()
        {
            foreach (var game in Games)
            {
                if (game.PlatformId != Guid.Empty && Platforms.ContainsItem(game.PlatformId))
                {
                    UsedPlatforms.AddMissing(game.PlatformId);
                }

                if (game.GenreIds.HasItems())
                {
                    UsedGenres.AddMissing(game.GenreIds.Where(a => Genres.ContainsItem(a)));
                }

                if (game.DeveloperIds.HasItems())
                {
                    UsedDevelopers.AddMissing(game.DeveloperIds.Where(a => Companies.ContainsItem(a)));
                }

                if (game.PublisherIds.HasItems())
                {
                    UsedPublishers.AddMissing(game.PublisherIds.Where(a => Companies.ContainsItem(a)));
                }

                if (game.TagIds.HasItems())
                {
                    UsedTags.AddMissing(game.TagIds.Where(a => Tags.ContainsItem(a)));
                }

                if (game.CategoryIds.HasItems())
                {
                    UsedCategories.AddMissing(game.CategoryIds.Where(a => Categories.ContainsItem(a)));
                }

                if (game.SeriesId != Guid.Empty && Series.ContainsItem(game.SeriesId))
                {
                    UsedSeries.AddMissing(game.SeriesId);
                }

                if (game.AgeRatingId != Guid.Empty && AgeRatings.ContainsItem(game.AgeRatingId))
                {
                    UsedAgeRatings.AddMissing(game.AgeRatingId);
                }

                if (game.RegionId != Guid.Empty && Regions.ContainsItem(game.RegionId))
                {
                    UsedRegions.AddMissing(game.RegionId);
                }

                if (game.SourceId != Guid.Empty && Sources.ContainsItem(game.SourceId))
                {
                    UsedSources.AddMissing(game.SourceId);
                }

                if (game.FeatureIds.HasItems())
                {
                    UsedFeastures.AddMissing(game.FeatureIds.Where(a => Features.ContainsItem(a)));
                }
            }
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
            Features = new FeaturesCollection(this);
            SoftwareApps = new AppSoftwareCollection(this);
        }

        public static string GetDefaultPath(bool portable)
        {
            if (portable)
            {
                return ExpandableVariables.PlayniteDirectory + @"\library";
            }
            else
            {
                return @"%AppData%\Playnite\library";
            }
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

        public static string GetFullDbPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.Contains(ExpandableVariables.PlayniteDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return path?.Replace(ExpandableVariables.PlayniteDirectory, PlaynitePaths.ProgramPath);
            }
            else if (path.Contains("%AppData%", StringComparison.OrdinalIgnoreCase))
            {
                return path?.Replace("%AppData%", Environment.ExpandEnvironmentVariables("%AppData%"), StringComparison.OrdinalIgnoreCase);
            }
            else if (!Paths.IsFullPath(path))
            {
                return Path.GetFullPath(path);
            }
            else
            {
                return path;
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

            if (!FileSystem.CanWriteToFolder(DatabasePath))
            {
                throw new Exception($"Can't write to \"{DatabasePath}\" folder.");
            }

            // This fixes an issue where people mess up their library with custom scripts
            // which create collection files instead of directories :|
            if (File.Exists(FilesDirectoryPath))
            {
                File.Delete(FilesDirectoryPath);
            }

            if (!dbExists)
            {
                FileSystem.CreateDirectory(DatabasePath);
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
            LoadUsedItems();

            // New DB setup
            if (!dbExists)
            {
                // Generate default platforms
                var platforms = EmulatorDefinition.GetDefinitions()
                    .SelectMany(a => a.Profiles.SelectMany(b => b.Platforms)).Distinct()
                    .Select(a => new Platform(a)).ToList();
                if (platforms.HasItems())
                {
                    var col = Platforms as ItemCollection<Platform>;
                    col.IsEventsEnabled = false;
                    col.Add(platforms);
                    col.IsEventsEnabled = true;
                }
            }

            IsOpen = true;
            DatabaseOpened?.Invoke(this, null);
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (e.AddedItems.HasItems())
            {
                foreach (var game in e.AddedItems)
                {
                    UpdateFieldsInUse(game.PlatformId, UsedPlatforms, PlatformsInUseUpdated, Platforms);
                    UpdateFieldsInUse(game.GenreIds, UsedGenres, GenresInUseUpdated, Genres);
                    UpdateFieldsInUse(game.DeveloperIds, UsedDevelopers, DevelopersInUseUpdated, Companies);
                    UpdateFieldsInUse(game.PublisherIds, UsedPublishers, PublishersInUseUpdated, Companies);
                    UpdateFieldsInUse(game.TagIds, UsedTags, TagsInUseUpdated, Tags);
                    UpdateFieldsInUse(game.CategoryIds, UsedCategories, CategoriesInUseUpdated, Categories);
                    UpdateFieldsInUse(game.AgeRatingId, UsedAgeRatings, AgeRatingsInUseUpdated, AgeRatings);
                    UpdateFieldsInUse(game.SeriesId, UsedSeries, SeriesInUseUpdated, Series);
                    UpdateFieldsInUse(game.RegionId, UsedRegions, RegionsInUseUpdated, Regions);
                    UpdateFieldsInUse(game.SourceId, UsedSources, SourcesInUseUpdated, Sources);
                    UpdateFieldsInUse(game.FeatureIds, UsedFeastures, FeaturesInUseUpdated, Features);
                }
            }
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            foreach (var upd in e.UpdatedItems)
            {
                UpdateFieldsInUse(upd.NewData.PlatformId, UsedPlatforms, PlatformsInUseUpdated, Platforms);
                UpdateFieldsInUse(upd.NewData.GenreIds, UsedGenres, GenresInUseUpdated, Genres);
                UpdateFieldsInUse(upd.NewData.DeveloperIds, UsedDevelopers, DevelopersInUseUpdated, Companies);
                UpdateFieldsInUse(upd.NewData.PublisherIds, UsedPublishers, PublishersInUseUpdated, Companies);
                UpdateFieldsInUse(upd.NewData.TagIds, UsedTags, TagsInUseUpdated, Tags);
                UpdateFieldsInUse(upd.NewData.CategoryIds, UsedCategories, CategoriesInUseUpdated, Categories);
                UpdateFieldsInUse(upd.NewData.AgeRatingId, UsedAgeRatings, AgeRatingsInUseUpdated, AgeRatings);
                UpdateFieldsInUse(upd.NewData.SeriesId, UsedSeries, SeriesInUseUpdated, Series);
                UpdateFieldsInUse(upd.NewData.RegionId, UsedRegions, RegionsInUseUpdated, Regions);
                UpdateFieldsInUse(upd.NewData.SourceId, UsedSources, SourcesInUseUpdated, Sources);
                UpdateFieldsInUse(upd.NewData.FeatureIds, UsedFeastures, FeaturesInUseUpdated, Features);
            }
        }

        private void UpdateFieldsInUse(Guid sourceData, List<Guid> useCollection, EventHandler handler, IItemCollection dbItems)
        {
            if (sourceData != Guid.Empty && dbItems.ContainsItem(sourceData))
            {
                if (useCollection.AddMissing(sourceData))
                {
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateFieldsInUse(List<Guid> sourceData, List<Guid> useCollection, EventHandler handler, IItemCollection dbItems)
        {
            if (sourceData.HasItems())
            {
                if (useCollection.AddMissing(sourceData.Where(a => dbItems.ContainsItem(a))))
                {
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateRemovedFieldsInUse<T>(List<T> removedObjects, List<Guid> useCollection, EventHandler handler) where T : DatabaseObject
        {
            if (removedObjects.HasItems())
            {
                var someRemoved = false;
                foreach (var item in removedObjects)
                {
                    if (useCollection.Remove(item.Id))
                    {
                        someRemoved = true;
                    }
                }

                if (someRemoved)
                {
                    handler?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void Features_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<GameFeature> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedFeastures, FeaturesInUseUpdated);
        }

        private void Sources_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<GameSource> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedSources, SourcesInUseUpdated);
        }

        private void Regions_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Region> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedRegions, RegionsInUseUpdated);
        }

        private void Series_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Series> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedSeries, SeriesInUseUpdated);
        }

        private void AgeRatings_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<AgeRating> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedAgeRatings, AgeRatingsInUseUpdated);
        }

        private void Categories_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Category> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedCategories, CategoriesInUseUpdated);
        }

        private void Tags_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Tag> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedTags, TagsInUseUpdated);
        }

        private void Companies_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Company> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedDevelopers, DevelopersInUseUpdated);
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedPublishers, PublishersInUseUpdated);
        }

        private void Genres_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Genre> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedGenres, GenresInUseUpdated);
        }

        private void Platforms_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Platform> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedPlatforms, PlatformsInUseUpdated);
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
            if (file.HasContent)
            {
                return AddFile(file.FileName, file.Content, parentId);
            }
            else if (!file.OriginalUrl.IsNullOrEmpty())
            {
                return AddFile(file.OriginalUrl, parentId);
            }
            else
            {
                throw new Exception("Cannot add file, no file data provided.");
            }
        }

        public string AddFile(string path, Guid parentId)
        {
            CheckDbState();
            var targetDir = Path.Combine(FilesDirectoryPath, parentId.ToString());
            var dbPath = string.Empty;

            if (path.IsHttpUrl())
            {
                try
                {
                    var extension = Path.GetExtension(new Uri(path).AbsolutePath);
                    var fileName = Guid.NewGuid().ToString() + extension;
                    HttpDownloader.DownloadFile(path, Path.Combine(targetDir, fileName));
                    dbPath = Path.Combine(parentId.ToString(), fileName);
                }
                catch (WebException e)
                {
                    logger.Error(e, $"Failed to add http {path} file to database.");
                    return null;
                }
            }
            else
            {
                var fileName = Path.GetFileName(path);
                // Re-use file if already part of db folder, don't copy.
                if (Paths.AreEqual(targetDir, Path.GetDirectoryName(path)))
                {
                    dbPath = Path.Combine(parentId.ToString(), fileName);
                }
                else
                {
                    try
                    {
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
                        FileSystem.CopyFile(path, Path.Combine(targetDir, fileName));
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"Failed to copy file {path} to database.");
                        return null;
                    }

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
            FileSystem.PrepareSaveFile(targetPath);
            File.WriteAllBytes(targetPath, content);
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

            try
            {
                lock (GetFileLock(dbPath))
                {
                    try
                    {
                        FileSystem.DeleteFileSafe(filePath);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to remove old database file {dbPath}.");
                    }

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
            }
            finally
            {
                ReleaseFileLock(dbPath);
            }

            DatabaseFileChanged?.Invoke(this, new DatabaseFileEventArgs(dbPath, FileEvent.Removed));
        }

        public BitmapImage GetFileAsImage(string dbPath, BitmapLoadProperties loadProperties = null)
        {
            CheckDbState();
            var filePath = GetFullFilePath(dbPath);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                lock (GetFileLock(dbPath))
                {
                    using (var fStream = FileSystem.OpenReadFileStreamSafe(filePath))
                    {
                        return BitmapExtensions.BitmapFromStream(fStream, loadProperties);
                    }
                }
            }
            finally
            {
                ReleaseFileLock(dbPath);
            }
        }

        public void CopyFile(string dbPath, string targetPath)
        {
            CheckDbState();
            var filePath = GetFullFilePath(dbPath);

            try
            {
                lock (GetFileLock(dbPath))
                {
                    FileSystem.PrepareSaveFile(targetPath);
                    File.Copy(filePath, targetPath);
                }
            }
            finally
            {
                ReleaseFileLock(dbPath);
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
            Genres.BeginBufferUpdate();
            Companies.BeginBufferUpdate();
            Tags.BeginBufferUpdate();
            Categories.BeginBufferUpdate();
            Series.BeginBufferUpdate();
            AgeRatings.BeginBufferUpdate();
            Regions.BeginBufferUpdate();
            Sources.BeginBufferUpdate();
            Emulators.BeginBufferUpdate();
            Features.BeginBufferUpdate();
            Games.BeginBufferUpdate();
            SoftwareApps.BeginBufferUpdate();
        }

        public void EndBufferUpdate()
        {
            Platforms.EndBufferUpdate();
            Genres.EndBufferUpdate();
            Companies.EndBufferUpdate();
            Tags.EndBufferUpdate();
            Categories.EndBufferUpdate();
            Series.EndBufferUpdate();
            AgeRatings.EndBufferUpdate();
            Regions.EndBufferUpdate();
            Sources.EndBufferUpdate();
            Emulators.EndBufferUpdate();
            Features.EndBufferUpdate();
            Games.EndBufferUpdate();
            SoftwareApps.EndBufferUpdate();
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

            var fileName = Guid.NewGuid().ToString();
            if (path.IsHttpUrl())
            {
                var url = new Uri(path);
                fileName += Path.GetExtension(url.AbsolutePath);
            }
            else
            {
                fileName += Path.GetExtension(path);
            }

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
                            using (var ms = new MemoryStream())
                            {
                                if (IconExtractor.ExtractMainIconFromFile(path, ms))
                                {
                                    fileName = Path.ChangeExtension(fileName, ".ico");
                                    metaFile = new MetadataFile(fileName, ms.ToArray());
                                }
                            }
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
                    var tga = BitmapExtensions.TgaToBitmap(metaFile.Content);
                    if (tga == null)
                    {
                        return null;
                    }
                    else
                    {
                        metaFile.Content = tga.ToPngArray();
                    }
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
                CommunityScore = game.CommunityScore,
                Hidden = game.Hidden,
                Favorite = game.Favorite
            };

            if (!game.Platform.IsNullOrEmpty())
            {
                toAdd.PlatformId = Platforms.Add(game.Platform).Id;
            }

            if (game.Developers?.Any() == true)
            {
                toAdd.DeveloperIds = Companies.Add(game.Developers).Select(a => a.Id).ToList();
            }

            if (game.Publishers?.Any() == true)
            {
                toAdd.PublisherIds = Companies.Add(game.Publishers).Select(a => a.Id).ToList();
            }

            if (game.Genres?.Any() == true)
            {
                toAdd.GenreIds = Genres.Add(game.Genres).Select(a => a.Id).ToList();
            }

            if (game.Categories?.Any() == true)
            {
                toAdd.CategoryIds = Categories.Add(game.Categories).Select(a => a.Id).ToList();
            }

            if (game.Tags?.Any() == true)
            {
                toAdd.TagIds = Tags.Add(game.Tags).Select(a => a.Id).ToList();
            }

            if (game.Features?.Any() == true)
            {
                toAdd.FeatureIds = Features.Add(game.Features).Select(a => a.Id).ToList();
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

        public Game ImportGame(GameInfo game, LibraryPlugin sourcePlugin)
        {
            return ImportGame(game, sourcePlugin.Id);
        }

        public Game ImportGame(GameInfo game, Guid pluginId)
        {
            var toAdd = GameInfoToGame(game, pluginId);
            toAdd.Icon = AddNewGameFile(game.Icon, toAdd.Id);
            toAdd.CoverImage = AddNewGameFile(game.CoverImage, toAdd.Id);
            toAdd.BackgroundImage = AddNewGameFile(game.BackgroundImage, toAdd.Id);
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

        public List<Game> ImportGames(LibraryPlugin library, bool forcePlayTimeSync, IList<ImportExclusionItem> excludedItems)
        {
            if (library.Capabilities?.HasCustomizedGameImport == true)
            {
                return library.ImportGames()?.ToList() ?? new List<Game>();
            }
            else
            {
                var addedGames = new List<Game>();
                foreach (var newGame in library.GetGames())
                {
                    if (excludedItems.Any(a => a.GameId == newGame.GameId && a.LibraryId == library.Id))
                    {
                        logger.Debug($"Excluding {newGame.Name} {library.Name} from import.");
                        continue;
                    }

                    var existingGame = Games.FirstOrDefault(a => a.GameId == newGame.GameId && a.PluginId == library.Id);
                    if (existingGame == null)
                    {
                        logger.Info(string.Format("Adding new game {0} from {1} plugin", newGame.GameId, library.Name));
                        try
                        {
                            addedGames.Add(ImportGame(newGame, library.Id));
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, "Failed to import game into database.");
                        }
                    }
                    else
                    {
                        var existingGameUpdated = false;
                        if (existingGame.IsInstalled != newGame.IsInstalled)
                        {
                            existingGame.IsInstalled = newGame.IsInstalled;
                            existingGameUpdated = true;
                        }

                        if (string.Equals(existingGame.InstallDirectory, newGame.InstallDirectory, StringComparison.OrdinalIgnoreCase) == false)
                        {
                            existingGame.InstallDirectory = newGame.InstallDirectory;
                            existingGameUpdated = true;
                        }
                        
                        if (existingGame.PlayAction == null || existingGame.PlayAction.IsHandledByPlugin)
                        {
                            if (existingGame.PlayAction != newGame.PlayAction)
                            {
                                existingGame.PlayAction = newGame.PlayAction;
                                existingGameUpdated = true;
                            }
                        }

                        if ((existingGame.Playtime == 0 && newGame.Playtime > 0) ||
                           (newGame.Playtime > 0 && forcePlayTimeSync))
                        {
                            if (existingGame.Playtime != newGame.Playtime)
                            {
                                existingGame.Playtime = newGame.Playtime;
                                existingGameUpdated = true;
                            }

                            if (existingGame.CompletionStatus == CompletionStatus.NotPlayed)
                            {
                                existingGame.CompletionStatus = CompletionStatus.Played;
                                existingGameUpdated = true;
                            }

                            if (existingGame.LastActivity == null && newGame.LastActivity != null)
                            {
                                existingGame.LastActivity = newGame.LastActivity;
                                existingGameUpdated = true;
                            }
                        }

                        if (existingGame.OtherActions?.Any() != true && newGame.OtherActions?.Any() == true)
                        {
                            existingGame.OtherActions = new ObservableCollection<GameAction>(newGame.OtherActions);
                            existingGameUpdated = true;
                        }

                        if (existingGameUpdated == true)
                        {
                            Games.Update(existingGame);
                        }
                    }
                }

                return addedGames;
            }
        }

        public static void GenerateSampleData(IGameDatabase database)
        {
            database.Platforms.Add("Windows");
            database.AgeRatings.Add("18+");
            database.Categories.Add("Category");
            database.Companies.Add("BioWare");
            database.Companies.Add("LucasArts");
            database.Genres.Add("RPG");
            database.Regions.Add("EU");
            database.Series.Add("Star Wars");
            database.Sources.Add("Retails");
            database.Tags.Add("Star Wars");
            database.Features.Add("Single Player");

            var designGame = new Game($"Star Wars: Knights of the Old Republic")
            {
                ReleaseDate = new DateTime(2009, 9, 5),
                PlatformId = database.Platforms.First().Id,
                PlayCount = 20,
                Playtime = 115200,
                LastActivity = DateTime.Today,
                IsInstalled = true,
                AgeRatingId = database.AgeRatings.First().Id,
                CategoryIds = new List<Guid> { database.Categories.First().Id },
                DeveloperIds = new List<Guid> { database.Companies.First().Id },
                PublisherIds = new List<Guid> { database.Companies.Last().Id },
                GenreIds = new List<Guid> { database.Genres.First().Id },
                RegionId = database.Regions.First().Id,
                SeriesId = database.Series.First().Id,
                SourceId = database.Sources.First().Id,
                TagIds = new List<Guid> { database.Tags.First().Id },
                FeatureIds = new List<Guid> { database.Features.First().Id },
                Description = "Star Wars: Knights of the Old Republic (often abbreviated as KotOR) is the first installment in the Knights of the Old Republic series. KotOR is the first computer role-playing game set in the Star Wars universe.",
                Version = "1.2",
                CommunityScore = 95,
                CriticScore = 50,
                UserScore = 15,
                Links = new ObservableCollection<Link> { new Link("Wiki", ""), new Link("HomePage", "") }
            };

            database.Games.Add(designGame);
        }

        private object GetFileLock(string filePath)
        {
            if (fileLocks.TryGetValue(filePath, out object fileLock))
            {
                return fileLock;
            }
            else
            {
                var lc = new object();
                fileLocks.TryAdd(filePath, lc);
                return lc;
            }
        }

        private void ReleaseFileLock(string filePath)
        {
            fileLocks.TryRemove(filePath, out var removed);
        }
    }
}
