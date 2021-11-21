using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        public static string ConvertToSortableName(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var converter = new SortableNameConverter(new[] { "The", "A", "An" });
            return converter.Convert(name);
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
            newName = RemoveTrademarks(newName);
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
            return str.IndexOf(value, 0, comparisonType) != -1;
        }

        public static bool ContainsAny(this string str, char[] chars)
        {
            return str.IndexOfAny(chars) >= 0;
        }

        public static bool IsHttpUrl(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, @"^https?:\/\/", RegexOptions.IgnoreCase);
        }

        public static bool IsUri(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }

        public static string UrlEncode(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return HttpUtility.UrlPathEncode(str);
        }

        public static string UrlDecode(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return HttpUtility.UrlDecode(str);
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
    }

    public class SortableNameConverter
    {
        public IEnumerable<string> Articles { get; }

        private Regex _regex;

        /// <summary>
        /// The minimum string length of numbers. If 4, XXIII or 23 will turn into 0023.
        /// </summary>
        private static int NumberLength = 2;

        private static string[] ExcludedRomanNumerals = new[] { "XL", "XD", "XXX", "D", "DM", "MII", "MIX", "MX", "MC" };

        public SortableNameConverter(IEnumerable<string> articles, bool batchOperation = false)
        {
            Articles = articles ?? throw new ArgumentNullException(nameof(articles));
            string articlesPattern = string.Join("|", articles.Select(Regex.Escape));
            var options = RegexOptions.ExplicitCapture;
            if (batchOperation)
                options |= RegexOptions.Compiled;

            //(?<![\w.]|^) prevents the numerical matches from happening at the start of the string (for example for X-COM or XIII) or attached to a word or . (to avoid S.T.A.L.K.E.R. -> S.T.A.50.K.E.R.)
            //(?!\.) prevents matching roman numerals with a period right after (again for cases like abbreviations with periods, but that start with a roman numeral character)
            //\u2160-\u2188 is the unicode range of roman numerals listed in RomanNumeralValues
            //using [0-9] here instead of \d because \d also matches ٠١٢٣٤٥٦٧٨٩ and I don't know what to do with those           
            //the (?i) is a modifier that makes the rest of the regex (to the right of it) case insensitive
            //see https://www.regular-expressions.info/modifiers.html
            _regex = new Regex($@"(?<![\w.]|^)((?<roman>[IVXLCDM\u2160-\u2188]+(?!\.))|(?<arabic>[0-9]+))(?=\W|$)|(?i)^(?<article>{articlesPattern})\s+", options);
        }

        public string Convert(string input)
        {
            return _regex.Replace(input, match =>
            {
                if (match.Groups["roman"].Success)
                {
                    if (match.Value == "I")
                    {
                        bool matchIsAtEndOfString = match.Index + 1 == input.Length;
                        bool matchComesAfterChapter = input.Substring(Math.Max(0, match.Index - 9), Math.Min(9, match.Index))
                                                           .Contains("chapter", StringComparison.InvariantCultureIgnoreCase);
                        if (matchIsAtEndOfString || matchComesAfterChapter)
                        {
                            return "1".PadLeft(NumberLength, '0');
                        }
                        else //if the I isn't at the end of the string, ignore it
                        {
                            return match.Value;
                        }
                    }
                    else if (ExcludedRomanNumerals.Contains(match.Value))
                    {
                        return match.Value;
                    }
                    return ConvertRomanNumeralToInt(match.Value)?.ToString(new string('0', NumberLength)) ?? match.Value;
                }
                else if (match.Groups["arabic"].Success)
                {
                    return match.Value.PadLeft(NumberLength, '0');
                }
                else if (match.Groups["article"].Success)
                {
                    return string.Empty;
                }
                return match.Value;
            });
        }

        private static Dictionary<char, int> RomanNumeralValues = new Dictionary<char, int>
        {
            { 'I', 1 }, { 'V', 5 }, { 'X', 10 }, { 'L', 50 }, { 'C', 100 }, { 'D', 500 }, { 'M', 1000 },
            //unicode uppercase
            {'Ⅰ', 1}, {'Ⅱ', 2}, {'Ⅲ', 3}, {'Ⅳ', 4}, {'Ⅴ', 5}, {'Ⅵ', 6}, {'Ⅶ', 7}, {'Ⅷ', 8}, {'Ⅸ', 9}, {'Ⅹ', 10}, {'Ⅺ', 11}, {'Ⅻ', 12}, {'Ⅼ', 50}, {'Ⅽ', 100}, {'Ⅾ', 500}, {'Ⅿ', 1000},
            //unicode lowercase
            {'ⅰ', 1}, {'ⅱ', 2}, {'ⅲ', 3}, {'ⅳ', 4}, {'ⅴ', 5}, {'ⅵ', 6}, {'ⅶ', 7}, {'ⅷ', 8}, {'ⅸ', 9}, {'ⅹ', 10}, {'ⅺ', 11}, {'ⅻ', 12}, {'ⅼ', 50}, {'ⅽ', 100}, {'ⅾ', 500}, {'ⅿ', 1000},
            //unicode big/exotic numbers
            {'ↀ', 1000}, {'ↁ', 5000}, {'ↂ', 10000}, {'Ↄ', 100}, {'ↄ', 100}, {'ↅ', 6}, {'ↆ', 50 }, {'ↇ', 50000}, {'ↈ', 100000 }
        };

        /// <summary>
        /// Convert a number from Roman numerals to an integer
        /// </summary>
        /// <param name="input">The roman numeral(s). Beware: this is not validated. Stuff like IVX will return nonsense numbers.</param>
        /// <param name="validate">If false, parse any roman numerals. If true, reject invalid ones</param>
        /// <returns>An integer form of the supplied roman numeral, or NULL if the supplied roman numeral is invalid and <paramref name="validate"/> is true</returns>
        //TODO: figure out if a number IS a roman numeral or if roman numerals are its components
        public static int? ConvertRomanNumeralToInt(string input, bool validate = true)
        {
            int output = 0;
            int biggestNumberToTheRight = 0;

            int prevCharGroupLength = 0;
            int lastNumericValue = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                char c = input[i];
                int value = RomanNumeralValues[c];
                bool subtract = value < biggestNumberToTheRight;
                if (subtract)
                {
                    output -= value;
                }
                else
                {
                    output += value;
                    biggestNumberToTheRight = value;
                }

                #region validation

                if (!validate)
                {
                    continue;
                }

                //reject things like IVX and VIX and IIX
                //subtractive numerals are only ever singular
                if (subtract && lastNumericValue < biggestNumberToTheRight)
                {
                    return null;
                }

                //reject things like VX or DM
                if (subtract && value * 5 > biggestNumberToTheRight)
                {
                    return null;
                }

                if (value == lastNumericValue)
                {
                    //Numerals that aren't 1 or 10ⁿ can't repeat
                    if (!IsPowerOf10Or1(value))
                    {
                        return null;
                    }

                    //No numeral can repeat 4 times
                    prevCharGroupLength++;
                    if (prevCharGroupLength == 4)
                    {
                        return null;
                    }
                }
                else
                {
                    prevCharGroupLength = 1;
                }

                lastNumericValue = value;

                #endregion validation
            }
            return output;
        }

        private static bool IsPowerOf10Or1(int x)
        {
            while (x > 9 && x % 10 == 0)
            {
                x /= 10;
            }
            return x == 1;
        }
    }
}
