using System.IO;
using System.Text.Json.Serialization;

namespace Playnite.Tests;

[TestFixture]
public class SerializationTests
{
    public class SerializationTestObject
    {
        public int prop1 { get; set; }
        public string? prop2 { get; set; }
    }

    [Test]
    public void JsonDerializationTests()
    {
        var validFile = Path.Combine(TestVars.ResourcesDir, "Serialization", "valid.json");
        var invalidFile = Path.Combine(TestVars.ResourcesDir, "Serialization", "invalid.json");
        Exception? serError = null;
        var validJson = File.ReadAllText(validFile);
        var invalidJson = File.ReadAllText(invalidFile);

        // Raw content test
        var validObj = Serialization.FromJson<SerializationTestObject>(validJson)!;
        Assert.AreEqual(666, validObj.prop1);
        Assert.AreEqual("test", validObj.prop2);

        var success = Serialization.TryFromJson<SerializationTestObject>(validJson, out validObj!, out serError);
        Assert.IsTrue(success);
        Assert.AreEqual(666, validObj.prop1);
        Assert.AreEqual("test", validObj.prop2);
        Assert.IsNull(serError);

        success = Serialization.TryFromJson<SerializationTestObject>(invalidJson, out validObj, out serError);
        Assert.IsFalse(success);
        Assert.IsNull(validObj);
        Assert.IsNotNull(serError);

        // File tests
        validObj = Serialization.FromJsonFile<SerializationTestObject>(validFile)!;
        Assert.AreEqual(666, validObj.prop1);
        Assert.AreEqual("test", validObj.prop2);

        success = Serialization.TryFromJsonFile<SerializationTestObject>(validFile, out validObj!, out serError);
        Assert.IsTrue(success);
        Assert.AreEqual(666, validObj.prop1);
        Assert.AreEqual("test", validObj.prop2);
        Assert.IsNull(serError);

        success = Serialization.TryFromJsonFile<SerializationTestObject>(invalidFile, out validObj, out serError);
        Assert.IsFalse(success);
        Assert.IsNull(validObj);
        Assert.IsNotNull(serError);

        // Steam test
        using (var fs = new FileStream(validFile, FileMode.Open, FileAccess.Read))
        {
            validObj = Serialization.FromJsonStream<SerializationTestObject>(fs)!;
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);
        }

        using (var fs = new FileStream(validFile, FileMode.Open, FileAccess.Read))
        {
            success = Serialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj!, out serError);
            Assert.IsTrue(success);
            Assert.AreEqual(666, validObj.prop1);
            Assert.AreEqual("test", validObj.prop2);
        }

        using (var fs = new FileStream(invalidFile, FileMode.Open, FileAccess.Read))
        {
            success = Serialization.TryFromJsonStream<SerializationTestObject>(fs, out validObj, out serError);
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

        var json = Serialization.ToJson(testObj, false);
        Assert.AreEqual(1, json.GetLineCount());
        var back = Serialization.FromJson<SerializationTestObject>(json)!;
        Assert.AreEqual(666, back.prop1);
        Assert.AreEqual("test", back.prop2);

        json = Serialization.ToJson(testObj, true);
        Assert.AreEqual(4, json.GetLineCount());
        back = Serialization.FromJson<SerializationTestObject>(json)!;
        Assert.AreEqual(666, back.prop1);
        Assert.AreEqual("test", back.prop2);
    }
}
