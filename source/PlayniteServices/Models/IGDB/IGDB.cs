using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.IGDB
{
    public enum WebSiteCategory : UInt64
    {
        Official = 1,
        Wikia = 2,
        Wikipedia = 3,
        Facebook = 4,
        Twitter = 5,
        Twitch = 6,
        Instagram = 8,
        Youtube = 9,
        Iphone = 10,
        Ipad = 11,
        Android = 12,
        Steam = 13
    }

    public class Game
    {
        public class Cover
        {
            public string url;
        }

        public class Website
        {
            public WebSiteCategory category;
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
        [BsonId(false)]
        [BsonIndex(true)]
        public UInt64 id
        {
            get; set;
        }

        public string name
        {
            get; set;
        }
    }

    public class Genre
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public UInt64 id
        {
            get; set;
        }

        public string name
        {
            get; set;
        }
    }
}
