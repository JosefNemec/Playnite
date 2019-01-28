using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Tests
{
    [TestFixture]
    public class ItchTests
    {
        [Test]
        public void InstallationPathsTest()
        {            
            DirectoryAssert.Exists(Itch.InstallationPath);
            FileAssert.Exists(Itch.ClientExecPath);
        }
    }
}
