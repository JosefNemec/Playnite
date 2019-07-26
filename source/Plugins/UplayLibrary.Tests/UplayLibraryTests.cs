using System;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.Tests;

namespace UplayLibrary.Tests
{
    [TestFixture]
    public class UplayLibraryTests
    {
        public static UplayLibrary CreateLibrary()
        {            
            return new UplayLibrary(PlayniteTests.GetTestingApi().Object);
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
