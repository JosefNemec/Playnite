using Nett;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Common
{
    public static class SerializationExtensions
    {
        public static string ToJson(this object obj, bool formatted = false)
        {
            return Serialization.ToJson(obj, formatted);
        }
    }

    public static class Serialization
    {
        private static ILogger logger = LogManager.GetLogger();

        public static string ToYaml(object obj)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(obj);
        }

        public static T FromYaml<T>(string yaml) where T : class
        {
            try
            {
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<T>(yaml);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to deserialize {typeof(T).FullName} from yaml:");
                logger.Debug(yaml);
                throw;
            }
        }

        public static T FromYamlFile<T>(string filePath) where T : class
        {
            return FromYaml<T>(File.ReadAllText(filePath));
        }

        public static string ToJson(object obj, bool formatted = false)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                Formatting = formatted ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public static T FromJson<T>(string json) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to deserialize {typeof(T).FullName} from json:");
                logger.Debug(json);
                throw;
            }
        }

        public static T FromJsonFile<T>(string filePath) where T : class
        {
            return FromJson<T>(File.ReadAllText(filePath));
        }

        public static bool TryFromJson<T>(string json, out T deserialized) where T : class
        {
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static T FromToml<T>(string toml) where T : class
        {
            try
            {
                return Toml.ReadString<T>(toml);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to deserialize {typeof(T).FullName} from toml:");
                logger.Debug(toml);
                throw;
            }
        }

        public static T FromTomlFile<T>(string filePath) where T : class
        {
            return FromToml<T>(File.ReadAllText(filePath));
        }
    }
}
