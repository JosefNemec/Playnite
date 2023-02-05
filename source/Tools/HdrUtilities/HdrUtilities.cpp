#include "HdrUtilities.h"

#include <vector>
#include <optional>

#include <Windows.h>

namespace
{
    std::optional<bool> NativeEnableHdr(bool enable)
    {
        UINT32 pathCount, modeCount;
        LONG result = GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount);
        if (result != ERROR_SUCCESS)
        {
            return std::nullopt;
        }

        std::vector<DISPLAYCONFIG_PATH_INFO> paths(pathCount);
        std::vector<DISPLAYCONFIG_MODE_INFO> modes(modeCount);

        result = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, paths.data(), &modeCount, modes.data(), nullptr);
        if (result != ERROR_SUCCESS)
        {
            return std::nullopt;
        }

        for (DISPLAYCONFIG_MODE_INFO const& mode : modes)
        {
            if (mode.infoType == DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE)
            {
                //The primary display is positioned at (0,0)
                if (mode.sourceMode.position.x == 0 && mode.sourceMode.position.y == 0)
                {
                    auto iter = std::find_if(
                        paths.cbegin(),
                        paths.cend(),
                        [&mode](DISPLAYCONFIG_PATH_INFO const& p) { return mode.id == p.sourceInfo.id; });

                    if (iter == paths.cend())
                    {
                        return std::nullopt;
                    }
                    DISPLAYCONFIG_PATH_TARGET_INFO const& targetInfo = iter->targetInfo;

                    //Retrieve current HDR state
                    DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO colorInfo = {};
                    colorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                    colorInfo.header.adapterId = targetInfo.adapterId;
                    colorInfo.header.id = targetInfo.id;
                    colorInfo.header.size = sizeof(colorInfo);
                    result = DisplayConfigGetDeviceInfo(&colorInfo.header);
                    if (result != ERROR_SUCCESS)
                    {
                        return std::nullopt;
                    }

                    bool wasHdrEnabled = colorInfo.advancedColorEnabled == 1;

                    //Enable or Disable HDR
                    if (colorInfo.advancedColorSupported)
                    {
                        DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE newColorInfo = {};
                        newColorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
                        newColorInfo.header.adapterId = targetInfo.adapterId;
                        newColorInfo.header.id = targetInfo.id;
                        newColorInfo.header.size = sizeof(newColorInfo);

                        newColorInfo.enableAdvancedColor = enable ? 1 : 0;
                        DisplayConfigSetDeviceInfo(&newColorInfo.header);
                    }

                    return wasHdrEnabled;
                }
            }
        }
        return std::nullopt;
    }

    std::optional<bool> NativeIsHdrSupported()
    {
        UINT32 pathCount, modeCount;
        LONG result = GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount);
        if (result != ERROR_SUCCESS)
        {
            return std::nullopt;
        }

        std::vector<DISPLAYCONFIG_PATH_INFO> paths(pathCount);
        std::vector<DISPLAYCONFIG_MODE_INFO> modes(modeCount);

        result = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, paths.data(), &modeCount, modes.data(), nullptr);
        if (result != ERROR_SUCCESS)
        {
            return std::nullopt;
        }

        for (DISPLAYCONFIG_MODE_INFO const& mode : modes)
        {
            if (mode.infoType == DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE)
            {
                //The primary display is positioned at (0,0)
                if (mode.sourceMode.position.x == 0 && mode.sourceMode.position.y == 0)
                {
                    auto iter = std::find_if(
                        paths.cbegin(),
                        paths.cend(),
                        [&mode](DISPLAYCONFIG_PATH_INFO const& p) { return mode.id == p.sourceInfo.id; });

                    if (iter == paths.cend())
                    {
                        return std::nullopt;
                    }
                    DISPLAYCONFIG_PATH_TARGET_INFO const& targetInfo = iter->targetInfo;

                    //Retrieve current HDR state
                    DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO colorInfo = {};
                    colorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                    colorInfo.header.adapterId = targetInfo.adapterId;
                    colorInfo.header.id = targetInfo.id;
                    colorInfo.header.size = sizeof(colorInfo);
                    result = DisplayConfigGetDeviceInfo(&colorInfo.header);
                    if (result != ERROR_SUCCESS)
                    {
                        return std::nullopt;
                    }

                    return colorInfo.advancedColorSupported == 1;
                }
            }
        }
        return std::nullopt;
    }
}

System::Nullable<bool> HdrUtilities::EnableHdr(bool enable)
{
    std::optional<bool> wasEnabled = NativeEnableHdr(enable);
    return wasEnabled.has_value() ? System::Nullable<bool>(wasEnabled.value()) : System::Nullable<bool>();
}

bool HdrUtilities::IsHdrSupported()
{
    std::optional<bool> isSupported = NativeIsHdrSupported();
    return isSupported.has_value() ? isSupported.value() : false;
}
