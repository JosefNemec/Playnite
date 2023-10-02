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
    }
}