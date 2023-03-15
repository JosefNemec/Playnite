using Playnite.Common;
using Playnite.Native;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class HdrUtilities
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        /// <summary>
        /// Determines if HDR is supported on the primary display
        /// </summary>
        /// <returns>True if HDR is supported on the primary display, false if unknown or not supported</returns>
        public static bool IsHdrSupported()
        {
            if (Computer.WindowsVersion < WindowsVersion.Win10)
            {
                return false;
            }

            try
            {
                DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
                if (!targetInfo.HasValue)
                {
                    logger.Error("Failed to retrieve primary display target info");
                    return false;
                }

                (bool isHdrSupported, _) = GetHdrStatus(targetInfo.Value);
                return isHdrSupported;
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to check if HDR is supported");
                return false;
            }
        }

        /// <summary>
        /// Determines if HDR is enabled on the primary display
        /// </summary>
        /// <returns>True if HDR is enabled on the primary display, false if unknown or not supported</returns>
        public static bool IsHdrEnabled()
        {
            if (Computer.WindowsVersion < WindowsVersion.Win10)
            {
                return false;
            }

            try
            {
                DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
                if (!targetInfo.HasValue)
                {
                    logger.Error("Failed to retrieve primary display target info");
                    return false;
                }

                (_, bool isHdrEnabled) = GetHdrStatus(targetInfo.Value);
                return isHdrEnabled;
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to check if HDR is enabled");
                return false;
            }
        }

        /// <summary>
        /// Enables/disables HDR on the primary display
        /// </summary>
        /// <param name="enable">True if enabling HDR, false if disabling HDR</param>
        public static void SetHdrEnabled(bool enable)
        {
            if (Computer.WindowsVersion < WindowsVersion.Win10)
            {
                return;
            }

            try
            {
                DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
                if (!targetInfo.HasValue)
                {
                    logger.Error("Failed to retrieve primary display target info");
                    return;
                }

                if (IsHdrSupported())
                {
                    //Enable or Disable HDR
                    var newColorInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
                    {
                        header =
                        {
                            type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,
                            adapterId = targetInfo.Value.adapterId,
                            id = targetInfo.Value.id,
                            size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>()
                        },
                        enableAdvancedColor = enable
                    };

                    User32.DisplayConfigSetDeviceInfo(ref newColorInfo.header);
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to set HDR state");
            }
        }

        private static bool IsPrimaryDisplayMode(DISPLAYCONFIG_MODE_INFO mode)
        {
            return
                mode.infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE &&
                mode.modeInfo.sourceMode.position.x == 0 &&
                mode.modeInfo.sourceMode.position.y == 0;
        }

        private static DISPLAYCONFIG_PATH_TARGET_INFO? GetPrimaryDisplayTargetInfo()
        {
            var result = User32.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
            if (result != WinError.ERROR_SUCCESS)
            {
                logger.Error("Failed to retrieve display config buffer sizes");
                return null;
            }

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = User32.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (result != WinError.ERROR_SUCCESS)
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

        private static (bool, bool) GetHdrStatus(DISPLAYCONFIG_PATH_TARGET_INFO targetInfo)
        {
            //Retrieve current HDR state
            var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
            {
                header =
                {
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                    adapterId = targetInfo.adapterId,
                    id = targetInfo.id,
                    size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>()
                }
            };

            var result = User32.DisplayConfigGetDeviceInfo(ref colorInfo);
            if (result != WinError.ERROR_SUCCESS)
            {
                logger.Error("Failed to retrieve advanced color info");
                return (false, false);
            }

            return (colorInfo.advancedColorSupported, colorInfo.advancedColorEnabled);
        }
    }
}
