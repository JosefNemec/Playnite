using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Playnite;

public class Serialization
{
    private static readonly ILogger logger = LogManager.GetLogger();

    private static readonly JsonSerializerOptions jsonDesSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    #region YAML
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

    public static bool TryFromYaml<T>(string yaml, out T? deserialized, out Exception? error) where T : class
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

    public static bool TryFromYamlFile<T>(string filePath, out T? deserialized, out Exception? error) where T : class
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
        using var sr = new StreamReader(stream, true);
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        return deserializer.Deserialize<T>(sr);
    }
    #endregion YAML

    #region JSON
    public static string ToJson(object obj, bool formatted = false)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = formatted,
            IncludeFields = true
        });
    }

    public static T? FromJson<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, jsonDesSettings);
        }
        catch (Exception e)
        {
            logger.Error(e, $"Failed to deserialize {typeof(T).FullName} from json:");
            logger.Debug(json);
            throw;
        }
    }

    public static T? FromJsonStream<T>(Stream stream) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(stream, jsonDesSettings);
        }
        catch (Exception e)
        {
            logger.Error(e, $"Failed to deserialize {typeof(T).FullName} from json stream.");
            throw;
        }
    }

    public static bool TryFromJsonStream<T>(Stream stream, out T? deserialized, out Exception? error) where T : class
    {
        try
        {
            deserialized = JsonSerializer.Deserialize<T>(stream, jsonDesSettings);
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

    public static T? FromJsonFile<T>(string filePath) where T : class
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return FromJsonStream<T>(fs);
    }

    public static bool TryFromJsonFile<T>(string filePath, out T? deserialized, out Exception? error) where T : class
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            deserialized = FromJsonStream<T>(fs);
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

    public static bool TryFromJson<T>(string json, out T? deserialized, out Exception? error) where T : class
    {
        try
        {
            deserialized = JsonSerializer.Deserialize<T>(json, jsonDesSettings);
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
    #endregion JSON
}
