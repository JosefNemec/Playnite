using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Playnite
{
    public static class ProcessStarter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Process StartUrl(string url)
        {
            logger.Debug($"Opening URL: {url}");
            return Process.Start(url);
        }

        public static Process StartProcess(string path)
        {
            return StartProcess(path, string.Empty, string.Empty);
        }

        public static Process StartProcess(string path, string arguments)
        {
            return StartProcess(path, arguments, string.Empty);
        }

        public static Process StartProcess(string path, string arguments, string workDir)
        {
            logger.Debug($"Starting process: {path}, {arguments}, {workDir}");
            var info = new ProcessStartInfo(path)
            {
                Arguments = arguments,
                WorkingDirectory = string.IsNullOrEmpty(workDir) ? (new FileInfo(path)).Directory.FullName : workDir
            };

            return Process.Start(info);
        }        
    }
}
