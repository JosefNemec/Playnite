using System.Runtime.InteropServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.User32;

namespace Playnite;

// TODO:
// This port of the original code to Vanara is currently not working.
// Structures between original implementation and Vanara definition do not match...
// Probably will need separate/different implementation since HDR handling is using undocumented WinAPIs...

public class HdrUtilities
{
    private static readonly ILogger logger = LogManager.GetLogger();

    /// <summary>
    /// Determines if HDR is supported on the primary display
    /// </summary>
    /// <returns>True if HDR is supported on the primary display, false if unknown or not supported</returns>
    public static bool IsHdrSupported()
    {
        throw new NotImplementedException();

        //try
        //{
        //    var targetInfo = GetPrimaryDisplayTargetInfo();
        //    if (!targetInfo.HasValue)
        //    {
        //        logger.Error("Failed to retrieve primary display target info");
        //        return false;
        //    }

        //    return GetHdrStatus(targetInfo.Value).supported;
        //}
        //catch (Exception e) when (!AppConfig.ThrowAllErrors)
        //{
        //    logger.Error(e, "Failed to check if HDR is supported");
        //    return false;
        //}
    }

    /// <summary>
    /// Determines if HDR is enabled on the primary display
    /// </summary>
    /// <returns>True if HDR is enabled on the primary display, false if unknown or not supported</returns>
    public static bool IsHdrEnabled()
    {
        throw new NotImplementedException();
        //try
        //{
        //    var targetInfo = GetPrimaryDisplayTargetInfo();
        //    if (!targetInfo.HasValue)
        //    {
        //        logger.Error("Failed to retrieve primary display target info");
        //        return false;
        //    }

        //    return GetHdrStatus(targetInfo.Value).enabled;
        //}
        //catch (Exception e) when (!AppConfig.ThrowAllErrors)
        //{
        //    logger.Error(e, "Failed to check if HDR is enabled");
        //    return false;
        //}
    }

    /// <summary>
    /// Enables/disables HDR on the primary display
    /// </summary>
    /// <param name="enable">True if enabling HDR, false if disabling HDR</param>
    public static void SetHdrEnabled(bool enable)
    {
        throw new NotImplementedException();
        //try
        //{
        //    var targetInfo = GetPrimaryDisplayTargetInfo();
        //    if (!targetInfo.HasValue)
        //    {
        //        logger.Error("Failed to retrieve primary display target info");
        //        return;
        //    }

        //    if (IsHdrSupported())
        //    {
        //        //Enable or Disable HDR
        //        var newColorInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
        //        {
        //            header =
        //            {
        //                type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,
        //                adapterId = targetInfo.Value.adapterId,
        //                id = targetInfo.Value.id,
        //                size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>()
        //            },
        //            value = enable ? DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE_VALUE.enableAdvancedColor : 0
        //        };

        //        User32.DisplayConfigSetDeviceInfo<DISPLAYCONFIG_DEVICE_INFO_HEADER>(newColorInfo.header);
        //    }
        //}
        //catch (Exception e) when (!AppConfig.ThrowAllErrors)
        //{
        //    logger.Error(e, "Failed to set HDR state");
        //}
    }

    private static bool IsPrimaryDisplayMode(DISPLAYCONFIG_MODE_INFO mode)
    {
        return
            mode.infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE &&
            mode.sourceMode.position.x == 0 &&
            mode.sourceMode.position.y == 0;
    }

    private static DISPLAYCONFIG_PATH_TARGET_INFO? GetPrimaryDisplayTargetInfo()
    {
        var result = User32.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
        if (result != Win32Error.ERROR_SUCCESS)
        {
            logger.Error("Failed to retrieve display config buffer sizes");
            return null;
        }

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        result = User32.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
        if (result != Win32Error.ERROR_SUCCESS)
        {
            logger.Error("Failed to retrieve display config info");
            return null;
        }

        var primaryDisplayMode = modes
             .FirstOrDefault(IsPrimaryDisplayMode);

        var targetInfo =
            paths.FirstOrDefault(p => p.sourceInfo.id == primaryDisplayMode.id)
            .targetInfo;

        return targetInfo;
    }

    private static (bool supported, bool enabled) GetHdrStatus(DISPLAYCONFIG_PATH_TARGET_INFO targetInfo)
    {
        //Retrieve current HDR state
        var colorInfo = User32.DisplayConfigGetDeviceInfo<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>(
            targetInfo.adapterId,
            targetInfo.id,
            DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO);
        return (colorInfo.value == DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_VALUE.advancedColorSupported,
            colorInfo.value == DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_VALUE.advancedColorEnabled);
    }
}
