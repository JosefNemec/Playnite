using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Tests
{
    [TestFixture]
    public class ItchioLibraryTets
    {
        public static ItchioLibrary CreateLibrary()
        {
            return new ItchioLibrary(PlayniteTests.GetTestingApi().Object);
        }

        [Test]
        public void GetInstalledGamesTest()
        {
            var library = CreateLibrary();
            var games = library.GetInstalledGames();
            CollectionAssert.IsNotEmpty(games);
        }

        [Test]
        public void GetLibraryGamesTest()
        {
            var library = CreateLibrary();
            var games = library.GetLibraryGames();
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
