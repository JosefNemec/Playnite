using Microsoft.Scripting.Actions.Calls;
using NLog.Filters;
using NLog.Targets;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public sealed class TextMatcher
    {
        private const int MaxFuzzyMatchTokenCacheSize = 2000;

        private const int MaxFuzzyMatchSimilarityCacheSize = 5000;

        private readonly Dictionary<string, string[]> tokenCache =
            new Dictionary<string, string[]>();

        private readonly Dictionary<string, double> similarityCache =
            new Dictionary<string, double>();

        private readonly Queue<string> tokenCacheOrder =
            new Queue<string>();

        private readonly Queue<string> similarityCacheOrder =
            new Queue<string>();

        private const double EarlyFailThreshold = .4;

        private static readonly char[] WordSeparators =
        {
            ' ',
            '-',
            '_',
            '.',
            ':',
            '(',
            ')'
        };

        private const int MaxRegexCacheSize = 12;

        public static readonly TimeSpan RegexTimeout =
            TimeSpan.FromMilliseconds(100);

        private readonly Dictionary<string, Regex> regexCache =
            new Dictionary<string, Regex>();

        private readonly Queue<string> regexCacheOrder =
            new Queue<string>();

        /// <summary>
        /// Stores regex patterns that previously failed compilation to avoid
        /// repeatedly throwing exceptions during live search input.
        /// </summary>
        private readonly HashSet<string> invalidRegexPatterns =
            new HashSet<string>();

        public bool NormalMatchAcronymStart { get; set; } = false;
        public bool FuzzyMatchAcronymStart { get; set; } = true;
       
        public bool IgnoreCase { get; set; } = true;
        public double MinimumFuzzyScore { get; set; } = 0.70;

        public bool IsMatch(
            string query,
            string candidate)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (candidate is null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            if (NormalMatchAcronymStart &&
                query.IsStartOfStringAcronym(candidate))
            {
                return true;
            }

            return candidate.IndexOf(
                query,
                IgnoreCase
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal)
                >= 0;
        }

        public bool IsRegexMatch(
            string pattern,
            string target)
        {
            if (pattern.IsNullOrEmpty() ||
                target.IsNullOrEmpty())
            {
                return false;
            }

            var regex = GetCompiledRegex(pattern);
            if (regex is null)
            {
                return false;
            }

            return regex.IsMatch(target);
        }

        private Regex GetCompiledRegex(
            string pattern)
        {
            var options = IgnoreCase ?
                RegexOptions.IgnoreCase :
                RegexOptions.Compiled;

            var cacheKey = CreateRegexCacheKey(
                pattern,
                options);

            if (invalidRegexPatterns.Contains(cacheKey))
            {
                return null;
            }

            if (regexCache.TryGetValue(
                cacheKey, out var regex))
            {
                return regex;
            }

            try
            {
                regex = new Regex(
                    pattern,
                    options,
                    RegexTimeout);

                regexCache[cacheKey] = regex;
                regexCacheOrder.Enqueue(cacheKey);

                while (regexCacheOrder.Count > MaxRegexCacheSize)
                {
                    var oldest = regexCacheOrder.Dequeue();
                    regexCache.Remove(oldest);
                }

                return regex;
            }
            catch (ArgumentException)
            {
                invalidRegexPatterns.Add(cacheKey);
                return null;
            }
        }

        private static string CreateRegexCacheKey(
            string pattern,
            RegexOptions options)
        {
            return string.Concat(pattern, "\u001F", (int)options);
        }

        public void ClearRegexCaches()
        {
            regexCache.Clear();
            regexCacheOrder.Clear();
            invalidRegexPatterns.Clear();
        }

        public bool IsFuzzyMatch(
            string query,
            string target)
        {
            var fuzzyScore = GetFuzzyScore(
                query,
                target);
            var minimumFuzzyScore = GetMinimumFuzzyScore(query);
            return fuzzyScore >= minimumFuzzyScore;
        }

        public double GetFuzzyScore(
            string query,
            string target)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (query.Length == 0 ||
                target.Length == 0)
            {
                return 0;
            }

            // To prevent unecessarily allocating new strings if there
            // is no whitespace at the start or end of the query/target.
            if (char.IsWhiteSpace(query[0]) ||
                char.IsWhiteSpace(query[query.Length - 1]))
            {
                query = query.Trim();
            }

            if (char.IsWhiteSpace(target[0]) ||
                char.IsWhiteSpace(target[target.Length - 1]))
            {
                target = target.Trim();
            }

            if (query.Length == 0 ||
                target.Length == 0)
            {
                return 0;
            }

            var comparison = IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            // Exact
            if (string.Equals(
                query,
                target,
                comparison))
            {
                return 1.0;
            }

            // p5 -> Persona 5
            // ac -> Assassin's Creed
            // GOW -> Gears of War (Not God of War, the inferior franchise)
            if (FuzzyMatchAcronymStart &&
                query.IsStartOfStringAcronym(target))
            {
                return .95;
            }

            // Tiny searches can produce very bad similarity scores due to the way Jaro-Winkler works,
            // so we can short circuit some of the more expensive checks for very short queries.
            // using a specially tuned scoring method for short queries.
            if (query.Length <= 2)
            {
                return ScoreShortQuery(
                    query,
                    target,
                    comparison);
            }

            if (target.IndexOf(query,comparison) >= 0)
            {
                return .95;
            }

            if (target.StartsWith(query, comparison))
            {
                return .90;
            }

            // Expensive path
            var queryWords = GetTokens(query);
            if (queryWords.Length == 0)
            {
                return 0;
            }

            var targetWords = GetTokens(target);
            return ScoreWords(
                queryWords,
                targetWords);
        }

        private double ScoreShortQuery(
            string query,
            string target,
            StringComparison comparison)
        {
            var words = GetTokens(target);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].StartsWith(query, comparison))
                {
                    return .90;
                }
            }

            return 0;
        }

        private double ScoreWords(
            string[] queryWords,
            string[] targetWords)
        {
            double totalScore = 0;

            for (int i = 0; i < queryWords.Length; i++)
            {
                var queryWord = queryWords[i];

                double bestScore = 0;

                for (int j = 0; j < targetWords.Length; j++)
                {
                    var targetWord = targetWords[j];

                    var similarity = GetSimilarity(
                        queryWord,
                        targetWord);

                    if (targetWord.StartsWith(
                        queryWord,
                        IgnoreCase
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal))
                    {
                        similarity += .15;
                    }

                    if (similarity > bestScore)
                    {
                        bestScore = similarity;

                        // Already a very good match, no need to check other words
                        if (bestScore >= .95)
                        {
                            break;
                        }
                    }
                }

                totalScore += bestScore;

                // Fail early
                var average =
                    totalScore /
                    (i + 1);

                if (average <
                    EarlyFailThreshold)
                {
                    return average;
                }
            }

            return totalScore / queryWords.Length;
        }

        private double GetSimilarity(
            string left,
            string right)
        {
            var key = string.Concat(left, "\u001F", right);
            if (similarityCache.TryGetValue(key,
                out var similarity))
            {
                return similarity;
            }

            similarity = left.GetJaroWinklerSimilarityIgnoreCase(right);
            similarityCache[key] = similarity;

            similarityCacheOrder.Enqueue(key);
            while (similarityCacheOrder.Count > MaxFuzzyMatchSimilarityCacheSize)
            {
                var oldest = similarityCacheOrder.Dequeue();
                similarityCache.Remove(oldest);
            }

            return similarity;
        }

        private string[] GetTokens(string text)
        {
            if (tokenCache.TryGetValue(text, out  var tokens))
            {
                return tokens;
            }

            tokens = Tokenize(text);
            tokenCache[text] = tokens;

            tokenCacheOrder.Enqueue(text);
            while (tokenCacheOrder.Count > MaxFuzzyMatchTokenCacheSize)
            {
                var oldest = tokenCacheOrder.Dequeue();
                tokenCache.Remove(oldest);
            }

            return tokens;
        }

        private static string[] Tokenize(string text)
        {
            var split =
                text.Split(
                    WordSeparators,
                    StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>(split.Length);
            for (int i = 0; i < split.Length; i++)
            {
                if (IsValidToken(split[i]))
                {
                    result.Add(split[i]);
                }
            }

            return result.ToArray();
        }

        private static bool IsValidToken(
            string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            int alphaNumericCount = 0;
            for (int i = 0;
                 i < value.Length;
                 i++)
            {
                if (char.IsLetterOrDigit(
                    value[i]))
                {
                    alphaNumericCount++;

                    if (alphaNumericCount >= 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private double GetMinimumFuzzyScore(string query)
        {
            if (query.Length <= 1)
            {
                return .98;
            }

            if (query.Length == 2)
            {
                return .92;
            }

            if (query.Length == 3)
            {
                return .80;
            }

            return MinimumFuzzyScore;
        }

    }
}
