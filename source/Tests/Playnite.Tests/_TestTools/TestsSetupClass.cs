using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
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
        public static void OneTimeSetUp()
        {
            // To register pack:// scheme
            var current = new Application();
            FileSystem.CreateDirectory(PlayniteTests.TempPath, true);
            NLogLogger.IsTraceEnabled = true;
            PlayniteSettings.ConfigureLogger();
            SDK.Data.Serialization.Init(new DataSerializer());
            SDK.Data.SQLite.Init((a, b) => new Sqlite(a, b));
            ResourceProvider.SetGlobalProvider(TestResourceProvider.Instance);
            Assert.AreEqual("Filters", ResourceProvider.GetString(LOC.Filters));
        }

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            PlayniteTests.SetEntryAssembly(Assembly.GetExecutingAssembly());
            OneTimeSetUp();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
        }
    }
}
