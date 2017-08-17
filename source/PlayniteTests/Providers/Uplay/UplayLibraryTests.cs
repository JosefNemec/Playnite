using NUnit.Framework;
using Playnite.Providers.Uplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Providers.Uplay
{
    [TestFixture]
    public class UplayLibraryTests
    {
        [Test]
        public void GetInstalledGamesTest()
        {
            var library = new UplayLibrary();
            var games = library.GetInstalledGames();
            CollectionAssert.IsNotEmpty(games);
        }

        [Test]
        public void UpdateGameWithMetadataTest()
        {
            var library = new UplayLibrary();
            var games = library.GetInstalledGames();
            var game = games.First();
            var metadata = library.UpdateGameWithMetadata(game);
            Assert.IsNotNull(metadata.Icon);
        }
    }
}
