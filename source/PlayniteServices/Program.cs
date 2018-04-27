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

namespace PlayniteServices
{
    public class Program
    {
        private static Database databaseCache;
        public static Database DatabaseCache
        {
            get
            {
                if (databaseCache == null)
                {
                    databaseCache = new Database(Database.DefaultLocation);


                }

                return databaseCache;
            }

            set
            {
                databaseCache = value;
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("customSettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("patreonTokens.json", optional: true, reloadOnChange: true);
                })
                .Build();

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
    }
}
