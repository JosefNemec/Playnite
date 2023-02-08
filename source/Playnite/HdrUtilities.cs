using Playnite.Native;
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
        /// <summary>
        /// Determines if HDR is supported on the primary display
        /// </summary>
        /// <returns>True if HDR is supported on the primary display, false if unknown or not supported</returns>
        public static bool IsHdrSupported()
        {
            DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
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
            DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
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
            DISPLAYCONFIG_PATH_TARGET_INFO? targetInfo = GetPrimaryDisplayTargetInfo();
            if (!targetInfo.HasValue)
            {
                return;
            }

            if (IsHdrSupported())
            {
                //Enable or Disable HDR
                var newColorInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE()
                {
                    header =
                    {
                        type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,
                        adapterId = targetInfo.Value.adapterId,
                        id = targetInfo.Value.id,
                        size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>()
                    }
                };

                newColorInfo.enableAdvancedColor = enable;

                User32.DisplayConfigSetDeviceInfo(ref newColorInfo.header);
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
            long result = User32.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
            if (result != WinError.ERROR_SUCCESS)
            {
                return null;
            }

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = User32.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (result != WinError.ERROR_SUCCESS)
            {
                return null;
            }

            DISPLAYCONFIG_MODE_INFO primaryDisplayMode = modes
                 .FirstOrDefault(IsPrimaryDisplayMode);

            DISPLAYCONFIG_PATH_TARGET_INFO targetInfo =
                paths.FirstOrDefault(p => p.sourceInfo.id == primaryDisplayMode.id)
                .targetInfo;

            return targetInfo;
        }

        private static (bool?, bool?) GetHdrStatus(DISPLAYCONFIG_PATH_TARGET_INFO targetInfo)
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

            long result = User32.DisplayConfigGetDeviceInfo(ref colorInfo);
            if (result != WinError.ERROR_SUCCESS)
            {
                return (null, null);
            }

            return (colorInfo.advancedColorSupported, colorInfo.advancedColorEnabled);
        }
    }
}
