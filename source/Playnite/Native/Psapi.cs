using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Native
{
    public class Psapi
    {
        private const string dllName = "Psapi.dll";

        [DllImport(dllName, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetMappedFileName(IntPtr hProcess, IntPtr lpv, StringBuilder lpFilename, int nSize);
    }
}
