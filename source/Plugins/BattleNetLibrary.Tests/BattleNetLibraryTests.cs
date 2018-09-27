using Moq;
using NUnit.Framework;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Tests
{
    [TestFixture]
    public class BattleNetLibraryTests
    {
        public static BattleNetLibrary CreateLibrary()
        {
            var api = new Mock<IPlayniteAPI>();
            return new BattleNetLibrary(api.Object);
        }

        [Test]
        public void GetInstalledGamesTest()
        {
            var library = CreateLibrary();
            var games = library.GetInstalledGames();
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
