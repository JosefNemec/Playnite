using Moq;
using NUnit.Framework;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class GameLibraryTests
    {
        [Test]
        public void PlaytimeImportTest()
        {
            var gameId = "tesId";
            var libPlugin = new Mock<ILibraryPlugin>();
            var timeToImport = 500;
            libPlugin.Setup(a => a.Id).Returns(Guid.NewGuid());
            libPlugin.Setup(a => a.GetGames()).Returns(() => new List<Game>
            {
                new Game()
                {
                    GameId = gameId,
                    Playtime = timeToImport,
                    PluginId = libPlugin.Object.Id
                }
            });

            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                GameLibrary.ImportGames(libPlugin.Object, db, true);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                timeToImport = 600;
                GameLibrary.ImportGames(libPlugin.Object, db, false);
                Assert.AreEqual(500, db.Games.First().Playtime);
                GameLibrary.ImportGames(libPlugin.Object, db, true);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);

                var g = db.Games.First();
                g.Playtime = 0;
                db.Games.Update(g);
                Assert.AreEqual(0, db.Games.First().Playtime);
                GameLibrary.ImportGames(libPlugin.Object, db, false);
                Assert.AreEqual(timeToImport, db.Games.First().Playtime);
            }
        }
    }
}
