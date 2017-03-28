using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite.Models;
using PlayniteUI;

namespace PlayniteUITests
{
    [TestClass]
    public class GamesEditorTests
    {
        [TestMethod]
        public void GetMultiGameEditObject_StandardTest()
        {
            // All common
            var gamesCommon = new List<Game>()
            {
                new Game()
                {
                    Name = "Game",
                    Genres = new List<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new List<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new List<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new List<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    StoreUrl = "Store Url",
                    WikiUrl = "Wiki Url",
                    CommunityHubUrl = "Community Url",
                    Description = "Description"

                },
                new Game()
                {
                    Name = "Game",
                    Genres = new List<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new List<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new List<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new List<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    StoreUrl = "Store Url",
                    WikiUrl = "Wiki Url",
                    CommunityHubUrl = "Community Url",
                    Description = "Description"
                },
                new Game()
                {
                    Name = "Game",
                    Genres = new List<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new List<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new List<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new List<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    StoreUrl = "Store Url",
                    WikiUrl = "Wiki Url",
                    CommunityHubUrl = "Community Url",
                    Description = "Description"
                }
            };

            var gameCommon = GamesEditor.GetMultiGameEditObject(gamesCommon);
            var firstGame = gamesCommon.First();
            Assert.AreEqual(firstGame.Name, gameCommon.Name);
            CollectionAssert.AreEqual(firstGame.Genres, gameCommon.Genres);
            Assert.AreEqual(firstGame.ReleaseDate, gameCommon.ReleaseDate);
            CollectionAssert.AreEqual(firstGame.Developers, gameCommon.Developers);
            CollectionAssert.AreEqual(firstGame.Publishers, gameCommon.Publishers);
            CollectionAssert.AreEqual(firstGame.Categories, gameCommon.Categories);
            Assert.AreEqual(firstGame.StoreUrl, gameCommon.StoreUrl);
            Assert.AreEqual(firstGame.WikiUrl, gameCommon.WikiUrl);
            Assert.AreEqual(firstGame.CommunityHubUrl, gameCommon.CommunityHubUrl);
            Assert.AreEqual(firstGame.Description, gameCommon.Description);


            // No common
            var gamesNoCommon = new List<Game>()
            {
                new Game()
                {
                    Name = "Game 1",
                    Genres = new List<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new List<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new List<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new List<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    StoreUrl = "Store Url 1",
                    WikiUrl = "Wiki Url 1",
                    CommunityHubUrl = "Community Url 1",
                    Description = "Description 1"

                },
                new Game()
                {
                    Name = "Game 2",
                    Genres = new List<string>() { "Genre 4", "Genre 5", "Genre 6" },
                    ReleaseDate = new DateTime(2012,6,20),
                    Developers = new List<string>() { "Developer 4", "Developer 5", "Developer 6" },
                    Publishers = new List<string>() { "Publisher 4", "Publisher 5", "Publisher 6" },
                    Categories = new List<string>() { "Tag 4", "Tag 5", "Tag 6" },
                    StoreUrl = "Store Url 2",
                    WikiUrl = "Wiki Url 2",
                    CommunityHubUrl = "Community Url 2",
                    Description = "Description 2"
                },
                new Game()
                {
                    Name = "Game 3",
                    Genres = new List<string>() { "Genre 7", "Genre 8", "Genre 9" },
                    ReleaseDate = new DateTime(2013,6,20),
                    Developers = new List<string>() { "Developer 7", "Developer 8", "Developer 9" },
                    Publishers = new List<string>() { "Publisher 7", "Publisher 8", "Publisher 9" },
                    Categories = new List<string>() { "Tag 7", "Tag 8", "Tag 9" },
                    StoreUrl = "Store Url 3",
                    WikiUrl = "Wiki Url 3",
                    CommunityHubUrl = "Community Url 3",
                    Description = "Description 3"
                }
            };

            var gameNoCommon = GamesEditor.GetMultiGameEditObject(gamesNoCommon);
            Assert.AreEqual(null, gameNoCommon.Name);
            CollectionAssert.AreEqual(null, gameNoCommon.Genres);
            Assert.AreEqual(null, gameNoCommon.ReleaseDate);
            CollectionAssert.AreEqual(null, gameNoCommon.Developers);
            CollectionAssert.AreEqual(null, gameNoCommon.Publishers);
            CollectionAssert.AreEqual(null, gameNoCommon.Categories);
            Assert.AreEqual(null, gameNoCommon.StoreUrl);
            Assert.AreEqual(null, gameNoCommon.WikiUrl);
            Assert.AreEqual(null, gameNoCommon.CommunityHubUrl);
            Assert.AreEqual(null, gameNoCommon.Description);
        }
    }
}
