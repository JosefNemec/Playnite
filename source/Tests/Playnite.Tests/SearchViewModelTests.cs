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
        public void MatchTextFilterTest()
        {
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op stea", "open steam settings"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("op stea", "open settings steam"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("open steam", "open steam settings"));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", "open steam settings"));

            Assert.IsFalse(SearchViewModel.MatchTextFilter("opass stea", "open steam settings"));
            Assert.IsFalse(SearchViewModel.MatchTextFilter("test", ""));

            Assert.IsTrue(SearchViewModel.MatchTextFilter("", ""));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, null));
            Assert.IsTrue(SearchViewModel.MatchTextFilter("", null));
            Assert.IsTrue(SearchViewModel.MatchTextFilter(null, ""));
        }
    }
}
