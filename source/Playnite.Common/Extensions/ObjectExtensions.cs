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

        public static bool Implements<TType>(this object source)
        {
            return typeof(TType).IsAssignableFrom(source.GetType());
        }
    }
}
