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

        public EmulatorsCollection(GameDatabase database) : base()
        {
            db = database;
        }

        private void RemoveUsage(Guid id)
        {
            foreach (var game in db.Games)
            {
                if (game.PlayAction?.Type == GameActionType.Emulator && game.PlayAction?.EmulatorId == id)
                {
                    game.PlayAction.EmulatorId = Guid.Empty;
                    game.PlayAction.EmulatorProfileId = Guid.Empty;
                    db.Games.Update(game);
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
