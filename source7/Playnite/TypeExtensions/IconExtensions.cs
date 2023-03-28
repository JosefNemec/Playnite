using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Vanara.PInvoke;

namespace System.Windows.Controls;

public static class IconExtensions
{
    public static byte[] ToByteArray(this Icon icon, System.Drawing.Imaging.ImageFormat format)
    {
        using var stream = new MemoryStream();
        using var bitmap = icon.ToBitmap();
        bitmap.Save(stream, format);
        return stream.ToArray();
    }

    public static BitmapSource ToImageSource(this Icon icon)
    {
        using Bitmap bitmap = icon.ToBitmap();
        var hBitmap = bitmap.GetHbitmap();
        var wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        if (!Gdi32.DeleteObject(hBitmap))
        {
            throw new Win32Exception();
        }

        return wpfBitmap;
    }
}