using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;

namespace TestApp
{
    public class TestApp
    {
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                var appInfoFile = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "appinfo.json");
                FileSystem.DeleteFile(appInfoFile);
                var appInfo = new TestAppProcInfo
                {
                    WorkingDir = Environment.CurrentDirectory,
                    Arguments = args
                };

                FileSystem.WriteStringToFile(appInfoFile, JsonConvert.SerializeObject(appInfo, Formatting.Indented));
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
