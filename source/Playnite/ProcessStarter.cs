using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class ProcessStarter
    {
        public static void StartUrl(string url)
        {
            Process.Start(url);
        }

        public static void StartProcess(string path, string arguments, string workDir)
        {
            var info = new ProcessStartInfo(path, arguments)
            {
                WorkingDirectory = string.IsNullOrEmpty(workDir) ? (new FileInfo(path)).Directory.FullName : workDir
            };

            Process.Start(info);
        }
    }
}
