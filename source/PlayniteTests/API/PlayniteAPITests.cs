using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.API;
using Playnite.Database;
using Playnite.Providers;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.API
{
    [TestFixture]
    public class PlayniteAPITests
    {
        [Test]
        public void LibraryPluginLoadTest()
        {
            var api = new PlayniteAPI(
                new GameDatabase(),
                new GameControllerFactory(),
                new Mock<IDialogsFactory>().Object,
                new Mock<IMainViewAPI>().Object,
                new Mock<IPlayniteInfoAPI>().Object,
                new Mock<IPlaynitePathsAPI>().Object);

            api.LoadLibraryProviders();
            Assert.AreEqual(1, api.LibraryPlugins.Count);
        }
    }
}
