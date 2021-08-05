using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Playnite.Native;

namespace Playnite.Common
{
    public class SigningTools
    {
        private static uint WinVerifyTrust(string fileName)
        {
            Guid wintrust_action_generic_verify_v2 = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
            uint result = 0;
            using (WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO(fileName,
                                                                        Guid.Empty))
            using (Wintrust.UnmanagedPointer guidPtr = new Wintrust.UnmanagedPointer(
                Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(Guid))),
                    WINTRUST_DATA.AllocMethod.HGlobal))
            using (Wintrust.UnmanagedPointer wvtDataPtr = new Wintrust.UnmanagedPointer(
                Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA))),
                WINTRUST_DATA.AllocMethod.HGlobal))
            {
                WINTRUST_DATA data = new WINTRUST_DATA(fileInfo);
                IntPtr pGuid = guidPtr;
                IntPtr pData = wvtDataPtr;
                Marshal.StructureToPtr(
                    wintrust_action_generic_verify_v2,
                    pGuid,
                    true);
                Marshal.StructureToPtr(
                    data,
                    pData,
                    true);
                result = User32.WinVerifyTrust(
                    IntPtr.Zero,
                    pGuid,
                    pData);
            }

            return result;
        }

        public static bool IsTrusted(string path)
        {
            return WinVerifyTrust(path) == 0;
        }
    }
}
