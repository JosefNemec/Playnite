﻿using NUnit.Framework;
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
using SteamKit2;

namespace SteamLibrary.Tests
{
    [TestFixture]
    public class SteamLibraryTests
    {
        public static SteamLibrary CreateLibrary()
        {
            var api = new Mock<IPlayniteAPI>();
            api.Setup(a => a.GetPluginUserDataPath(It.IsAny<ILibraryPlugin>())).Returns(() => SteamTests.TempPath);
            return new SteamLibrary(api.Object, null);
        }

        [Test]
        public void GetInstalledGamesTest()
        {
            var steamLib = CreateLibrary();
            var games = steamLib.GetInstalledGames();
            Assert.AreNotEqual(0, games.Count);
            CollectionAssert.AllItemsAreUnique(games);

            foreach (var game in games.Values)
            {
                Assert.IsFalse(string.IsNullOrEmpty(game.Name));
                Assert.IsFalse(string.IsNullOrEmpty(game.GameId));
                Assert.IsFalse(string.IsNullOrEmpty(game.InstallDirectory));
                Assert.IsNotNull(game.PlayAction);
                Assert.IsTrue(game.PlayAction.Type == GameActionType.URL);
            }
        }

        [Test]
        public void GetCategorizedGamesTest()
        {
            var steamLib = CreateLibrary();
            var user = steamLib.GetSteamUsers().First(a => a.Recent);
            var cats = steamLib.GetCategorizedGames(user.Id);
            var game = cats.First();
            CollectionAssert.IsNotEmpty(cats);
            if (!game.Hidden)
            {
                CollectionAssert.IsNotEmpty(game.Categories);
            }
            Assert.IsFalse(string.IsNullOrEmpty(game.GameId));
        }

        [Test]
        public void GetSteamUsersTest()
        {
            var steamLib = CreateLibrary();
            var users = steamLib.GetSteamUsers();
            CollectionAssert.IsNotEmpty(users);
            var user = users.First();
            Assert.IsFalse(string.IsNullOrEmpty(user.AccountName));
            Assert.IsFalse(string.IsNullOrEmpty(user.PersonaName));
        }

        [Test]
        public void GetAppStateTest()
        {
            var steamLib = CreateLibrary();
            var games = steamLib.GetInstalledGames();
            var state = Steam.GetAppState(games.Values.First().ToSteamGameID());
            Assert.IsTrue(state.Installed);
        }
    }
}