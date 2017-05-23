using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.IGDB
{
    public class Game
    {
        public class Cover
        {
            public string url;
        }

        public class Website
        {
            public UInt64 category;
            public string url;
        }

        public UInt64 id;
        public string name;
        public string summary;
        public List<UInt64> developers;
        public List<UInt64> publishers;
        public List<UInt64> genres;
        public Int64 first_release_date;
        public Cover cover;
        public List<Website> websites;
    }

    public class Company
    {
        public UInt64 id;
        public string name;
    }

    public class Genre
    {
        public UInt64 id;
        public string name;
    }
}
