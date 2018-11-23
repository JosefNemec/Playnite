using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class TagsCollection : ItemCollection<Tag>
    {
        private readonly GameDatabase db;

        public TagsCollection(GameDatabase database) : base()
        {
            db = database;
        }
    }
}
