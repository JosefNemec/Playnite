using LiteDB;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CompletionStatusSettings
    {
        [BsonId(false)]
        public int Id { get; set; } = 0;
        public Guid DefaultStatus { get; set; }
        public Guid PlayedStatus { get; set; }
    }

    public class CompletionStatusesCollection : ItemCollection<CompletionStatus>
    {
        private readonly GameDatabase db;

        private LiteCollection<CompletionStatusSettings> settingsCollection;
        private LiteCollection<CompletionStatusSettings> SettingsCollection
        {
            get
            {
                if (settingsCollection == null)
                {
                    settingsCollection = liteDb.GetCollection<CompletionStatusSettings>();
                }

                return settingsCollection;
            }
        }

        public CompletionStatusesCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.CompletionStatuses)
        {
            db = database;
        }

        public CompletionStatusSettings GetSettings()
        {
            if (SettingsCollection.Count() == 0)
            {
                var settings = new CompletionStatusSettings();
                SettingsCollection.Insert(settings);
                return settings;
            }
            else
            {
                return SettingsCollection.FindAll().First();
            }
        }

        public void SetSettings(CompletionStatusSettings settings)
        {
            settings.Id = 0;
            SettingsCollection.Upsert(settings);
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<CompletionStatus>().Id(a => a.Id, false);
        }

        private void RemoveUsage(Guid statusId)
        {
            foreach (var game in db.Games.Where(a => a.CompletionStatusId == statusId))
            {
                game.CompletionStatusId = Guid.Empty;
                db.Games.Update(game);
            }
        }

        public override bool Remove(CompletionStatus itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<CompletionStatus> itemsToRemove)
        {
            if (itemsToRemove.HasItems())
            {
                foreach (var item in itemsToRemove)
                {
                    RemoveUsage(item.Id);
                }
            }

            return base.Remove(itemsToRemove);
        }
    }
}
