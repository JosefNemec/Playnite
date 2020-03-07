using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.Common;
using Playnite.Common.Media.Icons;
using Playnite.SDK;

namespace System.Drawing
{
    public static class IconExtension
    {
        public static byte[] ToByteArray(this Icon icon, System.Drawing.Imaging.ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                using (var bitmap = icon.ToBitmap())
                {
                    bitmap.Save(stream, format);
                    return stream.ToArray();
                }
            }
        }

        public static BitmapSource ToImageSource(this Icon icon)
        {
            using (Bitmap bitmap = icon.ToBitmap())
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                BitmapSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                if (!Interop.DeleteObject(hBitmap))
                {
                    throw new Win32Exception();
                }

                return wpfBitmap;
            }
        }
    }
}
