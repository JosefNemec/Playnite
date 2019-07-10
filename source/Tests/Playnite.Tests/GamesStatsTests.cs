using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.Models;
using Playnite.SDK.Models;

namespace Playnite.Tests
{
    [TestFixture]
    public class GamesStatsTests
    {
        [Test]
        public void BasicTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Games.Add(new List<Game>
                {
                    new Game("Game 1"),
                    new Game("Steam game 1")
                    {
                        IsInstalled = true,
                        Hidden = true
                    },
                    new Game("Origin game 1")
                    {
                    },
                    new Game("GOG game 1")
                    {
                        Hidden = true
                    },
                    new Game("GOG game 2")
                    {
                        Hidden = false
                    }
                });

                var stats = new DatabaseStats(db);

                Assert.AreEqual(5, stats.Total);
                Assert.AreEqual(1, stats.Installed);
                Assert.AreEqual(2, stats.Hidden);
                Assert.AreEqual(0, stats.Favorite);

                var newGame = new Game("Game 2") { Favorite = true };
                db.Games.Add(newGame);
                Assert.AreEqual(6, stats.Total);
                Assert.AreEqual(1, stats.Favorite);

                newGame.IsInstalled = true;
                db.Games.Update(newGame);
                Assert.AreEqual(2, stats.Installed);
            }
        }
    }
}
