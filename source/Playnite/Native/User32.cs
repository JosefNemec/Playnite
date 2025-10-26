using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Native
{
    public class User32
    {
        private const string dllName = "User32.dll";

        [DllImport(dllName)]
        public static extern int GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

        [DllImport(dllName)]
        public static extern int QueryDisplayConfig(
            QUERY_DEVICE_CONFIG_FLAGS flags,
            ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray,
            ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray,
            IntPtr currentTopologyId);

        [DllImport(dllName)]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName);

        [DllImport(dllName)]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        [DllImport(dllName)]
        public static extern int DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_DEVICE_INFO_HEADER setPacket);

        [DllImport(dllName)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(dllName)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport(dllName)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport(dllName)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport(dllName)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(dllName)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport(dllName)]
        public static extern int ShowCursor(bool bShow);

        [DllImport(dllName, EntryPoint = "SetWindowPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags);

        public static void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags)
        {
            if (!_SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags))
            {
                throw new Win32Exception();
            }
        }

        [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr DefWindowProc([In] IntPtr hwnd, [In] int msg, [In] IntPtr wParam, [In] IntPtr lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, ref IntPtr lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, IntPtr lParam);

        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        public static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport(dllName)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

        [DllImport(dllName, EntryPoint = "GetMonitorInfo", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFO lpmi);

        [DllImport(dllName, EntryPoint = "DestroyIcon", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        public static MONITORINFO GetMonitorInfo(IntPtr hMonitor)
        {
            var mi = new MONITORINFO();
            if (!_GetMonitorInfo(hMonitor, mi))
            {
                throw new Win32Exception();
            }
            return mi;
        }

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport(dllName, CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport(dllName, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport(dllName, SetLastError = true)]
        public static extern bool LockWorkStation();

        [Flags]
        public enum ExitWindowsFlags
        {
            EWX_LOGOFF = 0x00000000,
            EWX_SHUTDOWN = 0x00000001,
            EWX_REBOOT = 0x00000002,
            EWX_FORCE = 0x00000004,
            EWX_POWEROFF = 0x00000008,
            EWX_FORCEIFHUNG = 0x00000010,
            EWX_QUICKRESOLVE = 0x00000020,
            EWX_RESTARTAPPS = 0x00000040,
            EWX_HYBRID_SHUTDOWN = 0x00400000,
            EWX_BOOTOPTIONS = 0x01000000,
            EWX_ARSO = 0x04000000 // Undocumented https://stackoverflow.com/a/72069512/1107424
        }

        [DllImport(dllName, SetLastError = true)]
        public static extern bool ExitWindowsEx(ExitWindowsFlags uFlags, uint dwReason);

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal int[] Privileges;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPrivileges, IntPtr NewState, int BufferLength, IntPtr PreviousState, ref int ReturnLength);

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_ADJUST_PRIVILEGES = 0X00000020;
        private const int TOKEN_QUERY = 0X00000008;
        private const int TOKEN_ALL_ACCESS = 0X001f01ff;
        private const int PROCESS_QUERY_INFORMATION = 0X00000400;

        public static bool EnablePrivilege(string lpszPrivilege, bool bEnablePrivilege)
        {
            bool retval = false;
            int ltkpOld = 0;
            IntPtr hToken = IntPtr.Zero;
            TOKEN_PRIVILEGES tkp = new TOKEN_PRIVILEGES();
            tkp.Privileges = new int[3];
            TOKEN_PRIVILEGES tkpOld = new TOKEN_PRIVILEGES();
            tkpOld.Privileges = new int[3];
            LUID tLUID = new LUID();
            tkp.PrivilegeCount = 1;
            if (bEnablePrivilege)
                tkp.Privileges[2] = SE_PRIVILEGE_ENABLED;
            else
                tkp.Privileges[2] = 0;
            if (LookupPrivilegeValue(null, lpszPrivilege, ref tLUID))
            {
                var proc = Process.GetCurrentProcess();
                if (proc.Handle != IntPtr.Zero)
                {
                    if (OpenProcessToken(proc.Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                        ref hToken) != 0)
                    {
                        tkp.PrivilegeCount = 1;
                        tkp.Privileges[2] = SE_PRIVILEGE_ENABLED;
                        tkp.Privileges[1] = tLUID.HighPart;
                        tkp.Privileges[0] = tLUID.LowPart;
                        const int bufLength = 256;
                        IntPtr tu = Marshal.AllocHGlobal(bufLength);
                        Marshal.StructureToPtr(tkp, tu, true);
                        if (AdjustTokenPrivileges(hToken, 0, tu, bufLength, IntPtr.Zero, ref ltkpOld) != 0)
                        {
                            // successful AdjustTokenPrivileges doesn't mean privilege could be changed
                            if (Marshal.GetLastWin32Error() == 0)
                            {
                                retval = true; // Token changed
                            }
                        }
                        TOKEN_PRIVILEGES tokp = (TOKEN_PRIVILEGES)Marshal.PtrToStructure(tu, typeof(TOKEN_PRIVILEGES));
                        Marshal.FreeHGlobal(tu);
                    }
                }
            }
            if (hToken != IntPtr.Zero)
            {
                Kernel32.CloseHandle(hToken);
            }
            return retval;
        }
    }
}
