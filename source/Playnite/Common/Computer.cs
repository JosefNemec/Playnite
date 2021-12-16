using Microsoft.Win32;
using Playnite.SDK;
using Playnite.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsDisplayAPI;

namespace Playnite.Common
{
    public class SystemInfo
    {
        public bool Is64Bit { get; set; }

        public string WindowsVersion { get; set; }

        public string WindowsEdition { get; set; }

        public int WindowsBuildVersion { get; set; }

        public string Cpu { get; set; }

        public int Ram { get; set; }

        public List<string> Gpus { get; set; }

        public List<ComputerScreen> Screens { get; set; }
    }

    public class ComputerScreen
    {
        public bool Primary { get; set; }
        public string DeviceName { get; set; }
        public System.Drawing.Rectangle Bounds { get; set; }
        public double Dpi { get; set; }
    }

    public enum WindowsVersion
    {
        Unknown,
        Win7,
        Win8,
        Win10,
        Win11
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
        //public enum MonitorDpiType
        //{
        //    MDT_EFFECTIVE_DPI = 0,
        //    MDT_ANGULAR_DPI = 1,
        //    MDT_RAW_DPI = 2,
        //}

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        //public struct MONITORINFOEX
        //{
        //    public int Size;
        //    public RECT Monitor;
        //    public RECT WorkArea;
        //    public uint Flags;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        //    public string DeviceName;
        //}

        //[DllImport("shcore.dll")]
        //public static extern uint GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        //[DllImport("user32")]
        //public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        //public delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        //[DllImport("user32.dll")]
        //public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        //public struct PHYSICAL_MONITOR
        //{
        //    public IntPtr hPhysicalMonitor;

        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        //    public string szPhysicalMonitorDescription;
        //}

        //[DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool GetPhysicalMonitorsFromHMONITOR(
        //IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        //[StructLayout(LayoutKind.Sequential)]
        //public struct Rect
        //{
        //    public int left;
        //    public int top;
        //    public int right;
        //    public int bottom;
        //}

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        //public struct DISPLAY_DEVICE
        //{
        //    [MarshalAs(UnmanagedType.U4)]
        //    public int cb;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        //    public string DeviceName;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        //    public string DeviceString;
        //    [MarshalAs(UnmanagedType.U4)]
        //    public DisplayDeviceStateFlags StateFlags;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        //    public string DeviceID;
        //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        //    public string DeviceKey;
        //}

        //[Flags()]
        //public enum DisplayDeviceStateFlags : int
        //{
        //    /// <summary>The device is part of the desktop.</summary>
        //    AttachedToDesktop = 0x1,
        //    MultiDriver = 0x2,
        //    /// <summary>The device is part of the desktop.</summary>
        //    PrimaryDevice = 0x4,
        //    /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        //    MirroringDriver = 0x8,
        //    /// <summary>The device is VGA compatible.</summary>
        //    VGACompatible = 0x10,
        //    /// <summary>The device is removable; it cannot be the primary display.</summary>
        //    Removable = 0x20,
        //    /// <summary>The device has more display modes than its output devices support.</summary>
        //    ModesPruned = 0x8000000,
        //    Remote = 0x4000000,
        //    Disconnect = 0x2000000
        //}

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        public static double DisplayConfigSourceDPIScaleToMultiplied(this WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale scale)
        {
            switch (scale)
            {
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Identity:
                    return 1;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale125Percent:
                    return 1.25;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale150Percent:
                    return 1.5;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale175Percent:
                    return 1.75;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale200Percent:
                    return 2;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale225Percent:
                    return 2.25;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale250Percent:
                    return 2.50;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale300Percent:
                    return 3;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale350Percent:
                    return 3.5;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale400Percent:
                    return 4;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale450Percent:
                    return 4.5;
                case WindowsDisplayAPI.Native.DisplayConfig.DisplayConfigSourceDPIScale.Scale500Percent:
                    return 5;
                default:
                    return 1;
            }
        }

        public static List<ComputerScreen> GetAllScreensV2()
        {
            var screens = new List<ComputerScreen>();
            var sources = WindowsDisplayAPI.DisplayConfig.PathDisplaySource.GetDisplaySources();
            var displays = WindowsDisplayAPI.DisplayConfig.PathDisplayTarget.GetDisplayTargets();

            foreach (var display in Display.GetDisplays())
            {
                var screen = new ComputerScreen
                {
                    Bounds = new System.Drawing.Rectangle
                    {
                        X = display.CurrentSetting.Position.X,
                        Y = display.CurrentSetting.Position.Y,
                        Width = display.CurrentSetting.Resolution.Width,
                        Height = display.CurrentSetting.Resolution.Height,
                    },
                    Primary = display.IsGDIPrimary
                };

                screen.Dpi = display.ToPathDisplaySource().CurrentDPIScale.DisplayConfigSourceDPIScaleToMultiplied();
                screen.DeviceName = display.ToPathDisplayTarget().FriendlyName;
                screens.Add(screen);
            }

            //display.ToPathDisplayTarget().FriendlyName

            //DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            //d.cb = Marshal.SizeOf(d);
            //try
            //{
            //    for (uint id = 0; EnumDisplayDevices(null, id, ref d, 0); id++)
            //    {
            //        Console.WriteLine(
            //            String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
            //                     id,
            //                     d.DeviceName,
            //                     d.DeviceString,
            //                     d.StateFlags,
            //                     d.DeviceID,
            //                     d.DeviceKey
            //                     )
            //                      );
            //        d.cb = Marshal.SizeOf(d);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(String.Format("{0}", ex.ToString()));
            //}

            //MonitorEnumProc callback = (IntPtr hDesktop, IntPtr hdc, ref Rect prect, int dev) =>
            //{
            //    var screen = new ComputerScreen()
            //    {
            //        //Handle = hDesktop,
            //        Bounds = new System.Drawing.Rectangle
            //        {
            //            X = prect.left,
            //            Y = prect.top,
            //            Width = prect.right - prect.left,
            //            Height = prect.bottom - prect.top,
            //        },
            //        Primary = (prect.left == 0) && (prect.top == 0),
            //    };

            //    GetDpiForMonitor(hDesktop, MonitorDpiType.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
            //    screen.Dpi = (double)dpiX / 96;

            //    //MONITORINFOEX mi = new MONITORINFOEX();
            //    //mi.Size = Marshal.SizeOf(typeof(MONITORINFOEX));
            //    //GetMonitorInfo(hDesktop, ref mi);

            //    var _physicalMonitorArray = new PHYSICAL_MONITOR[1];
            //    GetPhysicalMonitorsFromHMONITOR(hDesktop, 1, _physicalMonitorArray);

            //    screens.Add(screen);
            //    return true;
            //};

            //EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);

            return screens;
        }

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
                    if (version.Build >= 22000)
                    {
                        return WindowsVersion.Win11;
                    }
                    else
                    {
                        return WindowsVersion.Win10;
                    }
                }
                else
                {
                    return WindowsVersion.Unknown;
                }
            }
        }

        public static bool IsTLS13SystemWideEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.3\Client"))
                {
                    if (key != null)
                    {
                        var isEnabled = key.GetValue("Enabled");
                        if (isEnabled != null)
                        {
                            return Convert.ToBoolean(isEnabled);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to test TLS 1.3 state.");
            }

            return false;
        }

        public static int GetWindowsReleaseId()
        {
            var relVal = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "");
            if (relVal?.ToString().IsNullOrEmpty() == true)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(relVal);
            }
        }

        public static string GetWindowsProductName()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "").ToString();
        }

        public static Guid GetMachineGuid()
        {
            RegistryKey root = null;
            if (Environment.Is64BitOperatingSystem)
            {
                root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else
            {
                root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            }

            try
            {
                using (var cryptography = root.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography"))
                {
                    return Guid.Parse((string)cryptography.GetValue("MachineGuid"));
                }
            }
            finally
            {
                root.Dispose();
            }
        }

        public static SystemInfo GetSystemInfo()
        {
            var info = new SystemInfo
            {
                Is64Bit = Environment.Is64BitOperatingSystem,
                WindowsVersion = Environment.OSVersion.VersionString,
                WindowsBuildVersion = GetWindowsReleaseId(),
                WindowsEdition = GetWindowsProductName()
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
            return GetAllScreensV2();
        }

        public static ComputerScreen GetPrimaryScreen()
        {
            return GetAllScreensV2().FirstOrDefault(a => a.Primary);
        }

        public static int GetGetPrimaryScreenIndex()
        {
            var allScreens = Screen.AllScreens;
            for (int i = 0; i < allScreens.Length; i++)
            {
                if (allScreens[i].Primary)
                {
                    return i;
                }
            }

            return 0;
        }

        public static void SetMouseCursorVisibility(bool show)
        {
            if (show)
            {
                while (User32.ShowCursor(true) < 0)
                {
                }
            }
            else
            {
                while (User32.ShowCursor(false) >= 0)
                {
                }
            }
        }

        public static void Shutdown()
        {
            if (WindowsVersion == WindowsVersion.Win7)
            {
                ProcessStarter.StartProcess("shutdown.exe", "-s -t 0");
            }
            else
            {
                ProcessStarter.StartProcess("shutdown.exe", "-s -hybrid -t 0");
            }
        }

        public static void Restart()
        {
            ProcessStarter.StartProcess("shutdown.exe", "-r -t 0");
        }

        public static bool Sleep()
        {
            return Powrprof.SetSuspendState(false, true, false);
        }

        public static bool Hibernate()
        {
            return Powrprof.SetSuspendState(true, true, false);
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

        //private static string GetMonitorFriendlyName(LUID adapterId, uint targetId)
        //{
        //    var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
        //    {
        //        header =
        //        {
        //            size = (uint)Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
        //            adapterId = adapterId,
        //            id = targetId,
        //            type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
        //        }
        //    };
        //    var error = User32.DisplayConfigGetDeviceInfo(ref deviceName);
        //    if (error != WinError.ERROR_SUCCESS)
        //    {
        //        throw new Win32Exception(error);
        //    }

        //    return deviceName.monitorFriendlyDeviceName;
        //}

        //private static IEnumerable<string> GetAllMonitorsFriendlyNames()
        //{
        //    var error = User32.GetDisplayConfigBufferSizes(
        //        QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
        //        out uint pathCount,
        //        out uint modeCount);
        //    if (error != WinError.ERROR_SUCCESS)
        //    {
        //        throw new Win32Exception(error);
        //    }

        //    var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        //    var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
        //    error = User32.QueryDisplayConfig(
        //        QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
        //        ref pathCount,
        //        displayPaths,
        //        ref modeCount,
        //        displayModes,
        //        IntPtr.Zero);
        //    if (error != WinError.ERROR_SUCCESS)
        //    {
        //        throw new Win32Exception(error);
        //    }

        //    for (var i = 0; i < modeCount; i++)
        //    {
        //        if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
        //        {
        //            yield return GetMonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id);
        //        }
        //    }
        //}

        //public static string DeviceFriendlyName(this Screen screen)
        //{
        //    try
        //    {
        //        var allFriendlyNames = GetAllMonitorsFriendlyNames();
        //        for (var index = 0; index < Screen.AllScreens.Length; index++)
        //        {
        //            if (Equals(screen, Screen.AllScreens[index]))
        //            {
        //                return allFriendlyNames.ToArray()[index];
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e, "Failed to get display name.");
        //    }

        //    return screen.DeviceName;
        //}
    }
}
