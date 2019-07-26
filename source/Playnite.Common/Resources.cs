using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Playnite.Common
{
    public class Resources
    {

        public static void ExtractResource(string path, string name, string type, string destination)
        {
            IntPtr hMod = Interop.LoadLibraryEx(path, IntPtr.Zero, 0x00000002);
            IntPtr hRes = Interop.FindResource(hMod, name, type);
            uint size = Interop.SizeofResource(hMod, hRes);
            IntPtr pt = Interop.LoadResource(hMod, hRes);

            byte[] bPtr = new byte[size];
            Marshal.Copy(pt, bPtr, 0, (int)size);
            using (MemoryStream m = new MemoryStream(bPtr))
            {
                File.WriteAllBytes(destination, m.ToArray());
            }
        }

        public static long GetUriPackFileSize(string packUri)
        {
            var info = Application.GetResourceStream(new Uri(packUri));
            using (var stream = info.Stream)
            {
                return stream.Length;
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

        public static string GetIndirectResourceString(string fullName, string packageName, string resource)
        {
            var resUri = new Uri(resource);
            var resourceString = string.Empty;
            if (resource.StartsWith("ms-resource://"))
            {
                resourceString = $"@{{{fullName}? {resource}}}";
            }
            else if (resource.Contains('/'))
            {
                resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resource.Replace("ms-resource:", "").Trim('/')}}}";
            }
            else
            {
                resourceString = $"@{{{fullName}? ms-resource://{packageName}/resources/{resUri.Segments.Last()}}}";
            }

            var sb = new StringBuilder(1024);
            var result = Interop.SHLoadIndirectString(resourceString, sb, sb.Capacity, IntPtr.Zero);
            if (result == 0)
            {
                return sb.ToString();
            }

            resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resUri.Segments.Last()}}}";
            result = Interop.SHLoadIndirectString(resourceString, sb, sb.Capacity, IntPtr.Zero);
            if (result == 0)
            {
                return sb.ToString();
            }

            return string.Empty;
        }
    }
}
