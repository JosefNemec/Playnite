using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.Models;
using PlayniteUI;
using Playnite;
using LiteDB;

namespace PlayniteUITests
{
    [TestFixture]
    public class GamesEditorTests
    {
        [Test]
        public void GetMultiGameEditObject_StandardTest()
        {
            var platId = ObjectId.NewObjectId();

            // All common
            var gamesCommon = new List<Game>()
            {
                new Game()
                {
                    Name = "Game",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId

                },
                new Game()
                {
                    Name = "Game",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId
                },
                new Game()
                {
                    Name = "Game",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId
                }
            };

            var gameCommon = GameHandler.GetMultiGameEditObject(gamesCommon);
            var firstGame = gamesCommon.First();
            Assert.AreEqual(firstGame.Name, gameCommon.Name);
            CollectionAssert.AreEqual(firstGame.Genres, gameCommon.Genres);
            Assert.AreEqual(firstGame.ReleaseDate, gameCommon.ReleaseDate);
            CollectionAssert.AreEqual(firstGame.Developers, gameCommon.Developers);
            CollectionAssert.AreEqual(firstGame.Publishers, gameCommon.Publishers);
            CollectionAssert.AreEqual(firstGame.Categories, gameCommon.Categories);
            Assert.AreEqual(firstGame.Description, gameCommon.Description);
            Assert.AreEqual(firstGame.PlatformId, gameCommon.PlatformId);


            // No common
            var gamesNoCommon = new List<Game>()
            {
                new Game()
                {
                    Name = "Game 1",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description 1",
                    PlatformId = ObjectId.NewObjectId()

                },
                new Game()
                {
                    Name = "Game 2",
                    Genres = new ComparableList<string>() { "Genre 4", "Genre 5", "Genre 6" },
                    ReleaseDate = new DateTime(2012,6,20),
                    Developers = new ComparableList<string>() { "Developer 4", "Developer 5", "Developer 6" },
                    Publishers = new ComparableList<string>() { "Publisher 4", "Publisher 5", "Publisher 6" },
                    Categories = new ComparableList<string>() { "Tag 4", "Tag 5", "Tag 6" },
                    Description = "Description 2",
                    PlatformId = ObjectId.NewObjectId()
                },
                new Game()
                {
                    Name = "Game 3",
                    Genres = new ComparableList<string>() { "Genre 7", "Genre 8", "Genre 9" },
                    ReleaseDate = new DateTime(2013,6,20),
                    Developers = new ComparableList<string>() { "Developer 7", "Developer 8", "Developer 9" },
                    Publishers = new ComparableList<string>() { "Publisher 7", "Publisher 8", "Publisher 9" },
                    Categories = new ComparableList<string>() { "Tag 7", "Tag 8", "Tag 9" },
                    Description = "Description 3",
                    PlatformId = ObjectId.NewObjectId()
                }
            };

            var gameNoCommon = GameHandler.GetMultiGameEditObject(gamesNoCommon);
            Assert.IsNull(gameNoCommon.Name);
            Assert.IsNull(gameNoCommon.ReleaseDate);
            Assert.IsNull(gameNoCommon.Description);
            Assert.IsNull(gameNoCommon.PlatformId);
            CollectionAssert.AreEqual(null, gameNoCommon.Genres);
            CollectionAssert.AreEqual(null, gameNoCommon.Developers);
            CollectionAssert.AreEqual(null, gameNoCommon.Publishers);
            CollectionAssert.AreEqual(null, gameNoCommon.Categories);
        }
    }
}
