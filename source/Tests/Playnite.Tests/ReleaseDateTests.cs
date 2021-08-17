using NUnit.Framework;
using Playnite.Common;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class ReleaseDateTests
    {
        [Test]
        public void CompareToTest()
        {
            var date = new ReleaseDate(2000);
            var dateMonth = new ReleaseDate(2001, 2);
            var dateDay = new ReleaseDate(2002, 5, 12);
            Assert.AreEqual(-1, date.CompareTo(dateMonth));
            Assert.AreEqual(0, date.CompareTo(new ReleaseDate(2000)));
            Assert.AreEqual(1, dateDay.CompareTo(date));
        }

        [Test]
        public void EqualsTest()
        {
            var date1 = new ReleaseDate(2000, 1, 1);
            var date2 = new ReleaseDate(2000, 1, 1);
            var date3 = new ReleaseDate(2000, 1, 2);
            Assert.AreEqual(date1, date2);
            Assert.AreNotEqual(date1, date3);
            Assert.IsTrue(date1 == date2);
            Assert.IsTrue(date1 != date3);
        }

        [Test]
        public void SerializationTest()
        {
            Assert.AreEqual("2001-2-3", new ReleaseDate(2001, 2, 3).Serialize());
            Assert.AreEqual("2001-2", new ReleaseDate(2001, 2).Serialize());
            Assert.AreEqual("2001", new ReleaseDate(2001).Serialize());
            Assert.AreEqual(new ReleaseDate(2001, 2, 3), ReleaseDate.Deserialize("2001-2-3"));
            Assert.AreEqual(new ReleaseDate(2001, 2), ReleaseDate.Deserialize("2001-2"));
            Assert.AreEqual(new ReleaseDate(2001), ReleaseDate.Deserialize("2001"));
        }

        [Test]
        public void JsonSerializationTest()
        {
            Assert.AreEqual(
                Serialization.ToJson(new ReleaseDate(2001, 2, 3)),
                "{\"ReleaseDate\":\"2001-2-3\"}");
            Assert.AreEqual(
                new ReleaseDate(2001, 2, 3).GetClone(),
                new ReleaseDate(2001, 2, 3));
        }
    }
}
