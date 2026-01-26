using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void JeroWrenklinSimilarityTests()
        {
            Assert.AreEqual(1.0, "hello".GetJaroWinklerSimilarityIgnoreCase("hello"));
            Assert.AreNotEqual(1.0, "hello".GetJaroWinklerSimilarity("Hello"));
            Assert.AreEqual(1.0, "hello".GetJaroWinklerSimilarityIgnoreCase("Hello"));
            Assert.AreEqual(0.0, "kiwi".GetJaroWinklerSimilarity("banana"));
            Assert.AreEqual(1.0, "".GetJaroWinklerSimilarity(""));
            Assert.AreEqual(1.0, "".GetJaroWinklerSimilarityIgnoreCase(""));
        }

        [Test]
        public void LevenshteinDistanceTests()
        {
            Assert.AreEqual(0, "hello".GetLevenshteinDistance("hello"));
            Assert.AreNotEqual(0, "hello".GetLevenshteinDistance("Hello"));
            Assert.AreEqual(1, "hello".GetLevenshteinDistance("Hello"));
            Assert.AreEqual(0, "hello".GetLevenshteinDistanceIgnoreCase("Hello"));
            Assert.AreEqual(1, "car".GetLevenshteinDistance("cars"));
            Assert.AreEqual(2, "a car".GetLevenshteinDistance("car"));
            Assert.AreEqual(2, "car".GetLevenshteinDistance("car a"));
            Assert.AreEqual(0, "".GetLevenshteinDistance("")); // Empty strings means exact match
            Assert.AreEqual("abc".Length, "".GetLevenshteinDistance("abc")); // Empty string and non empty string means distance is length of other string

            Assert.AreEqual(3, "abc".GetLevenshteinDistance("def"));
            Assert.AreEqual(4, "hello".GetLevenshteinDistance("world"));
            Assert.AreEqual(2, "flaw".GetLevenshteinDistance("lawn"));
            Assert.AreEqual(3, "kitten".GetLevenshteinDistance("sitting"));

            var baseString = "my string";
            for (int i = 0; i < 10; i++)
            {
                var toMatchString = baseString + new string('a', i); // Each added character increases the distance by one
                Assert.AreEqual(i, baseString.GetLevenshteinDistance(toMatchString));
            }

            var stringPrefix = "apple";
            var stringsList = new List<string>
            {
                $"{stringPrefix} 123456789",
                $"{stringPrefix} 12345678",
                $"{stringPrefix} 1234567",
                $"{stringPrefix} 123456",
                $"{stringPrefix} 12345",
                $"{stringPrefix} 1234",
                $"{stringPrefix} 12",
                $"{stringPrefix} 1",
                $"{stringPrefix} 123",
            };

            var sortedList = stringsList.OrderBy(str => str.Length);

            // Since Levenshtein calculates number of different characters, it's possible to use the
            // length to verify that the ordering is correct if all use the same base string
            var sortedLevenshteinList = stringsList.OrderBy(str => str.GetLevenshteinDistance(stringPrefix));
            Assert.IsTrue(sortedList.SequenceEqual(sortedLevenshteinList));
        }

        [Test]
        public void ToGameKey_ResultTests()
        {
            // Basic normalization
            Assert.AreEqual("thewitcher3", "Witcher 3, The".ToGameKey());
            Assert.AreEqual("thewitcher3", "The Witcher 3".ToGameKey());

            // Case insensitivity
            Assert.AreEqual("nierautomata", "NieR: Automata".ToGameKey());
            Assert.AreEqual("nierautomata", "NIER: AUTOMATA".ToGameKey());

            // Special characters are removed
            Assert.AreEqual("nierautomata", "NieR: Automata™".ToGameKey());
            Assert.AreEqual("finalfantasyviiremake", "Final Fantasy VII: Remake!?$%$".ToGameKey());

            // Trailing bracketed metadata is removed
            Assert.AreEqual("nierautomata", "NieR: Automata [PC]".ToGameKey());
            Assert.AreEqual("nierautomata", "NieR: Automata (Steam)".ToGameKey());

            // Mix of previous cases
            Assert.AreEqual("thewitcher3", "Witcher 3, The (GOTY Edition)".ToGameKey());
            Assert.AreEqual("thelegendofzeldabreathofthewild", "Legend of Zelda, The: Breath of the Wild".ToGameKey());

            // Empty and null handling
            Assert.AreEqual(string.Empty, "".ToGameKey());
            Assert.AreEqual(string.Empty, ((string)null).ToGameKey());

            // Digits are preserved
            Assert.AreEqual("ff7", "FF7".ToGameKey());
        }

        [Test]
        public void ToGameKey_MatchingEquivalenceTests()
        {
            void AssertMatch(string a, string b)
            {
                Assert.AreEqual(a.ToGameKey(), b.ToGameKey(), $"Expected match: \"{a}\" <-> \"{b}\"");
            }

            // General cases
            AssertMatch("Middle-earth™: Shadow of War™", "Middle-earth: Shadow of War");
            AssertMatch("Command®   & Conquer™ Red_Alert 3™ : Uprising©:_Best Game", "Command & Conquer Red Alert 3: Uprising: Best Game");
            AssertMatch("Pokemon.Red.[US].[l33th4xor].Test.[22]", "Pokemon Red Test");
            AssertMatch("Pokemon.Red.[US].(l33th 4xor).Test.(22)", "Pokemon Red Test");
            AssertMatch("[PROTOTYPE]™", "[PROTOTYPE]");
            AssertMatch("(PROTOTYPE2)™", "(PROTOTYPE2)");

            // Articles
            AssertMatch("Witcher 3, The", "The Witcher 3");
            AssertMatch("Legend of Zelda, The: Breath of the Wild", "The Legend of Zelda: Breath of the Wild");

            // Platform / store metadata
            AssertMatch("NieR: Automata", "NieR: Automata [PC]");
            AssertMatch("NieR: Automata", "NieR: Automata (Steam)");
            AssertMatch("DOOM", "DOOM (2016)");
            AssertMatch("Final Fantasy VII Remake", "Final Fantasy VII Remake [PC]");

            // Special characters & punctuation
            AssertMatch("NieR: Automata™", "NieR - Automata");
            AssertMatch("Dragon's Dogma", "Dragons Dogma");

            // Case differences
            AssertMatch("nier automata", "NieR: Automata");
            AssertMatch("FINAL FANTASY X", "Final Fantasy X");

            // Whitespace & formatting
            AssertMatch("The Witcher 3", "   The   Witcher   3   ");
            AssertMatch("Dark Souls III", "Dark   Souls   III");

            // Edition words should not match unless you want them to
            Assert.AreNotEqual("Persona 5".ToGameKey(), "Persona 5 Royal".ToGameKey());

            // Numbers written differently will not match
            Assert.AreNotEqual("Final Fantasy VII".ToGameKey(), "Final Fantasy 7".ToGameKey());

            // Year metadata
            AssertMatch("Resident Evil 4", "Resident Evil 4 (2005)");

            // Region tags
            AssertMatch("Silent Hill 2", "Silent Hill 2 [USA]");
        }
    }
}