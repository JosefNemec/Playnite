using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TGASharpLib;

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

        public static BitmapImage BitmapFromStream(Stream stream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public static long GetSizeInMemory(this BitmapImage image)
        {
            return Convert.ToInt64(image.PixelHeight * image.PixelWidth * 4);
        }

        public static BitmapImage TgaToBitmap(TGA tga)
        {
            var tgaBitmap = tga.ToBitmap();
            using (var memory = new MemoryStream())
            {
                tgaBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static BitmapImage TgaToBitmap(string tgaPath)
        {
            return TgaToBitmap(new TGA(tgaPath));
        }

        public static BitmapImage TgaToBitmap(byte[] tgaContent)
        {
            return TgaToBitmap(new TGA(tgaContent));
        }
    }
}
