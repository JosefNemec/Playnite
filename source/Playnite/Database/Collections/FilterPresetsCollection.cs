using LiteDB;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;

namespace Playnite.Database
{
    public class FilterPresetsSettings
    {
        [BsonId(false)]
        public int Id { get; set; } = 0;
        public List<Guid> SortingOrder { get; set; } = new List<Guid>();
    }

    public class FilterPresetsSettingsUpdateEvent
    {
        /// <summary>
        /// Gets or sets old item state.
        /// </summary>
        public FilterPresetsSettings OldData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets new item state.
        /// </summary>
        public FilterPresetsSettings NewData
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of FilterPresetsSettingsUpdateEvent.
        /// </summary>
        /// <param name="oldData">Old state.</param>
        /// <param name="newData">New state.</param>
        public FilterPresetsSettingsUpdateEvent(FilterPresetsSettings oldData, FilterPresetsSettings newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public class FilterPresetsCollection : ItemCollection<FilterPreset>
    {
        public event EventHandler<FilterPresetsSettingsUpdateEvent> OnSettingsUpdated;

        private LiteCollection<FilterPresetsSettings> settingsCollection;
        private LiteCollection<FilterPresetsSettings> SettingsCollection
        {
            get
            {
                if (settingsCollection == null)
                {
                    settingsCollection = liteDb.GetCollection<FilterPresetsSettings>();
                }

                return settingsCollection;
            }
        }

        public FilterPresetsCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.FilterPresets)
        {
        }

        public FilterPresetsSettings GetSettings()
        {
            if (SettingsCollection.Count() == 0)
            {
                var settings = new FilterPresetsSettings();
                SettingsCollection.Insert(settings);
                return settings;
            }
            else
            {
                return SettingsCollection.FindAll().First();
            }
        }

        public void SetSettings(FilterPresetsSettings settings)
        {
            var oldSettings = GetSettings();
            settings.Id = 0;
            SettingsCollection.Upsert(settings);
            OnSettingsUpdated?.Invoke(this, new FilterPresetsSettingsUpdateEvent(oldSettings, settings));
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<FilterPreset>().Id(a => a.Id, false);
        }
    }
}
