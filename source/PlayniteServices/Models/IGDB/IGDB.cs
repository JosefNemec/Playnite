﻿using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.IGDB
{
    public enum WebSiteCategory : ulong
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

    public class SteamIdGame
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public ulong steamId
        {
            get; set;
        }

        public ulong igdbId
        {
            get; set;
        }

        public DateTime creation_time
        {
            get; set;
        }
    }

    public class GamesSearch
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public string keyword
        {
            get; set;
        }

        public List<Game> results
        {
            get; set;
        }

        public DateTime creation_time
        {
            get; set;
        }
    }

    public class Website
    {
        public WebSiteCategory category
        {
            get; set;
        }

        public string url
        {
            get; set;
        }
    }

    public class AlternativeName
    { 
        public string name
        {
            get; set;
        }

        public string comment
        {
            get; set;
        }
    }

    public class Image
    {
        public string url
        {
            get; set;
        }

        public string cloudinary_id
        {
            get; set;
        }

        public uint width
        {
            get; set;
        }

        public uint height
        {
            get; set;
        }
    }

    public class Video
    {
        public string name
        {
            get; set;
        }

        public string video_id
        {
            get; set;
        }
    }

    public class ReleaseDate
    {
        public ulong id { get; set; }
        public ulong game { get; set; }
        public ulong category { get; set; }
        public ulong platform { get; set; }
        public string human { get; set; }
        public long updated_at { get; set; }
        public long created_at { get; set; }
        public long date { get; set; }
        public uint region { get; set; }
        public uint y { get; set; }
        public uint m { get; set; }
    }

    public class ParsedGame : Game
    {
        [JsonIgnore]
        public DateTime creation_time
        {
            get; set;
        }

        public new List<string> developers
        {
            get; set;
        }

        public new List<string> publishers
        {
            get; set;
        }

        public new List<string> genres
        {
            get; set;
        }

        public new List<string> themes
        {
            get; set;
        }

        public new List<string> game_modes
        {
            get; set;
        }

        public new string cover
        {
            get; set;
        }

        public new List<string> platforms
        {
            get; set;
        }
    }

    public class Game
    {
        public ulong id
        {
            get; set;
        }

        public string url
        {
            get; set;
        }

        public string name
        {
            get; set;
        }

        public string summary
        {
            get; set;
        }

        public List<ulong> developers
        {
            get; set;
        }

        public List<ulong> publishers
        {
            get; set;
        }

        public List<ulong> genres
        {
            get; set;
        }

        public List<ulong> themes
        {
            get; set;
        }

        public List<ulong> game_modes
        {
            get; set;
        }

        public long first_release_date
        {
            get; set;
        }

        public Image cover
        {
            get; set;
        }

        public List<Website> websites
        {
            get; set;
        }

        public double rating
        {
            get; set;
        }

        public double aggregated_rating
        {
            get; set;
        }

        public double total_rating
        {
            get; set;
        }

        public List<AlternativeName> alternative_names
        {
            get; set;
        }

        public Dictionary<string, string> external
        {
            get; set;
        }

        public List<Image> screenshots
        {
            get; set;
        }

        public List<Image> artworks
        {
            get; set;
        }

        public List<Video> videos
        {
            get; set;
        }

        public List<ulong> platforms
        {
            get; set;
        }

        public List<ReleaseDate> release_dates
        {
            get; set;
        }
    }

    public class Platform
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public ulong id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class GameMode
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public ulong id
        {
            get; set;
        }

        public string name
        {
            get; set;
        }
    }

    public class Theme
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public ulong id
        {
            get; set;
        }

        public string name
        {
            get; set;
        }
    }

    public class Company
    {
        [BsonId(false)]
        [BsonIndex(true)]
        public ulong id
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
        public ulong id
        {
            get; set;
        }

        public string name
        {
            get; set;
        }
    }
}
