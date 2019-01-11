using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItchioLibrary.Tests
{
    [TestFixture]
    public class ButlerTests
    {
        [Test]
        public void InstallationPathsTest()
        {
            FileAssert.Exists(Butler.ExecutablePath);
            FileAssert.Exists(Butler.DatabasePath);
        }

        [Test]
        public void StartupTest()
        {
            using (var butler = new Butler())
            {
            }
        }
    }
}
