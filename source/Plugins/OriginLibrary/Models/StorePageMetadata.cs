using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Models
{
    public class StorePageMetadata
    {
        public class Components
        {
            public List<Dictionary<string, object>> items;
        }

        public class GameHub
        {
            public string name;
            public string type;
            public string locale;
            public string country;
            public Components components;
        }

        public GameHub gamehub;
    }
}
