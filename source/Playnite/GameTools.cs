using NLog;
using Playnite.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class GameTools
    {
        public static Game GetMultiGameEditObject(IEnumerable<Game> games)
        {
            var dummyGame = new Game();
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

            var firstGenres = firstGame.Genres;
            if (games.All(a => a.Genres.IsListEqual(firstGenres) == true))
            {
                dummyGame.Genres = firstGenres;
            }

            var firstReleaseDate = firstGame.ReleaseDate;
            if (games.All(a => a.ReleaseDate == firstReleaseDate) == true)
            {
                dummyGame.ReleaseDate = firstReleaseDate;
            }

            var firstDeveloper = firstGame.Developers;
            if (games.All(a => a.Developers.IsListEqual(firstDeveloper) == true))
            {
                dummyGame.Developers = firstDeveloper;
            }

            var firstPublisher = firstGame.Publishers;
            if (games.All(a => a.Publishers.IsListEqual(firstPublisher) == true))
            {
                dummyGame.Publishers = firstPublisher;
            }

            var firstCat = firstGame.Categories;
            if (games.All(a => a.Categories.IsListEqual(firstCat) == true))
            {
                dummyGame.Categories = firstCat;
            }

            var firstTag = firstGame.Tags;
            if (games.All(a => a.Tags.IsListEqual(firstTag) == true))
            {
                dummyGame.Tags = firstTag;
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

            var firstSeries = firstGame.Series;
            if (games.All(a => a.Series == firstSeries) == true)
            {
                dummyGame.Series = firstSeries;
            }

            var firstVersion = firstGame.Version;
            if (games.All(a => a.Version == firstVersion) == true)
            {
                dummyGame.Version = firstVersion;
            }

            var firstAgeRating = firstGame.AgeRating;
            if (games.All(a => a.AgeRating == firstAgeRating) == true)
            {
                dummyGame.AgeRating = firstAgeRating;
            }

            var firstRegion = firstGame.Region;
            if (games.All(a => a.Region == firstRegion) == true)
            {
                dummyGame.Region = firstRegion;
            }

            var firstSource = firstGame.Source;
            if (games.All(a => a.Source == firstSource) == true)
            {
                dummyGame.Source = firstSource;
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

            return dummyGame;
        }
    }
}
