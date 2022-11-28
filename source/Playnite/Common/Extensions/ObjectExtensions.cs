using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace System
{
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
    }
}
