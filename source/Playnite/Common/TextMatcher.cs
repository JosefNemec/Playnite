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

        private const int MaxRegexCacheSize = 15;

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

        public double MinimumSimilarity { get; set; } = 0.92;

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

        private string CreateRegexCacheKey(
            string pattern,
            RegexOptions options)
        {
            return pattern + "\u001F" + (int)options;
        }

        public void ClearRegexCaches()
        {
            regexCache.Clear();
            regexCacheOrder.Clear();
            invalidRegexPatterns.Clear();
        }

        public bool IsFuzzyMatch(
            string filter,
            string target)
        {
            if (IsMatch(filter, target))
            {
                return true;
            }

            if (FuzzyMatchAcronymStart &&
                filter.IsStartOfStringAcronym(target))
            {
                return true;
            }

            if (filter
                .GetJaroWinklerSimilarityIgnoreCase(
                    target)
                >= MinimumSimilarity)
            {
                return true;
            }

            if (filter.Length >
                target.Length)
            {
                return false;
            }

            var filterWords =
                filter.Split(
                    WordSeparators,
                    StringSplitOptions.RemoveEmptyEntries);

            var targetWords =
                target.Split(
                    WordSeparators,
                    StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0;
                 i < filterWords.Length;
                 i++)
            {
                var matched = false;

                var filterWord =
                    filterWords[i];

                for (int j = 0;
                     j < targetWords.Length;
                     j++)
                {
                    var targetWord =
                        targetWords[j];

                    if (targetWord
                        .ContainsInvariantCulture(
                            filterWord,
                            CompareOptions.IgnoreCase |
                            CompareOptions.IgnoreSymbols |
                            CompareOptions.IgnoreNonSpace))
                    {
                        matched = true;
                        break;
                    }

                    if (filterWord
                        .GetJaroWinklerSimilarityIgnoreCase(
                            targetWord)
                        >= MinimumSimilarity)
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
