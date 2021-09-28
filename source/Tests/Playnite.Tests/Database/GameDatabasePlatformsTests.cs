using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameDatabasePlatformsTests
    {
        [Test]
        public void PlatformGeneratorTest()
        {
            using (var temp = TempDirectory.Create())
            using (var db = new GameDatabase(temp.TempPath))
            {
                db.OpenDatabase();
                var platforms = db.Platforms;
                CollectionAssert.IsNotEmpty(db.Platforms);
                db.Platforms.Remove(platforms);
                CollectionAssert.IsEmpty(db.Platforms);
                db.Platforms.Add(new Platform("Test"));
                db.Platforms.Add(new Platform("Test2"));
                Assert.AreEqual(2, db.Platforms.Count);
            }
        }

        [Test]
        public void PlatformRemovalTest()
        {
            using (var temp = TempDirectory.Create())
            using (var db = new GameDatabase(temp.TempPath))
            {
                db.OpenDatabase();
                var platform = new Platform("Test");
                db.Platforms.Add(platform);
                var game = new Game("Test")
                {
                    PlatformIds = new List<Guid> { platform.Id }
                };

                db.Games.Add(game);
                db.Platforms.Remove(platform);
                var dbGame = db.Games[game.Id];
                CollectionAssert.IsEmpty(dbGame.PlatformIds);
                Assert.IsNull(db.Platforms.FirstOrDefault(a => a.Name == "Test"));
            }
        }
    }
}
