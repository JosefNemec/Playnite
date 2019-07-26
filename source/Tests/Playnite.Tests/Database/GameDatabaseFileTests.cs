using LiteDB;
using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameDatabaseFileTests
    {
        [Test]
        public void AddFileTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var file = PlayniteTests.GenerateFakeFile();
                var testId = Guid.NewGuid();
                var addedId = db.AddFile(file.FileName, file.Content, testId);
                FileAssert.Exists(Path.Combine(temp.TempPath, "files", addedId));
            }
        }

        [Test]
        public void AddFileHttpTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var testId = Guid.NewGuid();
                var addedId = db.AddFile(@"https://playnite.link/applogo.png", testId);
                FileAssert.Exists(Path.Combine(temp.TempPath, "files", addedId));
            }
        }

        [Test]
        public void AddFileHttp404Test()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var testId = Guid.NewGuid();
                var addedId = db.AddFile(@"https://playnite.link/doesntexists.png", testId);
                Assert.IsNull(addedId);
                var files = Directory.GetFiles(Path.Combine(temp.TempPath, "files", testId.ToString()));
                Assert.AreEqual(0, files.Count());
            }
        }
    }
}
