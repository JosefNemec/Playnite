using BattleNetLibrary.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Tests.Services
{
    [TestFixture]
    public class BattleNetAccountClientTests
    {
        [Test]
        public void GetLoginUrlTest()
        {
            Assert.IsNotNull(BattleNetAccountClient.GetDefaultApiStatus());
        }
    }
}
