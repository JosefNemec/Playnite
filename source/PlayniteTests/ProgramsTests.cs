using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite;

namespace PlayniteTests
{
    [TestFixture]
    public class ProgramsTests
    {
        [Test]
        public void GetPrograms_StandardTest()
        {
            var apps = Programs.GetPrograms();
            Assert.AreNotEqual(apps.Count, 0);
        }
    }
}
