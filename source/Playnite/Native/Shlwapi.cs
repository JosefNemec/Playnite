using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Native
{
    [Flags]
    public enum MatchPatternFlags : uint
    {
        Normal = 0x00000000,            // PMSF_NORMAL
        Multiple = 0x00000001,          // PMSF_MULTIPLE
        DontStripSpaces = 0x00010000    // PMSF_DONT_STRIP_SPACES
    }

    public class Shlwapi
    {
        private const string dllName = "Shlwapi.dll";

        [DllImport(dllName, BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
        public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        [DllImport(dllName, SetLastError = false)]
        public static extern int PathMatchSpecExW([MarshalAs(UnmanagedType.LPWStr)] string file, [MarshalAs(UnmanagedType.LPWStr)] string spec, MatchPatternFlags flags);
    }
}
