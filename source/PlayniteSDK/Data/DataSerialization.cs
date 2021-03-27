using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Data
{
    /// <summary>
    ///
    /// </summary>
    public class DontSerializeAttribute : Attribute
    {
    }

    /// <summary>
    ///
    /// </summary>
    public class SerializationPropertyNameAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="propertyName"></param>
        public SerializationPropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum Format
    {
        /// <summary>
        ///
        /// </summary>
        Json,
        /// <summary>
        ///
        /// </summary>
        Yaml
    }

    /// <summary>
    /// Describes data serializer.
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serailize an object to YAML string.
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
        /// Deserialize an object from a file containing YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromYamlFile<T>(string filePath) where T : class;

        /// <summary>
        /// Serailize an object to JSON string.
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
        void ToJsonSteam(object obj, Stream stream, bool formatted = false);

        /// <summary>
        /// Deserialize an object from JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        T FromJson<T>(string json) where T : class;

        /// <summary>
        /// Deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T FromJsonStream<T>(Stream stream) where T : class;

        /// <summary>
        /// Deserialize an object from a file containing JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromJsonFile<T>(string filePath) where T : class;

        /// <summary>
        /// Deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <returns></returns>
        T FromToml<T>(string toml) where T : class;

        /// <summary>
        /// Deserialize an object from a file containing TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromTomlFile<T>(string filePath) where T : class;

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

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        T FromStream<T>(Stream stream, Format dataFormat) where T : class;

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        T FromFile<T>(string path, Format dataFormat) where T : class;

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        T FromString<T>(string data, Format dataFormat) where T : class;

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        void ToFile(object data, string path, Format dataFormat);

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        string ToString(object data, Format dataFormat);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        /// <param name="deserialized"></param>
        /// <returns></returns>
        bool TryFromFile<T>(string path, Format dataFormat, out T deserialized) where T : class;

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <param name="deserialized"></param>
        /// <returns></returns>
        bool TryFromString<T>(string data, Format dataFormat, out T deserialized) where T : class;
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
        /// Serailize an object to YAML string.
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
        /// Serailize an object to JSON string.
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
        public static void ToJsonSteam(object obj, Stream stream, bool formatted = false)
        {
            serializer.ToJsonSteam(obj, stream, formatted);
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

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        public static T FromStream<T>(Stream stream, Format dataFormat) where T : class
        {
            return serializer.FromStream<T>(stream, dataFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        public static T FromFile<T>(string path, Format dataFormat) where T : class
        {
            return serializer.FromFile<T>(path, dataFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        public static T FromString<T>(string data, Format dataFormat) where T : class
        {
            return serializer.FromString<T>(data, dataFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        public static void ToFile(object data, string path, Format dataFormat)
        {
            serializer.ToFile(data, path, dataFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <returns></returns>
        public static string ToString(object data, Format dataFormat)
        {
            return serializer.ToString(data, dataFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="dataFormat"></param>
        /// <param name="deserialized"></param>
        /// <returns></returns>
        public static bool TryFromFile<T>(string path, Format dataFormat, out T deserialized) where T : class
        {
            return serializer.TryFromFile<T>(path, dataFormat, out deserialized);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dataFormat"></param>
        /// <param name="deserialized"></param>
        /// <returns></returns>
        public static bool TryFromString<T>(string data, Format dataFormat, out T deserialized) where T : class
        {
            return serializer.TryFromString<T>(data, dataFormat, out deserialized);
        }
    }
}
