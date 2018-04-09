using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite;
using Playnite.SDK.Models;

namespace PlayniteTests
{
    [TestFixture]
    public class CloneObjectTests
    {
        class TestObject : INotifyPropertyChanged
        {
            private int prop1;
            public int Prop1
            {
                get => prop1;
                set
                {
                    prop1 = value;
                    OnPropertyChanged("Prop1");
                }
            }

            private string prop2;
            public string Prop2
            {
                get => prop2;
                set
                {
                    prop2 = value;
                    OnPropertyChanged("Prop2");
                }
            }

            private List<int> prop3;
            public List<int> Prop3
            {
                get => prop3;
                set
                {
                    prop3 = value;
                    OnPropertyChanged("Prop3");
                }
            }

            private TestObject prop4;
            public TestObject Prop4
            {
                get => prop4;
                set
                {
                    prop4 = value;
                    OnPropertyChanged("Prop4");
                }
            }

            private GameState prop5;
            public GameState Prop5
            {
                get => prop5;
                set
                {
                    prop5 = value;
                    OnPropertyChanged("Prop5");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        [Test]
        public void CopyPropertiesTest()
        {
            var source = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop3 = new List<int>() { 1, 2 },
                Prop4 = new TestObject() { Prop2 = "test2" },
                Prop5 = new GameState() { Installed = true }
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
            Assert.AreEqual(source.Prop5.Installed, target.Prop5.Installed);

            // Null copy
            source.Prop2 = null;
            source.Prop3 = null;
            source.Prop4 = null;
            source.Prop5 = null;
            source.CopyProperties(target, true);
            Assert.IsNull(target.Prop2);
            Assert.IsNull(target.Prop3);
            Assert.IsNull(target.Prop4);
            Assert.IsNull(target.Prop5);

            // Diff only
            var changed = new List<string>();
            source = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop3 = new List<int>() { 1, 2 },
                Prop4 = new TestObject() { Prop2 = "test2" },
                Prop5 = new GameState() { Running = true }
                
            };

            target = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop5 = new GameState() { Running = true }
            };

            target.PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            source.CopyProperties(target, true);
            Assert.AreEqual(2, changed.Count);
            Assert.AreEqual("Prop3", changed[0]);
            Assert.AreEqual("Prop4", changed[1]);

            changed = new List<string>();
            target = new TestObject
            {
                Prop1 = 2,
                Prop2 = "test",
                Prop5 = new GameState() { Running = true }
            };

            target.PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            source.CopyProperties(target, false);
            Assert.AreEqual(5, changed.Count);
        }
    }
}
