using NUnit.Framework;
using Playnite.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class GameFieldComparerTests
    {
        [Test]
        public void CompareTest()
        {
            Assert.IsTrue(GameFieldComparer.StringEquals("Single Player", "Single Player"));
            Assert.IsTrue(GameFieldComparer.StringEquals("Single-Player", "Single Player"));
            Assert.IsTrue(GameFieldComparer.StringEquals("SinglePlayer", "Single Player"));
            Assert.IsTrue(GameFieldComparer.StringEquals("single player", "Single Player"));

            Assert.IsFalse(GameFieldComparer.StringEquals("SingleaPlayer", "Single Player"));
            Assert.IsFalse(GameFieldComparer.StringEquals("Single:Player", "Single Player"));
        }
    }
}
