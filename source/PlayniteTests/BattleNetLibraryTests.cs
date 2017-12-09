using NUnit.Framework;
using Playnite.Providers.BattleNet;
using Playnite.Providers.Uplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Providers.BattleNet
{
    [TestFixture]
    public class BattleNetLibraryTests
    {
        [Test]
        public void GetInstalledGamesTest()
        {
            var library = new BattleNetLibrary();
            var games = library.GetInstalledGames();
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
