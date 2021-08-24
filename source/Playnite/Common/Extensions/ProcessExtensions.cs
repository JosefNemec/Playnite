using Playnite.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public static class ProcessExtensions
    {
        public static bool TryGetMainModuleFileName(this Process process, out string fileName, int buffer = 1024)
        {
            fileName = null;
            var handle = Kernel32.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                var fileNameBuilder = new StringBuilder(buffer);
                uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
                var result = Kernel32.QueryFullProcessImageName(handle, 0, fileNameBuilder, ref bufferLength);
                fileName = result ? fileNameBuilder.ToString() : null;
                return result;
            }
            finally
            {
                Kernel32.CloseHandle(handle);
            }
        }

        public static bool TryGetParentId(this Process process, out int processId)
        {
            processId = 0;
            var handle = Kernel32.OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, process.Id);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                var info = new PROCESS_BASIC_INFORMATION();
                int status = Ntdll.NtQueryInformationProcess(handle, 0, ref info, Marshal.SizeOf(info), out var returnLength);
                if (status != 0)
                {
                    return false;
                }

                processId = info.InheritedFromUniqueProcessId.ToInt32();
                return true;
            }
            finally
            {
                Kernel32.CloseHandle(handle);
            }
        }

        public static bool IsRunning(string processPattern)
        {
            return Process.GetProcesses().FirstOrDefault(a => Regex.IsMatch(a.ProcessName, processPattern, RegexOptions.IgnoreCase)) != null;
        }

        public static string GetCommandLine(this Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }
        }
    }
}
