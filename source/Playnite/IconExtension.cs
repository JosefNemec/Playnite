using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public enum ShellIconSize : uint
    {
        SHGFI_ICON = 0x100,
        SHGFI_LARGEICON = 0x0,   // 32x32 pixels
        SHGFI_SMALLICON = 0x1    // 16x16 pixels
    }

    public static class IconExtension
    {
        [DllImport("Shell32.dll")]
        public extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Icon ExtractIconFromExe(string file, bool large)
        {
            int readIconCount = 0;
            IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
            IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };

            try
            {
                if (large)
                {
                    readIconCount = ExtractIconEx(file, 0, hIconEx, hDummy, 1);
                }
                else
                {
                    readIconCount = ExtractIconEx(file, 0, hDummy, hIconEx, 1);
                }

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero)
                {
                    Icon extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();
                    return extractedIcon;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not extract icon", e);
            }
            finally
            {
                foreach (IntPtr ptr in hIconEx)
                {
                    if (ptr != IntPtr.Zero)
                    {
                        DestroyIcon(ptr);
                    }
                }

                foreach (IntPtr ptr in hDummy)
                {
                    if (ptr != IntPtr.Zero)
                    {
                        DestroyIcon(ptr);
                    }
                }
            }
        }

        public static byte[] ToByteArray(this Icon icon, System.Drawing.Imaging.ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                icon.ToBitmap().Save(stream, format);
                return stream.ToArray();
            }
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }
    }
}
