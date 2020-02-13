using NLog;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class MultiEditGame : Game
    {
        public List<Guid> DistinctGenreIds { get; set; }
        public List<Guid> DistinctDeveloperIds { get; set; }
        public List<Guid> DistinctPublisherIds { get; set; }
        public List<Guid> DistinctCategoryIds { get; set; }
        public List<Guid> DistinctTagIds { get; set; }
        public List<Guid> DistinctFeatureIds { get; set; }
    }

    public class GameTools
    {
        public static MultiEditGame GetMultiGameEditObject(IEnumerable<Game> games)
        {
            var dummyGame = new MultiEditGame();
            if (games?.Any() != true)
            {
                return dummyGame;
            }

            var firstGame = games.First();

            var firstName = firstGame.Name;
            if (games.All(a => a.Name == firstName) == true)
            {
                dummyGame.Name = firstName;
            }

            var firstSortingName = firstGame.SortingName;
            if (games.All(a => a.SortingName == firstSortingName) == true)
            {
                dummyGame.SortingName = firstSortingName;
            }

            dummyGame.GenreIds = ListExtensions.GetCommonItems(games.Select(a => a.GenreIds)).ToList();
            dummyGame.DistinctGenreIds = ListExtensions.GetDistinctItems(games.Select(a => a.GenreIds)).ToList();

            dummyGame.DeveloperIds = ListExtensions.GetCommonItems(games.Select(a => a.DeveloperIds)).ToList();
            dummyGame.DistinctDeveloperIds = ListExtensions.GetDistinctItems(games.Select(a => a.DeveloperIds)).ToList();

            dummyGame.PublisherIds = ListExtensions.GetCommonItems(games.Select(a => a.PublisherIds)).ToList();
            dummyGame.DistinctPublisherIds = ListExtensions.GetDistinctItems(games.Select(a => a.PublisherIds)).ToList();

            dummyGame.CategoryIds = ListExtensions.GetCommonItems(games.Select(a => a.CategoryIds)).ToList();
            dummyGame.DistinctCategoryIds = ListExtensions.GetDistinctItems(games.Select(a => a.CategoryIds)).ToList();

            dummyGame.TagIds = ListExtensions.GetCommonItems(games.Select(a => a.TagIds)).ToList();
            dummyGame.DistinctTagIds = ListExtensions.GetDistinctItems(games.Select(a => a.TagIds)).ToList();

            dummyGame.FeatureIds = ListExtensions.GetCommonItems(games.Select(a => a.FeatureIds)).ToList();
            dummyGame.DistinctFeatureIds = ListExtensions.GetDistinctItems(games.Select(a => a.FeatureIds)).ToList();

            var firstReleaseDate = firstGame.ReleaseDate;
            if (games.All(a => a.ReleaseDate == firstReleaseDate) == true)
            {
                dummyGame.ReleaseDate = firstReleaseDate;
            }

            var firstDescription = firstGame.Description;
            if (games.All(a => a.Description == firstDescription) == true)
            {
                dummyGame.Description = firstDescription;
            }

            var firstPlatform = firstGame.PlatformId;
            if (games.All(a => a.PlatformId == firstPlatform) == true)
            {
                dummyGame.PlatformId = firstPlatform;
            }

            var firstLastActivity = firstGame.LastActivity;
            if (games.All(a => a.LastActivity == firstLastActivity) == true)
            {
                dummyGame.LastActivity = firstLastActivity;
            }

            var firstPlaytime = firstGame.Playtime;
            if (games.All(a => a.Playtime == firstPlaytime) == true)
            {
                dummyGame.Playtime = firstPlaytime;
            }

            var firstAdded = firstGame.Added;
            if (games.All(a => a.Added == firstAdded) == true)
            {
                dummyGame.Added = firstAdded;
            }

            var firstPlayCount = firstGame.PlayCount;
            if (games.All(a => a.PlayCount == firstPlayCount) == true)
            {
                dummyGame.PlayCount = firstPlayCount;
            }

            var firstSeries = firstGame.SeriesId;
            if (games.All(a => a.SeriesId == firstSeries) == true)
            {
                dummyGame.SeriesId = firstSeries;
            }

            var firstVersion = firstGame.Version;
            if (games.All(a => a.Version == firstVersion) == true)
            {
                dummyGame.Version = firstVersion;
            }

            var firstAgeRating = firstGame.AgeRatingId;
            if (games.All(a => a.AgeRatingId == firstAgeRating) == true)
            {
                dummyGame.AgeRatingId = firstAgeRating;
            }

            var firstRegion = firstGame.RegionId;
            if (games.All(a => a.RegionId == firstRegion) == true)
            {
                dummyGame.RegionId = firstRegion;
            }

            var firstSource = firstGame.SourceId;
            if (games.All(a => a.SourceId == firstSource) == true)
            {
                dummyGame.SourceId = firstSource;
            }

            var firstCompletionStatus = firstGame.CompletionStatus;
            if (games.All(a => a.CompletionStatus == firstCompletionStatus) == true)
            {
                dummyGame.CompletionStatus = firstCompletionStatus;
            }

            var firstUserScore = firstGame.UserScore;
            if (games.All(a => a.UserScore == firstUserScore) == true)
            {
                dummyGame.UserScore = firstUserScore;
            }

            var firstCriticScore = firstGame.CriticScore;
            if (games.All(a => a.CriticScore == firstCriticScore) == true)
            {
                dummyGame.CriticScore = firstCriticScore;
            }

            var firstCommunityScore = firstGame.CommunityScore;
            if (games.All(a => a.CommunityScore == firstCommunityScore) == true)
            {
                dummyGame.CommunityScore = firstCommunityScore;
            }

            var firstHidden = firstGame.Hidden;
            if (games.All(a => a.Hidden == firstHidden) == true)
            {
                dummyGame.Hidden = firstHidden;
            }

            var firstFavorite = firstGame.Favorite;
            if (games.All(a => a.Favorite == firstFavorite) == true)
            {
                dummyGame.Favorite = firstFavorite;
            }

            var firstScriptLanuage = firstGame.ActionsScriptLanguage;
            if (games.All(a => a.ActionsScriptLanguage == firstScriptLanuage) == true)
            {
                dummyGame.ActionsScriptLanguage = firstScriptLanuage;
            }

            var firstPreScript = firstGame.PreScript;
            if (games.All(a => string.Equals(a.PreScript, firstPreScript)))
            {
                dummyGame.PreScript = firstPreScript;
            }

            var firstPostScript = firstGame.PostScript;
            if (games.All(a => string.Equals(a.PostScript, firstPostScript)))
            {
                dummyGame.PostScript = firstPostScript;
            }

            return dummyGame;
        }
    }
}
