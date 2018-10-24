using NUnit.Framework;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class ItemCollectionTests
    {
        [Test]
        public void AddTest()
        {
            var newGame = new Game("TestGame");
            using (var temp = TempDirectory.Create())
            {                
                var col = new ItemCollection<Game>();
                col.InitializeCollection(temp.TempPath);
                col.Add(newGame);                
                var files = Directory.GetFiles(temp.TempPath);
                Assert.AreEqual(1, col.Count);
                Assert.AreEqual(1, files.Count());
                Assert.AreEqual(newGame.Id.ToString() + ".json", Path.GetFileName(files[0]));
            }
        }
    }
}
