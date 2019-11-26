using Microsoft.Win32;
using Playnite.SDK;
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

        public List<ComputerScreen> Screens { get; set; }
    }

    public class ComputerScreen
    {
        public System.Drawing.Rectangle WorkingArea { get; private set; }
        public bool Primary { get; private set; }
        public string DeviceName { get; private set; }
        public System.Drawing.Rectangle Bounds { get; private set; }
        public int BitsPerPixel { get; private set; }

        public ComputerScreen()
        {
        }

        public ComputerScreen(Screen screen)
        {
            WorkingArea = screen.WorkingArea;
            Primary = screen.Primary;
            DeviceName = screen.DeviceName;
            Bounds = screen.Bounds;
            BitsPerPixel = screen.BitsPerPixel;
        }
    }

    public enum WindowsVersion
    {
        Unknown,
        Win7,
        Win8,
        Win10
    }

    public enum HwCompany
    {
        Intel,
        AMD,
        Nvidia,
        VMware,
        Uknown
    }

    public static class Computer
    {
        private static readonly ILogger logger = LogManager.GetLogger();
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

            info.Screens = GetScreens();
            return info;
        }

        public static List<ComputerScreen> GetScreens()
        {
            return Screen.AllScreens.Select(a => a.ToComputerScreen()).ToList();
        }

        public static ComputerScreen GetPrimaryScreen()
        {
            return Screen.PrimaryScreen?.ToComputerScreen();
        }

        public static void Shutdown()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-s -t 0");
        }

        public static void Restart()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-r -t 0");
        }

        public static bool Sleep()
        {
            return Interop.SetSuspendState(false, true, true);
        }

        public static bool Hibernate()
        {
            return Interop.SetSuspendState(true, true, true);
        }

        public static ComputerScreen ToComputerScreen(this Screen screen)
        {
            if (screen == null)
            {
                return null;
            }
            else
            {
                return new ComputerScreen(screen);
            }
        }

        public static List<HwCompany> GetGpuVendors()
        {
            var gpus = new List<string>();
            var vendors = new List<HwCompany>();
            try
            {
                using (var video = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var obj in video.Get())
                    {
                        gpus.Add(obj["Name"].ToString());
                    }
                }

                foreach (var gpu in gpus)
                {
                    if (gpu.Contains("intel", StringComparison.OrdinalIgnoreCase))
                    {
                        vendors.AddMissing(HwCompany.Intel);
                    }
                    else if (gpu.Contains("nvidia", StringComparison.OrdinalIgnoreCase))
                    {
                        vendors.AddMissing(HwCompany.Nvidia);
                    }
                    else if (gpu.Contains("amd", StringComparison.OrdinalIgnoreCase))
                    {
                        vendors.AddMissing(HwCompany.AMD);
                    }
                    else if (gpu.Contains("vmware", StringComparison.OrdinalIgnoreCase))
                    {
                        vendors.AddMissing(HwCompany.VMware);
                    }
                    else
                    {
                        return new List<HwCompany> { HwCompany.Uknown };
                    }
                }

                if (vendors.Count > 0)
                {
                    return vendors;
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to get GPU vendor.");
            }

            return new List<HwCompany> { HwCompany.Uknown };
        }
    }
}
