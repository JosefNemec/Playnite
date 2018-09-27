using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class InteropMethods
    {
        [DllImport("Shell32.dll")]
        public extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
