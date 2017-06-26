using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.Models;
using PlayniteUI;

namespace PlayniteUITests
{
    [TestFixture]
    public class GamesEditorTests
    {
        [Test]
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
            Assert.AreEqual(null, gameNoCommon.Description);
        }
    }
}
