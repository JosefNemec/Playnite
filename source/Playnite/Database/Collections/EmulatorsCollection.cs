using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class EmulatorsCollection : ItemCollection<Emulator>
    {
        private readonly GameDatabase db;

        public EmulatorsCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base(mapper, type: GameDatabaseCollection.Emulators)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.Entity<Emulator>().
                Id(a => a.Id, false).
                Ignore(a => a.SelectableProfiles).
                Ignore(a => a.AllProfiles);
            mapper.Entity<BuiltInEmulatorProfile>().
                Ignore(a => a.Type);
            mapper.Entity<CustomEmulatorProfile>().
                Ignore(a => a.Type);
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games)
            {
                if (game.GameActions.HasItems())
                {
                    foreach (var action in game.GameActions)
                    {
                        if (action?.Type == GameActionType.Emulator && action?.EmulatorId == id)
                        {
                            action.EmulatorId = Guid.Empty;
                            action.EmulatorProfileId = null;
                            db.Games.Update(game);
                        }
                    }
                }
            }
        }

        public override bool Remove(Emulator itemToRemove)
        {
            RemoveUsage(itemToRemove.Id);
            return base.Remove(itemToRemove);
        }

        public override bool Remove(Guid id)
        {
            RemoveUsage(id);
            return base.Remove(id);
        }

        public override bool Remove(IEnumerable<Emulator> itemsToRemove)
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

        public override Emulator Add(string itemName)
        {
            throw new NotSupportedException();
        }

        public override Emulator Add(string itemName, Func<Emulator, string, bool> existingComparer)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<Emulator> Add(List<string> items)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<Emulator> Add(List<string> itemsToAdd, Func<Emulator, string, bool> existingComparer)
        {
            throw new NotSupportedException();
        }
    }
}
