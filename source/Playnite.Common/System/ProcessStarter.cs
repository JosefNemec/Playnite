using Playnite.SDK;
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
        private static ILogger logger = LogManager.GetLogger();

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
            var startupPath = path;
            if (path.Contains(".."))
            {
                startupPath = Path.GetFullPath(path);
            }

            var info = new ProcessStartInfo(startupPath)
            {
                Arguments = arguments,
                WorkingDirectory = string.IsNullOrEmpty(workDir) ? (new FileInfo(startupPath)).Directory.FullName : workDir
            };

            return Process.Start(info);
        }        
    }
}
