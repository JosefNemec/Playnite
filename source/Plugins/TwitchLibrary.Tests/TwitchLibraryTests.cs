using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Moq;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Tests;

namespace TwitchLibrary.Tests
{
    [TestFixture]
    public class TwitchLibraryTests
    {
        public static TwitchLibrary CreateLibrary()
        {
            return new TwitchLibrary(PlayniteTests.GetTestingApi().Object);
        }


        [Test]
        public void GetInstalledGamesTest()
        {
            var twitchLib = CreateLibrary();
            var games = twitchLib.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games.Values)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.GameId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsTrue(Directory.Exists(game.InstallDirectory));
                Assert.IsNotNull(game.PlayAction);
                Assert.IsTrue(game.PlayAction.Type == GameActionType.URL);
            }
        }
    }
}