using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using PlayniteServices.Databases;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Playnite.SDK;

namespace PlayniteServices
{
    public class Program
    {
        public static Database Database
        {
            get; private set;
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("customSettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("patreonTokens.json", optional: true, reloadOnChange: true);
                })
                .Build();

            Database = new Database(Database.Path);
            return host;
        }

        public static void Main(string[] args)
        {
            NLogLogger.ConfigureLogger();
            LogManager.Init(new NLogLogProvider());
            BuildWebHost(args).Run();
        }        
    }
}
