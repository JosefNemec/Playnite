using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
    public class StoreGamesFilteredListResponse
    {
        public class Product
        {
            public string title;
            public string image;
            public string url;
            public string supportUrl;
            public string forumUrl;
            public bool isGame;
            public string slug;
            public uint id;
        }

        public List<Product> products;
    }
}
