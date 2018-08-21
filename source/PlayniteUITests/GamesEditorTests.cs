using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
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
            var platId = Guid.NewGuid();

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
                    Categories = new ComparableList<string>() { "Cat 1", "Cat 2", "Cat 3" },
                    Tags = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Series = "Series 3",
                    Version = "Version 3",
                    AgeRating = "AgeRating 3",
                    Region = "Region 3",
                    Source = "Source 3",
                    CompletionStatus = CompletionStatus.Completed,
                    UserScore = 1,
                    CriticScore = 2,
                    CommunityScore = 99,
                    Favorite = false,
                    Hidden = true
                },
                new Game()
                {
                    Name = "Game",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Cat 1", "Cat 2", "Cat 3" },
                    Tags = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Series = "Series 3",
                    Version = "Version 3",
                    AgeRating = "AgeRating 3",
                    Region = "Region 3",
                    Source = "Source 3",
                    CompletionStatus = CompletionStatus.Completed,
                    UserScore = 1,
                    CriticScore = 2,
                    CommunityScore = 99,
                    Favorite = false,
                    Hidden = true
                },
                new Game()
                {
                    Name = "Game",
                    Genres = new ComparableList<string>() { "Genre 1", "Genre 2", "Genre 3" },
                    ReleaseDate = new DateTime(2011,6,20),
                    Developers = new ComparableList<string>() { "Developer 1", "Developer 2", "Developer 3" },
                    Publishers = new ComparableList<string>() { "Publisher 1", "Publisher 2", "Publisher 3" },
                    Categories = new ComparableList<string>() { "Cat 1", "Cat 2", "Cat 3" },
                    Tags = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Series = "Series 3",
                    Version = "Version 3",
                    AgeRating = "AgeRating 3",
                    Region = "Region 3",
                    Source = "Source 3",
                    CompletionStatus = CompletionStatus.Completed,
                    UserScore = 1,
                    CriticScore = 2,
                    CommunityScore = 99,
                    Favorite = false,
                    Hidden = true
                }
            };

            var gameCommon = GameTools.GetMultiGameEditObject(gamesCommon);
            var firstGame = gamesCommon.First();
            Assert.AreEqual(firstGame.Name, gameCommon.Name);
            CollectionAssert.AreEqual(firstGame.Genres, gameCommon.Genres);
            Assert.AreEqual(firstGame.ReleaseDate, gameCommon.ReleaseDate);
            CollectionAssert.AreEqual(firstGame.Developers, gameCommon.Developers);
            CollectionAssert.AreEqual(firstGame.Publishers, gameCommon.Publishers);
            CollectionAssert.AreEqual(firstGame.Categories, gameCommon.Categories);
            CollectionAssert.AreEqual(firstGame.Tags, gameCommon.Tags);
            Assert.AreEqual(firstGame.Description, gameCommon.Description);
            Assert.AreEqual(firstGame.PlatformId, gameCommon.PlatformId);
            Assert.AreEqual(firstGame.LastActivity, gameCommon.LastActivity);
            Assert.AreEqual(firstGame.Added, gameCommon.Added);
            Assert.AreEqual(firstGame.Playtime, gameCommon.Playtime);
            Assert.AreEqual(firstGame.PlayCount, gameCommon.PlayCount);
            Assert.AreEqual(firstGame.Series, gameCommon.Series);
            Assert.AreEqual(firstGame.Version, gameCommon.Version);
            Assert.AreEqual(firstGame.AgeRating, gameCommon.AgeRating);
            Assert.AreEqual(firstGame.Region, gameCommon.Region);
            Assert.AreEqual(firstGame.Source, gameCommon.Source);
            Assert.AreEqual(firstGame.CompletionStatus, gameCommon.CompletionStatus);
            Assert.AreEqual(firstGame.UserScore, gameCommon.UserScore);
            Assert.AreEqual(firstGame.CriticScore, gameCommon.CriticScore);
            Assert.AreEqual(firstGame.CommunityScore, gameCommon.CommunityScore);
            Assert.AreEqual(firstGame.Favorite, gameCommon.Favorite);
            Assert.AreEqual(firstGame.Hidden, gameCommon.Hidden);

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
                    Categories = new ComparableList<string>() { "Cat 1", "Cat 2", "Cat 3" },
                    Tags = new ComparableList<string>() { "Tag 1", "Tag 2", "Tag 3" },
                    Description = "Description 1",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,1),
                    Added = new DateTime(2012,1,1),
                    Playtime = 1,
                    PlayCount = 1,
                    Series = "Series 1",
                    Version = "Version 1",
                    AgeRating = "AgeRating 1",
                    Region = "Region 1",
                    Source = "Source 1",
                    CompletionStatus = CompletionStatus.Beaten,
                    UserScore = 1,
                    CriticScore = 1,
                    CommunityScore = 1,
                    Favorite = false,
                    Hidden = true
                },
                new Game()
                {
                    Name = "Game 2",
                    Genres = new ComparableList<string>() { "Genre 4", "Genre 5", "Genre 6" },
                    ReleaseDate = new DateTime(2012,6,20),
                    Developers = new ComparableList<string>() { "Developer 4", "Developer 5", "Developer 6" },
                    Publishers = new ComparableList<string>() { "Publisher 4", "Publisher 5", "Publisher 6" },
                    Categories = new ComparableList<string>() { "Cat 4", "Cat 5", "Cat 6" },
                    Tags = new ComparableList<string>() { "Tag 4", "Tag 5", "Tag 6" },
                    Description = "Description 2",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,2),
                    Added = new DateTime(2012,1,2),
                    Playtime = 2,
                    PlayCount = 2,
                    Series = "Series 2",
                    Version = "Version 2",
                    AgeRating = "AgeRating 2",
                    Region = "Region 2",
                    Source = "Source 2",
                    CompletionStatus = CompletionStatus.Completed,
                    UserScore = 2,
                    CriticScore = 2,
                    CommunityScore = 2,
                    Favorite = true,
                    Hidden = false
                },
                new Game()
                {
                    Name = "Game 3",
                    Genres = new ComparableList<string>() { "Genre 7", "Genre 8", "Genre 9" },
                    ReleaseDate = new DateTime(2013,6,20),
                    Developers = new ComparableList<string>() { "Developer 7", "Developer 8", "Developer 9" },
                    Publishers = new ComparableList<string>() { "Publisher 7", "Publisher 8", "Publisher 9" },
                    Categories = new ComparableList<string>() { "Cat 7", "Cat 8", "Cat 9" },
                    Tags = new ComparableList<string>() { "Tag 7", "Tag 8", "Tag 9" },
                    Description = "Description 3",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Series = "Series 3",
                    Version = "Version 3",
                    AgeRating = "AgeRating 3",
                    Region = "Region 3",
                    Source = "Source 3",
                    CompletionStatus = CompletionStatus.NotPlayed,
                    UserScore = 3,
                    CriticScore = 3,
                    CommunityScore = 3,
                    Favorite = false,
                    Hidden = false
                }
            };

            var gameNoCommon = GameTools.GetMultiGameEditObject(gamesNoCommon);
            Assert.IsNull(gameNoCommon.Name);
            Assert.IsNull(gameNoCommon.ReleaseDate);
            Assert.IsNull(gameNoCommon.Description);
            Assert.AreEqual(Guid.Empty, gameNoCommon.PlatformId);
            CollectionAssert.AreEqual(null, gameNoCommon.Genres);
            CollectionAssert.AreEqual(null, gameNoCommon.Developers);
            CollectionAssert.AreEqual(null, gameNoCommon.Publishers);
            CollectionAssert.AreEqual(null, gameNoCommon.Categories);
            CollectionAssert.AreEqual(null, gameNoCommon.Tags);
            Assert.IsNull(gameNoCommon.LastActivity);
            Assert.IsNull(gameNoCommon.Added);
            Assert.AreEqual(gameNoCommon.Playtime, 0);
            Assert.AreEqual(gameNoCommon.PlayCount, 0);
            Assert.IsNull(gameNoCommon.Series);
            Assert.IsNull(gameNoCommon.Version);
            Assert.IsNull(gameNoCommon.AgeRating);
            Assert.IsNull(gameNoCommon.Region);
            Assert.IsNull(gameNoCommon.Source);
            Assert.AreEqual(gameNoCommon.CompletionStatus, CompletionStatus.NotPlayed);
            Assert.IsNull(gameNoCommon.UserScore);
            Assert.IsNull(gameNoCommon.CriticScore);
            Assert.IsNull(gameNoCommon.CommunityScore);
            Assert.IsFalse(gameNoCommon.Hidden);
            Assert.IsFalse(gameNoCommon.Favorite);
        }
    }
}
