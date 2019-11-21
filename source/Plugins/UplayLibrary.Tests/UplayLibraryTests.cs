using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.Tests;
using ProtoBuf;

namespace UplayLibrary.Tests
{
    [TestFixture]
    public class UplayLibraryTests
    {
        public static UplayLibrary CreateLibrary()
        {            
            return new UplayLibrary(PlayniteTests.GetTestingApi().Object);
        }

        [Test]
        public void GetInstalledGamesTest()
        {
            var library = CreateLibrary();
            var games = library.GetInstalledGames();
            CollectionAssert.IsNotEmpty(games);
        }

        [Test]
        public void GetOwnedGamesTest()
        {
            var library = CreateLibrary();
            var games = library.GetLibraryGames();
            CollectionAssert.IsNotEmpty(games);
        }

        [Test]
        public void ConfigurationsParsingTest()
        {
            var cachePath = Uplay.ConfigurationsCachePath;
            FileAssert.Exists(cachePath);
            var products = Uplay.GetLocalProductCache();
            CollectionAssert.IsNotEmpty(products);
        }
    }
}
