using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class TextMatcherTests
    {
        [Test]
        public void IsMatchTest()
        {
            var matcher = new TextMatcher();

            Assert.IsTrue(
                matcher.IsMatch(
                    "persona",
                    "Persona 5"));

            Assert.IsTrue(
                matcher.IsMatch(
                    "sona",
                    "Persona 5"));

            Assert.IsFalse(
                matcher.IsMatch(
                    "final",
                    "Persona 5"));
        }

        [Test]
        public void IsMatchCaseSensitiveTest()
        {
            var matcher = new TextMatcher
            {
                IgnoreCase = false
            };

            Assert.IsFalse(
                matcher.IsMatch(
                    "persona",
                    "Persona"));
        }

        [Test]
        public void IsMatchCaseInsensitiveTest()
        {
            var matcher = new TextMatcher
            {
                IgnoreCase = true
            };

            Assert.IsTrue(
                matcher.IsMatch(
                    "persona",
                    "Persona"));
        }

        [Test]
        public void NormalAcronymMatchTest()
        {
            var matcher = new TextMatcher
            {
                NormalMatchAcronymStart = true
            };

            Assert.IsTrue(
                matcher.IsMatch(
                    "p5",
                    "Persona 5"));

            Assert.IsTrue(
                matcher.IsMatch(
                    "ac",
                    "Assassin's Creed"));

            Assert.IsFalse(
                matcher.IsMatch(
                    "xyz",
                    "Persona 5"));
        }

        [Test]
        public void RegexMatchTest()
        {
            var matcher = new TextMatcher();

            // Whole word matching
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"\bDemo\b",
                    "Demo Game"));

            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"\bDemo\b",
                    "Persona 3 Reloaded Demo"));

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"\bDemo\b",
                    "Demolition"));

            // IgnoreCase enabled by default
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"\bdemo\b",
                    "Demo Game"));

            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"\bDEMO\b",
                    "demo game"));

            // Start anchor
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"^Persona",
                    "Persona 5 Royal"));

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"^Persona",
                    "Super Persona 5 Royal"));

            // End anchor
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"Royal$",
                    "Persona 5 Royal"));

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"Royal$",
                    "Royal Edition Game"));

            // Character classes
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"Persona \d",
                    "Persona 5"));

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"Persona \d",
                    "Persona Royal"));

            // Alternation
            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"Persona|Metaphor",
                    "Metaphor ReFantazio"));

            Assert.IsTrue(
                matcher.IsRegexMatch(
                    @"Persona|Metaphor",
                    "Persona 3 Reload"));

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"Persona|Metaphor",
                    "Final Fantasy"));
        }

        [Test]
        public void InvalidRegexDoesNotThrowTest()
        {
            var matcher = new TextMatcher();
            Assert.DoesNotThrow(() =>
            {
                matcher.IsRegexMatch(
                    @"\bDemo\",
                    "Demo");
            });

            Assert.IsFalse(
                matcher.IsRegexMatch(
                    @"\bDemo\",
                    "Demo"));
        }

        [Test]
        public void InvalidRegexCanBeRepeatedTest()
        {
            var matcher = new TextMatcher();
            for (int i = 0; i < 100; i++)
            {
                Assert.IsFalse(
                    matcher.IsRegexMatch(
                        @"\bDemo\",
                        "Demo"));
            }
        }

        [Test]
        public void FuzzySimilarityMatchTest()
        {
            var matcher = new TextMatcher();
            Assert.IsTrue(
                matcher.IsFuzzyMatch(
                    "persna",
                    "Persona"));
        }

        [Test]
        public void FuzzyWordMatchTest()
        {
            var matcher = new TextMatcher();
            Assert.IsTrue(
                matcher.IsFuzzyMatch(
                    "pers 5",
                    "Persona 5 Royal"));
        }

        [Test]
        public void FuzzyAcronymMatchTest()
        {
            var matcher = new TextMatcher();
            Assert.IsTrue(
                matcher.IsFuzzyMatch(
                    "p5",
                    "Persona 5 Royal"));

            Assert.IsTrue(
                matcher.IsFuzzyMatch(
                    "aco",
                    "Assassin's Creed Odyssey"));
        }

        [Test]
        public void FuzzyNoMatchTest()
        {
            var matcher = new TextMatcher();

            Assert.IsFalse(
                matcher.IsFuzzyMatch(
                    "zelda",
                    "Persona 5"));
        }

        [Test]
        public void NullQueryThrowsTest()
        {
            var matcher = new TextMatcher();

            Assert.Throws<ArgumentNullException>(
                () => matcher.IsMatch(
                    null,
                    "Persona"));
        }

        [Test]
        public void NullTargetThrowsTest()
        {
            var matcher = new TextMatcher();

            Assert.Throws<ArgumentNullException>(
                () => matcher.IsMatch(
                    "Persona",
                    null));
        }

        [Test]
        public void ClearRegexCachesTest()
        {
            var matcher = new TextMatcher();

            matcher.IsRegexMatch(
                @"\bDemo\b",
                "Demo");

            matcher.ClearRegexCaches();

            Assert.DoesNotThrow(() =>
            {
                matcher.IsRegexMatch(
                    @"\bDemo\b",
                    "Demo");
            });
        }
    }
}