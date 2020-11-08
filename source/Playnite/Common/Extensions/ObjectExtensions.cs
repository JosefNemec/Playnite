using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public static T CrateInstance<T>(this Type type)
        {
            return (T)Activator.CreateInstance(type);
        }

        public static T CrateInstance<T>(this Type type, params object[] parameters)
        {
            return (T)Activator.CreateInstance(type, parameters);
        }

        public static object CreateGenericInstance(Type genericTypeDefinition, Type genericType)
        {
            Type resultType = genericTypeDefinition.MakeGenericType(genericType);
            return Activator.CreateInstance(resultType);
        }

        public static object CreateGenericInstance(Type genericTypeDefinition, Type genericType, params object[] parameters)
        {
            Type resultType = genericTypeDefinition.MakeGenericType(genericType);
            return Activator.CreateInstance(resultType, parameters);
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
    }
}
