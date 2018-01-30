using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class ListExtensions
    {
        public static bool IsNullOrEmpty(IList<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return true;
            }

            var allEmpty = true;
            foreach (var item in source)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    allEmpty = false;
                    break;
                }
            }

            return allEmpty;
        }

        public static bool IntersectsPartiallyWith(this List<string> source, List<string> target)
        {
            var intersects = true;

            foreach (var sourceItem in source)
            {
                if (!target.Any(a => a != null && a.IndexOf(sourceItem, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return false;
                }
            }

            return intersects;
        }
    }
}
