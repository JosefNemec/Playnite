﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace System
{
    public static class CloneObject
    {
        /// <summary>
        /// Perform a deep copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T GetClone<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }

        public static T GetClone<T>(this T source, JsonSerializerSettings settings)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source, settings), settings);
        }

        public static U GetClone<T, U>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(U);
            }

            return JsonConvert.DeserializeObject<U>(JsonConvert.SerializeObject(source));
        }

        public static U GetClone<T, U>(this T source, JsonSerializerSettings settings)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(U);
            }

            return JsonConvert.DeserializeObject<U>(JsonConvert.SerializeObject(source, settings), settings);
        }

        public static bool IsEqualJson(this object source, object targer)
        {
            var first = JsonConvert.SerializeObject(source);
            var second = JsonConvert.SerializeObject(targer);
            return first == second;
        }

        /// <summary>
        /// Extension for 'Object' that copies the properties to a destination object.
        /// Courtesy of http://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void CopyProperties(this object source, object destination, bool diffOnly, List<string> ignoreNames = null, bool acceptJsonIgnore = false)
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
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (ignoreNames?.Any() == true && ignoreNames.Contains(srcProp.Name))
                {
                    continue;
                }

                if (!srcProp.CanRead)
                {
                    continue;
                }

                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }

                if (!targetProperty.CanWrite)
                {
                    continue;
                }

                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }

                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }

                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }

                if (acceptJsonIgnore && targetProperty.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
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
                            int res = (int)genericComparable.GetMethod("CompareTo").Invoke(sourceValue, new object[] { targetValue });
                            if (res != 0)
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

}
