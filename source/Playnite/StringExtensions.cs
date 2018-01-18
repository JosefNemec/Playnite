using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite
{
    public static class StringExtensions
    {
        public static string MD5(this string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                var builder = new StringBuilder();

                foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                {
                    builder.Append(b.ToString("x2").ToLower());
                }

                return builder.ToString();
            }
        }

        public static string ConvertToSortableName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var newName = name;
            newName = Regex.Replace(newName, @"^the\s+", "", RegexOptions.IgnoreCase);
            newName = Regex.Replace(newName, @"^a\s+", "", RegexOptions.IgnoreCase);
            newName = Regex.Replace(newName, @"^an\s+", "", RegexOptions.IgnoreCase);
            return newName;
        }

        public static string RemoveTrademarks(string str)
        {
            return Regex.Replace(str, @"[™©®]", string.Empty);
        }

        public static string NormalizeGameName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var newName = name;
            newName = newName.Replace("_", " ");
            newName = newName.Replace(".", " ");
            newName = RemoveTrademarks(newName);
            newName = Regex.Replace(newName, @"\[.*?\]", "");
            newName = Regex.Replace(newName, @"\(.*?\)", "");
            newName = Regex.Replace(newName, @"\s*:\s*", ": ");
            newName = Regex.Replace(newName, @"\s*-\s*", ": ");
            newName = Regex.Replace(newName, @"\s+", " ");
            if (Regex.IsMatch(newName, @",\s*The$"))
            {
                newName = "The " + Regex.Replace(newName, @",\s*The$", "", RegexOptions.IgnoreCase);
            }

            return newName.Trim();
        }
    }
}
