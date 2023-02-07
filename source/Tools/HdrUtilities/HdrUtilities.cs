using System.Linq;
using System;
using System.Runtime.InteropServices;

public class HdrUtilities
{
    /// <summary>
    /// Determines if HDR is supported on the primary display
    /// </summary>
    /// <returns>True if HDR is supported on the primary display, false if unknown or not supported</returns>
    public static bool IsHdrSupported()
    {
        DisplayConfig.DisplayConfigPathTargetInfo? targetInfo = GetPrimaryDisplayTargetInfo();
        if (!targetInfo.HasValue)
        {
            return false;
        }

        (bool? isHdrSupported, _) = GetHdrStatus(targetInfo.Value);
        return isHdrSupported.GetValueOrDefault(false);
    }

    /// <summary>
    /// Determines if HDR is enabled on the primary display
    /// </summary>
    /// <returns>True if HDR is enabled on the primary display, false if unknown or not supported</returns>
    public static bool? IsHdrEnabled()
    {
        DisplayConfig.DisplayConfigPathTargetInfo? targetInfo = GetPrimaryDisplayTargetInfo();
        if (!targetInfo.HasValue)
        {
            return false;
        }

        (_, bool? isHdrEnabled) = GetHdrStatus(targetInfo.Value);
        return isHdrEnabled;
    }

    /// <summary>
    /// Enables/disables HDR on the primary display
    /// </summary>
    /// <param name="enable">True if enabling HDR, false if disabling HDR</param>
    public static void EnableHdr(bool enable)
    {
        DisplayConfig.DisplayConfigPathTargetInfo? targetInfo = GetPrimaryDisplayTargetInfo();
        if (!targetInfo.HasValue)
        {
            return;
        }

        if (IsHdrSupported())
        {
            //Enable or Disable HDR
            var newColorInfo = new DisplayConfig.DisplayConfigSetAdvancedColorState();
            newColorInfo.header.type = DisplayConfig.DisplayConfigDeviceInfoType.SetAdvancedColorState;
            newColorInfo.header.adapterId = targetInfo.Value.adapterId;
            newColorInfo.header.id = targetInfo.Value.id;
            newColorInfo.header.size = Marshal.SizeOf<DisplayConfig.DisplayConfigSetAdvancedColorState>();
            newColorInfo.enableAdvancedColor = enable;

            DisplayConfig.DisplayConfigSetDeviceInfo(ref newColorInfo.header);
        }
    }

    private static bool IsPrimaryDisplayMode(DisplayConfig.DisplayConfigModeInfo mode)
    {
        return
            mode.infoType == DisplayConfig.DisplayConfigModeInfoType.Source &&
            mode.sourceMode.position.x == 0 &&
            mode.sourceMode.position.y == 0;
    }

    private static DisplayConfig.DisplayConfigPathTargetInfo? GetPrimaryDisplayTargetInfo()
    {
        long result = DisplayConfig.GetDisplayConfigBufferSizes(DisplayConfig.Qdc.OnlyActivePaths, out uint pathCount, out uint modeCount);
        if (result != DisplayConfig.ErrorSuccess)
        {
            return null;
        }

        var paths = new DisplayConfig.DisplayConfigPathInfo[pathCount];
        var modes = new DisplayConfig.DisplayConfigModeInfo[modeCount];

        result = DisplayConfig.QueryDisplayConfig(DisplayConfig.Qdc.OnlyActivePaths, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
        if (result != DisplayConfig.ErrorSuccess)
        {
            return null;
        }

        DisplayConfig.DisplayConfigModeInfo primaryDisplayMode = modes
             .FirstOrDefault(IsPrimaryDisplayMode);

        DisplayConfig.DisplayConfigPathTargetInfo targetInfo =
            paths.FirstOrDefault(p => p.sourceInfo.id == primaryDisplayMode.id)
            .targetInfo;

        return targetInfo;
    }

    private static (bool?, bool?) GetHdrStatus(DisplayConfig.DisplayConfigPathTargetInfo targetInfo)
    {
        //Retrieve current HDR state
        var colorInfo = new DisplayConfig.DisplayConfigGetAdvancedColorInfo();
        colorInfo.header.type = DisplayConfig.DisplayConfigDeviceInfoType.GetAdvancedColorInfo;
        colorInfo.header.adapterId = targetInfo.adapterId;
        colorInfo.header.id = targetInfo.id;
        colorInfo.header.size = Marshal.SizeOf<DisplayConfig.DisplayConfigGetAdvancedColorInfo>();

        long result = DisplayConfig.DisplayConfigGetDeviceInfo(ref colorInfo.header);
        if (result != DisplayConfig.ErrorSuccess)
        {
            return (null, null);
        }

        return (colorInfo.advancedColorSupported, colorInfo.advancedColorEnabled);
    }
}