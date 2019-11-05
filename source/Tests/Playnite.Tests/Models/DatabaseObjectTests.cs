using NUnit.Framework;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Models
{
    [TestFixture]
    public class DatabaseObjectTests
    {
        [Test]
        public void CopyDiffToTest()
        {
            var obj1 = new DatabaseObject() { Name = "obj1" };
            var obj2 = new DatabaseObject() { Name = "obj2" };
            var obj3 = new DatabaseObject() { Name = "Obj1" };
            var obj4 = new DatabaseObject() { Name = "obj1" };
            var changes = 0;

            obj1.PropertyChanged += (s, e) => changes++;
            obj2.PropertyChanged += (s, e) => changes++;
            obj3.PropertyChanged += (s, e) => changes++;

            obj1.CopyDiffTo(obj2);
            Assert.AreEqual(1, changes);
            Assert.AreEqual("obj1", obj2.Name);

            changes = 0;
            obj1.CopyDiffTo(obj3);
            Assert.AreEqual(1, changes);
            Assert.AreEqual("obj1", obj3.Name);

            changes = 0;
            obj1.CopyDiffTo(obj4);
            Assert.AreEqual(0, changes);
            Assert.AreEqual("obj1", obj4.Name);
        }

        [Test]
        public void CopyDiffToArgumentTest()
        {
            var obj1 = new DatabaseObject() { Name = "obj1" };
            var obj2 = new Version();
            Assert.Throws<ArgumentNullException>(() => obj1.CopyDiffTo(null));
            Assert.Throws<ReferenceException>(() => obj1.CopyDiffTo(obj1));
            Assert.Throws<TypeMismatchException>(() => obj1.CopyDiffTo(obj2));
        }
    }
}
