using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.Tests
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            FileSystem.CreateDirectory(PlayniteTests.TempPath, true);
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}
