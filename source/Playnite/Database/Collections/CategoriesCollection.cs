using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CategoriesCollection : ItemCollection<Category>
    {
        private readonly GameDatabase db;

        public CategoriesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        // TODO: remove categories from games when removing from collection
    }
}
