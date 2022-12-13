using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace System
{
    public static class StringExtensions
    {
        private static readonly CultureInfo enUSCultInfo = new CultureInfo("en-US", false);

        public static string MD5(this string s)
        {
            var builder = new StringBuilder();
            foreach (byte b in MD5Bytes(s))
            {
                builder.Append(b.ToString("x2").ToLower());
            }

            return builder.ToString();
        }

        public static byte[] MD5Bytes(this string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                return provider.ComputeHash(Encoding.UTF8.GetBytes(s));
            }
        }

        public static string RemoveTrademarks(this string str, string remplacement = "")
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            return Regex.Replace(str, @"[™©®]", remplacement);
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static string Format(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        public static string TrimEndString(this string source, string value, StringComparison comp = StringComparison.Ordinal)
        {
            if (!source.EndsWith(value, comp))
            {
                return source;
            }

            return source.Remove(source.LastIndexOf(value, comp));
        }

        public static string ToTileCase(this string source, CultureInfo culture = null)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (culture != null)
            {
                return culture.TextInfo.ToTitleCase(source);
            }
            else
            {
                return enUSCultInfo.TextInfo.ToTitleCase(source);
            }
        }

        public static bool IsStartOfStringAcronym(this string acronymStart, string input)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(acronymStart)
                || acronymStart.Length < 2 || acronymStart.Length > input.Length)
            {
                return false;
            }

            for (int i = 0; i < acronymStart.Length; i++)
            {
                if (!char.IsLetterOrDigit(acronymStart[i]))
                {
                    return false;
                }
            }

            var acronymIndex = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsLetterOrDigit(input[i]) && (i == 0 || input[i - 1] == ' '))
                {
                    if (char.ToUpperInvariant(input[i]) != char.ToUpperInvariant(acronymStart[acronymIndex]))
                    {
                        return false;
                    }
                    else
                    {
                        acronymIndex++;
                        // If the acronym index and acronym start length is the same
                        // it means all the characters have been matched
                        if (acronymIndex == acronymStart.Length)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static string RemoveUnlessThatEmptiesTheString(string input, string pattern)
        {
            string output = Regex.Replace(input, pattern, string.Empty);

            if (string.IsNullOrWhiteSpace(output))
            {
                return input;
            }
            return output;
        }

        public static string NormalizeGameName(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var newName = name;
            newName = newName.RemoveTrademarks();
            newName = newName.Replace("_", " ");
            newName = newName.Replace(".", " ");
            newName = newName.Replace('’', '\'');
            newName = RemoveUnlessThatEmptiesTheString(newName, @"\[.*?\]");
            newName = RemoveUnlessThatEmptiesTheString(newName, @"\(.*?\)");
            newName = Regex.Replace(newName, @"\s*:\s*", ": ");
            newName = Regex.Replace(newName, @"\s+", " ");
            if (Regex.IsMatch(newName, @",\s*The$"))
            {
                newName = "The " + Regex.Replace(newName, @",\s*The$", "", RegexOptions.IgnoreCase);
            }

            return newName.Trim();
        }

        public static string GetSHA256Hash(this string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static string GetPathWithoutAllExtensions(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return Regex.Replace(path, @"(\.[A-Za-z0-9]+)+$", "");
        }

        public static bool Contains(this string str, string value, StringComparison comparisonType)
        {
            return str?.IndexOf(value, 0, comparisonType) != -1;
        }

        public static bool ContainsAny(this string str, char[] chars)
        {
            return str?.IndexOfAny(chars) >= 0;
        }

        public static bool IsHttpUrl(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            return Regex.IsMatch(str, @"^https?:\/\/", RegexOptions.IgnoreCase);
        }

        public static bool IsUri(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }

        public static string UrlEncode(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return HttpUtility.UrlPathEncode(str);
        }

        public static string UrlDecode(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return HttpUtility.UrlDecode(str);
        }

        public static int GetLineCount(this string str)
        {
            if (str == null)
            {
                return 0;
            }
            else
            {
                return Regex.Matches(str, "\n").Count + 1;
            }
        }

        // Courtesy of https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
        public static string Replace(this string str, string oldValue, string @newValue, StringComparison comparisonType)
        {
            // Check inputs.
            if (str == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(str));
            }
            if (str.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                return str;
            }
            if (oldValue == null)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentNullException(nameof(oldValue));
            }
            if (oldValue.Length == 0)
            {
                // Same as original .NET C# string.Replace behavior.
                throw new ArgumentException("String cannot be of zero length.");
            }

            // Prepare string builder for storing the processed string.
            // Note: StringBuilder has a better performance than String by 30-40%.
            StringBuilder resultStringBuilder = new StringBuilder(str.Length);

            // Analyze the replacement: replace or remove.
            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);

            // Replace all values.
            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
            {
                // Append all characters until the found replacement.
                int @charsUntilReplacment = foundAt - startSearchFromIndex;
                bool isNothingToAppend = @charsUntilReplacment == 0;
                if (!isNothingToAppend)
                {
                    resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilReplacment);
                }

                // Process the replacement.
                if (!isReplacementNullOrEmpty)
                {
                    resultStringBuilder.Append(@newValue);
                }

                // Prepare start index for the next search.
                // This needed to prevent infinite loop, otherwise method always start search
                // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
                // and comparisonType == "any ignore case" will conquer to replacing:
                // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length)
                {
                    // It is end of the input string: no more space for the next search.
                    // The input string ends with a value that has already been replaced.
                    // Therefore, the string builder with the result is complete and no further action is required.
                    return resultStringBuilder.ToString();
                }
            }

            // Append the last part to the result.
            int @charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, @charsUntilStringEnd);
            return resultStringBuilder.ToString();
        }

        public static string EndWithDirSeparator(this string source)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            return source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static bool ContainsInvariantCulture(this string source, string value, CompareOptions compareOptions)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, value, compareOptions) >= 0;
        }

        public static bool ContainsCurrentCulture(this string source, string value, CompareOptions compareOptions)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(source, value, compareOptions) >= 0;
        }
    }
}
