using Moq;
using NUnit.Framework;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Tests
{
    [TestFixture]
    public class BattleNetLibraryTests
    {
        public static EpicLibrary CreateLibrary()
        {
            var api = new Mock<IPlayniteAPI>();
            return new EpicLibrary(api.Object);
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
