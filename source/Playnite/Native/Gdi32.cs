using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Native
{
    public class Gdi32
    {
        private const string dllName = "Gdi32.dll";

        [DllImport(dllName, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
