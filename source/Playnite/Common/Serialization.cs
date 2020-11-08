using Nett;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Data;
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

    public class DataSerializer : IDataSerializer
    {
        public string ToYaml(object obj)
        {
            return Serialization.ToYaml(obj);
        }

        public T FromYaml<T>(string yaml) where T : class, new()
        {
            return Serialization.FromYaml<T>(yaml);
        }

        public T FromYamlFile<T>(string filePath) where T : class, new()
        {
            return Serialization.FromYamlFile<T>(filePath);
        }

        public string ToJson(object obj, bool formatted = false)
        {
            return Serialization.ToJson(obj, formatted);
        }

        public void ToJsonSteam(object obj, Stream stream, bool formatted = false)
        {
            Serialization.ToJsonSteam(obj, stream, formatted);
        }

        public T FromJson<T>(string json) where T : class, new()
        {
            return Serialization.FromJson<T>(json);
        }

        public T FromJsonStream<T>(Stream stream) where T : class, new()
        {
            return Serialization.FromJsonStream<T>(stream);
        }

        public T FromJsonFile<T>(string filePath) where T : class, new()
        {
            return Serialization.FromJsonFile<T>(filePath);
        }

        public T FromToml<T>(string toml) where T : class, new()
        {
            return Serialization.FromToml<T>(toml);
        }

        public T FromTomlFile<T>(string filePath) where T : class, new()
        {
            return Serialization.FromTomlFile<T>(filePath);
        }
    }

    public static class Serialization
    {
        private static readonly ILogger logger = LogManager.GetLogger();
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

        public static void ToJsonSteam(object obj, Stream stream, bool formatted = false)
        {
            using (var sw = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(sw))
            {
                var ser = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    Formatting = formatted ? Formatting.Indented : Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore
                });

                ser.Serialize(writer, obj);
            }
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

        public static T FromJsonStream<T>(Stream stream) where T : class
        {
            using (var sr = new StreamReader(stream))
            using (var reader = new JsonTextReader(sr))
            {
                return new JsonSerializer().Deserialize<T>(reader);
            }
        }

        public static T FromJsonFile<T>(string filePath) where T : class
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return FromJsonStream<T>(fs);
            }
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
