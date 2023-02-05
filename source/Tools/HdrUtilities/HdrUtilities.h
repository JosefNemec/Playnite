#pragma once

public ref class HdrUtilities
{
public:
	/// <summary>
	/// Enables/Disables HDR on the primary monitor
	/// </summary>
	/// <param name="enable">True if enabling HDR, false if disabling HDR</param>
	/// <returns>Returns the previous state of HDR on the primary monitor or null if it couldn't be determined</returns>
	static System::Nullable<bool> EnableHdr(bool enable);

	/// <summary>
	/// Determines if HDR is supported on the primary monitor
	/// </summary>
	/// <returns>True if HDR is supported on the primary monitor, false if unknown or not supported</returns>
	static bool IsHdrSupported();
};
