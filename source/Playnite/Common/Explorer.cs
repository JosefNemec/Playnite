using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class Explorer
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

        public static void NavigateToFileSystemEntry(string path)
        {
            var parentFolder = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parentFolder))
                return;

            SHParseDisplayName(parentFolder, IntPtr.Zero, out var nativeFolder, 0, out _);
            if (nativeFolder == IntPtr.Zero)
                return;

            var itemToSelect = Path.GetFileName(path);
            SHParseDisplayName(Path.Combine(parentFolder, itemToSelect), IntPtr.Zero, out var nativeFile, 0, out _);

            var fileArray = new[] { nativeFile == IntPtr.Zero ? nativeFolder : nativeFile };
            SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

            Marshal.FreeCoTaskMem(nativeFolder);
            if (nativeFile != IntPtr.Zero) Marshal.FreeCoTaskMem(nativeFile);
        }

        public static void OpenDirectory(string path)
        {
            // Adding trailing backslash ensures the path is treated as a directory
            // and using UseShellExecute avoids the process hanging issue
            if (!path.EndsWith("\\"))
                path += "\\";
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
