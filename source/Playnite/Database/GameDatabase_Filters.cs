using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playnite.Database
{
    public partial class GameDatabase : IGameDatabaseMain, IDisposable
    {
        public bool GetGameMatchesFilter(Game game, FilterPresetSettings filterSettings)
        {
            return GetGameMatchesFilter(game, FilterSettings.FromSdkFilterSettings(filterSettings), false);
        }

        public IEnumerable<Game> GetFilteredGames(FilterPresetSettings filterSettings)
        {
            return GetFilteredGames(FilterSettings.FromSdkFilterSettings(filterSettings), false);
        }

        public bool GetGameMatchesFilter(Game game, FilterPresetSettings filterSettings, bool useFuzzyNameMatch)
        {
            return GetGameMatchesFilter(game, FilterSettings.FromSdkFilterSettings(filterSettings), useFuzzyNameMatch);
        }

        public IEnumerable<Game> GetFilteredGames(FilterPresetSettings filterSettings, bool useFuzzyNameMatch)
        {
            return GetFilteredGames(FilterSettings.FromSdkFilterSettings(filterSettings), useFuzzyNameMatch);
        }

        public bool GetGameMatchesFilter(Game game, FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            return new FilterMatcher(filterSettings, useFuzzyNameMatch).Match(game);
        }

        public IEnumerable<Game> GetFilteredGames(FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            var fm = new FilterMatcher(filterSettings, useFuzzyNameMatch);
            foreach (var game in Games)
            {
                if (fm.Match(game))
                {
                    yield return game;
                }
            }
        }
    }

    internal class FilterMatcher
    {
        private readonly FilterSettings filterSettings;
        private readonly bool useFuzzyNameMatch;

        public FilterMatcher(FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            this.filterSettings = filterSettings;
            this.useFuzzyNameMatch = useFuzzyNameMatch;
        }

        public bool Match(Game game)
        {
            if (!MatchInstallStatus(game))
                return false;

            if (!MatchFavorite(game))
                return false;

            if (!MatchHidden(game))
                return false;

            if (!MatchLibrary(game))
                return false;

            if (!MatchName(game))
                return false;

            if (!MatchReleaseYear(game))
                return false;

            if (!MatchPlaytime(game))
                return false;

            if (!MatchInstallSize(game))
                return false;

            if (!MatchVersion(game))
                return false;

            if (!MatchCompletionStatus(game))
                return false;

            if (!MatchLastActivity(game))
                return false;

            if (!MatchRecentActivity(game))
                return false;

            if (!MatchDateAdded(game))
                return false;

            if (!MatchDateModified(game))
                return false;

            if (!MatchUserScore(game))
                return false;

            if (!MatchCommunityScore(game))
                return false;

            if (!MatchCriticScore(game))
                return false;

            if (!MatchSeries(game))
                return false;

            if (!MatchRegions(game))
                return false;

            if (!MatchSource(game))
                return false;

            if (!MatchAgeRatings(game))
                return false;

            if (!MatchGenres(game))
                return false;

            if (!MatchPlatforms(game))
                return false;

            if (!MatchPublishers(game))
                return false;

            if (!MatchDevelopers(game))
                return false;

            if (!MatchCategories(game))
                return false;

            if (!MatchTags(game))
                return false;

            if (!MatchFeatures(game))
                return false;

            return true;
        }

        private bool MatchInstallStatus(Game game)
        {
            if (filterSettings.IsInstalled == filterSettings.IsUnInstalled)
                return true;

            if (filterSettings.IsInstalled && game.IsInstalled)
                return true;

            if (filterSettings.IsUnInstalled && !game.IsInstalled)
                return true;

            return false;
        }

        private bool MatchFavorite(Game game) => !filterSettings.Favorite || (filterSettings.Favorite && game.Favorite);

        private bool MatchHidden(Game game) => filterSettings.Hidden == game.Hidden;

        private bool MatchLibrary(Game game)
        {
            if (filterSettings.Library?.IsSet != true)
                return true;

            return filterSettings.Library.Ids.Contains(game.PluginId);
        }

        private bool MatchName(Game game)
        {
            if (filterSettings.Name.IsNullOrEmpty())
            {
                return true;
            }

            if (game.Name.IsNullOrEmpty())
            {
                return false;
            }

            if (filterSettings.Name.Length >= 2 && filterSettings.Name[0] == '^')
            {
                return game.GetNameGroup() == filterSettings.Name[1];
            }

            if (!useFuzzyNameMatch || filterSettings.Name[0] == '!')
            {
                return game.Name.IndexOf(filterSettings.Name.Substring(1), StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return SearchViewModel.MatchTextFilter(filterSettings.Name, game.Name, true);
        }

        private bool MatchReleaseYear(Game game)
        {
            if (filterSettings.ReleaseYear?.IsSet != true)
                return true;

            if (game.ReleaseDate == null)
            {
                return filterSettings.ReleaseYear.Values.Contains(FilterSettings.MissingFieldString);
            }

            return filterSettings.ReleaseYear.Values.Contains(game.ReleaseYear.ToString());
        }

        private bool MatchPlaytime(Game game) => MatchEnumField(filterSettings.PlayTime, (int)game.PlaytimeCategory);

        private bool MatchInstallSize(Game game) => MatchEnumField(filterSettings.InstallSize, (int)game.InstallSizeGroup);

        private bool MatchVersion(Game game)
        {
            if (filterSettings.Version.IsNullOrEmpty() || game.Version == null)
                return true;

            return game.Version.Contains(filterSettings.Version, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchCompletionStatus(Game game) => IsFilterMatchingSingle(filterSettings.CompletionStatuses, game.CompletionStatusId, game.CompletionStatus);

        private bool MatchLastActivity(Game game) => MatchEnumField(filterSettings.LastActivity, (int)game.LastActivitySegment);

        private bool MatchRecentActivity(Game game) => MatchEnumField(filterSettings.RecentActivity, (int)game.RecentActivitySegment);

        private bool MatchDateAdded(Game game) => MatchEnumField(filterSettings.Added, (int)game.AddedSegment);

        private bool MatchDateModified(Game game) => MatchEnumField(filterSettings.Modified, (int)game.ModifiedSegment);

        private bool MatchUserScore(Game game) => IsScoreFilterMatching(filterSettings.UserScore, game.UserScoreGroup);

        private bool MatchCommunityScore(Game game) => IsScoreFilterMatching(filterSettings.CommunityScore, game.CommunityScoreGroup);

        private bool MatchCriticScore(Game game) => IsScoreFilterMatching(filterSettings.CriticScore, game.CriticScoreGroup);

        private bool MatchSeries(Game game) => IsFilterMatchingList(filterSettings.Series, game.SeriesIds, game.Series);

        private bool MatchRegions(Game game) => IsFilterMatchingList(filterSettings.Region, game.RegionIds, game.Regions);

        private bool MatchSource(Game game) => IsFilterMatchingSingle(filterSettings.Source, game.SourceId, game.Source);

        private bool MatchAgeRatings(Game game) => IsFilterMatchingList(filterSettings.AgeRating, game.AgeRatingIds, game.AgeRatings);

        private bool MatchGenres(Game game) => IsFilterMatchingList(filterSettings.Genre, game.GenreIds, game.Genres);

        private bool MatchPlatforms(Game game) => IsFilterMatchingList(filterSettings.Platform, game.PlatformIds, game.Platforms);

        private bool MatchPublishers(Game game) => IsFilterMatchingList(filterSettings.Publisher, game.PublisherIds, game.Publishers);

        private bool MatchDevelopers(Game game) => IsFilterMatchingList(filterSettings.Developer, game.DeveloperIds, game.Developers);

        private bool MatchCategories(Game game) => IsFilterMatchingList(filterSettings.Category, game.CategoryIds, game.Categories);

        private bool MatchTags(Game game) => IsFilterMatchingList(filterSettings.Tag, game.TagIds, game.Tags);

        private bool MatchFeatures(Game game) => IsFilterMatchingList(filterSettings.Feature, game.FeatureIds, game.Features);

        private static bool MatchEnumField(EnumFilterItemProperties enumFilter, int enumFieldValue)
        {
            if (enumFilter?.IsSet != true)
                return true;

            return enumFilter.Values.Contains(enumFieldValue);
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has 1 possible value (OR filtering logic)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="idData"></param>
        /// <param name="objectData"></param>
        /// <returns></returns>
        private static bool IsFilterMatchingSingle(IdItemFilterItemProperties filter, Guid idData, DatabaseObject objectData)
        {
            if (filter == null || !filter.IsSet)
            {
                return true;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                return filter.Texts.ContainsPartOfString(objectData.Name);
            }

            if (filter.Ids.HasItems())
            {
                return filter.Ids.Contains(idData);
            }

            return true;
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has multiple possible values
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gamePropertyIds"></param>
        /// <param name="gamePropertyObjects"></param>
        /// <returns></returns>
        private bool IsFilterMatchingList(IdItemFilterItemProperties filter, List<Guid> gamePropertyIds, IReadOnlyCollection<DatabaseObject> gamePropertyObjects)
        {
            if (filter == null || !filter.IsSet)
            {
                return true;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (gamePropertyObjects == null)
                {
                    return false;
                }

                bool gameHasItemWithStringMatch(string t)
                {
                    return gamePropertyObjects.Any(o => o.Name.Contains(t, StringComparison.InvariantCultureIgnoreCase));
                }

                if (filterSettings.UseAndFilteringStyle)
                {
                    return filter.Texts.All(gameHasItemWithStringMatch);
                }
                else
                {
                    return filter.Texts.Any(gameHasItemWithStringMatch);
                }
            }

            if (filter.Ids.HasItems())
            {
                bool gameHasItemWithIdMatch(Guid filterId)
                {
                    if (filterId == Guid.Empty)
                    {
                        return gamePropertyIds == null || gamePropertyIds.Count == 0;
                    }

                    return gamePropertyIds?.Contains(filterId) == true;
                }

                if (filterSettings.UseAndFilteringStyle)
                {
                    return filter.Ids.All(gameHasItemWithIdMatch);
                }
                else
                {
                    return filter.Ids.Any(gameHasItemWithIdMatch);
                }
            }

            return true;
        }

        private static bool IsScoreFilterMatching(EnumFilterItemProperties filter, ScoreGroup score)
        {
            if (filter?.IsSet != true)
                return true;

            return filter.Values?.Contains((int)score) == true;
        }
    }
}