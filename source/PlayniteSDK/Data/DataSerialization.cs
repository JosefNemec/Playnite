using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Data
{
    /// <summary>
    /// Represents serialization attribute to indicate that object member should be ignored during serialization.
    /// </summary>
    public class DontSerializeAttribute : Attribute
    {
    }

    /// <summary>
    /// Represents serialization attribute to set a specific serialization member name.
    /// </summary>
    public class SerializationPropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets serialization member name.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="SerializationPropertyNameAttribute"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        public SerializationPropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// Describes data serializer.
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serialize an object to YAML string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string ToYaml(object obj);

        /// <summary>
        /// Deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <returns></returns>
        T FromYaml<T>(string yaml) where T : class;

        /// <summary>
        /// Tries to deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromYaml<T>(string yaml, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromYaml<T>(string yaml, out T content, out Exception error) where T : class;

        /// <summary>
        /// Deserialize an object from a file containing YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromYamlFile<T>(string filePath) where T : class;

        /// <summary>
        /// Tries to serialize an object to YAML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromYamlFile<T>(string filePath, out T content) where T : class;

        /// <summary>
        /// Tries to serialize an object to YAML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromYamlFile<T>(string filePath, out T content, out Exception error) where T : class;

        /// <summary>
        /// Serialize an object to JSON string.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatted"></param>
        /// <returns></returns>
        string ToJson(object obj, bool formatted = false);

        /// <summary>
        /// Serialize an object to JSON string written to a stream.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <param name="formatted"></param>
        void ToJsonStream(object obj, Stream stream, bool formatted = false);

        /// <summary>
        /// Deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        T FromJson<T>(string json) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromJson<T>(string json, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromJson<T>(string json, out T content, out Exception error) where T : class;

        /// <summary>
        /// Deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T FromJsonStream<T>(Stream stream) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromJsonStream<T>(Stream stream, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromJsonStream<T>(Stream stream, out T content, out Exception error) where T : class;

        /// <summary>
        /// Deserialize an object from a file containing JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromJsonFile<T>(string filePath) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromJsonFile<T>(string filePath, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from JSON file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromJsonFile<T>(string filePath, out T content, out Exception error) where T : class;

        /// <summary>
        /// Deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <returns></returns>
        T FromToml<T>(string toml) where T : class;

        /// <summary>
        /// Tries to deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromToml<T>(string toml, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromToml<T>(string toml, out T content, out Exception error) where T : class;

        /// <summary>
        /// Deserialize an object from a file containing TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromTomlFile<T>(string filePath) where T : class;

        /// <summary>
        /// Tries to deserialize an object from TOML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        bool TryFromTomlFile<T>(string filePath, out T content) where T : class;

        /// <summary>
        /// Tries to deserialize an object from TOML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool TryFromTomlFile<T>(string filePath, out T content, out Exception error) where T : class;

        /// <summary>
        /// Creates clone of an object using json serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        T GetClone<T>(T source) where T : class;

        /// <summary>
        /// Creates clone of an object using json serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        U GetClone<T, U>(T source)
            where T : class
            where U : class;

        /// <summary>
        /// Compares two objects using json serialization.
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns></returns>
        bool AreObjectsEqual(object object1, object object2);
    }

    /// <summary>
    /// Represents data serialization utility.
    /// </summary>
    public class Serialization
    {
        private static IDataSerializer serializer;

        internal static void Init(IDataSerializer dataSerializer)
        {
            serializer = dataSerializer;
        }

        /// <summary>
        /// Serialize an object to YAML string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToYaml(object obj)
        {
            return serializer.ToYaml(obj);
        }

        /// <summary>
        /// Deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <returns></returns>
        public static T FromYaml<T>(string yaml) where T : class
        {
            return serializer.FromYaml<T>(yaml);
        }

        /// <summary>
        /// Tries to deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromYaml<T>(string yaml, out T content) where T : class
        {
            return serializer.TryFromYaml<T>(yaml, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromYaml<T>(string yaml, out T content, out Exception error) where T : class
        {
            return serializer.TryFromYaml<T>(yaml, out content, out error);
        }

        /// <summary>
        /// Deserialize an object from a file containing YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromYamlFile<T>(string filePath) where T : class
        {
            return serializer.FromYamlFile<T>(filePath);
        }

        /// <summary>
        /// Tries to deserialize an object from YAML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromYamlFile<T>(string filePath, out T content) where T : class
        {
            return serializer.TryFromYamlFile<T>(filePath, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from YAML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromYamlFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return serializer.TryFromYamlFile<T>(filePath, out content, out error);
        }

        /// <summary>
        /// Serialize an object to JSON string.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatted"></param>
        /// <returns></returns>
        public static string ToJson(object obj, bool formatted = false)
        {
            return serializer.ToJson(obj, formatted);
        }

        /// <summary>
        /// Serialize an object to JSON string written to a stream.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <param name="formatted"></param>
        public static void ToJsonStream(object obj, Stream stream, bool formatted = false)
        {
            serializer.ToJsonStream(obj, stream, formatted);
        }

        /// <summary>
        /// Deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T FromJson<T>(string json) where T : class
        {
            return serializer.FromJson<T>(json);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromJson<T>(string json, out T content) where T : class
        {
            return serializer.TryFromJson<T>(json, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromJson<T>(string json, out T content, out Exception error) where T : class
        {
            return serializer.TryFromJson<T>(json, out content, out error);
        }

        /// <summary>
        /// Deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T FromJsonStream<T>(Stream stream) where T : class
        {
            return serializer.FromJsonStream<T>(stream);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromJsonStream<T>(Stream stream, out T content) where T : class
        {
            return serializer.TryFromJsonStream<T>(stream, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromJsonStream<T>(Stream stream, out T content, out Exception error) where T : class
        {
            return serializer.TryFromJsonStream<T>(stream, out content, out error);
        }

        /// <summary>
        /// Deserialize an object from a file containing JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromJsonFile<T>(string filePath) where T : class
        {
            return serializer.FromJsonFile<T>(filePath);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromJsonFile<T>(string filePath, out T content) where T : class
        {
            return serializer.TryFromJsonFile<T>(filePath, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from JSON file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromJsonFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return serializer.TryFromJsonFile<T>(filePath, out content, out error);
        }

        /// <summary>
        /// Deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <returns></returns>
        public static T FromToml<T>(string toml) where T : class
        {
            return serializer.FromToml<T>(toml);
        }

        /// <summary>
        /// Tries to deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromToml<T>(string toml, out T content) where T : class
        {
            return serializer.TryFromToml<T>(toml, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromToml<T>(string toml, out T content, out Exception error) where T : class
        {
            return serializer.TryFromToml<T>(toml, out content, out error);
        }

        /// <summary>
        /// Deserialize an object from a file containing TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromTomlFile<T>(string filePath) where T : class
        {
            return serializer.FromTomlFile<T>(filePath);
        }

        /// <summary>
        /// Tries to deserialize an object from TOML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryFromTomlFile<T>(string filePath, out T content) where T : class
        {
            return serializer.TryFromTomlFile<T>(filePath, out content);
        }

        /// <summary>
        /// Tries to deserialize an object from TOML file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryFromTomlFile<T>(string filePath, out T content, out Exception error) where T : class
        {
            return serializer.TryFromTomlFile<T>(filePath, out content, out error);
        }

        /// <summary>
        /// Creates clone of an object using json serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T GetClone<T>(T source) where T : class
        {
            return serializer.GetClone<T>(source);
        }

        /// <summary>
        /// Creates clone of an object using json serialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static U GetClone<T, U>(T source)
            where T : class
            where U : class
        {
            return serializer.GetClone<T, U>(source);
        }

        /// <summary>
        /// Compares two objects using json serialization.
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns></returns>
        public static bool AreObjectsEqual(object object1, object object2)
        {
            return serializer.AreObjectsEqual(object1, object2);
        }
    }
}
