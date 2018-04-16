using NUnit.Framework;
using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            FileSystem.CreateDirectory(PlayniteUITests.TempPath, true);
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}
