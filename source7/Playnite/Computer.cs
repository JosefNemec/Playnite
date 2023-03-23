using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Vanara.PInvoke;
using System.Management;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.User32;

namespace Playnite;

public class SystemInfo
{
    public bool Is64Bit { get; set; }
    public string? WindowsVersion { get; set; }
    public string? ActualWindowsVersion { get; set; }
    public string? WindowsEdition { get; set; }
    public int WindowsBuildVersion { get; set; }
    public string? Cpu { get; set; }
    public int Ram { get; set; }
    public List<string>? Gpus { get; set; }
    public List<ComputerScreen>? Screens { get; set; }
}

public class ComputerScreen
{
    public System.Drawing.Rectangle WorkingArea { get; private set; }
    public bool Primary { get; private set; }
    public string? DeviceName { get; private set; }
    public System.Drawing.Rectangle Bounds { get; private set; }
    public int BitsPerPixel { get; private set; }

    public ComputerScreen()
    {
    }

    public ComputerScreen(Screen screen)
    {
        WorkingArea = screen.WorkingArea;
        Primary = screen.Primary;
        DeviceName = screen.DeviceFriendlyName();
        Bounds = screen.Bounds;
        BitsPerPixel = screen.BitsPerPixel;
    }
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
    private static readonly ILogger logger = LogManager.GetLogger();

    public static WindowsVersion WindowsVersion
    {
        get
        {
            var version = Environment.OSVersion.Version;
            if (version.Major == 6 && version.Minor == 1)
            {
                return WindowsVersion.Win7;
            }
            else if (version.Major == 6 && (version.Minor == 2 || version.Minor == 3))
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

    public static string? GetWindowsProductName()
    {
        return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "")?.ToString();
    }

    public static Guid GetMachineGuid()
    {
        RegistryKey? root;
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
                var guid = cryptography!.GetValue("MachineGuid");
                if (guid != null)
                {
                    return Guid.Parse((string)guid);
                }
                else
                {
                    return Guid.Empty;
                }
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
            ActualWindowsVersion = Computer.WindowsVersion.ToString(),
            WindowsBuildVersion = GetWindowsReleaseId(),
            WindowsEdition = GetWindowsProductName()
        };

        using (var win32Proc = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
        {
            foreach (var obj in win32Proc.Get())
            {
                info.Cpu = obj?["Name"]?.ToString()?.Trim() ?? "uknown";
                break;
            }
        }

        using (var memory = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
        {
            double totalCapacity = 0;
            foreach (var obj in memory.Get())
            {
                totalCapacity += Convert.ToDouble(obj["TotalPhysicalMemory"]);
            }

            info.Ram = Convert.ToInt32(totalCapacity / 1048576);
        }

        using (var video = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            info.Gpus = new List<string>();
            foreach (var obj in video.Get())
            {
                info.Gpus.Add(obj?["Name"]?.ToString() ?? "uknown");
            }
        }

        info.Screens = GetScreens();
        return info;
    }

    public static List<ComputerScreen> GetScreens()
    {
        return Screen.AllScreens.Select(a => a.ToComputerScreen()).ToList();
    }

    public static ComputerScreen? GetPrimaryScreen()
    {
        return Screen.PrimaryScreen?.ToComputerScreen();
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
        return PowrProf.SetSuspendState(false, true, false);
    }

    public static bool Hibernate()
    {
        return PowrProf.SetSuspendState(true, true, false);
    }

    public static ComputerScreen ToComputerScreen(this Screen screen)
    {
        return new ComputerScreen(screen);
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
                    gpus.Add(obj?["Name"]?.ToString() ?? "uknown");
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

    private static List<string> GetAllMonitorsFriendlyNames()
    {
        var names = new List<string>();
        if (!QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, out var paths, out var modes, out var topId).Succeeded)
        {
            return names;
        }

        foreach (var mode in modes.Where(m => m.infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET))
        {
            names.Add(DisplayConfigGetDeviceInfo<DISPLAYCONFIG_TARGET_DEVICE_NAME>(mode.adapterId, mode.id).monitorFriendlyDeviceName);
        }

        return names;
    }

    public static string DeviceFriendlyName(this Screen screen)
    {
        try
        {
            var screens = Screen.AllScreens;
            var allFriendlyNames = GetAllMonitorsFriendlyNames();
            if (allFriendlyNames.Count != screens.Length)
            {
            }

            for (var index = 0; index < screens.Length; index++)
            {
                if (Equals(screen, screens[index]))
                {
                    return allFriendlyNames.ElementAt(index);
                }
            }
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to get display name.");
        }

        return screen.DeviceName;
    }

    public static bool GetScreenReaderActive()
    {
        // In theory this method should be using SystemParametersInfo API with SPI_GETSCREENREADER
        // but according to my testing that returns screen reader presence even if no screen reader is running.
        // No idea why, so for now we will just check for Narrator, NVDA and JAWS readers.

        if (Process.GetProcessesByName("narrator").HasItems())
        {
            return true;
        }

        if (Process.GetProcessesByName("nvda").HasItems())
        {
            return true;
        }

        if (Process.GetProcessesByName("jfw").HasItems())
        {
            return true;
        }

        return false;
    }
}
