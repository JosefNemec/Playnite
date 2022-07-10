using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Playnite
{
    public class SortableNameConverter
    {
        private readonly Regex regex;
        private readonly Regex ignoredEndWordsRegex;

        /// <summary>
        /// The minimum string length of numbers. If 4, XXIII or 23 will turn into 0023.
        /// </summary>
        private static int numberLength = 2;

        private static string[] excludedRomanNumerals = new[] { "XL", "XD", "DX", "XXX", "L", "C", "D", "M", "MII", "MIX", "MX", "MC", "DC" };

        //Haven't observed game titles with zero, or four and above that would benefit from making those words sortable numbers
        private static Dictionary<string, int> numberWordValues = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase) { { "one", 1 }, { "two", 2 }, { "three", 3 } };

        private static Dictionary<char, int> romanNumeralValues = new Dictionary<char, int>
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
        /// 
        /// </summary>
        /// <param name="articles">Words to remove from the start of the title. Suggested: the contents of PlayniteSettings.GameSortingNameRemovedArticles, or "The", "A", "An".</param>
        /// <param name="batchOperation">Optimize for larger amounts of throughput. Slower for small amounts.</param>
        public SortableNameConverter(IEnumerable<string> articles, bool batchOperation = false)
        {
            if (articles == null)
            {
                throw new ArgumentNullException(nameof(articles));
            }

            //(?<![\w.]|^) prevents the numerical matches from happening at the start of the string (for example for X-COM or XIII) or attached to a word or . (to avoid S.T.A.L.K.E.R. -> S.T.A.50.K.E.R.)
            //(?!\.) prevents matching roman numerals with a period right after (again for cases like abbreviations with periods, but that start with a roman numeral character)
            //\u2160-\u2188 is the unicode range of roman numerals listed in RomanNumeralValues
            //using [0-9] here instead of \d because \d also matches ٠١٢٣٤٥٦٧٨٩ and I don't know what to do with those           
            //the (?i) is a modifier that makes the rest of the regex (to the right of it) case insensitive
            //see https://www.regular-expressions.info/modifiers.html

            var articlesPattern = string.Join("|", articles.Select(Regex.Escape));
            var articlesGroup = string.IsNullOrEmpty(articlesPattern) ? string.Empty : $@"^(?<article>{articlesPattern})\s+|";
            var regexStr = $@"(?<![\w.]|^)((?<roman>[IVXLCDM\u2160-\u2188]+(?!\.))|(?<arabic>[0-9]+))(?=\W|$)|(?i){articlesGroup}\b(?<numberword>{string.Join("|", numberWordValues.Keys)})\b";
            var options = RegexOptions.ExplicitCapture;
            if (batchOperation)
            {
                options |= RegexOptions.Compiled;
            }

            regex = new Regex(regexStr, options);
            ignoredEndWordsRegex = new Regex(@"(\s*[:-])?(\s+([a-z']+\s+(edition|cut)|hd|collection|remaster(ed)?|remake|ultimate|anthology|game of the))+$", options | RegexOptions.IgnoreCase);
        }

        public string Convert(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            input = StripEdition(input, out string edition);

            string output = regex.Replace(input, match =>
            {
                if (match.Groups["roman"].Success)
                {
                    if (match.Value == "I")
                    {
                        if (MatchComesAfterChapterOrEpisodeOrAtEndOfString(input, match))
                        {
                            return "1".PadLeft(numberLength, '0');
                        }
                        else
                        {
                            return match.Value;
                        }
                    }
                    else if (match.Value == "X")
                    {
                        if (MatchComesAfterChapterOrEpisodeOrAtEndOfString(input, match, maxDistanceFromEnd: 4) && !MatchComesBeforeDashAndWord(input, match))
                        {
                            return "10".PadLeft(numberLength, '0');
                        }
                        else
                        {
                            return match.Value;
                        }
                    }
                    else if (excludedRomanNumerals.Contains(match.Value))
                    {
                        return match.Value;
                    }
                    return ConvertRomanNumeralToInt(match.Value)?.ToString(new string('0', numberLength)) ?? match.Value;
                }
                else if (match.Groups["arabic"].Success)
                {
                    return match.Value.PadLeft(numberLength, '0');
                }
                else if (match.Groups["article"].Success)
                {
                    return string.Empty;
                }
                else if (match.Groups["numberword"].Success)
                {
                    if (MatchComesAfterChapterOrEpisodeOrAtEndOfString(input, match))
                    {
                        return numberWordValues[match.Value].ToString(new string('0', numberLength));
                    }
                    else
                    {
                        return match.Value;
                    }
                }
                return match.Value;
            });

            return output + edition;
        }

        /// <summary>
        /// Convert a number from Roman numerals to an integer
        /// </summary>
        /// <param name="input">The roman numeral.</param>
        /// <param name="validate">If false, parse any roman numeral. If true, reject invalid ones.</param>
        /// <returns>An integer form of the supplied roman numeral, or NULL if the supplied roman numeral is invalid and <paramref name="validate"/> is true.</returns>
        /// <exception cref="KeyNotFoundException">When the input contains non-numeral characters.</exception>
        public static int? ConvertRomanNumeralToInt(string input, bool validate = true)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            int output = 0;
            int biggestNumberToTheRight = 0;

            int prevCharGroupLength = 0;
            int lastNumericValue = 0;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                char c = input[i];
                if (!romanNumeralValues.TryGetValue(c, out int value))
                {
                    return null;
                }

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

                //reject things like VX or LC or DM
                //subtractions can't be half the bigger value
                //IV is as close as the two numbers get in value
                if (subtract && value * 5 > biggestNumberToTheRight)
                {
                    return null;
                }

                if (value == lastNumericValue)
                {
                    //Numerals that aren't 1 or 10ⁿ can't repeat
                    if (!IsOneOrPowerOfTen(value))
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

        private string StripEdition(string input, out string edition)
        {
            var match = ignoredEndWordsRegex.Match(input);
            if (match.Success)
            {
                edition = match.Value;
                return input.Remove(match.Index);
            }
            else
            {
                edition = string.Empty;
                return input;
            }
        }

        private static bool MatchComesAfterChapterOrEpisodeOrAtEndOfString(string input, Match match, int maxDistanceFromEnd = 0)
        {
            bool matchIsAtEndOfString = MatchIsNearEndOfString(input, match, maxDistanceFromEnd);
            string theBitImmediatelyPriorToTheMatch = input.Substring(Math.Max(0, match.Index - 9), length: Math.Min(9, match.Index));
            return matchIsAtEndOfString
                || theBitImmediatelyPriorToTheMatch.Contains("chapter", StringComparison.InvariantCultureIgnoreCase)
                || theBitImmediatelyPriorToTheMatch.Contains("season", StringComparison.InvariantCultureIgnoreCase)
                || theBitImmediatelyPriorToTheMatch.Contains("episode", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool MatchIsNearEndOfString(string input, Match match, int maxDistanceFromEnd)
        {
            int distance = input.Length - (match.Index + match.Length);
            return distance <= maxDistanceFromEnd;
        }

        private static bool MatchComesBeforeDashAndWord(string input, Match match)
        {
            if (MatchIsNearEndOfString(input, match, maxDistanceFromEnd: 1))
            {
                return false;
            }

            char nextChar = input[match.Index + match.Length];
            if (nextChar != '-')
            {
                return false;
            }

            string nextWord = "";

            for (int i = match.Index + match.Length + 1; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsWhiteSpace(c))
                {
                    break;
                }

                if (!char.IsLetter(c))
                {
                    return false;
                }

                nextWord += c;
            }

            return excludedRomanNumerals.Contains(nextWord) || ConvertRomanNumeralToInt(nextWord) == null;
        }

        private static bool IsOneOrPowerOfTen(int x)
        {
            while (x > 9 && x % 10 == 0)
            {
                x /= 10;
            }

            return x == 1;
        }
    }
}
