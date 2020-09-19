using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PlayniteServices.Databases;
using Xunit;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using PlayniteServices;
using Playnite.SDK;

namespace PlayniteServicesTests
{
    [CollectionDefinition("DefaultCollection")]
    public class DefaultTestCollection : ICollectionFixture<TestFixture<PlayniteServices.Startup>>
    {
    }

    public class TestFixture<TStartup> : IDisposable
    {
        private readonly TestServer server;
        public HttpClient Client { get; }
        public Database Database { get; }

        public TestFixture() : this(Path.Combine("source"))
        {
        }

        protected TestFixture(string solutionRelativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            var builder = new WebHostBuilder()
                .UseStartup(typeof(TStartup))
                .ConfigureServices(InitializeServices)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("customSettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("patreonTokens.json", optional: true, reloadOnChange: true);
                });

            server = new TestServer(builder);
            Client = server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");

            if (File.Exists(Database.Path))
            {
                File.Delete(Database.Path);
            }

            Database = new Database(Database.Path);

            NLogLogger.ConfigureLogger();
            LogManager.Init(new NLogLogProvider());
        }

        public void Dispose()
        {
            Client.Dispose();
            server.Dispose();
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.AddSingleton(manager);
        }
    }
}
