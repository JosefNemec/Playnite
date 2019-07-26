using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite;
using Playnite.SDK.Models;
using Newtonsoft.Json;

namespace Playnite.Tests
{
    [TestFixture]
    public class CloneObjectTests
    {
        class TestObject : ObservableObject
        {
            private int prop1;
            public int Prop1
            {
                get => prop1;
                set
                {
                    prop1 = value;
                    OnPropertyChanged();
                }
            }

            private string prop2;
            public string Prop2
            {
                get => prop2;
                set
                {
                    prop2 = value;
                    OnPropertyChanged();
                }
            }

            private List<int> prop3;
            public List<int> Prop3
            {
                get => prop3;
                set
                {
                    prop3 = value;
                    OnPropertyChanged();
                }
            }

            private TestObject prop4;
            public TestObject Prop4
            {
                get => prop4;
                set
                {
                    prop4 = value;
                    OnPropertyChanged();
                }
            }

            private long prop6;
            public long Prop6
            {
                get => prop6;
                set
                {
                    prop6 = value;
                    OnPropertyChanged();
                }
            }
        }

        public class JsonTestObject
        {
            public int Prop1 { get; set; }

            [JsonIgnore]
            public int Prop2 { get; set; }
        }

        [Test]
        public void JsonIgnoreTest()
        {
            var source = new JsonTestObject()
            {
                Prop1 = 10,
                Prop2 = 20
            };

            var target = new JsonTestObject();
            source.CopyProperties(target, false, null, true);
            Assert.AreEqual(10, target.Prop1);
            Assert.AreNotEqual(20, target.Prop2);

            target = new JsonTestObject();
            source.CopyProperties(target, false, null, false);
            Assert.AreEqual(10, target.Prop1);
            Assert.AreEqual(20, target.Prop2);
        }

        [Test]
        public void CopyPropertiesTest()
        {
            var source = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop3 = new List<int>() { 1, 2 },
                Prop4 = new TestObject() { Prop2 = "test2" }
            };

            var target = new TestObject();

            // Standard copy of all properties
            source.CopyProperties(target, true);            
            Assert.AreEqual(source.Prop1, target.Prop1);
            Assert.AreEqual(source.Prop2, target.Prop2);
            Assert.IsNotNull(target.Prop3);
            Assert.AreEqual(1, target.Prop3.First());
            Assert.IsNotNull(target.Prop4);
            Assert.AreEqual(source.Prop4.Prop3, target.Prop4.Prop3);

            // Null copy
            source.Prop2 = null;
            source.Prop3 = null;
            source.Prop4 = null;
            source.CopyProperties(target, true);
            Assert.IsNull(target.Prop2);
            Assert.IsNull(target.Prop3);
            Assert.IsNull(target.Prop4);

            // Diff only
            var changed = new List<string>();
            source = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop3 = new List<int>() { 1, 2 },
                Prop4 = new TestObject() { Prop2 = "test2" },
                Prop6 = 20
            };

            target = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop6 = 0
            };

            target.PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            source.CopyProperties(target, true);
            Assert.AreEqual(3, changed.Count);
            Assert.AreEqual("Prop3", changed[0]);
            Assert.AreEqual("Prop4", changed[1]);
            Assert.AreEqual("Prop6", changed[2]);

            changed = new List<string>();
            target = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test"
            };

            target.PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            source.CopyProperties(target, false);
            Assert.AreEqual(5, changed.Count);
        }
    }
}
