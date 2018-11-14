using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
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
}
