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
    public class GameDatabaseFileTests
    {
        [Test]
        public void CheckSumCreationTest()
        {
            var image = Path.Combine(Paths.ProgramFolder, "Resources", "Images", "applogo.png");
            var db = new GameDatabase(null);
            using (db.OpenDatabase(new MemoryStream()))
            {
                db.AddFile("test.png", "test.png", File.ReadAllBytes(image));
                var dbImage = db.GetFile("test.png");
                Assert.IsFalse(string.IsNullOrEmpty(dbImage.Metadata["checksum"].AsString));
            }
        }

        [Test]
        public void MultiAddtionTest()
        {
            var image = Path.Combine(Paths.ProgramFolder, "Resources", "Images", "applogo.png");
            var db = new GameDatabase(null);
            using (db.OpenDatabase(new MemoryStream()))
            {
                db.AddFile("test.png", "test.png", File.ReadAllBytes(image));
                var id = db.AddFileNoDuplicate("test2.png", "test2.png", File.ReadAllBytes(image));
                Assert.AreEqual("test.png", id);
                Assert.AreEqual(1, db.Database.FileStorage.FindAll().Count());
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(new MemoryStream()))
            {
                db.AddFileNoDuplicate("test.png", "test.png", File.ReadAllBytes(image));
                var id = db.AddFileNoDuplicate("test2.png", "test2.png", File.ReadAllBytes(image));
                Assert.AreEqual("test.png", id);
                Assert.AreEqual(1, db.Database.FileStorage.FindAll().Count());
            }

            db = new GameDatabase(null);
            using (db.OpenDatabase(new MemoryStream()))
            {
                db.AddFileNoDuplicate("test.png", "test.png", File.ReadAllBytes(image));
                db.AddFile("test2.png", "test2.png", File.ReadAllBytes(image));
                Assert.AreEqual(2, db.Database.FileStorage.FindAll().Count());
            }
        }
    }
}
