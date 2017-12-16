using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PlayniteServices.Databases;
using Xunit;

namespace PlayniteServicesTests
{
    [CollectionDefinition("DefaultCollection")]
    public class DefaultTestCollection : ICollectionFixture<TestFixture<PlayniteServices.Startup>>
    {
    }

    public class TestFixture<TStartup> : IDisposable
    {
        private readonly TestServer server;

        public TestFixture() : this(Path.Combine("source"))
        {
            if (File.Exists(Database.DefaultLocation))
            {
                File.Delete(Database.DefaultLocation);
            }
        }

        protected TestFixture(string solutionRelativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            var builder = new WebHostBuilder()
                .UseStartup(typeof(TStartup))
                .ConfigureServices(InitializeServices)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile("apikeys.json", optional: false, reloadOnChange: true);
                });

            server = new TestServer(builder);

            Client = server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }

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
