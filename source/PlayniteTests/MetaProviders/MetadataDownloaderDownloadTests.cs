using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.MetaProviders;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.MetaProviders
{
    [TestFixture]
    public class MetadataDownloaderDownloadTests
    {
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

        [Test]
        public async Task IGDBSourceTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "metadownload.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var games = new List<IGame>()
                {
                    new Game("Game1"),
                    new Game("Game2") { Provider = Provider.Steam, ProviderId = "Game2" },
                    new Game("Game3") // just to test that nonexistent game doesn't throw exception
                };

                db.AddGames(games);

                var igdbProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => false,
                    GetGameDataHandler = gameId =>
                    {
                        var game = new Game("IGDB Game " + gameId)
                        {
                            Description = $"IGDB Description {gameId}",
                            Developers = new ComparableList<string>() { $"IGDB Developer {gameId}" },
                            Genres = new ComparableList<string>() { $"IGDB Genre {gameId}" },
                            Links = new ObservableCollection<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") },
                            Publishers = new ComparableList<string>() { $"IGDB publisher {gameId}" },
                            ReleaseDate = new DateTime(2012, 6, 6),
                            Tags = new ComparableList<string>() { $"IGDB Tag {gameId}" }
                        };
                        var icon = new FileDefinition($"IGDBIconPath{gameId}.file", $"IGDBIconName{gameId}.file", randomFile);
                        var image = new FileDefinition($"IGDBImagePath{gameId}.file", $"IGDBImageName{gameId}.file", randomFile);
                        return new GameMetadata(game, icon, image, $"IGDB backgournd {gameId}");
                    },
                    SearchGamesHandler = gameName =>
                    {
                        return new List<MetadataSearchResult>()
                        {
                        new MetadataSearchResult("igdbid1", "Game1", DateTime.Now),
                        new MetadataSearchResult("igdbid2", "Game2", DateTime.Now)
                        };
                    }
                };

                var storeProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => true,
                    GetGameDataHandler = (gameId) =>
                    {
                        var game = new Game(gameId);
                        return new GameMetadata(game, null, null, string.Empty);
                    }
                };

                var downloader = new MockMetadataDownloader(storeProvider, storeProvider, storeProvider, storeProvider, igdbProvider);
                var settings = new MetadataDownloaderSettings();
                settings.ConfigureFields(MetadataSource.IGDB, true);
                await downloader.DownloadMetadata(
                    db.GamesCollection.FindAll().ToList(),
                    db, settings);

                var dbGames = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, downloader.CallCount);
                var game1 = dbGames[0];
                Assert.AreEqual("IGDB Description igdbid1", game1.Description);
                Assert.AreEqual("IGDB Developer igdbid1", game1.Developers[0]);
                Assert.AreEqual("IGDB Genre igdbid1", game1.Genres[0]);
                Assert.AreEqual("IGDB link igdbid1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url igdbid1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher igdbid1", game1.Publishers[0]);
                Assert.AreEqual("IGDB Tag igdbid1", game1.Tags[0]);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual("IGDB backgournd igdbid1", game1.BackgroundImage);
                Assert.AreEqual($"IGDBIconPathigdbid1.file", game1.Icon);
                Assert.AreEqual($"IGDBImagePathigdbid1.file", game1.Image);
                var game2 = dbGames[1];
                Assert.AreEqual("IGDB Description igdbid2", game2.Description);                

                Assert.AreEqual(4, db.Database.FileStorage.FindAll().Count());
            }
        }

        [Test]
        public async Task StoreSourceTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "metadownload.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var games = new List<IGame>()
                {
                    new Game("Game1"),
                    new Game("Game2") { Provider = Provider.Steam, ProviderId = "storeId" },
                    new Game("Game3") // just to test that nonexistent game doesn't throw exception
                };

                db.AddGames(games);

                var igdbProvider = new MockMetadataProvider();
                var storeProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => true,
                    GetGameDataHandler = (gameId) =>
                    {
                        var game = new Game("Store Game " + gameId)
                        {
                            Description = $"Store Description {gameId}",
                            Developers = new ComparableList<string>() { $"Store Developer {gameId}" },
                            Genres = new ComparableList<string>() { $"Store Genre {gameId}" },
                            Links = new ObservableCollection<Link>() { new Link($"Store link {gameId}", $"Store link url {gameId}") },
                            Publishers = new ComparableList<string>() { $"Store publisher {gameId}" },
                            ReleaseDate = new DateTime(2016, 2, 2),
                            Tags = new ComparableList<string>() { $"Store Tag {gameId}" }
                        };
                        var icon = new FileDefinition($"StoreIconPath{gameId}.file", $"StoreIconName{gameId}.file", randomFile);
                        var image = new FileDefinition($"StoreImagePath{gameId}.file", $"StoreImageName{gameId}.file", randomFile);
                        return new GameMetadata(game, icon, image, $"Store backgournd {gameId}");
                    }
                };

                var downloader = new MockMetadataDownloader(storeProvider, storeProvider, storeProvider, storeProvider, igdbProvider);
                var settings = new MetadataDownloaderSettings();
                settings.ConfigureFields(MetadataSource.Store, true);
                await downloader.DownloadMetadata(
                    db.GamesCollection.FindAll().ToList(),
                    db, settings);

                var dbGames = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(1, downloader.CallCount);
                var game2 = dbGames[1];
                Assert.AreEqual("Store Description storeId", game2.Description);
                Assert.AreEqual("Store Developer storeId", game2.Developers[0]);
                Assert.AreEqual("Store Genre storeId", game2.Genres[0]);
                Assert.AreEqual("Store link storeId", game2.Links[0].Name);
                Assert.AreEqual("Store link url storeId", game2.Links[0].Url);
                Assert.AreEqual("Store publisher storeId", game2.Publishers[0]);
                Assert.AreEqual("Store Tag storeId", game2.Tags[0]);
                Assert.AreEqual(2016, game2.ReleaseDate.Value.Year);
                Assert.AreEqual("Store backgournd storeId", game2.BackgroundImage);
                Assert.AreEqual($"StoreIconPathstoreId.file", game2.Icon);
                Assert.AreEqual($"StoreImagePathstoreId.file", game2.Image);
                var game1 = dbGames[0];
                Assert.IsNull(game1.Description);

                Assert.AreEqual(2, db.Database.FileStorage.FindAll().Count());
            }
        }

        [Test]
        public async Task IGDBStoreCombinedTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "metadownload.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var games = new List<IGame>()
                {
                    new Game("Game1"),
                    new Game("Game2") { Provider = Provider.Steam, ProviderId = "Game2" },
                    new Game("Game3") // just to test that nonexistent game doesn't throw exception
                };

                db.AddGames(games);

                var igdbProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => false,
                    GetGameDataHandler = gameId =>
                    {
                        var game = new Game("IGDB Game " + gameId);
                        game.Description = $"IGDB Description {gameId}";
                        game.Genres = new ComparableList<string>() { $"IGDB Genre {gameId}" };
                        game.Links = new ObservableCollection<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") };
                        game.Publishers = new ComparableList<string>() { $"IGDB publisher {gameId}" };
                        game.ReleaseDate = new DateTime(2012, 6, 6);
                        var icon = new FileDefinition($"IGDBIconPath{gameId}.file", $"IGDBIconName{gameId}.file", randomFile);
                        var image = new FileDefinition($"IGDBImagePath{gameId}.file", $"IGDBImageName{gameId}.file", randomFile);
                        return new GameMetadata(game, icon, image, $"IGDB backgournd {gameId}");
                    },
                    SearchGamesHandler = gameName =>
                    {
                        return new List<MetadataSearchResult>()
                        {
                        new MetadataSearchResult("igdbid1", "Game1", DateTime.Now),
                        new MetadataSearchResult("igdbid2", "Game2", DateTime.Now)
                        };
                    }
                };

                var storeProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => true,
                    GetGameDataHandler = (gameId) =>
                    {
                        var game = new Game(gameId);
                        game.Description = $"Store Description {gameId}";
                        game.Developers = new ComparableList<string>() { $"Store Developer {gameId}" };
                        game.Links = new ObservableCollection<Link>() { new Link($"Store link {gameId}", $"Store link url {gameId}") };
                        game.Publishers = new ComparableList<string>() { $"Store publisher {gameId}" };
                        game.ReleaseDate = new DateTime(2016, 2, 2);
                        var icon = new FileDefinition($"StoreIconPath{gameId}.file", $"StoreIconName{gameId}.file", randomFile);
                        var image = new FileDefinition($"StoreImagePath{gameId}.file", $"StoreImageName{gameId}.file", randomFile);
                        return new GameMetadata(game, icon, image, $"Store backgournd {gameId}");
                    }
                };

                var downloader = new MockMetadataDownloader(storeProvider, storeProvider, storeProvider, storeProvider, igdbProvider);
                var settings = new MetadataDownloaderSettings();

                // IGDB over Store
                settings.ConfigureFields(MetadataSource.IGDBOverStore, true);
                await downloader.DownloadMetadata(
                    db.GamesCollection.FindAll().ToList(),
                    db, settings);
                Assert.AreEqual(4, downloader.CallCount);

                var dbGames = db.GamesCollection.FindAll().ToList();
                var game1 = dbGames[0];
                Assert.AreEqual("IGDB Description igdbid1", game1.Description);
                Assert.IsNull(game1.Developers);
                Assert.AreEqual("IGDB Genre igdbid1", game1.Genres[0]);
                Assert.AreEqual("IGDB link igdbid1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url igdbid1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher igdbid1", game1.Publishers[0]);
                Assert.IsNull(game1.Tags);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual("IGDB backgournd igdbid1", game1.BackgroundImage);
                Assert.AreEqual($"IGDBIconPathigdbid1.file", game1.Icon);
                Assert.AreEqual($"IGDBImagePathigdbid1.file", game1.Image);

                var game2 = dbGames[1];
                Assert.AreEqual("IGDB Description igdbid2", game2.Description);
                Assert.AreEqual("Store Developer Game2", game2.Developers[0]);
                Assert.AreEqual("IGDB Genre igdbid2", game2.Genres[0]);
                Assert.AreEqual("IGDB link igdbid2", game2.Links[0].Name);
                Assert.AreEqual("IGDB link url igdbid2", game2.Links[0].Url);
                Assert.AreEqual("IGDB publisher igdbid2", game2.Publishers[0]);
                Assert.IsNull(game2.Tags);
                Assert.AreEqual(2012, game2.ReleaseDate.Value.Year);
                Assert.AreEqual("IGDB backgournd igdbid2", game2.BackgroundImage);
                Assert.AreEqual($"IGDBIconPathigdbid2.file", game2.Icon);
                Assert.AreEqual($"IGDBImagePathigdbid2.file", game2.Image);

                // Store over IGDB
                downloader.CallCount = 0;
                settings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                await downloader.DownloadMetadata(
                    db.GamesCollection.FindAll().ToList(),
                    db, settings);
                Assert.AreEqual(4, downloader.CallCount);

                dbGames = db.GamesCollection.FindAll().ToList();
                game1 = dbGames[0];
                Assert.AreEqual("IGDB Description igdbid1", game1.Description);
                Assert.IsNull(game1.Developers);
                Assert.AreEqual("IGDB Genre igdbid1", game1.Genres[0]);
                Assert.AreEqual("IGDB link igdbid1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url igdbid1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher igdbid1", game1.Publishers[0]);
                Assert.IsNull(game1.Tags);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.AreEqual("IGDB backgournd igdbid1", game1.BackgroundImage);
                Assert.AreEqual($"IGDBIconPathigdbid1.file", game1.Icon);
                Assert.AreEqual($"IGDBImagePathigdbid1.file", game1.Image);

                game2 = dbGames[1];
                Assert.AreEqual("Store Description Game2", game2.Description);
                Assert.AreEqual("Store Developer Game2", game2.Developers[0]);
                Assert.AreEqual("IGDB Genre igdbid2", game2.Genres[0]);
                Assert.AreEqual("Store link Game2", game2.Links[0].Name);
                Assert.AreEqual("Store link url Game2", game2.Links[0].Url);
                Assert.AreEqual("Store publisher Game2", game2.Publishers[0]);
                Assert.IsNull(game2.Tags);
                Assert.AreEqual(2016, game2.ReleaseDate.Value.Year);
                Assert.AreEqual("Store backgournd Game2", game2.BackgroundImage);
                Assert.AreEqual($"StoreIconPathGame2.file", game2.Icon);
                Assert.AreEqual($"StoreImagePathGame2.file", game2.Image);
            }
        }

        [Test]
        public async Task MissingDataTest()
        {
            // Test that downloader doesn't change existing values to null when missing by provider
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "metadownload.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var games = new List<IGame>()
                {
                    new Game("Game")
                    {
                        Provider = Provider.Steam,
                        ProviderId = "storeId",
                        Genres = new ComparableList<string>() { "Genre" },
                        ReleaseDate = new DateTime(2012, 6, 6),
                        Developers = new ComparableList<string>() { "Developer" },
                        Publishers = new ComparableList<string>() { "Publisher" },
                        Tags = new ComparableList<string>() { "Tag" },
                        Description = "Description",
                        Links = new ObservableCollection<Link>() { new Link() },
                        Icon = "icon",
                        Image = "image",
                        BackgroundImage = "backImage"
                    }
                };

                db.AddGames(games);

                var igdbProvider = new MockMetadataProvider();
                var storeProvider = new MockMetadataProvider
                {
                    GetSupportsIdSearchHandler = () => true,
                    GetGameDataHandler = (gameId) =>
                    {
                        return new GameMetadata(new Game("Store Game " + gameId), null, null, null);
                    }
                };

                var downloader = new MockMetadataDownloader(storeProvider, storeProvider, storeProvider, storeProvider, igdbProvider);
                var settings = new MetadataDownloaderSettings();

                
                var dbGames = db.GamesCollection.FindAll().ToList();
                var f = dbGames[0].ReleaseDate;
                var s = games[0].ReleaseDate;

                settings.ConfigureFields(MetadataSource.Store, true);
                await downloader.DownloadMetadata(
                    db.GamesCollection.FindAll().ToList(),
                    db, settings);

                dbGames = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(1, downloader.CallCount);
                var game = dbGames[0];
                Assert.AreEqual("Description", game.Description);
                Assert.AreEqual("icon", game.Icon);
                Assert.AreEqual("image", game.Image);
                Assert.AreEqual("backImage", game.BackgroundImage);
                Assert.AreEqual("Developer", game.Developers[0]);
                Assert.AreEqual("Publisher", game.Publishers[0]);
                Assert.AreEqual("Genre", game.Genres[0]);
                CollectionAssert.IsNotEmpty(game.Links);
                Assert.AreEqual("Tag", game.Tags[0]);
                Assert.AreEqual(2012, game.ReleaseDate.Value.Year);
            }
        }
    }
}
