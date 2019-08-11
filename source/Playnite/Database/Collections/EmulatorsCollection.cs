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

        public override Emulator Add(string itemName)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<Emulator> Add(List<string> items)
        {
            throw new NotSupportedException();
        }
    }
}
