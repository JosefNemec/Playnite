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
        public List<Guid> DistinctPlatformIds { get; set; }
        public List<Guid> DistinctRegionIds { get; set; }
        public List<Guid> DistinctAgeRatingIds { get; set; }
        public List<Guid> DistinctSeriesIds { get; set; }
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

            dummyGame.PlatformIds = ListExtensions.GetCommonItems(games.Select(a => a.PlatformIds)).ToList();
            dummyGame.DistinctPlatformIds = ListExtensions.GetDistinctItems(games.Select(a => a.PlatformIds)).ToList();

            dummyGame.SeriesIds = ListExtensions.GetCommonItems(games.Select(a => a.SeriesIds)).ToList();
            dummyGame.DistinctSeriesIds = ListExtensions.GetDistinctItems(games.Select(a => a.SeriesIds)).ToList();

            dummyGame.AgeRatingIds = ListExtensions.GetCommonItems(games.Select(a => a.AgeRatingIds)).ToList();
            dummyGame.DistinctAgeRatingIds = ListExtensions.GetDistinctItems(games.Select(a => a.AgeRatingIds)).ToList();

            dummyGame.RegionIds = ListExtensions.GetCommonItems(games.Select(a => a.RegionIds)).ToList();
            dummyGame.DistinctRegionIds = ListExtensions.GetDistinctItems(games.Select(a => a.RegionIds)).ToList();

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

            var firstNotes = firstGame.Notes;
            if (games.All(a => a.Notes == firstNotes) == true)
            {
                dummyGame.Notes = firstNotes;
            }

            var firstManual = firstGame.Manual;
            if (games.All(a => a.Manual == firstManual) == true)
            {
                dummyGame.Manual = firstManual;
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

            var firstInstallSize = firstGame.InstallSize;
            if (games.All(a => a.InstallSize == firstInstallSize) == true)
            {
                dummyGame.InstallSize = firstInstallSize;
            }

            var firstVersion = firstGame.Version;
            if (games.All(a => a.Version == firstVersion) == true)
            {
                dummyGame.Version = firstVersion;
            }

            var firstSource = firstGame.SourceId;
            if (games.All(a => a.SourceId == firstSource) == true)
            {
                dummyGame.SourceId = firstSource;
            }

            var firstCompletionStatus = firstGame.CompletionStatusId;
            if (games.All(a => a.CompletionStatusId == firstCompletionStatus) == true)
            {
                dummyGame.CompletionStatusId = firstCompletionStatus;
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

            var firstInstalled = firstGame.IsInstalled;
            if (games.All(a => a.IsInstalled == firstInstalled) == true)
            {
                dummyGame.IsInstalled = firstInstalled;
            }

            var firstInstallDir = firstGame.InstallDirectory;
            if (games.All(a => a.InstallDirectory == firstInstallDir) == true)
            {
                dummyGame.InstallDirectory = firstInstallDir;
            }

            var firstFavorite = firstGame.Favorite;
            if (games.All(a => a.Favorite == firstFavorite) == true)
            {
                dummyGame.Favorite = firstFavorite;
            }

            var firstPreScript = firstGame.PreScript;
            if (games.All(a => string.Equals(a.PreScript, firstPreScript, StringComparison.Ordinal)))
            {
                dummyGame.PreScript = firstPreScript;
            }

            var firstPostScript = firstGame.PostScript;
            if (games.All(a => string.Equals(a.PostScript, firstPostScript, StringComparison.Ordinal)))
            {
                dummyGame.PostScript = firstPostScript;
            }

            var firstGameStartedScript = firstGame.GameStartedScript;
            if (games.All(a => string.Equals(a.GameStartedScript, firstGameStartedScript, StringComparison.Ordinal)))
            {
                dummyGame.GameStartedScript = firstGameStartedScript;
            }

            var firstUseGlobalPreSrc = firstGame.UseGlobalPreScript;
            if (games.All(a => a.UseGlobalPreScript == firstUseGlobalPreSrc) == true)
            {
                dummyGame.UseGlobalPreScript = firstUseGlobalPreSrc;
            }

            var firstUseGlobalPostSrc = firstGame.UseGlobalPostScript;
            if (games.All(a => a.UseGlobalPostScript == firstUseGlobalPostSrc) == true)
            {
                dummyGame.UseGlobalPostScript = firstUseGlobalPostSrc;
            }

            var firstUseGlobalGameStartedSrc = firstGame.UseGlobalGameStartedScript;
            if (games.All(a => a.UseGlobalGameStartedScript == firstUseGlobalGameStartedSrc) == true)
            {
                dummyGame.UseGlobalGameStartedScript = firstUseGlobalGameStartedSrc;
            }

            var firstIncludeLibraryPluginAction = firstGame.IncludeLibraryPluginAction;
            if (games.All(a => a.IncludeLibraryPluginAction == firstIncludeLibraryPluginAction) == true)
            {
                dummyGame.IncludeLibraryPluginAction = firstIncludeLibraryPluginAction;
            }

            return dummyGame;
        }
    }
}
