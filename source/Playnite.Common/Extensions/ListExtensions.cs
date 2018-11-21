using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ListExtensions
    {
        public static ComparableList<T> ToComparable<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return null;
            }

            return new ComparableList<T>(source);
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return null;
            }

            return new ObservableCollection<T>(source);
        }

        public static bool HasItems<T>(this IEnumerable<T> source)
        {
            return source?.Any() == true;
        }

        public static bool IntersectsPartiallyWith(this List<string> source, List<string> target)
        {
            var intersects = true;

            foreach (var sourceItem in source)
            {
                if (!target.Any(a => a != null && a.IndexOf(sourceItem, StringComparison.InvariantCultureIgnoreCase) >= 0))
                {
                    return false;
                }
            }

            return intersects;
        }

        public static bool IntersectsExactlyWith(this List<string> source, List<string> target)
        {
            var intersects = false;

            foreach (var sourceItem in source)
            {
                if (target.Any(a => a != null && a.Equals(sourceItem, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return intersects;
        }

        public static bool ContainsInsensitive(this List<string> source, string value)
        {
            return source.Any(a => a.Equals(value, StringComparison.InvariantCultureIgnoreCase)) == true;
        }
    }
}
