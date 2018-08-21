using System;
using Moq;
using NUnit.Framework;
using Playnite.SDK;

namespace UplayLibrary.Tests
{
    [TestFixture]
    public class UplayLibraryTests
    {
        public static UplayLibrary CreateLibrary()
        {
            var api = new Mock<IPlayniteAPI>();
            return new UplayLibrary(api.Object);
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
