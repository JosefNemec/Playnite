using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public static class BitmapExtensions
    {
        public static BitmapSource CreateSourceFromURI(Uri imageUri)
        {
            return new BitmapImage(imageUri);            
        }

        public static byte[] ToPngArray(this BitmapImage image)
        {
            return ToPngArray((BitmapSource)image);
        }

        public static byte[] ToPngArray(this BitmapSource image)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static BitmapImage BitmapFromFile(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(imagePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
