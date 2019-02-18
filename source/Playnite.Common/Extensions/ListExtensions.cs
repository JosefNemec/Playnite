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

        public static bool ContainsString(this IEnumerable<string> source, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return source?.Any(a => a?.Equals(value, comparison) == true) == true;
        }

        public static bool ContainsStringPartial(this IEnumerable<string> source, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return source?.Any(a => value?.IndexOf(a, comparison) >= 0) == true;
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
