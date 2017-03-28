using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Playnite
{
    public static class CloneObject
    {
        /// <summary>
        /// Perform a deep copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
        }

        public static bool IsEqualJson(this object source, object targer)
        {
            var first = JsonConvert.SerializeObject(source);
            var second = JsonConvert.SerializeObject(targer);
            return first == second;
        }

        public static bool IsListEqual<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source == null && target == null)
            {
                return true;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            var firstNotSecond = source.Except(target).ToList();
            if (firstNotSecond.Count != 0)
            {
                return false;
            }

            var secondNotFirst = target.Except(source).ToList();
            if (secondNotFirst.Count != 0)
            {
                return false;
            }

            return true;
        }
    }

}
