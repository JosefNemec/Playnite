using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Playnite.Common
{
    public class SystemInfo
    {
        public bool Is64Bit { get; set; }

        public string WindowsVersion { get; set; }

        public string Cpu { get; set; }

        public int Ram { get; set; }

        public List<string> Gpus { get; set; }

        public List<Screen> Monitors { get; set; }
    }

    public enum WindowsVersion
    {
        Unknown,
        Win7,
        Win8,
        Win10
    }

    public static class Computer
    {
        public static WindowsVersion WindowsVersion
        {
            get
            {
                var version = Environment.OSVersion.Version;
                if (version.Major == 6 && version.Major == 1)
                {
                    return WindowsVersion.Win7;
                }
                else if (version.Major == 6 && (version.Major == 2 || version.Major == 3))
                {
                    return WindowsVersion.Win8;
                }
                else if (version.Major == 10)
                {
                    return WindowsVersion.Win10;
                }
                else
                {
                    return WindowsVersion.Unknown;
                }
            }
        }

        public static int GetWindowsReleaseId()
        {
            return Convert.ToInt32(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", ""));
        }

        public static SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo
            {
                Is64Bit = Environment.Is64BitOperatingSystem,
                WindowsVersion = Environment.OSVersion.VersionString
            };

            using (var win32Proc = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {                
                foreach (var obj in win32Proc.Get())
                {
                    info.Cpu = obj["Name"].ToString().Trim();
                    break;
                }
            }

            using (var memory = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            {
                double totalCapacity = 0;
                foreach (var obj in memory.Get())
                {
                    totalCapacity += Convert.ToDouble(obj["Capacity"]);
                }

                info.Ram = Convert.ToInt32(totalCapacity / 1048576);
            }

            using (var video = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                info.Gpus = new List<string>();
                foreach (var obj in video.Get())
                {
                    info.Gpus.Add(obj["Name"].ToString());
                }
            }

            info.Monitors = GetMonitors();
            return info;
        }

        public static List<Screen> GetMonitors()
        {
            return Screen.AllScreens.ToList();
        }

        public static void Shutdown()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-s -t 0");
        }

        public static void Restart()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-r -t 0");
        }

        public static void Hibernate()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-h -t 0");
        }
    }
}
