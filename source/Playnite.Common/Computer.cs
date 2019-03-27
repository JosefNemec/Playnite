using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Playnite.Common
{
    public class Monitor
    {
        public string Name { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public bool IsPrimary { get; set; }

        public Monitor()
        {
        }

        public Monitor(Screen screen)
        {
            IsPrimary = screen.Primary;
            Name = screen.DeviceName;
            Height = screen.Bounds.Height;
            Width = screen.Bounds.Width;
        }
    }

    public class SystemInfo
    {
        public bool Is64Bit { get; set; }

        public string WindowsVersion { get; set; }

        public string Cpu { get; set; }

        public int Ram { get; set; }

        public List<string> Gpus { get; set; }

        public List<Monitor> Monitors { get; set; }
    }

    public static class Computer
    {
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

            info.Monitors = GetMonitors().ToList();
            return info;
        }

        public static IEnumerable<Monitor> GetMonitors()
        {
            foreach (var screen in Screen.AllScreens)
            {
                yield return new Monitor(screen);
            }
        }
    }
}
