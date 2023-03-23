using System.Diagnostics;
using System.IO;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace Playnite;

public static class CmdLineTools
{
    public const string TaskKill = "taskkill";
    public const string Cmd = "cmd";
    public const string IPConfig = "ipconfig";
}

public static class ProcessStarter
{
    private static readonly ILogger logger = LogManager.GetLogger();

    public static uint ShellExecute(string cmdLine)
    {
        logger.Debug($"Executing shell command: {cmdLine}");

        if (CreateProcess(
            null,
            new StringBuilder(cmdLine),
            default,
            default,
            false,
            CREATE_PROCESS.NORMAL_PRIORITY_CLASS,
            default,
            null,
            STARTUPINFO.Default,
            out var procInfo))
        {
            using (procInfo)
                return procInfo.dwProcessId;
        }
        else
        {
            Win32Error.GetLastError().ThrowIfFailed();
            return 0;
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
        ArgumentException.ThrowIfNullOrEmpty(path);

        var startupPath = path;
        if (path.Contains("..", StringComparison.Ordinal))
        {
            startupPath = Path.GetFullPath(path);
        }

        var info = new ProcessStartInfo(startupPath)
        {
            Arguments = arguments,
            WorkingDirectory = workDir.IsNullOrWhiteSpace() ? new FileInfo(startupPath).Directory!.FullName : workDir
        };

        if (asAdmin)
        {
            info.Verb = "runas";
        }

        return Process.Start(info) ?? throw new Exception("Failed to start process");
    }

    public static int StartProcessWait(string path, string arguments, string workDir, bool noWindow = false)
    {
        logger.Debug($"Starting process: {path}, {arguments}, {workDir}");
        ArgumentException.ThrowIfNullOrEmpty(path);

        var startupPath = path;
        if (path.Contains("..", StringComparison.Ordinal))
        {
            startupPath = Path.GetFullPath(path);
        }

        var info = new ProcessStartInfo(startupPath)
        {
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrEmpty(workDir) ? new FileInfo(startupPath).Directory!.FullName : workDir
        };

        if (noWindow)
        {
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
        }

        using (var proc = Process.Start(info) ?? throw new Exception("Failed to start process"))
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
        ArgumentException.ThrowIfNullOrEmpty(path);

        var startupPath = path;
        if (path.Contains("..", StringComparison.Ordinal))
        {
            startupPath = Path.GetFullPath(path);
        }

        var info = new ProcessStartInfo(startupPath)
        {
            Arguments = arguments,
            WorkingDirectory = string.IsNullOrEmpty(workDir) ? new FileInfo(startupPath).Directory!.FullName : workDir,
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
