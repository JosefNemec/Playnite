using NUnit.Framework;
using OriginLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OriginLibrary.Tests
{
    [TestFixture]
    public class ModelsTests
    {
        [Test]
        public void InstallerDataDeserialization()
        {            
            var data = OriginLibrary.GetGameInstallerData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"));
            Assert.AreEqual(2, data.runtime.launchers.Count);
            Assert.AreEqual(true, data.runtime.launchers[1].requires64BitOS);

            data = OriginLibrary.GetGameInstallerData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "sub", "sub2"));
            Assert.AreEqual(2, data.runtime.launchers.Count);
            Assert.AreEqual(true, data.runtime.launchers[1].requires64BitOS);
        }
    }
}
