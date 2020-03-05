using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public enum AppBarStates
    {
        None = 0x00,
        AutoHide = 0x01,
        AlwaysOnTop = 0x02
    }

    public class WindowsEnvironment
    {
        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);

        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public Rectangle rc;
            public Int32 lParam;
        }

        public static AppBarStates GetTaskbarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = Interop.FindWindow("System_TrayWnd", null);
            if (msgData.hWnd == IntPtr.Zero)
            {
                msgData.hWnd = Interop.FindWindow("Shell_TrayWnd", null);
            }

            if (msgData.hWnd == IntPtr.Zero)
            {
                return AppBarStates.None;
            }
            else
            {
                return (AppBarStates)SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
            }
        }
    }
}