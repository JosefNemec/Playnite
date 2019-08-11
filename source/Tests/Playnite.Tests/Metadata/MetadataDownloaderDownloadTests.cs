﻿using Moq;
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
        private static Guid storePluginId = Guid.NewGuid();
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

        private IEnumerable<LibraryPlugin> GetLibraryPlugins(LibraryMetadataProvider provider, Guid libraryId)
        {
            var library = new Mock<LibraryPlugin>(MockBehavior.Loose, null);
            library.Setup(a => a.Id).Returns(storePluginId);
            library.Setup(a => a.GetMetadataDownloader()).Returns(provider);
            return new List<LibraryPlugin>() { library.Object };
        }

        [Test]
        public async Task IGDBSourceTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                int callCount = 0;
                var storeCalled = false;

                var games = new List<Game>()
                {
                    new Game("Game1"),
                    new Game("Game2") { PluginId = storePluginId, GameId = "Game2" },
                    new Game("Game3")
                };

                db.Games.Add(games);

                var igdbProvider = new Mock<LibraryMetadataProvider>();
                igdbProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;

                    if (g.Name == "Game3")
                    {
                        return GameMetadata.GetEmptyData();
                    }

                    var gameId = g.Name;
                    var game = new GameInfo()
                    {
                        Name = "IGDB Game " + gameId,
                        Description = $"IGDB Description {gameId}",
                        Developers = new List<string>() { $"IGDB Developer {gameId}" },
                        Genres = new List<string>() { $"IGDB Genre {gameId}" },
                        Links = new List<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") },
                        Publishers = new List<string>() { $"IGDB publisher {gameId}" },
                        ReleaseDate = new DateTime(2012, 6, 6),
                        Tags = new List<string>() { $"IGDB Tag {gameId}" }
                    };
                    var icon = new MetadataFile($"IGDBIconName{gameId}.file", randomFile);
                    var image = new MetadataFile($"IGDBImageName{gameId}.file", randomFile);
                    var background = new MetadataFile($"IGDB backgournd {gameId}");
                    return new GameMetadata(game, icon, image, background);
                });

                var storeProvider = new Mock<LibraryMetadataProvider>();
                storeProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    storeCalled = true;
                    return GameMetadata.GetEmptyData();
                });

                var downloader = new MetadataDownloader(db, igdbProvider.Object, GetLibraryPlugins(storeProvider.Object, storePluginId));
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = false };
                settings.Name.Import = false;
                settings.ConfigureFields(MetadataSource.IGDB, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);


                Assert.IsFalse(storeCalled);
                Assert.AreEqual(3, callCount);
                var game1 = db.Games[games[0].Id];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.AreEqual("IGDB Developer Game1", game1.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.AreEqual("IGDB Tag Game1", game1.Tags[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);
                var game2 = db.Games[games[1].Id];
                Assert.AreEqual("IGDB Description Game2", game2.Description);

                Assert.AreEqual(2, Directory.GetFiles(db.GetFileStoragePath(game1.Id)).Count());
                Assert.AreEqual(2, Directory.GetFiles(db.GetFileStoragePath(game2.Id)).Count());
            }
        }

        [Test]
        public async Task StoreSourceTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                int callCount = 0;
                var igdbCalled = false;

                var games = new List<Game>()
                {
                    new Game("Game1"),
                    new Game("Game2") { PluginId = storePluginId, GameId = "storeId" },
                    new Game("Game3")
                };

                db.Games.Add(games);

                var igdbProvider = new Mock<LibraryMetadataProvider>();
                igdbProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    igdbCalled = true;
                    return GameMetadata.GetEmptyData();
                });

                var storeProvider = new Mock<LibraryMetadataProvider>();
                storeProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    var gameId = g.GameId;
                    var game = new GameInfo()
                    {
                        Name = "Store Game " + gameId,
                        Description = $"Store Description {gameId}",
                        Developers = new List<string>() { $"Store Developer {gameId}" },
                        Genres = new List<string>() { $"Store Genre {gameId}" },
                        Links = new List<Link>() { new Link($"Store link {gameId}", $"Store link url {gameId}") },
                        Publishers = new List<string>() { $"Store publisher {gameId}" },
                        ReleaseDate = new DateTime(2016, 2, 2),
                        Tags = new List<string>() { $"Store Tag {gameId}" }
                    };
                    var icon = new MetadataFile($"StoreIconName{gameId}.file", randomFile);
                    var image = new MetadataFile($"StoreImageName{gameId}.file", randomFile);
                    var background = new MetadataFile($"Store backgournd {gameId}");
                    return new GameMetadata(game, icon, image, background);
                });

                var downloader = new MetadataDownloader(db, igdbProvider.Object, GetLibraryPlugins(storeProvider.Object, storePluginId));
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = false };
                settings.ConfigureFields(MetadataSource.Store, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);
                
                Assert.AreEqual(1, callCount);
                Assert.IsFalse(igdbCalled);
                var game2 = db.Games[games[1].Id];
                Assert.AreEqual("Store Description storeId", game2.Description);
                Assert.AreEqual("Store Developer storeId", game2.Developers[0].Name);
                Assert.AreEqual("Store Genre storeId", game2.Genres[0].Name);
                Assert.AreEqual("Store link storeId", game2.Links[0].Name);
                Assert.AreEqual("Store link url storeId", game2.Links[0].Url);
                Assert.AreEqual("Store publisher storeId", game2.Publishers[0].Name);
                Assert.AreEqual("Store Tag storeId", game2.Tags[0].Name);
                Assert.AreEqual(2016, game2.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game2.BackgroundImage);
                Assert.IsNotEmpty(game2.Icon);
                Assert.IsNotEmpty(game2.CoverImage);
                var game1 = db.Games[games[0].Id];
                Assert.IsNull(game1.Description);

                Assert.AreEqual(2, Directory.GetFiles(db.GetFileStoragePath(game2.Id)).Count());
                Assert.AreEqual(0, Directory.GetFiles(db.GetFileStoragePath(game1.Id)).Count());
            }
        }

        [Test]
        public async Task IGDBStoreCombinedTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                int callCount = 0;
                var games = new List<Game>()
                {
                    new Game("Game1"),
                    new Game("Game2") { PluginId = storePluginId, GameId = "Game2" },
                    new Game("Game3")
                };

                db.Games.Add(games);

                var igdbProvider = new Mock<LibraryMetadataProvider>();
                igdbProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    var gameId = g.Name;
                    var game = new GameInfo();
                    game.Name = "IGDB Game " + gameId;
                    game.Description = $"IGDB Description {gameId}";
                    game.Genres = new List<string>() { $"IGDB Genre {gameId}" };
                    game.Links = new List<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") };
                    game.Publishers = new List<string>() { $"IGDB publisher {gameId}" };
                    game.ReleaseDate = new DateTime(2012, 6, 6);
                    var icon = new MetadataFile($"IGDBIconName{gameId}.file", randomFile);
                    var image = new MetadataFile($"IGDBImageName{gameId}.file", randomFile);
                    var background = new MetadataFile($"IGDB backgournd {gameId}");
                    return new GameMetadata(game, icon, image, background);
                });

                var storeProvider = new Mock<LibraryMetadataProvider>();
                storeProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    var gameId = g.GameId;
                    var game = new GameInfo();
                    game.Name = gameId;
                    game.Description = $"Store Description {gameId}";
                    game.Developers = new List<string>() { $"Store Developer {gameId}" };
                    game.Links = new List<Link>() { new Link($"Store link {gameId}", $"Store link url {gameId}") };
                    game.Publishers = new List<string>() { $"Store publisher {gameId}" };
                    game.ReleaseDate = new DateTime(2016, 2, 2);
                    var icon = new MetadataFile($"StoreIconName{gameId}.file", randomFile);
                    var image = new MetadataFile($"StoreImageName{gameId}.file", randomFile);
                    var background = new MetadataFile($"Store backgournd {gameId}");
                    return new GameMetadata(game, icon, image, background);
                });

                var downloader = new MetadataDownloader(db, igdbProvider.Object, GetLibraryPlugins(storeProvider.Object, storePluginId));
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = false };

                // IGDB over Store
                settings.ConfigureFields(MetadataSource.IGDBOverStore, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);
                Assert.AreEqual(4, callCount);
                                
                var game1 = db.Games[games[0].Id];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.IsNull(game1.Developers);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.IsNull(game1.Tags);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                var game2 = db.Games[games[1].Id];
                Assert.AreEqual("IGDB Description Game2", game2.Description);
                Assert.AreEqual("Store Developer Game2", game2.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game2", game2.Genres[0].Name);
                Assert.AreEqual("IGDB link Game2", game2.Links[0].Name);
                Assert.AreEqual("IGDB link url Game2", game2.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game2", game2.Publishers[0].Name);
                Assert.IsNull(game2.Tags);
                Assert.AreEqual(2012, game2.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game2.BackgroundImage);
                Assert.IsNotEmpty(game2.Icon);
                Assert.IsNotEmpty(game2.CoverImage);

                // Store over IGDB
                callCount = 0;
                settings.ConfigureFields(MetadataSource.StoreOverIGDB, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);
                Assert.AreEqual(4, callCount);

                game1 = db.Games[games[0].Id];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.IsNull(game1.Developers);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.IsNull(game1.Tags);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                game2 = db.Games[games[1].Id];
                Assert.AreEqual("Store Description Game2", game2.Description);
                Assert.AreEqual("Store Developer Game2", game2.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game2", game2.Genres[0].Name);
                Assert.AreEqual("Store link Game2", game2.Links[0].Name);
                Assert.AreEqual("Store link url Game2", game2.Links[0].Url);
                Assert.AreEqual("Store publisher Game2", game2.Publishers[0].Name);
                Assert.IsNull(game2.Tags);
                Assert.AreEqual(2016, game2.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game2.BackgroundImage);
                Assert.IsNotEmpty(game2.Icon);
                Assert.IsNotEmpty(game2.CoverImage);
            }
        }

        [Test]
        public async Task MissingDataTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                int callCount = 0;

                var importedGame = db.ImportGame(new GameInfo()
                {
                    Name = "Game",
                    GameId = "storeId",
                    Genres = new List<string>() { "Genre" },
                    ReleaseDate = new DateTime(2012, 6, 6),
                    Developers = new List<string>() { "Developer" },
                    Publishers = new List<string>() { "Publisher" },
                    Tags = new List<string>() { "Tag" },
                    Description = "Description",
                    Links = new List<Link>() { new Link() }                    
                });

                importedGame.PluginId = storePluginId;
                importedGame.Icon = "icon";
                importedGame.CoverImage = "image";
                importedGame.BackgroundImage = "backImage";
                db.Games.Update(importedGame);

                var igdbProvider = new Mock<LibraryMetadataProvider>();
                igdbProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    return GameMetadata.GetEmptyData();
                });

                var storeProvider = new Mock<LibraryMetadataProvider>();
                storeProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    var gameId = g.GameId;
                    return new GameMetadata(new GameInfo() { Name = "Store Game " + gameId }, null, null, null);
                });

                var downloader = new MetadataDownloader(db, igdbProvider.Object, GetLibraryPlugins(storeProvider.Object, storePluginId));
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = false };

                var dbGames = db.Games.ToList();
                var f = dbGames[0].ReleaseDate;
                var s = importedGame.ReleaseDate;

                settings.ConfigureFields(MetadataSource.Store, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, callCount);
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
                Assert.AreEqual(2012, game.ReleaseDate.Value.Year);
            }
        }

        [Test]
        public async Task SkipExistingTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                Game.DatabaseReference = db;
                int callCount = 0;
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
                    UserScore = 1,
                    CommunityScore = 2,
                    CriticScore = 3
                });

                addedGame.Icon = "Icon";
                addedGame.CoverImage = "Image";
                addedGame.BackgroundImage = "BackgroundImage";

                var igdbProvider = new Mock<LibraryMetadataProvider>();
                igdbProvider.Setup(x => x.GetMetadata(It.IsAny<Game>())).Returns((Game g) =>
                {
                    callCount++;
                    var gameId = g.Name;
                    var game = new GameInfo()
                    {
                        Name = "IGDB Game " + gameId,
                        Description = $"IGDB Description {gameId}",
                        Developers = new List<string>() { $"IGDB Developer {gameId}" },
                        Genres = new List<string>() { $"IGDB Genre {gameId}" },
                        Links = new List<Link>() { new Link($"IGDB link {gameId}", $"IGDB link url {gameId}") },
                        Publishers = new List<string>() { $"IGDB publisher {gameId}" },
                        ReleaseDate = new DateTime(2012, 6, 6),
                        Tags = new List<string>() { $"IGDB Tag {gameId}" }
                    };
                    var icon = new MetadataFile($"IGDBIconName{gameId}.file", randomFile);
                    var image = new MetadataFile($"IGDBImageName{gameId}.file", randomFile);
                    var background = new MetadataFile($"IGDB backgournd {gameId}");
                    return new GameMetadata(game, icon, image, background);
                });

                var downloader = new MetadataDownloader(db, igdbProvider.Object, null);
                var settings = new MetadataDownloaderSettings() { SkipExistingValues = true };

                // No download - all values are kept
                settings.ConfigureFields(MetadataSource.IGDB, true);
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);

                var dbGames = db.Games.ToList();
                Assert.AreEqual(0, callCount);

                var game1 = dbGames[0];
                Assert.AreEqual("Description", game1.Description);
                Assert.AreEqual("Developers", game1.Developers[0].Name);
                Assert.AreEqual("Genres", game1.Genres[0].Name);
                Assert.AreEqual("Link", game1.Links[0].Name);
                Assert.AreEqual("URL", game1.Links[0].Url);
                Assert.AreEqual("Publishers", game1.Publishers[0].Name);
                Assert.AreEqual("Tags", game1.Tags[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed even when present
                settings.SkipExistingValues = false;
                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, callCount);

                game1 = dbGames[0];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.AreEqual("IGDB Developer Game1", game1.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.AreEqual("IGDB Tag Game1", game1.Tags[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);

                // Single download - values are changed when skip enabled and values are not present
                callCount = 0;
                settings.SkipExistingValues = true;
                db.Games.Remove(game1);
                db.Games.Add(new Game("Game1"));

                await downloader.DownloadMetadataAsync(
                    db.Games.ToList(), settings, null, null);

                dbGames = db.Games.ToList();
                Assert.AreEqual(1, callCount);

                game1 = dbGames[0];
                Assert.AreEqual("IGDB Description Game1", game1.Description);
                Assert.AreEqual("IGDB Developer Game1", game1.Developers[0].Name);
                Assert.AreEqual("IGDB Genre Game1", game1.Genres[0].Name);
                Assert.AreEqual("IGDB link Game1", game1.Links[0].Name);
                Assert.AreEqual("IGDB link url Game1", game1.Links[0].Url);
                Assert.AreEqual("IGDB publisher Game1", game1.Publishers[0].Name);
                Assert.AreEqual("IGDB Tag Game1", game1.Tags[0].Name);
                Assert.AreEqual(2012, game1.ReleaseDate.Value.Year);
                Assert.IsNotEmpty(game1.BackgroundImage);
                Assert.IsNotEmpty(game1.Icon);
                Assert.IsNotEmpty(game1.CoverImage);
            }
        }
    }
}
