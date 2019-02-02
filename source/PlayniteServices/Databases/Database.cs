using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Databases
{
    public class Database
    {
        public static string Path
        {
            get
            {
                var path = Startup.Configuration.GetSection("DbPath").Value;
                if (System.IO.Path.IsPathRooted(path))
                {
                    return path;
                }
                else
                {
                    return System.IO.Path.Combine(Paths.ExecutingDirectory, path);
                }               
            }
        }

        private LiteDatabase liteDB;

        public Database(string path)
        {
            liteDB =  new LiteDatabase(string.Format("Filename={0}", path));
        }

        public LiteCollection<T> GetCollection<T>(string name)
        {
            return liteDB.GetCollection<T>(name);
        }

        public LiteCollection<BsonDocument> GetCollection(string name)
        {
            return liteDB.GetCollection(name);
        }

        public bool DropCollection(string name)
        {
            return liteDB.DropCollection(name);
        }
    }
}
