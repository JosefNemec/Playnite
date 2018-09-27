using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Tests
{
    public class TwitchTests
    {
        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "playnite_twitch_unittests");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
    }
}
