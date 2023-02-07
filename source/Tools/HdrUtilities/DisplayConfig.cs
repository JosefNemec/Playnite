using System;
using System.Runtime.InteropServices;

internal static class DisplayConfig
{
    #region Constants

    public static long ErrorSuccess => 0;

    #endregion

    #region Functions

    [DllImport("user32")]
    public static extern int GetDisplayConfigBufferSizes(Qdc flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

    [DllImport("user32")]
    public static extern int QueryDisplayConfig(Qdc flags, ref uint numPathArrayElements, [In, Out] DisplayConfigPathInfo[] pathArray, ref uint numModeInfoArrayElements, [In, Out] DisplayConfigModeInfo[] modeInfoArray, IntPtr currentTopologyId);

    [DllImport("user32")]
    public static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigDeviceInfoHeader requestPacket);

    [DllImport("user32")]
    public static extern int DisplayConfigSetDeviceInfo(ref DisplayConfigDeviceInfoHeader requestPacket);

    #endregion

    #region Enums

    public enum DisplayConfigDeviceInfoType
    {
        GetTargetName = 2,
        GetAdvancedColorInfo = 9,
        SetAdvancedColorState = 10,
    }

    public enum Qdc
    {
        OnlyActivePaths = 0x2
    }

    public enum DisplayConfigModeInfoType
    {
        Source = 1,
        Target = 2,
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigDeviceInfoHeader
    {
        public DisplayConfigDeviceInfoType type;
        public int size;
        public Luid adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, Size = 32)]
    public struct DisplayConfigGetAdvancedColorInfo
    {
        public DisplayConfigDeviceInfoHeader header;
        public uint value;

        public bool advancedColorSupported => (value & 0x1) == 0x1;
        public bool advancedColorEnabled => (value & 0x2) == 0x2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayConfigSetAdvancedColorState
    {
        public DisplayConfigDeviceInfoHeader header;
        public uint value;

        public bool enableAdvancedColor
        {
            get => (value & 0x1) == 1;
            set
            {
                uint mask = 0x1;
                if (value)
                {
                    this.value |= mask;
                }
                else
                {
                    this.value &= ~mask;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Luid { }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct DisplayConfigSourceMode
    {
        [FieldOffset(12)]
        public Point position;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct DisplayConfigPathSourceInfo
    {
        [FieldOffset(8)]
        public uint id;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct DisplayConfigPathTargetInfo
    {
        [FieldOffset(0)]
        public Luid adapterId;
        [FieldOffset(8)]
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, Size = 72)]
    public struct DisplayConfigPathInfo
    {
        public DisplayConfigPathSourceInfo sourceInfo;
        public DisplayConfigPathTargetInfo targetInfo;
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct DisplayConfigModeInfo
    {
        [FieldOffset(0)]
        public DisplayConfigModeInfoType infoType;
        [FieldOffset(4)]
        public uint id;
        [FieldOffset(16)]
        public DisplayConfigSourceMode sourceMode;
    }

    #endregion
}