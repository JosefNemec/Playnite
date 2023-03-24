using Playnite;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Vanara.PInvoke;
using static Vanara.PInvoke.Kernel32;

namespace System.Diagnostics;

public static class ProcessExtensions
{
    // NtQueryInformationProcess and PROCESS_BASIC_INFORMATION are incomplete in Vanara
    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr ExitStatus;
        public IntPtr PebAddress;
        public IntPtr AffinityMask;
        public IntPtr BasePriority;
        public IntPtr UniquePID;
        public IntPtr InheritedFromUniqueProcessId;
    }

    [DllImport("Ntdll.dll", SetLastError = true)]
    private static extern int NtQueryInformationProcess(IntPtr hProcess, int pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

    public static bool TryGetMainModuleFilePath(this Process process, [NotNullWhen(true)] out string? filePath)
    {
        filePath = null;
        using var proc = Kernel32.OpenProcess((uint)ProcessAccess.PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)process.Id);
        var sb = new StringBuilder(Paths.MaxPathLength);
        var bufferLength = (uint)sb.Capacity;
        var result = Kernel32.QueryFullProcessImageName(proc, PROCESS_NAME.PROCESS_NAME_WIN32, sb, ref bufferLength);
        filePath = result ? sb.ToString() : null;
        return result;
    }

    public static bool TryGetParentId(this Process process, out int processId)
    {
        processId = 0;
        using var proc = Kernel32.OpenProcess((uint)ProcessAccess.PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)process.Id);
        var info = new PROCESS_BASIC_INFORMATION();
        int status = NtQueryInformationProcess(proc.DangerousGetHandle(), 0, ref info, Marshal.SizeOf(info), out var returnLength);
        if (status != 0)
        {
            return false;
        }

        processId = info.InheritedFromUniqueProcessId.ToInt32();
        return true;
    }

    public static bool IsRunning(string processNameRegex)
    {
        return Process.GetProcesses().Any(a => Regex.IsMatch(a.ProcessName, processNameRegex, RegexOptions.IgnoreCase));
    }

    public static string? GetCommandLine(this Process process)
    {
        using var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
        using var objects = searcher.Get();
        return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
    }
}
