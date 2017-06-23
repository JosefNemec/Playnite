using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using PlayniteServices.Databases;

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

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
