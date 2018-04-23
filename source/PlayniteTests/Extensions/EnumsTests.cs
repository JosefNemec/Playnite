using NUnit.Framework;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Extensions
{
    public enum TestEnum
    {
        [System.ComponentModel.Description("desc1")]
        Test1,
        Test2
    }

    [TestFixture]
    public class EnumsTests
    {
        [Test]
        public void Test()
        {
            Assert.AreEqual("desc1", Enums.GetEnumDescription(TestEnum.Test1));
            Assert.AreEqual("Test2", Enums.GetEnumDescription(TestEnum.Test2));
        }
    }
}
