using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Playnite.Providers.GOG
{
    public class AccountBasicRespose
    {
        public string accessToken;
        public int accessTokenExpires;
        public string avatar;
        public int cacheExpires;
        public string clientId;
        public bool isLoggedIn;
        public string userId;
        public string username;
    }

    public class LibraryGameResponse
    {
        // For some reason game stats are returned as empty array if no stats exist for a game.
        // But single object representation is returned instead if stats do exits, so we need to handle this special case.
        class StatsCollectionConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var token = JToken.ReadFrom(reader);
                if (token.Type == JTokenType.Object)
                {
                    var result = token.ToObject<Dictionary<string, Stats>>();
                    serializer.Populate(token.CreateReader(), result);
                    return result;
                }
                else
                {
                    return null;
                }               
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }


        public class Game
        {
            public string id;
            public bool achievementSupport;
            public string image;
            public string title;
            public string url;
        }

        public class Stats
        {
            public DateTime? lastSession;
            public int playtime;
        }

        public Game game;
        [JsonConverter(typeof(StatsCollectionConverter))]
        public Dictionary<string, Stats> stats;
    }

    public class PagedResponse<T>
    {
        public class Embedded
        {
            public List<T> items;
        }

        public int page;
        public int pages;
        public int total;
        public int limit;
        public Embedded _embedded;
    }

    public class GetOwnedGamesResult
    {
        // Release date can sometimes contains invalid date if game is not released yet.        
        class ReleaseDateConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    DateTime date = (DateTime)value;
                    writer.WriteValue(date.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var dataString = (string)reader.Value;
                if (DateTime.TryParseExact(dataString, "yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) == true)
                {
                    return date;
                }
                else
                {
                    return null;
                }
            }

            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class Tag
        {
            public string id;
            public string name;
            public string productCount;
        }

        public class Availability
        {
            public bool isAvailable;
            public bool isAvailableInAccount;
        }

        public class WorksOn
        {
            public bool Windows;
            public bool Mac;
            public bool Linux;
        }

        public class ReleaseDate
        {
            [JsonConverter(typeof(ReleaseDateConverter))]
            public DateTime? date;
            public int timezone_type;
            public string timezone;
        }

        public class Product
        {
            public bool isGalaxyCompatible;
            public int id;
            public Availability availability;
            public string title;
            public string image;
            public string url;
            public WorksOn worksOn;
            public string category;
            public int rating;
            public bool isComingSoon;
            public bool isMovie;
            public bool isGame;
            public string slug;
            public bool isNew;
            public int dlcCount;
            public ReleaseDate releaseDate;
            public bool isBaseProductMissing;
            public bool isHidingDisabled;
            public bool isInDevelopment;
            public bool isHidden;
        }

        public string sortBy;
        public int page;
        public int totalProducts;
        public int totalPages;
        public int productsPerPage;
        public int moviesCount;
        public List<Tag> tags;
        public List<Product> products;
        public int updatedProductsCount;
        public int hiddenUpdatedProductsCount;
        public bool hasHiddenProducts;
    }

    public class ProductApiDetail
    {
        public class Compatibility
        {
            public bool windows;
            public bool osx;
            public bool linux;
        }

        public class Links
        {
            public string purchase_link;
            public string product_card;
            public string support;
            public string forum;
        }

        public class Images
        {
            public string background;
            public string logo;
            public string logo2x;
            public string icon;
            public string sidebarIcon;
            public string sidebarIcon2x;
        }

        public class Description
        {
            public string lead;
            public string full;
            public string whats_cool_about_it;
        }

        public int id;
        public string title;
        public string slug;
        public Compatibility content_system_compatibility;
        public Dictionary<string, string> languages;
        public Links links;
        public bool is_secret;
        public string game_type;
        public bool is_pre_order;
        public Images images;
        public Description description;
        public DateTime? release_date;
    }

    public class StorePageResult
    {
        public class ProductDetails
        {
            public class Feature
            {
                public string title;
                public string slug;
            }

            public class SluggedName
            {
                public string name;
                public string slug;
            }

            public List<SluggedName> genres;
            public List<Feature> features;
            public SluggedName publisher;
            public SluggedName developer;
            public int? releaseDate;
            public int id;
            public int rating;
        }

        public ProductDetails gameProductData;
    }
}
