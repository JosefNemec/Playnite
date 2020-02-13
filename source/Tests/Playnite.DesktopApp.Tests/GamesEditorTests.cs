using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite;

namespace Playnite.DesktopApp.Tests
{
    [TestFixture]
    public class GamesEditorTests
    {
        [Test]
        public void GetMultiGameEditObject_StandardTest()
        {
            var platId = Guid.NewGuid();
            var genres = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var developers = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var publishers = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var categories = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var tags = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var features = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var series = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var ratings = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var regions = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            var sources = new List<Guid>
            {
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()
            };

            // All common
            var gamesCommon = new List<Game>()
            {
                new Game()
                {
                    Name = "Game",
                    GenreIds = new List<Guid>() { genres[0], genres[1], genres[2] },
                    ReleaseDate = new DateTime(2011,6,20),
                    DeveloperIds = new List<Guid>() { developers[0], developers[1], developers[2] },
                    PublisherIds = new List<Guid>() { publishers[0], publishers[1], publishers[2] },
                    CategoryIds = new List<Guid>() { categories[0], categories[1], categories[2] },
                    TagIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    FeatureIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    SeriesId = series[0],
                    Version = "Version 3",
                    AgeRatingId = ratings[0],
                    RegionId = regions[0],
                    SourceId = sources[0],
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
                    ReleaseDate = new DateTime(2011,6,20),
                    GenreIds = new List<Guid>() { genres[0], genres[1], genres[2] },
                    DeveloperIds = new List<Guid>() { developers[0], developers[1], developers[2] },
                    PublisherIds = new List<Guid>() { publishers[0], publishers[1], publishers[2] },
                    CategoryIds = new List<Guid>() { categories[0], categories[1], categories[2] },
                    TagIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    FeatureIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    SeriesId = series[0],
                    AgeRatingId = ratings[0],
                    RegionId = regions[0],
                    SourceId = sources[0],
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Version = "Version 3",
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
                    GenreIds = new List<Guid>() { genres[0], genres[1], genres[2] },
                    ReleaseDate = new DateTime(2011,6,20),
                    DeveloperIds = new List<Guid>() { developers[0], developers[1], developers[2] },
                    PublisherIds = new List<Guid>() { publishers[0], publishers[1], publishers[2] },
                    CategoryIds = new List<Guid>() { categories[0], categories[1], categories[2] },
                    TagIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    FeatureIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    Description = "Description",
                    PlatformId = platId,
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    SeriesId = series[0],
                    Version = "Version 3",
                    AgeRatingId = ratings[0],
                    RegionId = regions[0],
                    SourceId = sources[0],
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
            CollectionAssert.AreEqual(firstGame.GenreIds, gameCommon.GenreIds);
            Assert.AreEqual(firstGame.ReleaseDate, gameCommon.ReleaseDate);
            CollectionAssert.AreEqual(firstGame.DeveloperIds, gameCommon.DeveloperIds);
            CollectionAssert.AreEqual(firstGame.PublisherIds, gameCommon.PublisherIds);
            CollectionAssert.AreEqual(firstGame.CategoryIds, gameCommon.CategoryIds);
            CollectionAssert.AreEqual(firstGame.TagIds, gameCommon.TagIds);
            CollectionAssert.AreEqual(firstGame.FeatureIds, gameCommon.FeatureIds);
            Assert.AreEqual(firstGame.Description, gameCommon.Description);
            Assert.AreEqual(firstGame.PlatformId, gameCommon.PlatformId);
            Assert.AreEqual(firstGame.LastActivity, gameCommon.LastActivity);
            Assert.AreEqual(firstGame.Added, gameCommon.Added);
            Assert.AreEqual(firstGame.Playtime, gameCommon.Playtime);
            Assert.AreEqual(firstGame.PlayCount, gameCommon.PlayCount);
            Assert.AreEqual(firstGame.SeriesId, gameCommon.SeriesId);
            Assert.AreEqual(firstGame.Version, gameCommon.Version);
            Assert.AreEqual(firstGame.AgeRatingId, gameCommon.AgeRatingId);
            Assert.AreEqual(firstGame.RegionId, gameCommon.RegionId);
            Assert.AreEqual(firstGame.SourceId, gameCommon.SourceId);
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
                    ReleaseDate = new DateTime(2011,6,20),
                    GenreIds = new List<Guid>() { genres[0], genres[1], genres[2] },
                    DeveloperIds = new List<Guid>() { developers[0], developers[1], developers[2] },
                    PublisherIds = new List<Guid>() { publishers[0], publishers[1], publishers[2] },
                    CategoryIds = new List<Guid>() { categories[0], categories[1], categories[2] },
                    TagIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    FeatureIds = new List<Guid>() { tags[0], tags[1], tags[2] },
                    SeriesId = series[0],
                    AgeRatingId = ratings[0],
                    RegionId = regions[0],
                    SourceId = sources[0],
                    Description = "Description 1",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,1),
                    Added = new DateTime(2012,1,1),
                    Playtime = 1,
                    PlayCount = 1,
                    Version = "Version 1",
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
                    GenreIds = new List<Guid>() { genres[3], genres[4], genres[5] },
                    DeveloperIds = new List<Guid>() { developers[3], developers[4], developers[5] },
                    PublisherIds = new List<Guid>() { publishers[3], publishers[4], publishers[5] },
                    CategoryIds = new List<Guid>() { categories[3], categories[4], categories[5] },
                    TagIds = new List<Guid>() { tags[3], tags[4], tags[5] },
                    FeatureIds = new List<Guid>() { tags[3], tags[4], tags[5] },
                    SeriesId = series[1],
                    AgeRatingId = ratings[1],
                    RegionId = regions[1],
                    SourceId = sources[1],
                    ReleaseDate = new DateTime(2012,6,20),
                    Description = "Description 2",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,2),
                    Added = new DateTime(2012,1,2),
                    Playtime = 2,
                    PlayCount = 2,
                    Version = "Version 2",
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
                    GenreIds = new List<Guid>() { genres[6], genres[7], genres[8] },
                    DeveloperIds = new List<Guid>() { developers[6], developers[7], developers[8] },
                    PublisherIds = new List<Guid>() { publishers[6], publishers[7], publishers[8] },
                    CategoryIds = new List<Guid>() { categories[6], categories[7], categories[8] },
                    TagIds = new List<Guid>() { tags[6], tags[7], tags[8] },
                    FeatureIds = new List<Guid>() { tags[6], tags[7], tags[8] },
                    SeriesId = series[2],
                    AgeRatingId = ratings[2],
                    RegionId = regions[2],
                    SourceId = sources[2],
                    ReleaseDate = new DateTime(2013,6,20),
                    Description = "Description 3",
                    PlatformId = Guid.NewGuid(),
                    LastActivity = new DateTime(2012,1,3),
                    Added = new DateTime(2012,1,3),
                    Playtime = 3,
                    PlayCount = 3,
                    Version = "Version 3",
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
            CollectionAssert.IsEmpty(gameNoCommon.GenreIds);
            CollectionAssert.IsEmpty(gameNoCommon.DeveloperIds);
            CollectionAssert.IsEmpty(gameNoCommon.PublisherIds);
            CollectionAssert.IsEmpty(gameNoCommon.CategoryIds);
            CollectionAssert.IsEmpty(gameNoCommon.TagIds);
            CollectionAssert.IsEmpty(gameNoCommon.FeatureIds);
            Assert.IsNull(gameNoCommon.LastActivity);
            Assert.IsNull(gameNoCommon.Added);
            Assert.AreEqual(gameNoCommon.Playtime, 0);
            Assert.AreEqual(gameNoCommon.PlayCount, 0);
            Assert.AreEqual(Guid.Empty, gameNoCommon.SeriesId);
            Assert.IsNull(gameNoCommon.Version);
            Assert.AreEqual(Guid.Empty, gameNoCommon.AgeRatingId);
            Assert.AreEqual(Guid.Empty, gameNoCommon.RegionId);
            Assert.AreEqual(Guid.Empty, gameNoCommon.SourceId);
            Assert.AreEqual(gameNoCommon.CompletionStatus, CompletionStatus.NotPlayed);
            Assert.IsNull(gameNoCommon.UserScore);
            Assert.IsNull(gameNoCommon.CriticScore);
            Assert.IsNull(gameNoCommon.CommunityScore);
            Assert.IsFalse(gameNoCommon.Hidden);
            Assert.IsFalse(gameNoCommon.Favorite);
        }
    }
}
