using System;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.Tests;

namespace BethesdaLibrary.Tests
{
    [TestFixture]
    public class BethesdaLibraryTests
    {
        public static BethesdaLibrary CreateLibrary()
        {
            return new BethesdaLibrary(PlayniteTests.GetTestingApi().Object);
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