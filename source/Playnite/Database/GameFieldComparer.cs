using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GameFieldComparer : IEqualityComparer<string>
    {
        private static readonly Regex regex = new Regex(@"[\s-]", RegexOptions.Compiled);

        public bool Equals(string x, string y)
        {
            return StringEquals(x, y);
        }

        public static bool StringEquals(string x, string y)
        {
            return string.Equals(
                regex.Replace(x, ""),
                regex.Replace(y, ""),
                StringComparison.OrdinalIgnoreCase);
        }

        public static bool FieldEquals<T>(T x, string y) where T : DatabaseObject
        {
            return StringEquals(x.Name, y);
        }

        public static bool FieldEquals<T>(T x, T y) where T : DatabaseObject
        {
            return StringEquals(x.Name, y.Name);
        }

        public int GetHashCode(string obj)
        {
            return regex.Replace(obj, "").ToLower().GetHashCode();
        }
    }
}
