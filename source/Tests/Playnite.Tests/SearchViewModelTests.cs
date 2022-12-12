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
            Assert.IsTrue(SearchViewModel.MatchTextFilter("c", "has chalupa"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op stea", "open steam settings"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op STea", "open settings steam"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("open steam", "open steam settings"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", "open steam settings"));

            Assert.IsFalse(SearchViewModel.MatchTextFilter("opass stea", "open steam settings"));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("test", ""));

            Assert.IsTrue(SearchViewModel.MatchTextFilter("", ""));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, null));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", null));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, ""));

            // Acronym tests
            Assert.IsTrue(SearchViewModel.MatchTextFilter("gow", "Gears of War", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("goW", " Gears of War", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("GOW", "god of war 3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter(" GOW", "god of war 3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("GOW ", "god of war 3", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("GOW", "god   of war   3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("g OW", "god of war 3", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("gOw3", "god of war 3", true));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("gOw3", "god of war 2", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("COD", "Call of Duty®: Modern Warfare® II", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("CODMW", "Call of Duty®: Modern Warfare® II", true));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cod", "Call of Duty ®", true));
        }

        [Test]
        [SetCulture("cs-CZ")]
        public void CzechCultureMatchTextFilterTest()
        {
            // ch is a separate character in Czech so this would fail with wrong culture settings in string comparison
            Assert.IsTrue(SearchViewModel.MatchTextFilter("c", "has chalupa"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cupa", "has chalčupa"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("čupa", "has chalčupa"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("cupa", "has chalČupa"));
        }
    }
}
