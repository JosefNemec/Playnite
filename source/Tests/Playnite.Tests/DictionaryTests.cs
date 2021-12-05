using NUnit.Framework;
using Playnite;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class DictionaryTests
    {
        [Test]
        public void AddOrUpdateTest()
        {
            var dict = new Dictionary<int, int>
            {
                { 1, 0 }
            };

            dict.AddOrUpdate(1, 2);
            Assert.AreEqual(1, dict.Keys.Count);
            Assert.AreEqual(2, dict[1]);

            dict.AddOrUpdate(2, 3);
            Assert.AreEqual(2, dict.Keys.Count);
            Assert.AreEqual(3, dict[2]);
        }
    }
}
