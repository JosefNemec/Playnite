using Playnite.SDK;
using Playnite.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Playnite.Common
{
    public static class CmdLineTools
    {
        public const string TaskKill = "taskkill";
        public const string Cmd = "cmd";
        public const string IPConfig = "ipconfig";
    }

    public static class ProcessStarter
    {
        private static ILogger logger = LogManager.GetLogger();

        public static int ShellExecute(string cmdLine)
        {
            logger.Debug($"Executing shell command: {cmdLine}");
            var startInfo = new STARTUPINFO();
            var procInfo = new PROCESS_INFORMATION();
            var procAtt = new SECURITY_ATTRIBUTES();
            var threadAtt = new SECURITY_ATTRIBUTES();
            procAtt.nLength = Marshal.SizeOf(procAtt);
            threadAtt.nLength = Marshal.SizeOf(threadAtt);

            try
            {
                if (Kernel32.CreateProcess(
                    null,
                    cmdLine,
                    ref procAtt,
                    ref threadAtt,
                    false,
                    0x0020,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out procInfo))
                {
                    return procInfo.dwProcessId;
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (procInfo.hProcess != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(procInfo.hProcess);
                }

                if (procInfo.hThread != IntPtr.Zero)
                {
                    Kernel32.CloseHandle(procInfo.hThread);
                }
            }
        }

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
                return Process.Start(CmdLineTools.Cmd, $"/C start {url}");
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
            logger.Debug($"Starting process: {path}, {arguments}, {workDir}, {asAdmin}");
            if (path.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("Cannot start process, executable path is specified.");
            }

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

        public static int StartProcessWait(string path, string arguments, string workDir, bool noWindow = false)
        {
            logger.Debug($"Starting process: {path}, {arguments}, {workDir}");
            if (path.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("Cannot start process, executable path is specified.");
            }

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

            if (noWindow)
            {
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
            }

            using (var proc = Process.Start(info))
            {
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

        public static int StartProcessWait(
            string path,
            string arguments,
            string workDir,
            out string stdOutput,
            out string stdError)
        {
            logger.Debug($"Starting process: {path}, {arguments}, {workDir}");
            if (path.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("Cannot start process, executable path is specified.");
            }

            var startupPath = path;
            if (path.Contains(".."))
            {
                startupPath = Path.GetFullPath(path);
            }

            var info = new ProcessStartInfo(startupPath)
            {
                Arguments = arguments,
                WorkingDirectory = string.IsNullOrEmpty(workDir) ? (new FileInfo(startupPath)).Directory.FullName : workDir,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var stdout = string.Empty;
            var stderr = string.Empty;
            using (var proc = new Process())
            {
                proc.StartInfo = info;
                proc.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        stdout += e.Data + Environment.NewLine;
                    }
                };

                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        stderr += e.Data + Environment.NewLine;
                    }
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                stdOutput = stdout;
                stdError = stderr;
                return proc.ExitCode;
            }
        }
    }
}
