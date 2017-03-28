using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Playnite;

namespace PlayniteTests
{
    [TestClass()]
    public class ProgramsTests
    {
        [TestMethod()]
        public void GetPrograms_StandardTest()
        {
            var apps = Programs.GetPrograms();
            Assert.AreNotEqual(apps.Count, 0);
        }
    }
}
