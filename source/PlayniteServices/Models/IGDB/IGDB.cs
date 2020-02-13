using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;

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
        Steam = 13,
        Reddit = 14,
        Itch = 15,
        Epic = 16,
        GOG = 17,
        LinkedIn = 18,
        Pinterest = 19,
        SoundCloud = 20,
    }

    public enum GameCategory : ulong
    {
        MainGame = 0,
        DLC = 1,
        Expansion = 2,
        Bundle = 3,
        StandaloneExpansion = 4
    }

    public enum ReleaseDateCategory
    {
        YYYYMMMMDD = 0,
        YYYYMMMM = 1,
        YYYY = 2,
        YYYYQ1 = 3,
        YYYYQ2 = 4,
        YYYYQ3 = 5,
        YYYYQ4 = 6,
        TBD = 7
    }

    public enum Region
    {
        Europe = 1,
        NorthAmerica = 2,
        Australia = 3,
        NewZealand = 4,
        Japan = 5,
        China = 6,
        Asia = 7,
        Worldwide = 8,
    }

    public enum ExternalGameCategory : int
    {
        Steam = 1,
        Gog = 5,
        YouTube = 10,
        Microsoft = 11,
        Apple = 13,
        Twitch = 14,
        Android = 15
    }

    public enum PlatformCategory
    {
        Console = 1,
        Arcade = 2,
        Platform = 3,
        OperatingSystem = 4,
        PortableConsole = 5,
        Computer = 6,
    }

    public class Website : IgdbItem
    {
        public WebSiteCategory category { get; set; }
        public ulong game { get; set; }
        public bool trusted { get; set; }
    }

    public class AlternativeName : IgdbItem
    {
        public string comment { get; set; }
        public ulong game { get; set; }
    }

    public class Image : IgdbItem
    {
        public bool animated { get; set; }
        public bool alpha_channel { get; set; }
        public string image_id { get; set; }
        public uint width { get; set; }
        public uint height { get; set; }
    }

    public class Video : IgdbItem
    {
        public ulong game { get; set; }
        public string video_id { get; set; }
    }

    public class TimeTobeat : IgdbItem
    {
        public ulong game { get; set; }
        public ulong hastly { get; set; }
        public ulong normally { get; set; }
        public ulong completely { get; set; }
    }

    public class ReleaseDate : IgdbItem
    {
        public ReleaseDateCategory category;
        public long date { get; set; }
        public ulong game { get; set; }
        public string human { get; set; }
        public int m { get; set; }
        public ulong platform { get; set; }
        public Region region { get; set; }
        public int y { get; set; }
    }

    public class IgdbItem
    {
        public ulong id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
    }

    public class ExternalGame : IgdbItem
    {
        public ExternalGameCategory category { get; set; }
        public ulong game { get; set; }
        public string uid { get; set; }
        public int year { get; set; }
    }

    public class Platform : IgdbItem
    {
        public string abbreviation { get; set; }
        public string alternative_name { get; set; }
        public PlatformCategory category { get; set; }
        public int generation { get; set; }
        public ulong platform_logo { get; set; }
        public ulong product_family { get; set; }
        public string summary { get; set; }
    }

    public class Franchise : IgdbItem
    {
        public List<ulong> games { get; set; }
    }

    public class ProductFamily : IgdbItem
    {
    }

    public class GameMode : IgdbItem
    {
    }

    public class Theme : IgdbItem
    {
    }

    public class Company : IgdbItem
    {
        public ulong logo { get; set; }
    }

    public class GameImage : Image
    {
        public ulong game { get; set; }
    }

    public class Genre : IgdbItem
    {
    }

    public class PlayerPerspective : IgdbItem
    {
    }

    public class InvolvedCompany : IgdbItem
    {
        public ulong game { get; set; }
        public ulong company { get; set; }
        public bool developer { get; set; }
        public bool porting { get; set; }
        public bool publisher { get; set; }
        public bool supporting { get; set; }
    }

    public class ExpandedInvolvedCompany : InvolvedCompany
    {
        public new Company company { get; set; }
    }

    public class Collection : IgdbItem
    {
        public List<ulong> games { get; set; }
    }

    public class ExpandedGame : Game
    {
        public new Franchise franchise { get; set; }
        public new Collection collection { get; set; }
        public new Game version_parent { get; set; }
        public new TimeTobeat time_to_beat { get; set; }
        public new List<ExpandedInvolvedCompany> involved_companies { get; set; }
        public List<Genre> genres_v3 { get; set; }
        public new List<Theme> themes { get; set; }
        public List<GameMode> game_modes_v3 { get; set; }
        public GameImage cover_v3 { get; set; }
        public new List<Website> websites { get; set; }
        public new List<AlternativeName> alternative_names { get; set; }
        public new List<ExternalGame> external_games { get; set; }
        public new List<GameImage> screenshots { get; set; }
        public new List<GameImage> artworks { get; set; }
        public new List<Video> videos { get; set; }
        public new List<Platform> platforms { get; set; }
        public new List<ReleaseDate> release_dates { get; set; }
        public new List<PlayerPerspective> player_perspectives { get; set; }

        // fallback properties for 4.x
        public new string cover { get; set; }
        public List<string> developers { get; set; }
        public List<string> publishers { get; set; }
        public new List<string> genres { get; set; }
        public new List<string> game_modes { get; set; }
    }

    public class Game : IgdbItem
    {
        public string summary { get; set; }
        public string storyline { get; set; }
        public double popularity { get; set; }
        public ulong franchise { get; set; }
        public ulong collection { get; set; }
        public ulong version_parent { get; set; }
        public string version_title { get; set; }
        public GameCategory category { get; set; }
        public ulong time_to_beat { get; set; }
        public List<ulong> involved_companies { get; set; }
        public List<ulong> genres { get; set; }
        public List<ulong> themes { get; set; }
        public List<ulong> game_modes { get; set; }
        public long first_release_date { get; set; }
        public ulong cover { get; set; }
        public List<ulong> websites { get; set; }
        public double rating { get; set; }
        public double aggregated_rating { get; set; }
        public double total_rating { get; set; }
        public List<ulong> alternative_names { get; set; }
        public List<ulong> external_games { get; set; }
        public List<ulong> screenshots { get; set; }
        public List<ulong> artworks { get; set; }
        public List<ulong> videos { get; set; }
        public List<ulong> platforms { get; set; }
        public List<ulong> release_dates { get; set; }
        public List<ulong> age_ratings { get; set; }
        public List<ulong> similar_games { get; set; }
        public List<ulong> player_perspectives { get; set; }
        public ulong parent_game { get; set; }
    }

    public static class ModelsUtils
    {
        public static string GetIgdbSearchString(string gameName)
        {
            var temp = gameName.Replace(":", " ").Replace("-", " ").ToLower().Trim();
            return Regex.Replace(temp, @"\s+", " ");
        }
    }
}
