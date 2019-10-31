using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Tests
{
    [TestFixture]
    public class ItchioMetadataProviderTests
    {
        [Test]
        public void GetMetadataTest()
        {
            var provider = new ItchioMetadataProvider(ItchioLibraryTets.CreateLibrary());
            var metadata = provider.GetMetadata(new Game { GameId = "356189" });
        }
    }
}
