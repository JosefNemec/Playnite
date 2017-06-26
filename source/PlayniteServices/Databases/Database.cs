using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Databases
{
    public class Database
    {
        private static string defaultLocation = string.Empty;
        public static string DefaultLocation
        {
            get
            {
                if (!string.IsNullOrEmpty(defaultLocation))
                {
                    return defaultLocation;
                }
                else
                {
                    var dbLocation = Startup.Configuration.GetSection("DbLocation");
                    if (dbLocation == null || string.IsNullOrEmpty(dbLocation.Value))
                    {
                        throw new Exception("Missing database location configuration.");
                    }

                    return dbLocation.Value;
                }                
            }

            set
            {
                defaultLocation = value;
            }
        }

        private LiteDatabase liteDB;

        public Database(string path)
        {
            liteDB =  new LiteDatabase(string.Format("Filename={0};Mode=Exclusive", path));
        }

        public LiteCollection<T> GetCollection<T>(string name)
        {
            return liteDB.GetCollection<T>(name);
        }
    }
}
