using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests
{
    class PlayniteUITests
    {
        public static string ResourcesPath
        {
            get
            {
                return Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources");
            }
        }

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "playnite_unittests");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
    }
}
