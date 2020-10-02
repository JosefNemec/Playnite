using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Data
{
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
        T FromYaml<T>(string yaml) where T : class, new();

        /// <summary>
        /// Deserialize an object from a file containing YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromYamlFile<T>(string filePath) where T : class, new();

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
        T FromJson<T>(string json) where T : class, new();

        /// <summary>
        /// Deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T FromJsonStream<T>(Stream stream) where T : class, new();

        /// <summary>
        /// Deserialize an object from a file containing JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromJsonFile<T>(string filePath) where T : class, new();

        /// <summary>
        /// Deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <returns></returns>
        T FromToml<T>(string toml) where T : class, new();

        /// <summary>
        /// Deserialize an object from a file containing TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        T FromTomlFile<T>(string filePath) where T : class, new();
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
        public static T FromYaml<T>(string yaml) where T : class, new()
        {
            return serializer.FromYaml<T>(yaml);
        }

        /// <summary>
        /// Deserialize an object from a file containing YAML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromYamlFile<T>(string filePath) where T : class, new()
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
        public static T FromJson<T>(string json) where T : class, new()
        {
            return serializer.FromJson<T>(json);
        }

        /// <summary>
        /// Deserialize an object from JSON data stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T FromJsonStream<T>(Stream stream) where T : class, new()
        {
            return serializer.FromJsonStream<T>(stream);
        }

        /// <summary>
        /// Deserialize an object from a file containing JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromJsonFile<T>(string filePath) where T : class, new()
        {
            return serializer.FromJsonFile<T>(filePath);
        }

        /// <summary>
        /// Deserialize an object from TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toml"></param>
        /// <returns></returns>
        public static T FromToml<T>(string toml) where T : class, new()
        {
            return serializer.FromToml<T>(toml);
        }

        /// <summary>
        /// Deserialize an object from a file containing TOML string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T FromTomlFile<T>(string filePath) where T : class, new()
        {
            return serializer.FromTomlFile<T>(filePath);
        }
    }
}
