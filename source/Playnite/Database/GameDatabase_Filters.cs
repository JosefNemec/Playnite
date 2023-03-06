using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (!filterSettings.IsActive)
            {
                if (game.Hidden)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (filterSettings.UseAndFilteringStyle)
            {
                return FilterByStyleAnd(game, filterSettings, useFuzzyNameMatch);
            }
            else
            {
                return FilterByStyleOr(game, filterSettings, useFuzzyNameMatch);
            }
        }

        public IEnumerable<Game> GetFilteredGames(FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            foreach (var game in Games)
            {
                if (GetGameMatchesFilter(game, filterSettings, useFuzzyNameMatch))
                {
                    yield return game;
                }
            }
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has multiple possible values (OR filtering logic)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="idData"></param>
        /// <param name="objectData"></param>
        /// <returns></returns>
        private bool IsFilterMatching(IdItemFilterItemProperties filter, List<Guid> idData, IEnumerable<DatabaseObject> objectData)
        {
            if (objectData == null && (filter == null || !filter.IsSet))
            {
                return true;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                return filter.Texts.IntersectsPartiallyWith(objectData.Select(a => a.Name));
            }
            else if (filter.Ids.HasItems())
            {
                if (filter.Ids.Contains(Guid.Empty) && !idData.HasItems())
                {
                    return true;
                }
                else if (!idData.HasItems())
                {
                    return false;
                }
                else
                {
                    return filter.Ids.Intersect(idData).Any();
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has 1 possible value (AND filtering logic)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="idData"></param>
        /// <param name="objectData"></param>
        /// <returns></returns>
        private bool IsFilterMatchingSingleOnly(IdItemFilterItemProperties filter, Guid idData, DatabaseObject objectData)
        {
            if (objectData == null && (filter == null || !filter.IsSet))
            {
                return true;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                return filter.Texts.All(t => objectData.Name.Contains(t, StringComparison.InvariantCultureIgnoreCase));
            }
            if (filter.Ids.HasItems())
            {
                if (filter.Ids.Count != 1)
                {
                    return false;
                }

                if (filter.Ids.Contains(idData))
                {
                    return true;
                }
            }
            else if (idData == Guid.Empty)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has multiple possible values (AND filtering logic)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="listData"></param>
        /// <param name="objectData"></param>
        /// <returns></returns>
        private bool IsFilterMatchingList(IdItemFilterItemProperties filter, List<Guid> listData, IEnumerable<DatabaseObject> objectData)
        {
            if (objectData == null && (filter == null || !filter.IsSet))
            {
                return true;
            }

            if (!filter.Text.IsNullOrEmpty())
            {
                if (objectData == null)
                {
                    return false;
                }

                return filter.Texts.All(t => objectData.Any(o => o.Name.Contains(t, StringComparison.InvariantCultureIgnoreCase)));
            }
            if (filter.Ids.HasItems())
            {
                if (listData == null || !listData.HasItems())
                {
                    return false;
                }
                else if (filter.Ids.Intersect(listData).Count() != filter.Ids.Count())
                {
                    return false;
                }
            }
            else if (listData != null && listData.HasItems())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Match a filter dropdown selection to a field that has 1 possible value (OR filtering logic)
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="idData"></param>
        /// <param name="objectData"></param>
        /// <returns></returns>
        private bool IsFilterMatchingSingle(IdItemFilterItemProperties filter, Guid idData, DatabaseObject objectData)
        {
            if (objectData == null && (filter == null || !filter.IsSet))
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
            else if (filter.Ids.HasItems())
            {
                if (filter.Ids.Contains(Guid.Empty) && idData == Guid.Empty)
                {
                    return true;
                }
                else if (idData == Guid.Empty)
                {
                    return false;
                }
                else
                {
                    return filter.Ids.Contains(idData);
                }
            }
            else
            {
                return true;
            }
        }

        private bool IsScoreFilterMatching(EnumFilterItemProperties filter, ScoreGroup score)
        {
            return filter.Values.Contains((int)score);
        }

        private bool IsScoreFilterMatchingSingle(EnumFilterItemProperties filter, ScoreGroup score)
        {
            if (filter.Values.Count != 1)
            {
                return false;
            }
            return filter.Values.Contains((int)score);
        }

        private bool FilterByStyleAnd(Game game, FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            // ------------------ Installed
            bool installedResult = false;
            if ((filterSettings.IsInstalled && filterSettings.IsUnInstalled) ||
                (!filterSettings.IsInstalled && !filterSettings.IsUnInstalled))
            {
                installedResult = true;
            }
            else
            {
                if (filterSettings.IsInstalled && game.IsInstalled)
                {
                    installedResult = true;
                }
                else if (filterSettings.IsUnInstalled && !game.IsInstalled)
                {
                    installedResult = true;
                }
            }

            if (!installedResult)
            {
                return false;
            }

            // ------------------ Hidden
            if (filterSettings.Hidden != game.Hidden)
            {
                return false;
            }

            // ------------------ Favorite
            if (filterSettings.Favorite && !game.Favorite)
            {
                return false;
            }

            // ------------------ Providers
            if (filterSettings.Library?.IsSet == true)
            {
                if (filterSettings.Library.Ids?.Count != 1)
                {
                    return false;
                }
                else
                {
                    if (filterSettings.Library.Ids?.Contains(game.PluginId) == false)
                    {
                        return false;
                    }
                }
            }

            // ------------------ Name filter
            if (!GetNameFilterResult(game, filterSettings, useFuzzyNameMatch))
            {
                return false;
            }

            // ------------------ Release Year
            if (filterSettings.ReleaseYear?.IsSet == true)
            {
                if (filterSettings.ReleaseYear.Values.Count != 1)
                {
                    return false;
                }
                else if (game.ReleaseDate == null || !filterSettings.ReleaseYear.Values.Contains(game.ReleaseYear.ToString()))
                {
                    return false;
                }
            }

            // ------------------ Playtime
            if (filterSettings.PlayTime?.IsSet == true)
            {
                if (filterSettings.PlayTime.Values.Count != 1)
                {
                    return false;
                }
                else if (filterSettings.PlayTime.Values.First() == ((int)game.PlaytimeCategory) == false)
                {
                    return false;
                }
            }

            // ------------------ InstallSize
            if (filterSettings.InstallSize?.IsSet == true)
            {
                if (filterSettings.InstallSize.Values.Count != 1)
                {
                    return false;
                }
                else if (filterSettings.InstallSize.Values.First() == ((int)game.InstallSizeGroup) == false)
                {
                    return false;
                }
            }

            // ------------------ Version
            if (!filterSettings.Version.IsNullOrEmpty() && game.Version?.Contains(filterSettings.Version, StringComparison.OrdinalIgnoreCase) != true)
            {
                return false;
            }

            // ------------------ Completion Status
            if (filterSettings.CompletionStatuses?.IsSet == true)
            {
                if (!IsFilterMatchingSingleOnly(filterSettings.CompletionStatuses, game.CompletionStatusId, game.CompletionStatus))
                {
                    return false;
                }
            }

            // ------------------ Last Activity
            if (filterSettings.LastActivity?.IsSet == true)
            {
                if (filterSettings.LastActivity.Values.Count != 1)
                {
                    return false;
                }
                else if (!filterSettings.LastActivity.Values.Contains((int)game.LastActivitySegment))
                {
                    return false;
                }
            }

            // ------------------ Recent Activity
            if (filterSettings.RecentActivity?.IsSet == true)
            {
                if (filterSettings.RecentActivity.Values.Count != 1)
                {
                    return false;
                }
                else if (!filterSettings.RecentActivity.Values.Contains((int)game.RecentActivitySegment))
                {
                    return false;
                }
            }

            // ------------------ Added
            if (filterSettings.Added?.IsSet == true)
            {
                if (filterSettings.Added.Values.Count != 1)
                {
                    return false;
                }
                else if (!filterSettings.Added.Values.Contains((int)game.AddedSegment))
                {
                    return false;
                }
            }

            // ------------------ Modified
            if (filterSettings.Modified?.IsSet == true)
            {
                if (filterSettings.Modified.Values.Count != 1)
                {
                    return false;
                }
                else if (!filterSettings.Modified.Values.Contains((int)game.ModifiedSegment))
                {
                    return false;
                }
            }

            // ------------------ User Score
            if (filterSettings.UserScore != null)
            {
                if (!IsScoreFilterMatchingSingle(filterSettings.UserScore, game.UserScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Community Score
            if (filterSettings.CommunityScore != null)
            {
                if (!IsScoreFilterMatchingSingle(filterSettings.CommunityScore, game.CommunityScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Critic Score
            if (filterSettings.CriticScore != null)
            {
                if (!IsScoreFilterMatchingSingle(filterSettings.CriticScore, game.CriticScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Series filter
            if (filterSettings.Series?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Series, game.SeriesIds, game.Series))
                {
                    return false;
                }
            }

            // ------------------ Region filter
            if (filterSettings.Region?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Region, game.RegionIds, game.Regions))
                {
                    return false;
                }
            }

            // ------------------ Source filter
            if (filterSettings.Source?.IsSet == true)
            {
                if (!IsFilterMatchingSingleOnly(filterSettings.Source, game.SourceId, game.Source))
                {
                    return false;
                }
            }

            // ------------------ AgeRating filter
            if (filterSettings.AgeRating?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.AgeRating, game.AgeRatingIds, game.AgeRatings))
                {
                    return false;
                }
            }

            // ------------------ Genre
            if (filterSettings.Genre?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Genre, game.GenreIds, game.Genres))
                {
                    return false;
                }
            }

            // ------------------ Platform
            if (filterSettings.Platform?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Platform, game.PlatformIds, game.Platforms))
                {
                    return false;
                }
            }

            // ------------------ Publisher
            if (filterSettings.Publisher?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Publisher, game.PublisherIds, game.Publishers))
                {
                    return false;
                }
            }

            // ------------------ Developer
            if (filterSettings.Developer?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Developer, game.DeveloperIds, game.Developers))
                {
                    return false;
                }
            }

            // ------------------ Category
            if (filterSettings.Category?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Category, game.CategoryIds, game.Categories))
                {
                    return false;
                }
            }

            // ------------------ Tags
            if (filterSettings.Tag?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Tag, game.TagIds, game.Tags))
                {
                    return false;
                }
            }

            // ------------------ Features
            if (filterSettings.Feature?.IsSet == true)
            {
                if (!IsFilterMatchingList(filterSettings.Feature, game.FeatureIds, game.Features))
                {
                    return false;
                }
            }

            return true;
        }

        private bool FilterByStyleOr(Game game, FilterSettings filterSettings, bool useFuzzyNameMatch)
        {
            // ------------------ Installed
            bool installedResult = false;
            if ((filterSettings.IsInstalled && filterSettings.IsUnInstalled) ||
                (!filterSettings.IsInstalled && !filterSettings.IsUnInstalled))
            {
                installedResult = true;
            }
            else
            {
                if (filterSettings.IsInstalled && game.IsInstalled)
                {
                    installedResult = true;
                }
                else if (filterSettings.IsUnInstalled && !game.IsInstalled)
                {
                    installedResult = true;
                }
            }

            if (!installedResult)
            {
                return false;
            }

            // ------------------ Hidden
            bool hiddenResult = true;
            if (filterSettings.Hidden && game.Hidden)
            {
                hiddenResult = true;
            }
            else if (!filterSettings.Hidden && game.Hidden)
            {
                return false;
            }
            else if (filterSettings.Hidden && !game.Hidden)
            {
                return false;
            }

            if (!hiddenResult)
            {
                return false;
            }

            // ------------------ Favorite
            bool favoriteResult = false;
            if (filterSettings.Favorite && game.Favorite)
            {
                favoriteResult = true;
            }
            else if (!filterSettings.Favorite)
            {
                favoriteResult = true;
            }

            if (!favoriteResult)
            {
                return false;
            }

            // ------------------ Providers
            bool librariesFilter = false;
            if (filterSettings.Library?.IsSet == true)
            {
                librariesFilter = filterSettings.Library.Ids?.Contains(game.PluginId) == true;
            }
            else
            {
                librariesFilter = true;
            }

            if (!librariesFilter)
            {
                return false;
            }

            // ------------------ Name filter
            if (!GetNameFilterResult(game, filterSettings, useFuzzyNameMatch))
            {
                return false;
            }

            // ------------------ Release Year
            if (filterSettings.ReleaseYear?.IsSet == true)
            {
                if (game.ReleaseDate == null && !filterSettings.ReleaseYear.Values.Contains(FilterSettings.MissingFieldString))
                {
                    return false;
                }
                else if (game.ReleaseDate != null && !filterSettings.ReleaseYear.Values.Contains(game.ReleaseYear.ToString()))
                {
                    return false;
                }
            }

            // ------------------ Playtime
            if (filterSettings.PlayTime?.IsSet == true && !filterSettings.PlayTime.Values.Contains((int)game.PlaytimeCategory))
            {
                return false;
            }

            // ------------------ InstallSize
            if (filterSettings.InstallSize?.IsSet == true && !filterSettings.InstallSize.Values.Contains((int)game.InstallSizeGroup))
            {
                return false;
            }

            // ------------------ Version
            if (!filterSettings.Version.IsNullOrEmpty() && game.Version?.Contains(filterSettings.Version, StringComparison.OrdinalIgnoreCase) != true)
            {
                return false;
            }

            // ------------------ Completion Status
            if (filterSettings.CompletionStatuses?.IsSet == true)
            {
                if (!IsFilterMatchingSingle(filterSettings.CompletionStatuses, game.CompletionStatusId, game.CompletionStatus))
                {
                    return false;
                }
            }

            // ------------------ Last Activity
            if (filterSettings.LastActivity?.IsSet == true && !filterSettings.LastActivity.Values.Contains((int)game.LastActivitySegment))
            {
                return false;
            }

            // ------------------ Recent Activity
            if (filterSettings.RecentActivity?.IsSet == true && !filterSettings.RecentActivity.Values.Contains((int)game.RecentActivitySegment))
            {
                return false;
            }

            // ------------------ Added
            if (filterSettings.Added?.IsSet == true && !filterSettings.Added.Values.Contains((int)game.AddedSegment))
            {
                return false;
            }

            // ------------------ Modified
            if (filterSettings.Modified?.IsSet == true && !filterSettings.Modified.Values.Contains((int)game.ModifiedSegment))
            {
                return false;
            }

            // ------------------ User Score
            if (filterSettings.UserScore != null)
            {
                if (!IsScoreFilterMatching(filterSettings.UserScore, game.UserScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Community Score
            if (filterSettings.CommunityScore != null)
            {
                if (!IsScoreFilterMatching(filterSettings.CommunityScore, game.CommunityScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Critic Score
            if (filterSettings.CriticScore != null)
            {
                if (!IsScoreFilterMatching(filterSettings.CriticScore, game.CriticScoreGroup))
                {
                    return false;
                }
            }

            // ------------------ Series filter
            if (filterSettings.Series?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Series, game.SeriesIds, game.Series))
                {
                    return false;
                }
            }

            // ------------------ Region filter
            if (filterSettings.Region?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Region, game.RegionIds, game.Regions))
                {
                    return false;
                }
            }

            // ------------------ Source filter
            if (filterSettings.Source?.IsSet == true)
            {
                if (!IsFilterMatchingSingle(filterSettings.Source, game.SourceId, game.Source))
                {
                    return false;
                }
            }

            // ------------------ AgeRating filter
            if (filterSettings.AgeRating?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.AgeRating, game.AgeRatingIds, game.AgeRatings))
                {
                    return false;
                }
            }

            // ------------------ Genre
            if (filterSettings.Genre?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Genre, game.GenreIds, game.Genres))
                {
                    return false;
                }
            }

            // ------------------ Platform
            if (filterSettings.Platform?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Platform, game.PlatformIds, game.Platforms))
                {
                    return false;
                }
            }

            // ------------------ Publisher
            if (filterSettings.Publisher?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Publisher, game.PublisherIds, game.Publishers))
                {
                    return false;
                }
            }

            // ------------------ Developer
            if (filterSettings.Developer?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Developer, game.DeveloperIds, game.Developers))
                {
                    return false;
                }
            }

            // ------------------ Category
            if (filterSettings.Category?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Category, game.CategoryIds, game.Categories))
                {
                    return false;
                }
            }

            // ------------------ Tags
            if (filterSettings.Tag?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Tag, game.TagIds, game.Tags))
                {
                    return false;
                }
            }

            // ------------------ Features
            if (filterSettings.Feature?.IsSet == true)
            {
                if (!IsFilterMatching(filterSettings.Feature, game.FeatureIds, game.Features))
                {
                    return false;
                }
            }

            return true;
        }

        private bool GetNameFilterResult(Game game, FilterSettings filterSettings, bool useFuzzyMatch)
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

            if (!useFuzzyMatch || filterSettings.Name[0] == '!' && useFuzzyMatch)
            {
                return game.Name.IndexOf(filterSettings.Name.Substring(1), StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return SearchViewModel.MatchTextFilter(filterSettings.Name, game.Name, true);
        }
    }
}
