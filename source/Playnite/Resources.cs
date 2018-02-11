using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Playnite
{
    public class Resources
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr FindResource(IntPtr hModule, string lpName, string lpType);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        public static void ExtractResource(string path, string name, string type, string destination)
        {
            IntPtr hMod = LoadLibraryEx(path, IntPtr.Zero, 0x00000002);
            IntPtr hRes = FindResource(hMod, name, type);
            uint size = SizeofResource(hMod, hRes);
            IntPtr pt = LoadResource(hMod, hRes);

            byte[] bPtr = new byte[size];
            Marshal.Copy(pt, bPtr, 0, (int)size);
            using (MemoryStream m = new MemoryStream(bPtr))
            {
                File.WriteAllBytes(destination, m.ToArray());
            }
        }

        public static string ReadFileFromResource(string resource)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resource))
            {
                var tr = new StreamReader(stream);
                return tr.ReadToEnd();
            }
        }
    }
}
