using NUnit.Framework;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SearchViewModelTests
    {
        [Test]
        [SetCulture("en-US")]
        public void MatchTextFilterTest()
        {
            Assert.IsTrue(SearchViewModel.MatchTextFilter("c", "has chalupa", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op stea", "open steam settings", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op STea", "open settings steam", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("open steam", "open steam settings", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", "open steam settings", false));

            Assert.IsFalse(SearchViewModel.MatchTextFilter("opass stea", "open steam settings", false));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("test", "", false));

            Assert.IsTrue(SearchViewModel.MatchTextFilter("", "", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, null, false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", null, false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, "", false));

            // Acronym tests
            Assert.IsTrue(SearchViewModel.MatchTextFilter("gow", "Gears of War", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("goW", " Gears of War", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("GOW", "god of war 3", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("GOW", "god   of war   3", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("gOw3", "god of war 3", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("COD", "Call of Duty®: Modern Warfare® II", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("CODMW", "Call of Duty®: Modern Warfare® II", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cod", "Call of Duty ®", true));

            Assert.IsFalse(SearchViewModel.MatchTextFilter("goW", " Gears of War", false));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("GOW", "god of war 3", false));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("GOW3", "god of war 3", false));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("CODMW", "Call of Duty®: Modern Warfare® II", false));
            Assert.IsFalse(SearchViewModel.MatchTextFilter(" GOW", "god of war 3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("GOW ", "god of war 3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("gOw3", "god of war 2", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("g OW", "god of war 3", true));

            // JaroWinklerSimilarity tests
            var filter = "mario pary";
            var minimumSimilarity = 0.90;
            var gameNames = new List<string> { "Mario Party", "Mario Party 1", "Mario Party 2", "Mario Party Advance", "Mario Tennis", "Super Mario 64" };
            foreach (var gameName in gameNames)
            {
                Assert.AreEqual(filter.GetJaroWinklerSimilarityIgnoreCase(gameName) >= minimumSimilarity, SearchViewModel.MatchTextFilter(filter, gameName, false, minimumSimilarity));
            }
        }

        [Test]
        [SetCulture("cs-CZ")]
        public void CzechCultureMatchTextFilterTest()
        {
            // ch is a separate character in Czech so this would fail with wrong culture settings in string comparison
            Assert.IsTrue(SearchViewModel.MatchTextFilter("c", "has chalupa", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cupa", "has chalčupa", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("čupa", "has chalčupa", false));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cupa", "has chalČupa", false));
        }
    }
}
