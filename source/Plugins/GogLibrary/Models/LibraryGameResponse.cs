using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
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
}
