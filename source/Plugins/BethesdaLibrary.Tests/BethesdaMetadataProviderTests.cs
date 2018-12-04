using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary.Tests
{
    [TestFixture]
    public class BethesdaMetadataProviderTests
    {
        [Test]
        public void UpdateGameWithMetadataTest()
        {
            var library = BethesdaLibraryTests.CreateLibrary();
            var provider = new BethesdaMetadataProvider();
            var games = library.GetInstalledGames();
            var game = games.First();
            var metadata = provider.GetMetadata(game);
            Assert.IsNotNull(metadata.Icon);
        }
    }
}
