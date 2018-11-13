using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class DatabaseSettings
    {
        public int Version
        {
            get; set;
        }

        public DatabaseSettings()
        {
        }
    }
}
