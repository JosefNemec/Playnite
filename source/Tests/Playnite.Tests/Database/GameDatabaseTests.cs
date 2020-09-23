using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite;
using Moq;
using NUnit.Framework;
using Playnite.Settings;
using Playnite.Common;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameDatabaseTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            // Some test are reading resources, which cannot be access until pack:// namespace is initialized
            // http://stackoverflow.com/questions/6005398/uriformatexception-invalid-uri-invalid-port-specified
            string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
        }

        [Test]
        public void GetMigratedDbPathTest()
        {
            Assert.AreEqual(@"{PlayniteDir}\games", GameDatabase.GetMigratedDbPath(@"games.db"));
            Assert.AreEqual(@"c:\games", GameDatabase.GetMigratedDbPath(@"c:\games.db"));
            Assert.AreEqual(@"c:\test\games", GameDatabase.GetMigratedDbPath(@"c:\test\games.db"));
            var appData = Environment.ExpandEnvironmentVariables("%AppData%");
            Assert.AreEqual(@"%AppData%\playnite\games", GameDatabase.GetMigratedDbPath(Path.Combine(appData, "playnite", "games.db")));
        }

        [Test]
        public void GetFullDbPathTest()
        {
            var appData = Environment.ExpandEnvironmentVariables("%AppData%");
            var progPath = PlaynitePaths.ProgramPath;
            Assert.AreEqual(Path.Combine(appData, @"playnite\games"), GameDatabase.GetFullDbPath(@"%AppData%\playnite\games"));
            Assert.AreEqual(Path.Combine(progPath, "games"), GameDatabase.GetFullDbPath(@"{PlayniteDir}\games"));
            Assert.AreEqual(@"c:\test\games", GameDatabase.GetFullDbPath(@"c:\test\games"));
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, @"games"), GameDatabase.GetFullDbPath("games"));
        }

        [Test]
        public void ListUpdateTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Games.Add(new Game()
                {
                    GameId = "testid",
                    Name = "Test Game"
                });

                db.Games.Add(new Game()
                {
                    GameId = "testid2",
                    Name = "Test Game 2"
                });

                Assert.AreEqual(2, db.Games.Count);

                var games = db.Games.ToList();
                games[1].Name = "Changed Name";
                db.Games.Update(games[1]);

                games = db.Games.ToList();
                Assert.AreEqual("Changed Name", games[1].Name);

                db.Games.Remove(games[1]);
                Assert.AreEqual(1, db.Games.Count);
            }
        }

        [Test]
        public void DeleteGameImageCleanupTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var game = new Game("Test");
                db.Games.Add(game);
                game.Icon = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                game.BackgroundImage = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                game.CoverImage = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);

                Assert.IsNotEmpty(game.Icon);
                Assert.IsNotEmpty(game.BackgroundImage);
                Assert.IsNotEmpty(game.CoverImage);

                var files = Directory.GetFiles(db.GetFileStoragePath(game.Id));
                Assert.AreEqual(3, files.Count());

                db.Games.Remove(game);
                files = Directory.GetFiles(db.GetFileStoragePath(game.Id));
                Assert.AreEqual(0, files.Count());
            }
        }

        [Test]
        public void FieldsInUseTests()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                GameDatabase.GenerateSampleData(db);

                var eventCount = 0;
                db.AgeRatingsInUseUpdated += (_, __) => eventCount++;
                db.CategoriesInUseUpdated += (_, __) => eventCount++;
                db.DevelopersInUseUpdated += (_, __) => eventCount++;
                db.FeaturesInUseUpdated += (_, __) => eventCount++;
                db.GenresInUseUpdated += (_, __) => eventCount++;
                db.PlatformsInUseUpdated += (_, __) => eventCount++;
                db.PublishersInUseUpdated += (_, __) => eventCount++;
                db.RegionsInUseUpdated += (_, __) => eventCount++;
                db.SeriesInUseUpdated += (_, __) => eventCount++;
                db.SourcesInUseUpdated += (_, __) => eventCount++;
                db.TagsInUseUpdated += (_, __) => eventCount++;

                Assert.AreEqual(1, db.UsedAgeRatings.Count);
                Assert.AreEqual(1, db.UsedCategories.Count);
                Assert.AreEqual(1, db.UsedDevelopers.Count);
                Assert.AreEqual(1, db.UsedFeastures.Count);
                Assert.AreEqual(1, db.UsedGenres.Count);
                Assert.AreEqual(1, db.UsedPlatforms.Count);
                Assert.AreEqual(1, db.UsedPublishers.Count);
                Assert.AreEqual(1, db.UsedRegions.Count);
                Assert.AreEqual(1, db.UsedSeries.Count);
                Assert.AreEqual(1, db.UsedSources.Count);
                Assert.AreEqual(1, db.UsedTags.Count);

                var addedCat = db.Categories.Add("new category");
                var addedCat2 = db.Categories.Add("new category2");
                Assert.AreEqual(1, db.UsedCategories.Count);

                var game = db.Games.First();
                game.CategoryIds.Add(addedCat.Id);
                db.Games.Update(game);
                Assert.AreEqual(2, db.UsedCategories.Count);
                db.Categories.Remove(addedCat);
                Assert.AreEqual(1, db.UsedCategories.Count);

                var newGame = new Game("test");
                newGame.CategoryIds = new List<Guid> { addedCat.Id, addedCat2.Id };
                db.Games.Add(newGame);
                Assert.AreEqual(2, db.UsedCategories.Count);
            }
        }
    }
}
