using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary.Tests
{
    [TestFixture]
    public class UplayMetadataProviderTests
    {
        [Test]
        public void UpdateGameWithMetadataTest()
        {
            var library = UplayLibraryTests.CreateLibrary();
            var provider = new UplayMetadataProvider();            
            var games = library.GetInstalledGames();
            var game = games.First();
            var metadata = provider.GetMetadata(game);
            Assert.IsNotNull(metadata.Icon);
        }
    }
}
