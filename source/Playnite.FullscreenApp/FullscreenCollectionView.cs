using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp
{
    public class FullscreenCollectionView : BaseCollectionView
    {
        private PlayniteSettings settings;
        private FullscreenViewSettings viewSettings;

        public FullscreenCollectionView(
            IGameDatabase database,
            PlayniteSettings settings,
            ExtensionFactory extensions) : base(database, extensions, settings.Fullscreen.FilterSettings)
        {
            this.settings = settings;
            Database.Games.ItemCollectionChanged += Database_GamesCollectionChanged;
            Database.Games.ItemUpdated += Database_GameUpdated;
            viewSettings = settings.Fullscreen.ViewSettings;
            viewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            using (CollectionView.DeferRefresh())
            {
                SetViewDescriptions();
                Items.AddRange(Database.Games.Select(x => new GamesCollectionViewEntry(x, GetLibraryPlugin(x), settings)));
            };
        }

        public override void Dispose()
        {
            base.Dispose();
            Database.Games.ItemCollectionChanged -= Database_GamesCollectionChanged;
            Database.Games.ItemUpdated -= Database_GameUpdated;
            viewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((new string[]
            {
                nameof(FullscreenViewSettings.SortingOrder),
                nameof(FullscreenViewSettings.SortingOrderDirection)
            }).Contains(e.PropertyName))
            {
                Logger.Debug("Updating collection view settings.");
                using (CollectionView.DeferRefresh())
                {
                    CollectionView.SortDescriptions.Clear();
                    SetViewDescriptions();
                }
            }
        }

        private void SetViewDescriptions()
        {
            var sortDirection = viewSettings.SortingOrderDirection == SortOrderDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            CollectionView.SortDescriptions.Add(new SortDescription(viewSettings.SortingOrder.ToString(), sortDirection));
            if (viewSettings.SortingOrder != SortOrder.Name)
            {
                CollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            } 
        }

        private void Database_GameUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            foreach (var update in args.UpdatedItems)
            {
                var existingItem = Items.FirstOrDefault(a => a.Game.Id == update.NewData.Id);
                if (existingItem != null)
                {
                    // Forces CollectionView to re-sort items without full list refresh.
                    Items.OnItemMoved(existingItem, 0, 0);
                }
            }
        }

        private void Database_GamesCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> args)
        {
            // DO NOT use *Range methods for "Items" object.
            // It can throw weird exceptions in virtualization panel, directly in WPF (without known fix from MS).
            // https://github.com/JosefNemec/Playnite/issues/796

            if (args.RemovedItems.Count > 0)
            {
                var removeIds = new HashSet<Guid>(args.RemovedItems.Select(a => a.Id));
                var toRemove = Items.Where(a => removeIds.Contains(a.Id))?.ToList();
                if (toRemove != null)
                {
                    foreach (var item in toRemove)
                    {
                        item.Dispose();
                        Items.Remove(item);
                    }
                }
            }

            var addList = new List<GamesCollectionViewEntry>();
            foreach (var game in args.AddedItems)
            {
                addList.Add(new GamesCollectionViewEntry(game, GetLibraryPlugin(game), settings));
            }

            if (addList.Count > 0)
            {
                foreach (var item in addList)
                {
                    Items.Add(item);
                }
            }
        }
    }
}
