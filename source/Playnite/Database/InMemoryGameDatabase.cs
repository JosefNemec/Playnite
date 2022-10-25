using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Playnite.Database
{
    public class InMemoryItemCollection<TItem> : ItemCollection<TItem> where TItem : DatabaseObject
    {
        public InMemoryItemCollection() : base(null, false)
        {
        }
    }

    public class InMemoryGameDatabase : IGameDatabaseMain
    {
        public IItemCollection<Game> Games { get; } = new InMemoryItemCollection<Game>();
        public IItemCollection<Platform> Platforms { get; } = new InMemoryItemCollection<Platform>();
        public IItemCollection<Emulator> Emulators { get; } = new InMemoryItemCollection<Emulator>();
        public IItemCollection<Genre> Genres { get; } = new InMemoryItemCollection<Genre>();
        public IItemCollection<Company> Companies { get; } = new InMemoryItemCollection<Company>();
        public IItemCollection<Tag> Tags { get; } = new InMemoryItemCollection<Tag>();
        public IItemCollection<Category> Categories { get; } = new InMemoryItemCollection<Category>();
        public IItemCollection<Series> Series { get; } = new InMemoryItemCollection<Series>();
        public IItemCollection<AgeRating> AgeRatings { get; } = new InMemoryItemCollection<AgeRating>();
        public IItemCollection<Region> Regions { get; } = new InMemoryItemCollection<Region>();
        public IItemCollection<GameSource> Sources { get; } = new InMemoryItemCollection<GameSource>();
        public IItemCollection<GameFeature> Features { get; } = new InMemoryItemCollection<GameFeature>();
        public IItemCollection<GameScannerConfig> GameScanners { get; } = new InMemoryItemCollection<GameScannerConfig>();
        public IItemCollection<CompletionStatus> CompletionStatuses => new InMemoryItemCollection<CompletionStatus>();
        public IItemCollection<ImportExclusionItem> ImportExclusions => new InMemoryItemCollection<ImportExclusionItem>();
        public IItemCollection<FilterPreset> FilterPresets => new InMemoryItemCollection<FilterPreset>();
        public bool IsOpen => true;

        public AppSoftwareCollection SoftwareApps => throw new NotImplementedException();

        public List<Guid> UsedPlatforms => throw new NotImplementedException();

        public List<Guid> UsedGenres => throw new NotImplementedException();

        public List<Guid> UsedDevelopers => throw new NotImplementedException();

        public List<Guid> UsedPublishers => throw new NotImplementedException();

        public List<Guid> UsedTags => throw new NotImplementedException();

        public List<Guid> UsedCategories => throw new NotImplementedException();

        public List<Guid> UsedSeries => throw new NotImplementedException();

        public List<Guid> UsedAgeRatings => throw new NotImplementedException();

        public List<Guid> UsedRegions => throw new NotImplementedException();

        public List<Guid> UsedSources => throw new NotImplementedException();

        public List<Guid> UsedFeastures => throw new NotImplementedException();

        public List<Guid> UsedCompletionStatuses => throw new NotImplementedException();

#pragma warning disable CS0067
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
#pragma warning restore CS0067

        public InMemoryGameDatabase()
        {
        }

        public Game ImportGame(GameMetadata game)
        {
            throw new NotImplementedException();
        }

        public Game ImportGame(GameMetadata game, LibraryPlugin sourcePlugin)
        {
            throw new NotImplementedException();
        }

        public void SetDatabasePath(string path)
        {
            throw new NotImplementedException();
        }

        public void OpenDatabase()
        {
        }

        public string GetFileStoragePath(Guid parentId)
        {
            throw new NotImplementedException();
        }

        public string GetFullFilePath(string dbPath)
        {
            throw new NotImplementedException();
        }

        public string AddFile(MetadataFile file, Guid parentId, bool isImage)
        {
            throw new NotImplementedException();
        }

        public string AddFile(string path, Guid parentId, bool isImage)
        {
            throw new NotImplementedException();
        }

        public void RemoveFile(string dbPath)
        {
            throw new NotImplementedException();
        }

        public BitmapImage GetFileAsImage(string dbPath, BitmapLoadProperties loadProperties = null)
        {
            throw new NotImplementedException();
        }

        public void CopyFile(string dbPath, string targetPath)
        {
            throw new NotImplementedException();
        }

        public void BeginBufferUpdate()
        {
            throw new NotImplementedException();
        }

        public void EndBufferUpdate()
        {
            throw new NotImplementedException();
        }

        public IDisposable BufferedUpdate()
        {
            throw new NotImplementedException();
        }

        public List<Game> ImportGames(LibraryPlugin library, CancellationToken cancelToken, PlaytimeImportMode playtimeImportMode)
        {
            throw new NotImplementedException();
        }

        public CompletionStatusSettings GetCompletionStatusSettings()
        {
            throw new NotImplementedException();
        }

        public void SetCompletionStatusSettings(CompletionStatusSettings settings)
        {
            throw new NotImplementedException();
        }

        public GameScannersSettings GetGameScannersSettings()
        {
            throw new NotImplementedException();
        }

        public void SetGameScannersSettings(GameScannersSettings settings)
        {
            throw new NotImplementedException();
        }

        public HashSet<string> GetImportedRomFiles(string emulatorDir)
        {
            throw new NotImplementedException();
        }

        public bool GetGameMatchesFilter(Game game, FilterPresetSettings filterSettings)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Game> GetFilteredGames(FilterPresetSettings filterSettings)
        {
            throw new NotImplementedException();
        }

        public bool GetGameMatchesFilter(Game game, FilterSettings filterSettings)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Game> GetFilteredGames(FilterSettings filterSettings)
        {
            throw new NotImplementedException();
        }
    }
}
