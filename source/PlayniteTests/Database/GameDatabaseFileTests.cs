using LiteDB;
using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.Models;
using Playnite.Settings;
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
        public void AddFileTest()
        {
            using (var temp = TempDirectory.Create(false))
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var file = PlayniteTests.GenerateFakeFile();
                var testId = Guid.NewGuid();
                var addedId = db.AddFile(file.FileName, file.Content, testId);
                FileAssert.Exists(Path.Combine(temp.TempPath, "files", addedId));
            }
        }
    }
}
