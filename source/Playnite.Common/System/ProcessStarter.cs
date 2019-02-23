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
            try
            {
                return Process.Start(url);
            }
            catch (Exception e)
            {
                // There are some crash report with 0x80004005 error when opening standard URL.
                logger.Error(e, "Failed to open URL.");
                return Process.Start("cmd.exe", $"/C start {url}");
            }
        }

        public static Process StartProcess(string path, bool asAdmin = false)
        {
            return StartProcess(path, string.Empty, string.Empty, asAdmin);
        }

        public static Process StartProcess(string path, string arguments, bool asAdmin = false)
        {
            return StartProcess(path, arguments, string.Empty, asAdmin);
        }

        public static Process StartProcess(string path, string arguments, string workDir, bool asAdmin = false)
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

            if (asAdmin)
            {
                info.Verb = "runas";
            }

            return Process.Start(info);
        }        
    }
}
