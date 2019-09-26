using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Extensions
{
    [TestFixture]
    public class ObjectExtensionsTests
    {
        public class AttributeTestClass
        {
            [RequiresRestart]
            public bool Prop1 { get; set; }

            public bool Prop2 { get; set; }
        }

        [Test]
        public void HasPropertyAssignedAttributeTest()
        {
            Assert.IsTrue(typeof(AttributeTestClass).HasPropertyAttribute<RequiresRestartAttribute>(nameof(AttributeTestClass.Prop1)));
            Assert.IsFalse(typeof(AttributeTestClass).HasPropertyAttribute<RequiresRestartAttribute>(nameof(AttributeTestClass.Prop2)));
            Assert.IsFalse(typeof(AttributeTestClass).HasPropertyAttribute<RequiresRestartAttribute>("SomePropNoexistetn"));
        }
    }
}
