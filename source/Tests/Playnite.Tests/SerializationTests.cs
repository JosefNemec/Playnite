using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdkSerialization = Playnite.SDK.Data.Serialization;

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

        public class SerializationTestObject
        {
            public int prop1 { get; set; }
            public string prop2 { get; set; }
        }

        [Test]
        public void JsonDerializationTests()
        {
            var validFile = Path.Combine(PlayniteTests.ResourcesPath, "Serialization", "valid.json");
            var invalidFile = Path.Combine(PlayniteTests.ResourcesPath, "Serialization", "invalid.json");
            Exception serError = null;
            var validJson = File.ReadAllText(validFile);
            var invalidJson = File.ReadAllText(invalidFile);

            // Raw content test
            var validObj = SdkSerialization.FromJson<SerializationTestObject>(validJson);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);

            var success = SdkSerialization.TryFromJson<SerializationTestObject>(validJson, out validObj);
            Assert.IsTrue(success);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);

            success = SdkSerialization.TryFromJson<SerializationTestObject>(validJson, out validObj, out serError);
            Assert.IsTrue(success);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);
            Assert.IsNull(serError);

            success = SdkSerialization.TryFromJson<SerializationTestObject>(invalidJson, out validObj);
            Assert.IsFalse(success);
            Assert.IsNull(validObj);

            success = SdkSerialization.TryFromJson<SerializationTestObject>(invalidJson, out validObj, out serError);
            Assert.IsFalse(success);
            Assert.IsNull(validObj);
            Assert.IsNotNull(serError);

            // File tests
            validObj = SdkSerialization.FromJsonFile<SerializationTestObject>(validFile);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);

            success = SdkSerialization.TryFromJsonFile<SerializationTestObject>(validFile, out validObj);
            Assert.IsTrue(success);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);

            success = SdkSerialization.TryFromJsonFile<SerializationTestObject>(validFile, out validObj, out serError);
            Assert.IsTrue(success);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);
            Assert.IsNull(serError);

            success = SdkSerialization.TryFromJsonFile<SerializationTestObject>(invalidFile, out validObj);
            Assert.IsFalse(success);
            Assert.IsNull(validObj);

            success = SdkSerialization.TryFromJsonFile<SerializationTestObject>(invalidFile, out validObj, out serError);
            Assert.IsFalse(success);
            Assert.IsNull(validObj);
            Assert.IsNotNull(serError);

            // Steam test
            using (var fs = new FileStream(validFile, FileMode.Open, FileAccess.Read))
            {
                validObj = SdkSerialization.FromJsonStream<SerializationTestObject>(fs);
                Assert.AreEqual(666, validObj.prop1);
                Assert.AreEqual("test", validObj.prop2);
            }

            using (var fs = new FileStream(validFile, FileMode.Open, FileAccess.Read))
            {
                success = SdkSerialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj);
                Assert.IsTrue(success);
                Assert.AreEqual(666, validObj.prop1);
                Assert.AreEqual("test", validObj.prop2);
            }

            using (var fs = new FileStream(validFile, FileMode.Open, FileAccess.Read))
            {
                success = SdkSerialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj, out serError);
                Assert.IsTrue(success);
                Assert.AreEqual(666, validObj.prop1);
                Assert.AreEqual("test", validObj.prop2);
            }

            using (var fs = new FileStream(invalidFile, FileMode.Open, FileAccess.Read))
            {
                success = SdkSerialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj);
                Assert.IsFalse(success);
                Assert.IsNull(validObj);
            }

            using (var fs = new FileStream(invalidFile, FileMode.Open, FileAccess.Read))
            {
                success = SdkSerialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj, out serError);
                Assert.IsFalse(success);
                Assert.IsNull(validObj);
                Assert.IsNotNull(serError);
            }
        }

        [Test]
        public void JsonSerializationTests()
        {
            var testObj = new SerializationTestObject
            {
                prop1 = 666,
                prop2 = "test"
            };

            var json = SdkSerialization.ToJson(testObj, false);
            Assert.AreEqual(1, json.GetLineCount());
            var back = SdkSerialization.FromJson<SerializationTestObject>(json);
            Assert.AreEqual(666, back.prop1);
            Assert.AreEqual("test", back.prop2);

            json = SdkSerialization.ToJson(testObj, true);
            Assert.AreEqual(4, json.GetLineCount());
            back = SdkSerialization.FromJson<SerializationTestObject>(json);
            Assert.AreEqual(666, back.prop1);
            Assert.AreEqual("test", back.prop2);
        }
    }
}
