using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Tests
{
    [TestFixture]
    public class BattleNetMetadataProviderTests
    {
        [Test]
        public void StandardDownloadTest()
        {
            var provider = new BattleNetMetadataProvider();
            var data = provider.GetMetadata(new Game() { GameId = "D3" });
            Assert.IsNotNull(data.GameInfo);
            Assert.IsNotNull(data.Icon);
            Assert.IsNotNull(data.CoverImage);
            Assert.IsNotNull(data.BackgroundImage);
            CollectionAssert.IsNotEmpty(data.GameInfo.Links);
        }
    }
}
