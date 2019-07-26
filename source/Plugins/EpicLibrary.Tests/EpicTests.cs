using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Tests
{
    public class EpicTests
    {
        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "playnite_epic_unittests");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
    }
}
