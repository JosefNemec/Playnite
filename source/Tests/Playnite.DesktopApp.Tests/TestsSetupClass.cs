using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.DesktopApp.Tests
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            PlayniteTests.SetEntryAssembly(Assembly.GetExecutingAssembly());
            Playnite.Tests.TestsSetupClass.OneTimeSetUp();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
        }
    }
}
