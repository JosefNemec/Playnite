using LiteDB;
using Newtonsoft.Json;
using NUnit.Framework;
using Playnite.Converters;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Converters
{
    [TestFixture]
    public class ObjectIdJsonConverterTests
    {
        public class TestClass
        {
            public ObjectId Id
            {
                get; set;
            }

            public List<ObjectId> TestList
            {
                get; set;
            }
        }

        [Test]
        public void ConvertTest()
        {
            var obj = new TestClass()
            {
                Id = ObjectId.NewObjectId(),
                TestList = new List<ObjectId>()
                {
                    ObjectId.NewObjectId(),
                    ObjectId.NewObjectId()
                }
            };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new ObjectIdContractResolver()
            };

            var stringData = JsonConvert.SerializeObject(obj, settings);
            var desData = JsonConvert.DeserializeObject<TestClass>(stringData, settings);
            Assert.AreEqual(obj.Id, desData.Id);
            CollectionAssert.AreEqual(obj.TestList, desData.TestList);
        }
    }
}
