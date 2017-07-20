using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite;
using Playnite.Models;

namespace PlayniteTests
{
    [TestFixture]
    public class GamesStatsTests
    {
        //[Test]
        //public void BasicTest()
        //{
        //    var stats = new GamesStats();
        //    var games = new ObservableCollection<IGame>
        //    {
        //        new Game("Game 1"),
        //        new Game("Steam game 1")
        //        {
        //            Provider = Provider.Steam,
        //            PlayTask = new GameTask(),
        //            Hidden = true
        //        },
        //        new Game("Origin game 1")
        //        {
        //            Provider = Provider.Origin
        //        },
        //        new Game("GOG game 1")
        //        {
        //            Provider = Provider.GOG,
        //            Hidden = true
        //        },
        //        new Game("GOG game 2")
        //        {
        //            Provider = Provider.GOG,
        //            Hidden = false
        //        }
        //    };

        //    stats.SetGames(games);

        //    Assert.AreEqual(5, stats.Total);
        //    Assert.AreEqual(2, stats.GOG);
        //    Assert.AreEqual(1, stats.Steam);
        //    Assert.AreEqual(1, stats.Origin);
        //    Assert.AreEqual(1, stats.Custom);
        //    Assert.AreEqual(1, stats.Installed);
        //    Assert.AreEqual(2, stats.Hidden);

        //    games.Add(new Game("Game 2"));
        //    Assert.AreEqual(6, stats.Total);
        //    Assert.AreEqual(2, stats.Custom);

        //    games[1].Hidden = false;
        //    Assert.AreEqual(1, stats.Hidden);

        //    games[0].PlayTask = new GameTask();
        //    Assert.AreEqual(2, stats.Installed);
        //}
    }
}
