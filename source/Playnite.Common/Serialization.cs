using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Common
{
    public static class Serialization
    {
        public static string ToYaml(object obj)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(obj);
        }

        public static T FromYaml<T>(string yaml) where T : class
        {
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            return deserializer.Deserialize<T>(yaml);
        }

        public static string ToJson(object obj, bool formatted = false)
        {
            return JsonConvert.SerializeObject(obj, formatted ? Formatting.Indented : Formatting.None);
        }

        public static T FromJson<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
