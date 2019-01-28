using LiteDB;
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

    public enum GameCategory : ulong
    {
        MainGame = 0,
        DLC = 1,
        Expansion = 2,
        Bundle = 3,
        StandaloneExpansion = 4
    }

    public class SteamIdGame
    {
        [BsonId(false)]
        public ulong steamId { get; set; }
        public ulong igdbId { get; set; }
        public DateTime creation_time { get; set; }
    }

    public class Website
    {
        public WebSiteCategory category;
        public string url;
    }

    public class AlternativeName
    {
        public string name;
        public string comment;
    }

    public class Image
    {
        public string url;
        public string cloudinary_id;
        public uint width;
        public uint height;
    }

    public class Video
    {
        public string name;
        public string video_id;
    }

    public class TimeTobeat
    {
        public ulong hastly;
        public ulong normally;
        public ulong completely;
    }

    public class ReleaseDate
    {
        public ulong id;
        public ulong game;
        public ulong category;
        public ulong platform;
        public string human;
        public long updated_at;
        public long created_at;
        public long date;
        public uint region;
        public uint y;
        public uint m;
    }

    public class IgdbItem
    {
        public ulong id;
        public string name;
        public string slug;
        public string url;
    }

    public class ParsedGame : Game
    {
        public new List<string> developers;
        public new List<string> publishers;
        public new List<string> genres;
        public new List<string> themes;
        public new List<string> game_modes;
        public new string cover;
        public new List<string> platforms;
    }

    public class Game : IgdbItem
    {
        public string summary;
        public string storyline;
        public double popularity;
        public ulong franchise;
        public ulong collection;
        public ulong game;
        public ulong version_parent;
        public GameCategory category;
        public TimeTobeat time_to_beat;
        public List<ulong> developers;
        public List<ulong> publishers;
        public List<ulong> genres;
        public List<ulong> themes;
        public List<ulong> game_modes;
        public long first_release_date;
        public Image cover;
        public List<Website> websites;
        public double rating;
        public double aggregated_rating;
        public double total_rating;
        public List<AlternativeName> alternative_names;
        public Dictionary<string, string> external;
        public List<Image> screenshots;
        public List<Image> artworks;
        public List<Video> videos;
        public List<ulong> platforms;
        public List<ReleaseDate> release_dates;
    }

    public class Platform : IgdbItem
    {
        public Image logo;
    }

    public class GameMode : IgdbItem
    {
    }

    public class Theme : IgdbItem
    {
    }

    public class Company : IgdbItem
    {
        public Image logo;
    }

    public class Genre : IgdbItem
    {
    }
}
