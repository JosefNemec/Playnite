using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Tests
{
    [TestFixture]
    public class EpicLibraryTests
    {
        public static EpicLibrary CreateLibrary()
        {
            return new EpicLibrary(PlayniteTests.GetTestingApi().Object);
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
