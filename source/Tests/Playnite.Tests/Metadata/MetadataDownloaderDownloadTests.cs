using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.Metadata;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Tests.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests.Metadata
{
    [TestFixture]
    public class MetadataDownloaderDownloadTests
    {
        public class OnDemandTestMetadataProvider : OnDemandMetadataProvider
        {
            public override List<MetadataField> AvailableFields => availableFields;

            private List<MetadataField> availableFields;
            private GameMetadata metadata;

            public OnDemandTestMetadataProvider(ref GameMetadata metadata, ref List<MetadataField> availableFields)
            {
                this.metadata = metadata;
                this.availableFields = availableFields;
            }

            public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
            {
                return metadata.BackgroundImage;
            }

            public override int? GetCommunityScore(GetMetadataFieldArgs args)
            {
                return metadata.CommunityScore;
            }

            public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
            {
                return metadata.CoverImage;
            }

            public override int? GetCriticScore(GetMetadataFieldArgs args)
            {
                return metadata.CriticScore;
            }

            public override string GetDescription(GetMetadataFieldArgs args)
            {
                return metadata.Description;
            }

            public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
            {
                return metadata.Developers;
            }

            public override IEnumerable<MetadataProperty> GetGenres(GetMetadataFieldArgs args)
            {
                return metadata.Genres;
            }

            public override MetadataFile GetIcon(GetMetadataFieldArgs args)
            {
                return metadata.Icon;
            }

            public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
            {
                return metadata.Links;
            }

            public override string GetName(GetMetadataFieldArgs args)
            {
                return metadata.Name;
            }

            public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
            {
                return metadata.Publishers;
            }

            public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
            {
                return metadata.ReleaseDate;
            }

            public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
            {
                return metadata.Tags;
            }

            public override IEnumerable<MetadataProperty> GetFeatures(GetMetadataFieldArgs args)
            {
                return metadata.Features;
            }

            public override IEnumerable<MetadataProperty> GetAgeRatings(GetMetadataFieldArgs args)
            {
                return metadata.AgeRatings;
            }

            public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
            {
                return metadata.Platforms;
            }

            public override IEnumerable<MetadataProperty> GetRegions(GetMetadataFieldArgs args)
            {
                return metadata.Regions;
            }

            public override IEnumerable<MetadataProperty> GetSeries(GetMetadataFieldArgs args)
            {
                return metadata.Series;
            }

            public override ulong? GetInstallSize(GetMetadataFieldArgs args)
            {
                return metadata.InstallSize;
            }
        }

        public class TestMetadataPlugin : MetadataPlugin
        {
            public int CallCount { get; set; } = 0;
            public const string DataString = "plugin";
            public override string Name => "TestMetadataPlugin";
            public override Guid Id { get; } = Guid.NewGuid();
            private List<MetadataField> supportedFields;
            public override List<MetadataField> SupportedFields => supportedFields;
            public GameMetadata ReturnMetadata = new GameMetadata
            {
                Description = DataString
            };

            public TestMetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
            {
            }

            public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
            {
                CallCount++;
                return new OnDemandTestMetadataProvider(ref ReturnMetadata, ref supportedFields);
            }

            public void SetSupportedFields(List<MetadataField> fields)
            {
                supportedFields = fields;
            }
        }

        public class TestLibraryMetadataProvider : LibraryMetadataProvider
        {
            public const string DataString = "store";
            public int CallCount = 0;
            public GameMetadata ReturnMetadata { get; set; } = new GameMetadata
            {
                Description = DataString
            };

            public override GameMetadata GetMetadata(Game game)
            {
                CallCount++;
                return ReturnMetadata;
            }
        }

        private Random random = new Random();
        private byte[] randomFile
        {
            get
            {
                var b = new byte[20];
                random.NextBytes(b);
                return b;
            }
        }

        private TestMetadataPlugin GetTestPlugin()
        {
            return new TestMetadataPlugin(null);
        }

        private TestLibraryMetadataProvider GetTestLibraryProvider()
        {
            return new TestLibraryMetadataProvider();
        }

        [Test]
        public void ProcessFieldTest()
        {
            var storeId = Guid.NewGuid();
            var storeDownloader = GetTestLibraryProvider();
            var testPlugin = GetTestPlugin();

            List<MetadataPlugin> metadataDownloaders = new List<MetadataPlugin>()
            {
                testPlugin
            };

            Dictionary<Guid, LibraryMetadataProvider> libraryDownloaders = new Dictionary<Guid, LibraryMetadataProvider>()
            {
                { storeId, storeDownloader }
            };

            var existingMetadata = new Dictionary<Guid, GameMetadata>();
            var existingPluginData = new Dictionary<Guid, OnDemandMetadataProvider>();
            var fieldSettings = new MetadataFieldSettings();
            var downloader = new MetadataDownloader(null, metadataDownloaders, libraryDownloaders);
            var game = new Game();
            var cancelToken = new CancellationTokenSource();

            // Store is not called if custom game
            var downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.IsNull(downloadedMetadata);
            Assert.AreEqual(0, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(0, existingMetadata.Count);

            // Store download works
            game.PluginId = storeId;
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.Description);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Already downloaded data are reqused
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Store is still used and plugin is not called
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty, testPlugin.Id }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.Description);
            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Plugin is not used if not supporting field
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty, testPlugin.Id }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.Description);
            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Plugin data is used
            testPlugin.SetSupportedFields(new List<MetadataField> { MetadataField.Description });
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { testPlugin.Id, Guid.Empty }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.AreEqual(TestMetadataPlugin.DataString, downloadedMetadata.Description);
            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(1, testPlugin.CallCount);
            Assert.AreEqual(1, existingPluginData.Count);

            // Not data are returned if specific fields doesn't have them
            testPlugin.ReturnMetadata.Description = null;
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { testPlugin.Id }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.IsNull(downloadedMetadata);

            // Second data are used if first doesn't have them
            testPlugin.ReturnMetadata.Description = string.Empty;
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { testPlugin.Id, Guid.Empty }),
                MetadataField.Description,
                existingMetadata,
                existingPluginData,
                cancelToken.Token);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.Description);
        }

        [Test]
        public async Task MissingDataTest()
        {
            // Tests that existing data are not overriden by empty metadata from external providers.
            var storeId = Guid.NewGuid();
            var storeDownloader = GetTestLibraryProvider();
            storeDownloader.ReturnMetadata = new GameMetadata();
            var testPlugin = GetTestPlugin();
            testPlugin.ReturnMetadata = new GameMetadata();
            testPlugin.SetSupportedFields(new List<MetadataField>
            {
                MetadataField.Description,
                MetadataField.Icon,
                MetadataField.CoverImage,
                MetadataField.BackgroundImage,
                MetadataField.Links,
                MetadataField.Publishers,
                MetadataField.Developers,
                MetadataField.Tags,
                MetadataField.Genres,
                MetadataField.ReleaseDate,
                MetadataField.Features,
                MetadataField.InstallSize
            });

            List<MetadataPlugin> metadataDownloaders = new List<MetadataPlugin>()
            {
                testPlugin
            };

            Dictionary<Guid, LibraryMetadataProvider> libraryDownloaders = new Dictionary<Guid, LibraryMetadataProvider>()
            {
                { storeId, storeDownloader }
            };

            using (var temp = TempDirectory.Create())
            using (var db = new GameDatabase(temp.TempPath))
            {
                db.OpenDatabase();
                Game.DatabaseReference = db;

                var importedGame = db.ImportGame(new GameMetadata()
                {
                    Name = "Game",
                    GameId = "storeId",
                    Genres = new HashSet<MetadataProperty> { new MetadataNameProperty("Genre") },
                    ReleaseDate = new ReleaseDate(2012, 6, 6),
                    Developers = new HashSet<MetadataProperty> { new MetadataNameProperty("Developer") },
                    Publishers = new HashSet<MetadataProperty> { new MetadataNameProperty("Publisher") },
                    Tags = new HashSet<MetadataProperty> { new MetadataNameProperty("Tag") },
                    Features = new HashSet<MetadataProperty> { new MetadataNameProperty("Feature") },
                    Description = "Description",
                    Links = new List<Link> { new Link() },
                    InstallSize = 1000
                });

                importedGame.PluginId = storeId;
                importedGame.Icon = "icon";
                importedGame.CoverImage = "image";
                importedGame.BackgroundImage = "backImage";
                db.Games.Update(importedGame);

                var downloader = new MetadataDownloader(db, metadataDownloaders, libraryDownloaders);
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = false };

                var dbGames = db.Games.ToList();
                var f = dbGames[0].ReleaseDate;
                var s = importedGame.ReleaseDate;

                settings.ConfigureFields(new List<Guid> { testPlugin.Id, Guid.Empty }, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, new CancellationTokenSource().Token);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, testPlugin.CallCount);
                Assert.AreEqual(1, storeDownloader.CallCount);
                var game = dbGames[0];
                Assert.AreEqual("Description", game.Description);
                Assert.AreEqual("icon", game.Icon);
                Assert.AreEqual("image", game.CoverImage);
                Assert.AreEqual("backImage", game.BackgroundImage);
                Assert.AreEqual("Developer", game.Developers[0].Name);
                Assert.AreEqual("Publisher", game.Publishers[0].Name);
                Assert.AreEqual("Genre", game.Genres[0].Name);
                CollectionAssert.IsNotEmpty(game.Links);
                Assert.AreEqual("Tag", game.Tags[0].Name);
                Assert.AreEqual("Feature", game.Features[0].Name);
                Assert.AreEqual(2012, game.ReleaseDate.Value.Year);
                Assert.AreEqual(1000, game.InstallSize);
            }
        }

        [Test]
        public async Task SkipExistingTest()
        {
            // Tests that existing data are not overriden even if metadata provider has them.
            var testPlugin = GetTestPlugin();
            testPlugin.SetSupportedFields(new List<MetadataField>
            {
                MetadataField.Description,
                MetadataField.Icon,
                MetadataField.CoverImage,
                MetadataField.BackgroundImage,
                MetadataField.Links,
                MetadataField.Publishers,
                MetadataField.Developers,
                MetadataField.Tags,
                MetadataField.Genres,
                MetadataField.ReleaseDate,
                MetadataField.Features,
                MetadataField.InstallSize
            });

            var gameId = "Game1";
            var icon = new MetadataFile($"IGDBIconName{gameId}.file", randomFile);
            var image = new MetadataFile($"IGDBImageName{gameId}.file", randomFile);
            var background = new MetadataFile($"IGDB backgournd {gameId}");
            testPlugin.ReturnMetadata = new GameMetadata()
            {
                Name = "IGDB Game " + gameId,
                Description = $"IGDB Description {gameId}",
                Developers = new HashSet<MetadataProperty> { new MetadataNameProperty($"IGDB Developer {gameId}") },
                Genres = new HashSet<MetadataProperty> { new MetadataNameProperty($"IGDB Genre {gameId}") },
                Links = new List<Link> { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") },
                Publishers = new HashSet<MetadataProperty> { new MetadataNameProperty($"IGDB publisher {gameId}") },
                ReleaseDate = new ReleaseDate(2012, 6, 6),
                Tags = new HashSet<MetadataProperty> { new MetadataNameProperty($"IGDB Tag {gameId}") },
                Features = new HashSet<MetadataProperty> { new MetadataNameProperty($"IGDB Feature {gameId}") },
                Icon = icon,
                BackgroundImage = background,
                CoverImage = image,
                InstallSize = 2000
            };

            List<MetadataPlugin> metadataDownloaders = new List<MetadataPlugin>()
            {
                testPlugin
            };

            using (var temp = TempDirectory.Create())
            using (var db = new GameDatabase(temp.TempPath))
            using (var token = new CancellationTokenSource())
            {
                db.OpenDatabase();
                Game.DatabaseReference = db;
                var addedGame = db.ImportGame(new GameMetadata()
                {
                    Name = "Game1",
                    Description = "Description",
                    Developers = new HashSet<MetadataProperty> { new MetadataNameProperty("Developers") },
                    Genres = new HashSet<MetadataProperty> { new MetadataNameProperty("Genres") },
                    Links = new List<Link>() { new Link("Link", "URL") },
                    Publishers = new HashSet<MetadataProperty> { new MetadataNameProperty("Publishers") },
                    ReleaseDate = new ReleaseDate(2012, 6, 6),
                    Tags = new HashSet<MetadataProperty> { new MetadataNameProperty("Tags") },
                    Features = new HashSet<MetadataProperty> { new MetadataNameProperty("Features") },
                    UserScore = 1,
                    CommunityScore = 2,
                    CriticScore = 3,
                    InstallSize = 1000
                });

                addedGame.Icon = "Icon";
                addedGame.CoverImage = "Image";
                addedGame.BackgroundImage = "BackgroundImage";

                var downloader = new MetadataDownloader(db, metadataDownloaders, new List<LibraryPlugin>());
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = true };

                // No download - all values are kept
                settings.ConfigureFields(new List<Guid> { testPlugin.Id }, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, token.Token);

                var dbGames = db.Games.ToList();
                Assert.AreEqual(0, testPlugin.CallCount);

                var game1 = dbGames[0];
                Assert.AreEqual("Description", game1.Description);
                Assert.AreEqual("Developers", game1.Developers[0].Name);
                Assert.AreEqual("Genres", game1.Genres[0].Name);
                Assert.AreEqual("Link", game1.Links[0].Name);
                Assert.AreEqual("URL", game1.Links[0].Url);
                Assert.AreEqual("Publishers", game1.Publishers[0].Name);
                Assert.AreEqual("Tags", game1.Tags[0].Name);
                Assert.AreEqual("Features", game1.Features[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual(1000, game1.InstallSize);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed even when present
                settings.SkipExistingValues = false;
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, token.Token);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, testPlugin.CallCount);

                game1 = dbGames[0];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.AreEqual("IGDB Developer Game1", game1.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.AreEqual("IGDB Tag Game1", game1.Tags[0].Name);
                Assert.AreEqual("IGDB Feature Game1", game1.Features[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual(2000, game1.InstallSize);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed when skip enabled and values are not present
                testPlugin.CallCount = 0;
                settings.SkipExistingValues = true;
                db.Games.Remove(game1);
                db.Games.Add(new Game("Game1"));

                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, token.Token);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, testPlugin.CallCount);

                game1 = dbGames[0];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.AreEqual("IGDB Developer Game1", game1.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.AreEqual("IGDB Tag Game1", game1.Tags[0].Name);
                Assert.AreEqual("IGDB Feature Game1", game1.Features[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual(2000, game1.InstallSize);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);
            }
        }

        [Test]
        public void PlatformRegionIdHandlingTest()
        {
            using (var temp = TempDirectory.Create())
            using (var db = new TestGameDatabase(temp.TempPath))
            {
                db.OpenDatabase();
                db.ClearPlatforms();
                db.ClearRegions();

                var testPlugin = GetTestPlugin();
                testPlugin.SetSupportedFields(new List<MetadataField> { MetadataField.Platform, MetadataField.Region });
                var addedGame = db.ImportGame(new GameMetadata { Name = "Game1" });
                var downloader = new MetadataDownloader(db, new List<MetadataPlugin> { testPlugin }, new List<LibraryPlugin>());
                var settings = new MetadataDownloaderSettings { SkipExistingValues = false };
                settings.ConfigureFields(new List<Guid> { testPlugin.Id }, true);

                testPlugin.ReturnMetadata.Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") };
                testPlugin.ReturnMetadata.Regions = new HashSet<MetadataProperty> { new MetadataSpecProperty("world") };
                downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, new CancellationTokenSource().Token).GetAwaiter().GetResult();
                Assert.AreEqual("pc_windows", db.Platforms.First().SpecificationId);
                Assert.AreEqual("pc_windows", addedGame.Platforms[0].SpecificationId);
                Assert.AreEqual("PC (Windows)", addedGame.Platforms[0].Name);

                Assert.AreEqual("world", db.Regions.First().SpecificationId);
                Assert.AreEqual("world", addedGame.Regions[0].SpecificationId);
                Assert.AreEqual("World", addedGame.Regions[0].Name);
            }
        }
    }
}
