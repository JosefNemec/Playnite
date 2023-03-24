using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

public static class ObjectExtensions
{
    public static bool HasMethod(this object obj, string methodName)
    {
        if (obj == null)
        {
            return false;
        }

        try
        {
            return obj.GetType().GetMethod(methodName) != null;
        }
        catch (AmbiguousMatchException)
        {
            // Ambiguous means there is more than one result
            return true;
        }
    }

    public static object CrateInstance(this Type type)
    {
        var obj = Activator.CreateInstance(type);
        if (obj is null)
        {
            throw new Exception($"Failed to create instance of {type.Name}");
        }

        return obj;
    }

    public static T CrateInstance<T>(this Type type)
    {
        var obj = Activator.CreateInstance(type);
        if (obj is not T)
        {
            throw new Exception($"Failed to create instance of {type.Name}");
        }

        return (T)obj;
    }

    public static T CrateInstance<T>(this Type type, params object[] parameters)
    {
        var obj = Activator.CreateInstance(type, parameters);
        if (obj is not T)
        {
            throw new Exception($"Failed to create instance of {type.Name}");
        }

        return (T)obj;
    }

    public static object CreateGenericInstance(Type genericTypeDefinition, Type genericType)
    {
        Type resultType = genericTypeDefinition.MakeGenericType(genericType);
        var obj = Activator.CreateInstance(resultType);
        if (obj is null)
        {
            throw new Exception($"Failed to create generic instance of {genericTypeDefinition.Name}>{genericType.Name}");
        }

        return obj;
    }

    public static object CreateGenericInstance(Type genericTypeDefinition, Type genericType, params object[] parameters)
    {
        Type resultType = genericTypeDefinition.MakeGenericType(genericType);
        var obj = Activator.CreateInstance(resultType, parameters);
        if (obj is null)
        {
            throw new Exception($"Failed to create generic instance of {genericTypeDefinition.Name}>{genericType.Name}");
        }

        return obj;
    }

    public static bool HasPropertyAttribute<TAttribute>(this Type type, string propertyName) where TAttribute : Attribute
    {
        var prop = type.GetProperty(propertyName);
        if (prop == null)
        {
            return false;
        }
        else
        {
            return prop.GetCustomAttribute(typeof(TAttribute)) != null;
        }
    }

    public static bool IsGenericList(this Type type, [NotNullWhen(true)] out Type? itemType)
    {
        var isGeneric = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>));
        if (isGeneric)
        {
            itemType = type.GenericTypeArguments.First();
            return true;
        }
        else
        {
            itemType = null;
            return false;
        }
    }

    private static readonly JsonSerializerOptions jsonSerializerSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = false,
        IncludeFields = true,
        PropertyNameCaseInsensitive = false
    };

    /// <summary>
    /// Perform a deep copy of the object, using Json as a serialisation method.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T? GetClone<T>(this T source)
    {
        if (source is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(source, jsonSerializerSettings));
    }

    public static U? GetClone<T, U>(this T source)
    {
        if (source is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<U>(JsonSerializer.Serialize(source, jsonSerializerSettings));
    }

    public static bool IsEqualJson(this object source, object targer)
    {
        var first = JsonSerializer.Serialize(source, jsonSerializerSettings);
        var second = JsonSerializer.Serialize(targer, jsonSerializerSettings);
        return first == second;
    }

    /// <summary>
    /// Extension for 'Object' that copies the properties to a destination object.
    /// Courtesy of http://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    public static void CopyProperties(this object source, object destination, bool diffOnly, List<string>? ignoreNames = null, bool acceptJsonIgnore = false)
    {
        // If any this null throw an exception
        if (source == null || destination == null)
            throw new Exception("Source or/and Destination Objects are null");
        // Getting the Types of the objects
        Type typeDest = destination.GetType();
        Type typeSrc = source.GetType();

        // Iterate the Properties of the source instance and
        // populate them from their desination counterparts
        PropertyInfo[] srcProps = typeSrc.GetProperties();
        foreach (var srcProp in srcProps)
        {
            if (ignoreNames?.Any() == true && ignoreNames.Contains(srcProp.Name))
            {
                continue;
            }

            if (!srcProp.CanRead)
            {
                continue;
            }

            var targetProperty = typeDest.GetProperty(srcProp.Name);
            if (targetProperty == null)
            {
                continue;
            }

            if (!targetProperty.CanWrite)
            {
                continue;
            }

            var method = targetProperty.GetSetMethod(true);
            if (method != null && method.IsPrivate)
            {
                continue;
            }

            if ((targetProperty.GetSetMethod()?.Attributes & MethodAttributes.Static) != 0)
            {
                continue;
            }

            if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
            {
                continue;
            }

            if (acceptJsonIgnore && Attribute.IsDefined(targetProperty, typeof(JsonIgnoreAttribute)))
            {
                continue;
            }

            var sourceValue = srcProp.GetValue(source);
            var targetValue = targetProperty.GetValue(destination);
            if (sourceValue == null && targetValue == null)
            {
                continue;
            }

            // TODO Add support for lists
            if (sourceValue is IComparable && diffOnly)
            {
                var equal = ((IComparable)sourceValue).CompareTo(targetValue) == 0;
                if (!equal)
                {
                    targetProperty.SetValue(destination, sourceValue);
                }
            }
            else
            {
                if (sourceValue != null)
                {
                    var genericComparable = sourceValue.GetType().GetInterface("IComparable`1");
                    if (genericComparable != null && genericComparable.GenericTypeArguments.Any(a => a == sourceValue.GetType()) && diffOnly)
                    {
                        var res = genericComparable.GetMethod("CompareTo")!.Invoke(sourceValue, targetValue != null ? new object[] { targetValue } : null);
                        if (res is int resValue && resValue != 0)
                        {
                            targetProperty.SetValue(destination, sourceValue);
                        }

                        continue;
                    }
                }

                targetProperty.SetValue(destination, sourceValue);
            }
        }
    }
}
