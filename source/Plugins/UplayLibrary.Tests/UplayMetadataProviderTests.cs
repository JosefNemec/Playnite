using NUnit.Framework;
using Playnite.SDK.Models;
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
            var metadata = provider.GetMetadata(new Game() { GameId = games.First().GameId });
            Assert.IsNotNull(metadata.Icon);
        }
    }
}
