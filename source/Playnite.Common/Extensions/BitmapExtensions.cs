using Playnite.Common;
using Playnite.SDK;
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

namespace System.Drawing.Imaging
{
    public class BitmapLoadProperties
    {
        public DpiScale? DpiScale { get; set; }
        public int MaxDecodePixelWidth { get; set; } = 0;
        public string Source { get; set; }

        public BitmapLoadProperties(int decodePixelWidth)
        {
            MaxDecodePixelWidth = decodePixelWidth;
        }

        public BitmapLoadProperties(int decodePixelWidth, DpiScale? dpiScale)
        {
            MaxDecodePixelWidth = decodePixelWidth;
            DpiScale = dpiScale;
        }
    }

    public static class BitmapExtensions
    {
        private static ILogger logger = LogManager.GetLogger();

        public static BitmapSource CreateSourceFromURI(Uri imageUri)
        {
            return CreateSourceFromURI(imageUri.LocalPath);
        }

        public static BitmapSource CreateSourceFromURI(string imageUri)
        {
            return BitmapFromFile(imageUri);
        }

        public static byte[] ToPngArray(this BitmapImage image)
        {
            return ToPngArray((BitmapSource)image);
        }

        public static byte[] ToPngArray(this BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static BitmapImage GetClone(this BitmapImage image, BitmapLoadProperties loadProperties = null)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return BitmapExtensions.BitmapFromStream(stream, loadProperties);
            }
        }

        public static Windows.Controls.Image ToImage(this BitmapImage bitmap, BitmapScalingMode scaling = BitmapScalingMode.Fant)
        {
            var image = new Windows.Controls.Image()
            {
                Source = bitmap
            };

            RenderOptions.SetBitmapScalingMode(image, scaling);
            return image;
        }

        public static BitmapImage BitmapFromFile(string imagePath, BitmapLoadProperties loadProperties = null)
        {
            try
            {
                using (var fStream = FileSystem.OpenReadFileStreamSafe(imagePath))
                {
                    return BitmapFromStream(fStream, loadProperties);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to create bitmap from file {imagePath}.");
                return null;
            }
        }

        public static BitmapImage BitmapFromStream(Stream stream, BitmapLoadProperties loadProperties = null)
        {
            try
            {
                var properties = Images.GetImageProperties(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                if (loadProperties?.MaxDecodePixelWidth > 0 && properties?.Width > loadProperties?.MaxDecodePixelWidth)
                {
                    if (loadProperties.DpiScale != null)
                    {
                        bitmap.DecodePixelWidth = Convert.ToInt32(loadProperties.MaxDecodePixelWidth * loadProperties.DpiScale.Value.DpiScaleX);
                    }
                    else
                    {
                        bitmap.DecodePixelWidth = loadProperties.MaxDecodePixelWidth;
                    }
                }
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to create bitmap from stream.");
                return null;
            }
        }

        public static long GetSizeInMemory(this BitmapImage image)
        {
            return Convert.ToInt64(image.PixelHeight * image.PixelWidth * 4);
        }

        public static BitmapImage TgaToBitmap(TGA tga)
        {
            try
            {
                using (var tgaBitmap = tga.ToBitmap())
                {
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
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to create bitmap from TGA image.");
                return null;
            }
        }

        public static Bitmap ToBitmap(this BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap, BitmapLoadProperties loadProperties = null)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Seek(0, SeekOrigin.Begin);
                var properties = Images.GetImageProperties(memory);
                memory.Seek(0, SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                if (loadProperties.DpiScale != null)
                {
                    bitmapImage.DecodePixelWidth = Convert.ToInt32(loadProperties.MaxDecodePixelWidth * loadProperties.DpiScale.Value.DpiScaleX);
                }
                else
                {
                    bitmapImage.DecodePixelWidth = loadProperties.MaxDecodePixelWidth;
                }
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
