using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
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

        public static bool HasNonEmptyItems(this IEnumerable<string> source)
        {
            return source?.Any(a => !a.IsNullOrEmpty()) == true;
        }

        public static bool IntersectsPartiallyWith(this IEnumerable<string> source, IEnumerable<string> target, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (source == null && target == null)
            {
                return false;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            var intersects = false;
            foreach (var sourceItem in source)
            {
                if (target.Any(a => a?.IndexOf(sourceItem, comparison) >= 0))
                {
                    return true;
                }
            }

            return intersects;
        }

        public static bool IntersectsExactlyWith(this IEnumerable<string> source, IEnumerable<string> target, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (source == null && target == null)
            {
                return false;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            var intersects = false;
            foreach (var sourceItem in source)
            {
                if (target.Any(a => a?.Equals(sourceItem, comparison) == true))
                {
                    return true;
                }
            }

            return intersects;
        }

        /// <summary>
        /// Checks if source collection constains specified string completely.
        /// </summary>
        public static bool ContainsString(this IEnumerable<string> source, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return source?.Any(a => a?.Equals(value, comparison) == true) == true;
        }

        /// <summary>
        /// Checks if part of specified string is part of the collection.
        /// </summary>
        public static bool ContainsStringPartial(this IEnumerable<string> source, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return source?.Any(a => a?.IndexOf(value, comparison) >= 0) == true;
        }

        /// <summary>
        /// Checks if source collection constains part of specified string.
        /// </summary>
        public static bool ContainsPartOfString(this IEnumerable<string> source, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return source?.Any(a => value?.IndexOf(a, comparison) >= 0) == true;
        }

        /// <summary>
        /// Checks if two collections contain the same items in any order.
        /// </summary>
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

        /// <summary>
        /// Gets items contained in all colletions.
        /// </summary>
        public static HashSet<T> GetCommonItems<T>(IEnumerable<IEnumerable<T>> lists)
        {
            if (lists?.Any() != true || lists?.First()?.Any() != true)
            {
                return new HashSet<T>();
            }

            var set = new HashSet<T>(lists.First());
            foreach (var list in lists)
            {
                if (list != null)
                {
                    set.IntersectWith(list);
                }
            }

            return set;
        }

        /// <summary>
        /// Gets items distinct to all collections.
        /// </summary>
        public static HashSet<T> GetDistinctItems<T>(IEnumerable<IEnumerable<T>> lists)
        {
            if (lists?.Any() != true)
            {
                return new HashSet<T>();
            }

            var set = new List<T>();
            foreach (var list in lists)
            {
                if (list != null)
                {
                    set.AddRange(list);
                }
            }

            var listsCounts = lists.Count();
            return new HashSet<T>(set.GroupBy(a => a).Where(a => a.Count() < listsCounts).Select(a => a.Key));
        }
    }
}
