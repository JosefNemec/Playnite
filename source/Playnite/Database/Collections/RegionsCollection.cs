using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class RegionsCollection : ItemCollection<Region>
    {
        private readonly GameDatabase db;

        public RegionsCollection(GameDatabase database) : base()
        {
            db = database;
        }
    }
}
