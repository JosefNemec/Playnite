using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GenresCollection : ItemCollection<Genre>
    {
        private readonly GameDatabase db;

        public GenresCollection(GameDatabase database) : base()
        {
            db = database;
        }
    }
}
