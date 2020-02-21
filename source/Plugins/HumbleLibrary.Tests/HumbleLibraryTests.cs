using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Playnite;
using Playnite.Tests;

namespace HumbleLibrary.Tests
{
    [TestFixture]
    public class HumbleLibraryTests
    {
        public static HumbleLibrary CreateLibrary()
        {
            return new HumbleLibrary(PlayniteTests.GetTestingApi().Object);
        }

        [Test]
        public void GetTroveGamesTest()
        {
            var games = HumbleLibrary.GetTroveGames();
            CollectionAssert.IsNotEmpty(games);
        }
    }
}
