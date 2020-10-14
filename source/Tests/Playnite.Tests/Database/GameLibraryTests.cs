using Moq;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameLibraryTests
    {
        [Test]
        public void PlaytimeImportTest()
        {
            var gameId = "tesId";
            var libPlugin = new Mock<LibraryPlugin>(MockBehavior.Loose, null);
            var timeToImport = 500;
            libPlugin.Setup(a => a.Id).Returns(Guid.NewGuid());
            libPlugin.Setup(a => a.GetGames()).Returns(() => new List<GameInfo>
            {
                new GameInfo()
                {
                    GameId = gameId,
                    Playtime = timeToImport
                }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.ImportGames(libPlugin.Object, true, new List<ImportExclusionItem>());
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                timeToImport = 600;
                db.ImportGames(libPlugin.Object, false, new List<ImportExclusionItem>());
                Assert.AreEqual(500, db.Games.First().Playtime);
                db.ImportGames(libPlugin.Object, true, new List<ImportExclusionItem>());
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                var g = db.Games.First();
                g.Playtime = 0;
                db.Games.Update(g);
                Assert.AreEqual(0, db.Games.First().Playtime);
                db.ImportGames(libPlugin.Object, false, new List<ImportExclusionItem>());
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);
            }
        }
    }
}
