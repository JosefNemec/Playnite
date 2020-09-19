using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents class with various extension methods for IEnumerable lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Converts collection to <see cref="ObservableCollection{T}"/> collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return null;
            }

            return new ObservableCollection<T>(source);
        }

        /// <summary>
        /// Check if collection has any items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasItems<T>(this IEnumerable<T> source)
        {
            return source?.Any() == true;
        }

        /// <summary>
        /// Check if collection has any items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool HasItems<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source?.Any(predicate) == true;
        }

        /// <summary>
        /// Adds new item to the list only if it's not already part of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns>True if item was added, false if it's already part of the list.</returns>
        public static bool AddMissing<T>(this IList<T> source, T item)
        {
            if (!source.Contains(item))
            {
                source.Add(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds new items to the list only if they are not already part of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="items"></param>
        /// <returns>True if an item was added, false if none item was added.</returns>
        public static bool AddMissing<T>(this IList<T> source, IEnumerable<T> items)
        {
            if (!items.HasItems())
            {
                return false;
            }

            var anyAdded = false;
            foreach (var item in items)
            {
                if (AddMissing(source, item))
                {
                    anyAdded = true;
                }
            }

            return anyAdded;
        }

        /// <summary>
        /// Checks if collection has any non-empty string items.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasNonEmptyItems(this IEnumerable<string> source)
        {
            return source?.Any(a => !string.IsNullOrEmpty(a)) == true;
        }

        /// <summary>
        /// Checks if source collection contains any items from target one, even if just partially.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="comparison"></param>
        /// <returns>True of target collection contains items that are also part of source collection.</returns>
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

        /// <summary>
        /// Checks if source collection contains any items from target one.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="comparison"></param>
        /// <returns>True of target collection contains items that are also part of source collection.</returns>
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

            if (source.Count() != target.Count())
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
        /// Checks if two collections contain the same items in any order.
        /// </summary>
        public static bool IsListEqual<T>(this IEnumerable<T> source, IEnumerable<T> target, IEqualityComparer<T> comparer)
        {
            if (source == null && target == null)
            {
                return true;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            if (source.Count() != target.Count())
            {
                return false;
            }

            var firstNotSecond = source.Except(target, comparer).ToList();
            if (firstNotSecond.Count != 0)
            {
                return false;
            }

            var secondNotFirst = target.Except(source, comparer).ToList();
            if (secondNotFirst.Count != 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if two collections contain the same items in the same order.
        /// </summary>
        public static bool IsListEqualExact<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source == null && target == null)
            {
                return true;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            if (source.Count() != target.Count())
            {
                return false;
            }

            return source.SequenceEqual(target);
        }

        /// <summary>
        /// Check if collection contains all items from other collection (in any order).
        /// </summary>
        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source == null && target == null)
            {
                return true;
            }

            if ((source == null && target != null) || (source != null && target == null))
            {
                return false;
            }

            var targetCount = target.Count();
            if (targetCount > source.Count())
            {
                return false;
            }

            return target.Intersect(source).Count() == targetCount;
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
                if (list == null)
                {
                    set.IntersectWith(new List<T>());
                }
                else
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

        /// <summary>
        /// Merge collection together.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static List<T> Merge<T>(IEnumerable<IEnumerable<T>> lists)
        {
            var allItems = new List<T>();
            foreach (var list in lists)
            {
                if (list.HasItems())
                {
                    allItems.AddRange(list);
                }
            }

            return allItems;
        }

        /// <summary>
        /// Merge two collection together.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<T> Merge<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1.HasItems() && list2.HasItems())
            {
                var allItems = new List<T>(list1.Count() + list2.Count());
                allItems.AddRange(list1);
                allItems.AddRange(list2);
            }
            else if (list1.HasItems() && !list2.HasItems())
            {
                return list1.ToList();
            }
            else if (!list1.HasItems() && list2.HasItems())
            {
                return list2.ToList();
            }

            return new List<T>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this ObservableCollection<T> source, Action<T> action)
        {
            if (source.HasItems() != true)
            {
                return;
            }

            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
