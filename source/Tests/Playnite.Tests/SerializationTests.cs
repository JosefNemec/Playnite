using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        public class TestClass
        {
            [Newtonsoft.Json.JsonIgnore]
            public int Test1 { get; set;}

            [Playnite.SDK.Data.JsonDontSerialize]
            public int Test2 { get; set; }

            public int Test3 { get; set; }

            public int Test4;

            public int Test5 { get; set; }

            public int Test6 { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            public int Test7;

            public bool ShouldSerializeTest5()
            {
                return true;
            }

            public bool ShouldSerializeTest6()
            {
                return false;
            }
        }

        [Test]
        public void JsonDontSerializeAttributeTest()
        {
            var obj = new TestClass
            {
                Test1 = 1,
                Test2 = 2,
                Test3 = 3,
                Test4 = 4,
                Test5 = 5,
                Test6 = 6,
                Test7 = 7
            };

            var str = Serialization.ToJson(obj);
            Assert.IsFalse(str.Contains("Test1"));
            Assert.IsFalse(str.Contains("Test2"));
            Assert.IsTrue(str.Contains("Test3"));
            Assert.IsTrue(str.Contains("Test4"));
            Assert.IsTrue(str.Contains("Test5"));
            Assert.IsFalse(str.Contains("Test6"));
            Assert.IsFalse(str.Contains("Test7"));
        }
    }
}
