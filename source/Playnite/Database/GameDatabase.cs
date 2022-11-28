using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Playnite.Emulators;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Common;
using Playnite.Settings;
using Playnite.SDK.Plugins;
using System.Net;
using Playnite.Common.Web;
using System.Drawing.Imaging;
using System.Threading;
using System.Collections.Concurrent;
using Playnite.Common.Media.Icons;
using System.Reflection;
using SdkModels = Playnite.SDK.Models;

namespace Playnite.Database
{
    /// Not the greatest way of doing it but it's a superset of SDK exposed IGameDatabase.
    /// The whole interface should not be exposed in the SDK since it contains a lot of "internal" members.
    public interface IGameDatabaseMain : IGameDatabase
    {
        List<Guid> UsedPlatforms { get; }
        List<Guid> UsedGenres { get; }
        List<Guid> UsedDevelopers { get; }
        List<Guid> UsedPublishers { get; }
        List<Guid> UsedTags { get; }
        List<Guid> UsedCategories { get; }
        List<Guid> UsedSeries { get; }
        List<Guid> UsedAgeRatings { get; }
        List<Guid> UsedRegions { get; }
        List<Guid> UsedSources { get; }
        List<Guid> UsedFeastures { get; }
        List<Guid> UsedCompletionStatuses { get; }

        AppSoftwareCollection SoftwareApps { get; }

        event EventHandler<DatabaseFileEventArgs> DatabaseFileChanged;
        event EventHandler PlatformsInUseUpdated;
        event EventHandler GenresInUseUpdated;
        event EventHandler DevelopersInUseUpdated;
        event EventHandler PublishersInUseUpdated;
        event EventHandler TagsInUseUpdated;
        event EventHandler CategoriesInUseUpdated;
        event EventHandler AgeRatingsInUseUpdated;
        event EventHandler SeriesInUseUpdated;
        event EventHandler RegionsInUseUpdated;
        event EventHandler SourcesInUseUpdated;
        event EventHandler FeaturesInUseUpdated;
        event EventHandler CompletionStatusesInUseUpdated;

        void SetDatabasePath(string path);
        void OpenDatabase();
        string GetFileStoragePath(Guid parentId);
        string GetFullFilePath(string dbPath);
        string AddFile(MetadataFile file, Guid parentId, bool isImage);
        string AddFile(string path, Guid parentId, bool isImage);
        void RemoveFile(string dbPath);
        BitmapImage GetFileAsImage(string dbPath, BitmapLoadProperties loadProperties = null);
        void CopyFile(string dbPath, string targetPath);
        void BeginBufferUpdate();
        void EndBufferUpdate();
        IDisposable BufferedUpdate();
        List<Game> ImportGames(LibraryPlugin library, CancellationToken cancelToken, PlaytimeImportMode playtimeImportMode);
        CompletionStatusSettings GetCompletionStatusSettings();
        void SetCompletionStatusSettings(CompletionStatusSettings settings);
        GameScannersSettings GetGameScannersSettings();
        void SetGameScannersSettings(GameScannersSettings settings);
        HashSet<string> GetImportedRomFiles(string emulatorDir);
        bool GetGameMatchesFilter(Game game, FilterSettings filterSettings);
        IEnumerable<Game> GetFilteredGames(FilterSettings filterSettings);
    }

    public partial class GameDatabase : IGameDatabaseMain, IDisposable
    {
        public const double MaximumRecommendedIconSize = 0.1;
        public const double MaximumRecommendedCoverSize = 1;
        public const double MaximumRecommendedBackgroundSize = 4;

        private static ILogger logger = LogManager.GetLogger();

        private static readonly Dictionary<string, Type> collectionsSpec = new Dictionary<string, Type>
        {
            { nameof(Platforms), typeof(PlatformsCollection) },
            { nameof(Emulators), typeof(EmulatorsCollection) },
            { nameof(Genres), typeof(GenresCollection) },
            { nameof(Companies), typeof(CompaniesCollection) },
            { nameof(Tags), typeof(TagsCollection) },
            { nameof(Categories), typeof(CategoriesCollection) },
            { nameof(AgeRatings), typeof(AgeRatingsCollection) },
            { nameof(Series), typeof(SeriesCollection) },
            { nameof(Regions), typeof(RegionsCollection) },
            { nameof(Sources), typeof(GamesSourcesCollection) },
            { nameof(Features), typeof(FeaturesCollection) },
            { nameof(SoftwareApps), typeof(AppSoftwareCollection) },
            { nameof(Games), typeof(GamesCollection) },
            { nameof(GameScanners), typeof(GameScannersCollection) },
            { nameof(FilterPresets), typeof(FilterPresetsCollection) },
            { nameof(ImportExclusions), typeof(ImportExclusionsCollection) },
            { nameof(CompletionStatuses), typeof(CompletionStatusesCollection) }
        };

        #region Locks

        private readonly object databaseConfigFileLock = new object();
        private readonly ConcurrentDictionary<string, object> fileLocks = new ConcurrentDictionary<string, object>();

        #endregion Locks

        #region Paths

        public string DatabasePath
        {
            get; private set;
        }

        internal const string filesDirName = "files";
        private const string settingsFileName = "database.json";
        private const string gamesDirName = "games";
        private const string platformsDirName = "platforms";
        private const string emulatorsDirName = "emulators";
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
        private const string gameScannersDirName = "scanners";
        private const string filterPresetsDirName = "filterpresets";
        private const string importExclusionsDirName = "importexclusions";
        private const string completionStatusesDirName = "completionstatuses";

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
        private string GameScannersDirectoryPath { get => Path.Combine(DatabasePath, gameScannersDirName); }
        private string FilterPresetsDirectoryPath { get => Path.Combine(DatabasePath, filterPresetsDirName); }
        private string ImportExclusionsDirectoryPath { get => Path.Combine(DatabasePath, importExclusionsDirName); }
        private string CompletionStatusesDirectoryPath { get => Path.Combine(DatabasePath, completionStatusesDirName); }

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
        public IItemCollection<GameScannerConfig> GameScanners { get; private set; }
        public IItemCollection<FilterPreset> FilterPresets { get; private set; }
        public IItemCollection<ImportExclusionItem> ImportExclusions { get; private set; }
        public IItemCollection<CompletionStatus> CompletionStatuses { get; private set; }

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
        public List<Guid> UsedCompletionStatuses { get; } = new List<Guid>();

        #endregion Lists

        public static GameDatabase Instance { get; private set; }

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

        public static readonly ushort NewFormatVersion = 4;

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
        public event EventHandler CompletionStatusesInUseUpdated;

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
                (GameScanners as GameScannersCollection).InitializeCollection(GameScannersDirectoryPath);
                (FilterPresets as FilterPresetsCollection).InitializeCollection(FilterPresetsDirectoryPath);
                (ImportExclusions as ImportExclusionsCollection).InitializeCollection(ImportExclusionsDirectoryPath);
                (CompletionStatuses as CompletionStatusesCollection).InitializeCollection(CompletionStatusesDirectoryPath);

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
                CompletionStatuses.ItemCollectionChanged += CompletionStatuses_ItemCollectionChanged;
            }
        }

        private void LoadUsedItems()
        {
            foreach (var game in Games)
            {
                if (game.PlatformIds.HasItems())
                {
                    UsedPlatforms.AddMissing(game.PlatformIds.Where(a => Platforms.ContainsItem(a)));
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

                if (game.SeriesIds.HasItems())
                {
                    UsedSeries.AddMissing(game.SeriesIds.Where(a => Series.ContainsItem(a)));
                }

                if (game.AgeRatingIds.HasItems())
                {
                    UsedAgeRatings.AddMissing(game.AgeRatingIds.Where(a => AgeRatings.ContainsItem(a)));
                }

                if (game.RegionIds.HasItems())
                {
                    UsedRegions.AddMissing(game.RegionIds.Where(a => Regions.ContainsItem(a)));
                }

                if (game.SourceId != Guid.Empty && Sources.ContainsItem(game.SourceId))
                {
                    UsedSources.AddMissing(game.SourceId);
                }

                if (game.FeatureIds.HasItems())
                {
                    UsedFeastures.AddMissing(game.FeatureIds.Where(a => Features.ContainsItem(a)));
                }

                if (game.CompletionStatusId != Guid.Empty && CompletionStatuses.ContainsItem(game.CompletionStatusId))
                {
                    UsedCompletionStatuses.AddMissing(game.CompletionStatusId);
                }
            }
        }

        #endregion Intialization

        public GameDatabase() : this(null)
        {
        }

        public static LiteDB.BsonMapper GetCollectionMapper()
        {
            var mapper = new LiteDB.BsonMapper()
            {
                SerializeNullValues = false,
                TrimWhitespace = false,
                EmptyStringToNull = true,
                IncludeFields = false,
                IncludeNonPublic = false
            };

            foreach (var col in collectionsSpec)
            {
                col.Value.
                    GetMethod(nameof(GamesCollection.MapLiteDbEntities), BindingFlags.Public | BindingFlags.Static).
                    Invoke(null, new object[] { mapper });
            }

            return mapper;
        }

        public GameDatabase(string path)
        {
            var mapper = GetCollectionMapper();
            DatabasePath = GetFullDbPath(path);

            foreach (var col in collectionsSpec)
            {
                var collection = Activator.CreateInstance(col.Value, this, mapper);
                typeof(GameDatabase).GetProperty(col.Key).SetValue(this, collection);
            }
        }

        public void Dispose()
        {
            if (!IsOpen)
            {
                return;
            }

            foreach (var col in collectionsSpec)
            {
                var prop = typeof(GameDatabase).GetProperty(col.Key);
                typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)).Invoke(prop.GetValue(this), null);
            }
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
                var platforms = Emulation.Platforms.Where(a => a.IgdbId != 0).Select(a => new Platform(a.Name) { SpecificationId = a.Id }).ToList();
                if (platforms.HasItems())
                {
                    var col = Platforms as ItemCollection<Platform>;
                    col.IsEventsEnabled = false;
                    col.Add(platforms);
                    col.IsEventsEnabled = true;
                }

                // Generate default regions
                var regions = Emulation.Regions.Where(a => a.DefaultImport).Select(a => new Region(a.Name) { SpecificationId = a.Id }).ToList();
                if (regions.HasItems())
                {
                    var col = Regions as ItemCollection<Region>;
                    col.IsEventsEnabled = false;
                    col.Add(regions);
                    col.IsEventsEnabled = true;
                }

                // Generate default completion statuses
                var compCol = CompletionStatuses as CompletionStatusesCollection;
                var defStatuses = new string[] { "Not Played", "Played", "Beaten", "Completed", "Playing", "Abandoned", "On Hold", "Plan to Play" };
                foreach (var status in defStatuses)
                {
                    compCol.IsEventsEnabled = false;
                    compCol.Add(status);
                    compCol.IsEventsEnabled = true;
                }

                var set = new CompletionStatusSettings
                {
                    DefaultStatus = compCol.First(a => a.Name == defStatuses[0]).Id,
                    PlayedStatus = compCol.First(a => a.Name == defStatuses[1]).Id
                };

                compCol.SetSettings(set);

                // Generate default filter presets
                var filters = FilterPresets as FilterPresetsCollection;
                filters.IsEventsEnabled = false;
                filters.Add(new FilterPreset
                {
                    Name = "All",
                    ShowInFullscreeQuickSelection = true,
                    GroupingOrder = GroupableField.None,
                    SortingOrder = SortOrder.Name,
                    SortingOrderDirection = SortOrderDirection.Ascending,
                    Settings = new FilterPresetSettings()
                });

                filters.Add(new FilterPreset
                {
                    Name = "Recently Played",
                    ShowInFullscreeQuickSelection = true,
                    GroupingOrder = GroupableField.None,
                    SortingOrder = SortOrder.LastActivity,
                    SortingOrderDirection = SortOrderDirection.Descending,
                    Settings = new FilterPresetSettings { IsInstalled = true }
                });

                filters.Add(new FilterPreset
                {
                    Name = "Favorites",
                    ShowInFullscreeQuickSelection = true,
                    GroupingOrder = GroupableField.None,
                    SortingOrder = SortOrder.Name,
                    SortingOrderDirection = SortOrderDirection.Ascending,
                    Settings = new FilterPresetSettings { Favorite = true }
                });

                filters.Add(new FilterPreset
                {
                    Name = "Most Played",
                    ShowInFullscreeQuickSelection = true,
                    GroupingOrder = GroupableField.None,
                    SortingOrder = SortOrder.Playtime,
                    SortingOrderDirection = SortOrderDirection.Descending,
                    Settings = new FilterPresetSettings()
                });

                filters.IsEventsEnabled = true;
            }

            IsOpen = true;
            if (PlayniteApplication.Current != null)
            {
                PlayniteApplication.Current.SyncContext.Send((_) => DatabaseOpened?.Invoke(this, null), null);
            }
            else
            {
                DatabaseOpened?.Invoke(this, null);
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            if (e.AddedItems.HasItems())
            {
                foreach (var game in e.AddedItems)
                {
                    UpdateFieldsInUse(game.PlatformIds, UsedPlatforms, PlatformsInUseUpdated, Platforms);
                    UpdateFieldsInUse(game.GenreIds, UsedGenres, GenresInUseUpdated, Genres);
                    UpdateFieldsInUse(game.DeveloperIds, UsedDevelopers, DevelopersInUseUpdated, Companies);
                    UpdateFieldsInUse(game.PublisherIds, UsedPublishers, PublishersInUseUpdated, Companies);
                    UpdateFieldsInUse(game.TagIds, UsedTags, TagsInUseUpdated, Tags);
                    UpdateFieldsInUse(game.CategoryIds, UsedCategories, CategoriesInUseUpdated, Categories);
                    UpdateFieldsInUse(game.AgeRatingIds, UsedAgeRatings, AgeRatingsInUseUpdated, AgeRatings);
                    UpdateFieldsInUse(game.SeriesIds, UsedSeries, SeriesInUseUpdated, Series);
                    UpdateFieldsInUse(game.RegionIds, UsedRegions, RegionsInUseUpdated, Regions);
                    UpdateFieldsInUse(game.SourceId, UsedSources, SourcesInUseUpdated, Sources);
                    UpdateFieldsInUse(game.FeatureIds, UsedFeastures, FeaturesInUseUpdated, Features);
                    UpdateFieldsInUse(game.CompletionStatusId, UsedCompletionStatuses, CompletionStatusesInUseUpdated, CompletionStatuses);
                }
            }
        }

        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            foreach (var upd in e.UpdatedItems)
            {
                UpdateFieldsInUse(upd.NewData.PlatformIds, UsedPlatforms, PlatformsInUseUpdated, Platforms);
                UpdateFieldsInUse(upd.NewData.GenreIds, UsedGenres, GenresInUseUpdated, Genres);
                UpdateFieldsInUse(upd.NewData.DeveloperIds, UsedDevelopers, DevelopersInUseUpdated, Companies);
                UpdateFieldsInUse(upd.NewData.PublisherIds, UsedPublishers, PublishersInUseUpdated, Companies);
                UpdateFieldsInUse(upd.NewData.TagIds, UsedTags, TagsInUseUpdated, Tags);
                UpdateFieldsInUse(upd.NewData.CategoryIds, UsedCategories, CategoriesInUseUpdated, Categories);
                UpdateFieldsInUse(upd.NewData.AgeRatingIds, UsedAgeRatings, AgeRatingsInUseUpdated, AgeRatings);
                UpdateFieldsInUse(upd.NewData.SeriesIds, UsedSeries, SeriesInUseUpdated, Series);
                UpdateFieldsInUse(upd.NewData.RegionIds, UsedRegions, RegionsInUseUpdated, Regions);
                UpdateFieldsInUse(upd.NewData.SourceId, UsedSources, SourcesInUseUpdated, Sources);
                UpdateFieldsInUse(upd.NewData.FeatureIds, UsedFeastures, FeaturesInUseUpdated, Features);
                UpdateFieldsInUse(upd.NewData.CompletionStatusId, UsedCompletionStatuses, CompletionStatusesInUseUpdated, CompletionStatuses);
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
                if (useCollection.AddMissing(sourceData.Where(a => dbItems.ContainsItem(a)).ToArray()))
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

        private void CompletionStatuses_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<CompletionStatus> e)
        {
            UpdateRemovedFieldsInUse(e.RemovedItems, UsedCompletionStatuses, CompletionStatusesInUseUpdated);
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

        public string AddFile(MetadataFile file, Guid parentId, bool isImage)
        {
            if (!file.HasImageData)
            {
                logger.Error("Cannot add file to database, no file data provided.");
                return null;
            }

            string localPath = null;
            try
            {
                localPath = file.GetLocalFile(CancellationToken.None);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get local file from metadata file");
            }

            if (localPath.IsNullOrEmpty())
            {
                return null;
            }

            var finalFile = AddFile(localPath, parentId, isImage);
            if (localPath.StartsWith(PlaynitePaths.TempPath))
            {
                FileSystem.DeleteFile(localPath);
            }

            return finalFile;
        }

        public string AddFile(string path, Guid parentId, bool isImage)
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
                    var downPath = Path.Combine(targetDir, fileName);
                    HttpDownloader.DownloadFile(path, downPath);
                    if (isImage)
                    {
                        var converted = Images.ConvertToCompatibleFormat(downPath, Path.Combine(targetDir, Path.GetFileNameWithoutExtension(fileName)));
                        if (converted.IsNullOrEmpty())
                        {
                            FileSystem.DeleteFile(downPath);
                            return null;
                        }
                        else if (converted == downPath)
                        {
                            dbPath = Path.Combine(parentId.ToString(), fileName);
                        }
                        else
                        {
                            dbPath = Path.Combine(parentId.ToString(), Path.GetFileName(converted));
                            FileSystem.DeleteFile(downPath);
                        }
                    }
                    else
                    {
                        dbPath = Path.Combine(parentId.ToString(), fileName);
                    }
                }
                catch (WebException e)
                {
                    logger.Error(e, $"Failed to add http {path} file to database.");
                    return null;
                }
            }
            else
            {
                try
                {
                    var fileName = Path.GetFileName(path);
                    // Re-use file if already part of db folder, don't copy.
                    if (Paths.AreEqual(targetDir, Path.GetDirectoryName(path)))
                    {
                        dbPath = Path.Combine(parentId.ToString(), fileName);
                    }
                    else
                    {
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
                        if (isImage)
                        {
                            var converted = Images.ConvertToCompatibleFormat(path, Path.Combine(targetDir, Path.GetFileNameWithoutExtension(fileName)));
                            if (converted.IsNullOrEmpty())
                            {
                                return null;
                            }
                            else if (converted == path)
                            {
                                FileSystem.CopyFile(path, Path.Combine(targetDir, fileName));
                                dbPath = Path.Combine(parentId.ToString(), fileName);
                            }
                            else
                            {
                                dbPath = Path.Combine(parentId.ToString(), Path.GetFileName(converted));
                            }
                        }
                        else
                        {
                            FileSystem.CopyFile(path, Path.Combine(targetDir, fileName));
                            dbPath = Path.Combine(parentId.ToString(), fileName);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to add {path} file to database.");
                    return null;
                }
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
            GameScanners.BeginBufferUpdate();
            FilterPresets.BeginBufferUpdate();
            ImportExclusions.BeginBufferUpdate();
            CompletionStatuses.BeginBufferUpdate();
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
            GameScanners.EndBufferUpdate();
            FilterPresets.EndBufferUpdate();
            ImportExclusions.EndBufferUpdate();
            CompletionStatuses.EndBufferUpdate();
        }

        public IDisposable BufferedUpdate()
        {
            return new EventBufferHandler(this);
        }

        private Game GameInfoToGame(GameMetadata game, Guid pluginId)
        {
            var toAdd = new Game()
            {
                PluginId = pluginId,
                Name = game.Name,
                GameId = game.GameId,
                Description = game.Description,
                InstallDirectory = game.InstallDirectory,
                SortingName = game.SortingName,
                GameActions = game.GameActions == null ? null : new ObservableCollection<GameAction>(game.GameActions),
                ReleaseDate = game.ReleaseDate,
                Links = game.Links == null ? null : new ObservableCollection<Link>(game.Links),
                Roms = game.Roms == null ? null : new ObservableCollection<GameRom>(game.Roms),
                IsInstalled = game.IsInstalled,
                Playtime = game.Playtime,
                PlayCount = game.PlayCount,
                LastActivity = game.LastActivity,
                Version = game.Version,
                UserScore = game.UserScore,
                CriticScore = game.CriticScore,
                CommunityScore = game.CommunityScore,
                Hidden = game.Hidden,
                Favorite = game.Favorite,
                InstallSize = game.InstallSize
            };

            if (game.Platforms?.Any() == true)
            {
                toAdd.PlatformIds = Platforms.Add(game.Platforms).Select(a => a.Id).ToList();
            }

            if (game.Regions?.Any() == true)
            {
                toAdd.RegionIds = Regions.Add(game.Regions).Select(a => a.Id).ToList();
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

            if (game.AgeRatings?.Any() == true)
            {
                toAdd.AgeRatingIds = AgeRatings.Add(game.AgeRatings).Select(a => a.Id).ToList();
            }

            if (game.Series?.Any() == true)
            {
                toAdd.SeriesIds = Series.Add(game.Series).Select(a => a.Id).ToList();
            }

            if (game.Source != null)
            {
                toAdd.SourceId = Sources.Add(game.Source).Id;
            }

            if (game.CompletionStatus != null)
            {
                toAdd.CompletionStatusId = CompletionStatuses.Add(game.CompletionStatus).Id;
            }

            return toAdd;
        }

        public Game ImportGame(GameMetadata game)
        {
            return ImportGame(game, Guid.Empty);
        }

        public Game ImportGame(GameMetadata game, LibraryPlugin sourcePlugin)
        {
            return ImportGame(game, sourcePlugin.Id);
        }

        public Game ImportGame(GameMetadata game, Guid pluginId)
        {
            var toAdd = GameInfoToGame(game, pluginId);

            if (game.Icon != null)
            {
                toAdd.Icon = AddFile(game.Icon, toAdd.Id, true);
            }

            if (game.CoverImage != null)
            {
                toAdd.CoverImage = AddFile(game.CoverImage, toAdd.Id, true);
            }

            if (game.BackgroundImage != null)
            {
                toAdd.BackgroundImage = AddFile(game.BackgroundImage, toAdd.Id, true);
            }

            toAdd.IncludeLibraryPluginAction = true;
            Games.Add(toAdd);
            return toAdd;
        }

        public List<Game> ImportGames(LibraryPlugin library, CancellationToken cancelToken, PlaytimeImportMode playtimeImportMode)
        {
            using (BufferedUpdate())
            {
                var statusSettings = GetCompletionStatusSettings();
                bool updateCompletionStatus(Game game, CompletionStatusSettings settings)
                {
                    var updated = false;
                    if ((game.Playtime > 0 && (game.CompletionStatusId == Guid.Empty || game.CompletionStatusId == settings.DefaultStatus)) &&
                        game.CompletionStatusId != statusSettings.PlayedStatus)
                    {
                        game.CompletionStatusId = statusSettings.PlayedStatus;
                        updated = true;
                    }
                    else if ((game.Playtime == 0 && game.CompletionStatusId == Guid.Empty) &&
                        game.CompletionStatusId != statusSettings.DefaultStatus)
                    {
                        game.CompletionStatusId = statusSettings.DefaultStatus;
                        updated = true;
                    }

                    return updated;
                }

                if (library.Properties?.HasCustomizedGameImport == true)
                {
                    var importedGames = library.ImportGames(new LibraryImportGamesArgs { CancelToken = cancelToken })?.ToList() ?? new List<Game>();
                    foreach (var game in importedGames)
                    {
                        updateCompletionStatus(game, statusSettings);
                    }

                    return importedGames;
                }
                else
                {
                    var addedGames = new List<Game>();
                    foreach (var newGame in library.GetGames(new LibraryGetGamesArgs { CancelToken = cancelToken }) ?? new List<GameMetadata>())
                    {
                        if (ImportExclusions[ImportExclusionItem.GetId(newGame.GameId, library.Id)] != null)
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
                                if (newGame.Playtime != 0)
                                {
                                    var originalPlaytime = newGame.Playtime;
                                    newGame.Playtime = 0;
                                    if (playtimeImportMode == PlaytimeImportMode.Always ||
                                        playtimeImportMode == PlaytimeImportMode.NewImportsOnly)
                                    {
                                        newGame.Playtime = originalPlaytime;
                                    }
                                }

                                var importedGame = ImportGame(newGame, library.Id);
                                addedGames.Add(importedGame);
                                if (updateCompletionStatus(importedGame, statusSettings))
                                {
                                    Games.Update(importedGame);
                                }
                            }
                            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                            {
                                logger.Error(e, "Failed to import game into database.");
                            }
                        }
                        else
                        {
                            var existingGameUpdated = false;
                            if (!existingGame.IsCustomGame && !existingGame.OverrideInstallState)
                            {
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
                            }

                            if (playtimeImportMode == PlaytimeImportMode.Always && newGame.Playtime > 0)
                            {
                                if (existingGame.Playtime != newGame.Playtime)
                                {
                                    existingGame.Playtime = newGame.Playtime;
                                    existingGameUpdated = true;
                                }

                                if (existingGame.LastActivity == null && newGame.LastActivity != null)
                                {
                                    existingGame.LastActivity = newGame.LastActivity;
                                    existingGameUpdated = true;
                                }

                                if (updateCompletionStatus(existingGame, statusSettings))
                                {
                                    existingGameUpdated = true;
                                }
                            }

                            if (!existingGame.IsInstalled && newGame.InstallSize != null && newGame.InstallSize > 0 &&
                                existingGame.InstallSize != newGame.InstallSize)
                            {
                                existingGame.InstallSize = newGame.InstallSize;
                                existingGameUpdated = true;
                            }

                            if (existingGameUpdated)
                            {
                                Games.Update(existingGame);
                            }
                        }
                    }

                    return addedGames;
                }
            }
        }

        public CompletionStatusSettings GetCompletionStatusSettings()
        {
            return (CompletionStatuses as CompletionStatusesCollection).GetSettings();
        }

        public void SetCompletionStatusSettings(CompletionStatusSettings settings)
        {
            (CompletionStatuses as CompletionStatusesCollection).SetSettings(settings);
        }

        public GameScannersSettings GetGameScannersSettings()
        {
            return (GameScanners as GameScannersCollection).GetSettings();
        }

        public void SetGameScannersSettings(GameScannersSettings settings)
        {
            (GameScanners as GameScannersCollection).SetSettings(settings);
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
                ReleaseDate = new ReleaseDate(2009, 9, 5),
                PlatformIds = new List<Guid> { database.Platforms.First().Id },
                PlayCount = 20,
                Playtime = 115200,
                LastActivity = DateTime.Today,
                IsInstalled = true,
                AgeRatingIds =  new List<Guid> { database.AgeRatings.First().Id },
                CategoryIds = new List<Guid> { database.Categories.First().Id },
                DeveloperIds = new List<Guid> { database.Companies.First().Id },
                PublisherIds = new List<Guid> { database.Companies.Last().Id },
                GenreIds = new List<Guid> { database.Genres.First().Id },
                RegionIds =  new List<Guid> { database.Regions.First().Id },
                SeriesIds = new List<Guid> { database.Series.First().Id },
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

        public HashSet<string> GetImportedRomFiles(string emulatorDir)
        {
            var importedRoms = new HashSet<string>();
            foreach (var game in Games.Where(a => a.Roms.HasItems()))
            {
                try
                {
                    foreach (var rom in game.Roms)
                    {
                        if (rom.Path.IsNullOrWhiteSpace() || rom.Name.IsNullOrWhiteSpace())
                        {
                            continue;
                        }

                        var path = game.ExpandVariables(rom.Path, true, emulatorDir).ToLowerInvariant();
                        string absPath = null;
                        try
                        {
                            absPath = Path.GetFullPath(path);
                        }
                        catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(e, $"Failed to get absolute ROM path:\n{rom.Path}\n{path}");
                        }

                        if (!absPath.IsNullOrEmpty())
                        {
                            importedRoms.Add(absPath);
                        }
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to get roms from a game.");
                    logger.Debug(Serialization.ToJson(game.Roms));
                }
            }

            return importedRoms;
        }

        public void SetAsSingletonInstance()
        {
            if (Instance != null)
            {
                throw new Exception("Database singleton intance already exists.");
            }

            Instance = this;
        }
    }
}
