using System;
using Moq;
using NUnit.Framework;
using Playnite.SDK;

namespace BethesdaLibrary.Tests
{
    [TestFixture]
    public class BethesdaLibraryTests
    {
        public static BethesdaLibrary CreateLibrary()
        {
            var api = new Mock<IPlayniteAPI>();
            return new BethesdaLibrary(api.Object);
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