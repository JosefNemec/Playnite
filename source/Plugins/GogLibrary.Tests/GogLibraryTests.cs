using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.SDK.Models;
using Playnite;
using Moq;
using Playnite.SDK;
using Playnite.Tests;

namespace GogLibrary.Tests
{
    [TestFixture]
    public class GogLibraryTests
    {
        public static GogLibrary CreateLibrary()
        {
            return new GogLibrary(PlayniteTests.GetTestingApi().Object);
        }

        [Test]
        public void GetInstalledGamesRegistry()
        {
            var gogLib = CreateLibrary();
            var games = gogLib.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games.Values)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.GameId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayAction);
            }
        }

    }
}