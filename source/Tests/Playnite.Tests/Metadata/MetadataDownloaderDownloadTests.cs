using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.Metadata;
using Playnite.Models;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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

            public override MetadataFile GetBackgroundImage()
            {
                return metadata.BackgroundImage;
            }

            public override int? GetCommunityScore()
            {
                return metadata.GameInfo.CommunityScore;
            }

            public override MetadataFile GetCoverImage()
            {
                return metadata.CoverImage;
            }

            public override int? GetCriticScore()
            {
                return metadata.GameInfo?.CriticScore;
            }

            public override string GetDescription()
            {
                return metadata.GameInfo?.Description;
            }

            public override List<string> GetDevelopers()
            {
                return metadata.GameInfo?.Developers;
            }

            public override List<string> GetGenres()
            {
                return metadata.GameInfo?.Genres;
            }

            public override MetadataFile GetIcon()
            {
                return metadata.Icon;
            }

            public override List<Link> GetLinks()
            {
                return metadata.GameInfo?.Links;
            }

            public override string GetName()
            {
                return metadata.GameInfo?.Name;
            }

            public override List<string> GetPublishers()
            {
                return metadata.GameInfo?.Publishers;
            }

            public override DateTime? GetReleaseDate()
            {
                return metadata.GameInfo?.ReleaseDate;
            }

            public override List<string> GetTags()
            {
                return metadata.GameInfo?.Tags;
            }

            public override List<string> GetFeatures()
            {
                return metadata.GameInfo?.Features;
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
                GameInfo = new GameInfo
                {
                    Description = DataString
                }
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
                GameInfo = new GameInfo
                {
                    Description = DataString
                }
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

            // Store is not called if custom game
            var downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

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
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.GameInfo.Description);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Already downloaded data are reqused
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Store is still used and plugin is not called
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty, testPlugin.Id }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.GameInfo.Description);
            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(0, testPlugin.CallCount);
            Assert.AreEqual(1, existingMetadata.Count);

            // Plugin is not used if not supporting field
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { Guid.Empty, testPlugin.Id }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.GameInfo.Description);
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
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.AreEqual(TestMetadataPlugin.DataString, downloadedMetadata.GameInfo.Description);
            Assert.IsNotNull(downloadedMetadata);
            Assert.AreEqual(1, storeDownloader.CallCount);
            Assert.AreEqual(1, testPlugin.CallCount);
            Assert.AreEqual(1, existingPluginData.Count);

            // Not data are returned if specific fields doesn't have them
            testPlugin.ReturnMetadata.GameInfo.Description = null;
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { testPlugin.Id }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.IsNull(downloadedMetadata);

            // Second data are used if first doesn't have them
            downloadedMetadata = downloader.ProcessField(
                game,
                new MetadataFieldSettings(true, new List<Guid> { testPlugin.Id, Guid.Empty }),
                MetadataField.Description,
                (a) => a.GameInfo?.Description,
                existingMetadata,
                existingPluginData);

            Assert.AreEqual(TestLibraryMetadataProvider.DataString, downloadedMetadata.GameInfo.Description);
        }

        [Test]
        public async Task MissingDataTest()
        {
            // Tests that existing data are not overriden by empty metadata from external providers.
            var storeId = Guid.NewGuid();
            var storeDownloader = GetTestLibraryProvider();
            storeDownloader.ReturnMetadata = GameMetadata.GetEmptyData();
            var testPlugin = GetTestPlugin();
            testPlugin.ReturnMetadata = GameMetadata.GetEmptyData();
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
                MetadataField.Features
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
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;

                var importedGame = db.ImportGame(new GameInfo()
                {
                    Name = "Game",
                    GameId = "storeId",
                    Genres = new List<string>() { "Genre" },
                    ReleaseDate = new DateTime(2012, 6, 6),
                    Developers = new List<string>() { "Developer" },
                    Publishers = new List<string>() { "Publisher" },
                    Tags = new List<string>() { "Tag" },
                    Features = new List<string>() { "Feature" },
                    Description = "Description",
                    Links = new List<Link>() { new Link() }
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
                    db.Games.ToList(), settings, new PlayniteSettings(), null, null);

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
                MetadataField.Features
            });

            var gameId = "Game1";
            var icon = new MetadataFile($"IGDBIconName{gameId}.file", randomFile);
            var image = new MetadataFile($"IGDBImageName{gameId}.file", randomFile);
            var background = new MetadataFile($"IGDB backgournd {gameId}");
            testPlugin.ReturnMetadata = new GameMetadata(new GameInfo()
            {
                Name = "IGDB Game " + gameId,
                Description = $"IGDB Description {gameId}",
                Developers = new List<string>() { $"IGDB Developer {gameId}" },
                Genres = new List<string>() { $"IGDB Genre {gameId}" },
                Links = new List<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") },
                Publishers = new List<string>() { $"IGDB publisher {gameId}" },
                ReleaseDate = new DateTime(2012, 6, 6),
                Tags = new List<string>() { $"IGDB Tag {gameId}" },
                Features = new List<string>() { $"IGDB Feature {gameId}" }
            }, icon, image, background);

            List<MetadataPlugin> metadataDownloaders = new List<MetadataPlugin>()
            {
                testPlugin
            };

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                var addedGame = db.ImportGame(new GameInfo()
                {
                    Name = "Game1",
                    Description = "Description",
                    Developers = new List<string>() { "Developers" },
                    Genres = new List<string>() { "Genres" },
                    Links = new List<Link>() { new Link("Link", "URL") },
                    Publishers = new List<string>() { "Publishers" },
                    ReleaseDate = new DateTime(2012, 6, 6),
                    Tags = new List<string>() { "Tags" },
                    Features = new List<string>() { "Features" },
                    UserScore = 1,
                    CommunityScore = 2,
                    CriticScore = 3
                });

                addedGame.Icon = "Icon";
                addedGame.CoverImage = "Image";
                addedGame.BackgroundImage = "BackgroundImage";

                var downloader = new MetadataDownloader(db, metadataDownloaders, new List<LibraryPlugin>());
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = true };

                // No download - all values are kept
                settings.ConfigureFields(new List<Guid> { testPlugin.Id }, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, null);

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
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed even when present
                settings.SkipExistingValues = false;
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, null);

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
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed when skip enabled and values are not present
                testPlugin.CallCount = 0;
                settings.SkipExistingValues = true;
                db.Games.Remove(game1);
                db.Games.Add(new Game("Game1"));

                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, new PlayniteSettings(), null, null);

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
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);
            }
        }
    }
}
