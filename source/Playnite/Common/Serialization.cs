using Nett;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

    public class JsonResolver : DefaultContractResolver
    {
        public static JsonResolver Global { get; } = new JsonResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (Attribute.IsDefined(member, typeof(SerializationPropertyNameAttribute)))
            {
                var att = (SerializationPropertyNameAttribute)Attribute.GetCustomAttribute(member, typeof(SerializationPropertyNameAttribute));
                prop.PropertyName = att.PropertyName;
            }

            return prop;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            return objectType.
                GetMembers(BindingFlags.Public | BindingFlags.Instance).
                Where(a => a is PropertyInfo || a is FieldInfo).
                Where(a => !Attribute.IsDefined(a, typeof(DontSerializeAttribute)) && !Attribute.IsDefined(a, typeof(JsonIgnoreAttribute))).
                ToList();
        }
    }

    public class DataSerializer : IDataSerializer
    {
        public string ToYaml(object obj)
        {
            return Serialization.ToYaml(obj);
        }

        public T FromYaml<T>(string yaml) where T : class
        {
            return Serialization.FromYaml<T>(yaml);
        }

        public T FromYamlFile<T>(string filePath) where T : class
        {
            return Serialization.FromYamlFile<T>(filePath);
        }

        public string ToJson(object obj, bool formatted = false)
        {
            return Serialization.ToJson(obj, formatted);
        }

        public void ToJsonStream(object obj, Stream stream, bool formatted = false)
        {
            Serialization.ToJsonStream(obj, stream, formatted);
        }

        public T FromJson<T>(string json) where T : class
        {
            return Serialization.FromJson<T>(json);
        }

        public T FromJsonStream<T>(Stream stream) where T : class
        {
            return Serialization.FromJsonStream<T>(stream);
        }

        public T FromJsonFile<T>(string filePath) where T : class
        {
            return Serialization.FromJsonFile<T>(filePath);
        }

        public T FromToml<T>(string toml) where T : class
        {
            return Serialization.FromToml<T>(toml);
        }

        public T FromTomlFile<T>(string filePath) where T : class
        {
            return Serialization.FromTomlFile<T>(filePath);
        }

        public bool TryFromYaml<T>(string yaml, out T content) where T : class
        {
            return Serialization.TryFromYaml(yaml, out content);
        }

        public bool TryFromYaml<T>(string yaml, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromYaml(yaml, out content, out error);
        }

        public bool TryFromYamlFile<T>(string filePath, out T content) where T : class
        {
            return Serialization.TryFromYamlFile(filePath, out content);
        }

        public bool TryFromYamlFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromYamlFile(filePath, out content, out error);
        }

        public bool TryFromJson<T>(string json, out T content) where T : class
        {
            return Serialization.TryFromJson(json, out content);
        }

        public bool TryFromJson<T>(string json, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromJson(json, out content, out error);
        }

        public bool TryFromJsonStream<T>(Stream stream, out T content) where T : class
        {
            return Serialization.TryFromJsonStream(stream, out content);
        }

        public bool TryFromJsonStream<T>(Stream stream, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromJsonStream(stream, out content, out error);
        }

        public bool TryFromJsonFile<T>(string filePath, out T content) where T : class
        {
            return Serialization.TryFromJsonFile(filePath, out content);
        }

        public bool TryFromJsonFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromJsonFile(filePath, out content, out error);
        }

        public bool TryFromToml<T>(string toml, out T content) where T : class
        {
            return Serialization.TryFromToml(toml, out content);
        }

        public bool TryFromToml<T>(string toml, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromToml(toml, out content, out error);
        }

        public bool TryFromTomlFile<T>(string filePath, out T content) where T : class
        {
            return Serialization.TryFromTomlFile(filePath, out content);
        }

        public bool TryFromTomlFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return Serialization.TryFromTomlFile(filePath, out content, out error);
        }

        public bool AreObjectsEqual(object object1, object object2)
        {
            return object1.IsEqualJson(object2);
        }

        public T GetClone<T>(T source) where T : class
        {
            return source.GetClone<T>();
        }

        public U GetClone<T, U>(T source)
            where T : class
            where U : class
        {
            return source.GetClone<T, U>();
        }
    }

    public static class Serialization
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly JsonSerializerSettings jsonDesSettings = new JsonSerializerSettings
        {
            ContractResolver = JsonResolver.Global,
            MaxDepth = 128
        };

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

        public static bool TryFromYaml<T>(string yaml, out T deserialized) where T : class
        {
            try
            {
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                deserialized = deserializer.Deserialize<T>(yaml);
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromYaml<T>(string yaml, out T deserialized, out Exception error) where T : class
        {
            try
            {
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                deserialized = deserializer.Deserialize<T>(yaml);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }

        public static T FromYamlFile<T>(string filePath) where T : class
        {
            return FromYaml<T>(FileSystem.ReadStringFromFile(filePath));
        }

        public static bool TryFromYamlFile<T>(string filePath, out T deserialized) where T : class
        {
            try
            {
                deserialized = FromYaml<T>(FileSystem.ReadStringFromFile(filePath));
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromYamlFile<T>(string filePath, out T deserialized, out Exception error) where T : class
        {
            try
            {
                deserialized = FromYaml<T>(FileSystem.ReadStringFromFile(filePath));
                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }

        public static T FromYamlStream<T>(Stream stream) where T : class
        {
            using (var sr = new StreamReader(stream, true))
            {
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<T>(sr);
            }
        }

        public static string ToJson(object obj, bool formatted = false, params JsonConverter[] converters)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                Formatting = formatted ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = JsonResolver.Global,
                Converters = converters,
                MaxDepth = 128
            });
        }

        public static void ToJsonStream(object obj, Stream stream, bool formatted = false)
        {
            using (var sw = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            using (var writer = new JsonTextWriter(sw))
            {
                var ser = JsonSerializer.Create(new JsonSerializerSettings()
                {
                    Formatting = formatted ? Formatting.Indented : Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = JsonResolver.Global,
                    MaxDepth = 128
                });

                ser.Serialize(writer, obj);
            }
        }

        public static T FromJson<T>(string json) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, jsonDesSettings);
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
                return JsonSerializer.Create(jsonDesSettings).Deserialize<T>(reader);
            }
        }

        public static bool TryFromJsonStream<T>(Stream stream, out T deserialized) where T : class
        {
            try
            {
                using (var sr = new StreamReader(stream))
                using (var reader = new JsonTextReader(sr))
                {
                    deserialized = JsonSerializer.Create(jsonDesSettings).Deserialize<T>(reader);
                }

                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromJsonStream<T>(Stream stream, out T deserialized, out Exception error) where T : class
        {
            try
            {
                using (var sr = new StreamReader(stream))
                using (var reader = new JsonTextReader(sr))
                {
                    deserialized = JsonSerializer.Create(jsonDesSettings).Deserialize<T>(reader);
                }

                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }

        public static T FromJsonFile<T>(string filePath) where T : class
        {
            filePath = Paths.FixPathLength(filePath);
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return FromJsonStream<T>(fs);
            }
        }

        public static bool TryFromJsonFile<T>(string filePath, out T deserialized) where T : class
        {
            try
            {
                filePath = Paths.FixPathLength(filePath);
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    deserialized = FromJsonStream<T>(fs);
                }

                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromJsonFile<T>(string filePath, out T deserialized, out Exception error) where T : class
        {
            try
            {
                filePath = Paths.FixPathLength(filePath);
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    deserialized = FromJsonStream<T>(fs);
                }

                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }

        public static bool TryFromJson<T>(string json, out T deserialized) where T : class
        {
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(json, jsonDesSettings);
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromJson<T>(string json, out T deserialized, out Exception error) where T : class
        {
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(json, jsonDesSettings);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
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

        public static bool TryFromToml<T>(string toml, out T deserialized) where T : class
        {
            try
            {
                deserialized = Toml.ReadString<T>(toml);
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromToml<T>(string toml, out T deserialized, out Exception error) where T : class
        {
            try
            {
                deserialized = Toml.ReadString<T>(toml);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }

        public static T FromTomlFile<T>(string filePath) where T : class
        {
            return FromToml<T>(FileSystem.ReadStringFromFile(filePath));
        }

        public static bool TryFromTomlFile<T>(string filePath, out T deserialized) where T : class
        {
            try
            {
                deserialized = FromToml<T>(FileSystem.ReadStringFromFile(filePath));
                return true;
            }
            catch
            {
                deserialized = null;
                return false;
            }
        }

        public static bool TryFromTomlFile<T>(string filePath, out T deserialized, out Exception error) where T : class
        {
            try
            {
                deserialized = FromToml<T>(FileSystem.ReadStringFromFile(filePath));
                error = null;
                return true;
            }
            catch (Exception e)
            {
                deserialized = null;
                error = e;
                return false;
            }
        }
    }
}
