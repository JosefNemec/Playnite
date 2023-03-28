using System.Reflection;
using System.Text;
using System.Windows;

namespace Playnite.Tests;

[SetUpFixture]
public class TestsSetupClass
{
    internal class TestSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state)
        {
            d(state);
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            d(state);
        }
    }

    public static void OneTimeSetUp()
    {
        // To register pack:// scheme
        //var current = new Application();
        //FileSystem.CreateDirectory(PlayniteTests.TempPath, true);
        //NLogLogger.IsTraceEnabled = true;
        //PlayniteSettings.ConfigureLogger();
        //SDK.Data.SQLite.Init((a, b) => new Sqlite(a, b));
        //ResourceProvider.SetGlobalProvider(TestResourceProvider.Instance);
        //Assert.AreEqual("Filters", ResourceProvider.GetString(LOC.Filters));

        SyncContext.SetMainContext(new TestSynchronizationContext());
    }

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        //PlayniteTests.SetEntryAssembly(Assembly.GetExecutingAssembly());
        OneTimeSetUp();
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
    }
}
