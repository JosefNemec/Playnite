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
        private readonly PlayniteSettings settings;

        public IGameDatabaseMain Database { get; private set; }
        public RangeObservableCollection<GamesCollectionViewEntry> Items { get; private set; }
        public bool IgnoreViewConfigChanges { get; set; } = false;

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

        public BaseCollectionView(IGameDatabaseMain database, ExtensionFactory extensions, FilterSettings filterSettings, PlayniteSettings settings)
        {
            Database = database;
            this.extensions = extensions;
            this.filterSettings = filterSettings;
            this.settings = settings;
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

        private bool Filter(object item)
        {
            if (!(item is GamesCollectionViewEntry entry))
            {
                return false;
            }

            return Database.GetGameMatchesFilter(entry.Game, filterSettings, settings.FuzzyMatchingInNameFilter);
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (IgnoreViewConfigChanges)
            {
                return;
            }

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

        public abstract void RefreshView();

        public void NotifyItemPropertyChanges(params string[] changedProperties)
        {
            if (!Items.HasItems())
            {
                return;
            }

            Logger.Trace("NotifyItemPropertyChanges: ");
            changedProperties.ForEach(prop => Logger.Trace(prop));
            foreach (var item in Items)
            {
                changedProperties.ForEach(prop => item.OnPropertyChanged(prop));
            }
        }
    }
}
