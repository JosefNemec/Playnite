using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Tests
{
    [SetUpFixture]
    public class TestsSetupClass
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // To register pack:// scheme
            var current = new Application();
            PlayniteTests.SetEntryAssembly(Assembly.GetExecutingAssembly());
            FileSystem.CreateDirectory(PlayniteTests.TempPath, true);
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}
