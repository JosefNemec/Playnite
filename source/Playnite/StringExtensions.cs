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

        public static string NormalizeGameName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var newName = name;
            newName = newName.Replace("_", " ");
            newName = Regex.Replace(newName, @"[™©®]", string.Empty);
            newName = Regex.Replace(newName, @"\s+", " ");
            newName = Regex.Replace(newName, @"\s*:\s*", ": ");
            newName = Regex.Replace(newName, @"\s*-\s*", ": ");
            if (Regex.IsMatch(newName, @",\s*The$"))
            {
                newName = "The " + Regex.Replace(newName, @",\s*The$", "");
            }

            return newName.Trim();
        }
    }
}
