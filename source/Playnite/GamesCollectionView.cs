using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.Settings;
using Playnite.Plugins;
using Playnite.SDK.Plugins;

namespace Playnite
{
    public abstract class BaseCollectionView : ObservableObject, IDisposable
    {
        public static ILogger Logger = LogManager.GetLogger();
        private ExtensionFactory extensions;
        private FilterSettings filterSettings;

        public IGameDatabase Database { get; private set; }
        public RangeObservableCollection<GamesCollectionViewEntry> Items { get; private set; }

        private ListCollectionView collectionView;
        public ListCollectionView CollectionView
        {
            get => collectionView;
            private set
            {
                collectionView = value;
                OnPropertyChanged();
            }
        }

        public BaseCollectionView(IGameDatabase database, ExtensionFactory extensions, FilterSettings filterSettings)
        {
            Database = database;
            this.extensions = extensions;
            this.filterSettings = filterSettings;
            Items = new RangeObservableCollection<GamesCollectionViewEntry>();
            filterSettings.FilterChanged += FilterSettings_FilterChanged;
            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            CollectionView.Filter = Filter;
        }

        public virtual void Dispose()
        {
            filterSettings.FilterChanged -= FilterSettings_FilterChanged;
            Items = null;
        }

        private bool IsFilterMatching(FilterItemProperites filter, List<Guid> idData, IEnumerable<DatabaseObject> objectData)
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

                if (filter.Text.Contains(Common.Constants.ListSeparator))
                {
                    return filter.Texts.IntersectsPartiallyWith(objectData?.Select(a => a.Name));
                }
                else
                {
                    return objectData.Any(a => a.Name.Contains(filter.Text, StringComparison.InvariantCultureIgnoreCase));
                }
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

        private bool IsFilterMatchingSingle(FilterItemProperites filter, Guid idData, DatabaseObject objectData)
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

                if (filter.Text.Contains(Common.Constants.ListSeparator))
                {
                    return filter.Texts.ContainsPartOfString(objectData.Name);
                }
                else
                {
                    return objectData.Name.Contains(filter.Text, StringComparison.InvariantCultureIgnoreCase);
                }
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

        private bool IsScoreFilterMatching(EnumFilterItemProperites filter, ScoreGroup score)
        {
            return filter.Values.Contains((int)score);
        }

        private bool Filter(object item)
        {
            var entry = (GamesCollectionViewEntry)item;
            var game = entry.Game;
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
                var libInter = filterSettings.Library.Ids?.Intersect(new List<Guid> { game.PluginId });
                librariesFilter = libInter?.Any() == true;
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
            if (!filterSettings.Name.IsNullOrEmpty())
            {
                if (game.Name.IsNullOrEmpty())
                {
                    return false;
                }
                else if (game.Name.IndexOf(filterSettings.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
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

            // ------------------ Version
            if (!filterSettings.Version.IsNullOrEmpty() && game.Version?.Contains(filterSettings.Version) != true)
            {
                return false;
            }

            // ------------------ Completion Status
            if (filterSettings.CompletionStatus?.IsSet == true && !filterSettings.CompletionStatus.Values.Contains((int)game.CompletionStatus))
            {
                return false;
            }

            // ------------------ Last Activity
            if (filterSettings.LastActivity?.IsSet == true && !filterSettings.LastActivity.Values.Contains((int)game.LastActivitySegment))
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
                if (!IsFilterMatchingSingle(filterSettings.Series, game.SeriesId, game.Series))
                {
                    return false;
                }
            }

            // ------------------ Region filter
            if (filterSettings.Region?.IsSet == true)
            {
                if (!IsFilterMatchingSingle(filterSettings.Region, game.RegionId, game.Region))
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
                if (!IsFilterMatchingSingle(filterSettings.AgeRating, game.AgeRatingId, game.AgeRating))
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
                if (!IsFilterMatchingSingle(filterSettings.Platform, game.PlatformId, game.Platform))
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

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            Logger.Debug("Refreshing collection view filter.");
            CollectionView.Refresh();
        }

        public LibraryPlugin GetLibraryPlugin(Game game)
        {
            if (game.PluginId != Guid.Empty && extensions.Plugins.TryGetValue(game.PluginId, out var plugin))
            {
                return (LibraryPlugin)plugin.Plugin;
            }

            return null;
        }
    }
}
