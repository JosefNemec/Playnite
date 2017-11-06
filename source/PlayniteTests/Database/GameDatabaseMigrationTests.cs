using LiteDB;
using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.Models;
using Playnite.Providers.Steam;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameDatabaseMigrationTests
    {
        [Test]
        public void PlatformGeneratorTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "defaultplatforms.db");
            FileSystem.DeleteFile(path);
            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var platforms = db.PlatformsCollection.FindAll();
                CollectionAssert.IsNotEmpty(platforms);                
                db.RemovePlatform(platforms);
                CollectionAssert.IsEmpty(db.PlatformsCollection.FindAll());
                db.AddPlatform(new Platform("Test"));
                db.AddPlatform(new Platform("Test2"));
            }
                        
            using (db.OpenDatabase(path))
            {
                var platforms = db.PlatformsCollection.FindAll().ToList();
                Assert.AreEqual(2, platforms.Count);
                Assert.AreEqual("Test", platforms[0].Name);
                Assert.AreEqual("Test2", platforms[1].Name);
            }
        }

        [Test]
        public void Migration0toCurrentTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "migration_0_Current.db");
            FileSystem.DeleteFile(path);

            var games = new List<Playnite.Models.Old0.Game>()
            {
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId1",
                    Name = "Test Name 1",
                    CommunityHubUrl = @"http://communityurl.com",
                    StoreUrl = @"http://storeurl.com",
                    WikiUrl = @"http://wiki.com"
                },
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId2",
                    Name = "Test Name 2",
                    CommunityHubUrl = @"http://communityurl.com"
                },
                new Playnite.Models.Old0.Game()
                {
                    Provider = Provider.Custom,
                    ProviderId = "TestId3",
                    Name = "Test Name 3"
                }
            };

            using (var database = new LiteDatabase(path))
            {
                database.Engine.UserVersion = 0;
                var collection = database.GetCollection<Playnite.Models.Old0.Game>("games");
                foreach (var game in games)
                {
                    var id = collection.Insert(game);
                    var genericCollection = database.GetCollection("games");
                    var record = genericCollection.FindById(id);
                    record.AsDocument["_type"] = "Playnite.Models.Game, Playnite";
                    genericCollection.Update(record.AsDocument);
                }
            }

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                Assert.IsTrue(db.GamesCollection.Count() == 3);
                var migratedGames = db.GamesCollection.FindAll().ToList();
                Assert.AreEqual(3, migratedGames[0].Links.Count);
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Store").Url));
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Wiki").Url));
                Assert.IsFalse(string.IsNullOrEmpty(migratedGames[0].Links.First(a => a.Name == "Forum").Url));
                Assert.AreEqual(1, migratedGames[1].Links.Count);
                Assert.IsNull(migratedGames[2].Links);
            }
        }
    }
}
