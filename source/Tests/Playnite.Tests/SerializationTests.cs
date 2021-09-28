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
        public class JsonIgnoreTestClass
        {
            [Newtonsoft.Json.JsonIgnore]
            public int Test1 { get; set;}

            [Playnite.SDK.Data.DontSerialize]
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

        public class PropertyNameTestClass
        {
            [Newtonsoft.Json.JsonProperty("test-1")]
            public int Test1 { get; set; }

            [Playnite.SDK.Data.SerializationPropertyName("test-2")]
            public int Test2 { get; set; }

            public int Test3 { get; set; }
        }

        [Test]
        public void JsonDontSerializeAttributeTest()
        {
            var obj = new JsonIgnoreTestClass
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

            var str2 = @"{""Test1"":1,""Test2"":2,""Test3"":3,""Test4"":4,""Test5"":5}";
            var obj2 = Serialization.FromJson<JsonIgnoreTestClass>(str2);
            Assert.AreEqual(0, obj2.Test1);
            Assert.AreEqual(0, obj2.Test2);
            Assert.AreEqual(3, obj2.Test3);
            Assert.AreEqual(4, obj2.Test4);
            Assert.AreEqual(5, obj2.Test5);
        }

        [Test]
        public void JsonPropertyNameTest()
        {
            var obj = new PropertyNameTestClass
            {
                Test1 = 1,
                Test2 = 2,
                Test3 = 3
            };

            var str = Serialization.ToJson(obj);
            Assert.IsTrue(str.Contains("test-1"));
            Assert.IsTrue(str.Contains("test-2"));
            Assert.IsTrue(str.Contains("Test3"));

            var str2 = @"{""test-1"":1,""test-2"":2,""Test3"":3}";
            var obj2 = Serialization.FromJson<PropertyNameTestClass>(str2);
            Assert.AreEqual(1, obj2.Test1);
            Assert.AreEqual(2, obj2.Test2);
            Assert.AreEqual(3, obj2.Test3);
        }
    }
}
